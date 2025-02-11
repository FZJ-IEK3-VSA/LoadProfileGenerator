using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using Common;
using Common.JSON;
using MassSimulation.CityGeneration;
using MassSimulation.Scenarios;
using MassSimulation.Simulators;
using MPI;

namespace MassSimulation
{
    /// <summary>
    /// MPI Worker class that is instantiated once per MPI process and handles the whole simulation.
    /// </summary>
    internal class Worker
    {
        private readonly Intracommunicator comm;
        private readonly int rank;
        private readonly int numWorkers;
        private readonly string workerName;
        private readonly string inputPath;

        private LPGMassSimulator lpgSimulator;
        private List<PointOfInterestSimulator> poiSimulators = [];
        private TransportSimulator? transportSimulator;

        private CalcParameters? calcParameters;
        private Scenario? scenario;
        private ScenarioPart? scenarioPart;

        private readonly MPILogger logger;

        public Worker(Intracommunicator comm, string[] args)
        {
            // parse command line arguments
            if (args.Length == 0)
                throw new LPGException("Did not receive any command line arguments.");
            if (args.Length > 1)
                throw new LPGException($"Received unexpected command line arguments: {args}");
            inputPath = args[0];

            this.comm = comm;
            rank = comm.Rank;
            numWorkers = comm.Size;
            workerName = MPI.Environment.ProcessorName;

            logger = new MPILogger(true, rank);
        }

        /// <summary>
        /// Main method that runs the simulation, including setup, execution and postprocessing.
        /// </summary>
        public void Run()
        {
            logger.Info("Starting mass simulation with " + numWorkers + " workers.");

            var start = DateTime.Now;
            try
            {
                InitSimulation(inputPath);
            }
            catch (Exception e)
            {
                logger.Error($"Exception during initialization:\n{e}");
                throw;
            }
            logger.Info($"Finished initialization in {DateTime.Now - start}");

            var simulationStart = DateTime.Now;
            RunSimulation();
            comm.Barrier();
            logger.Info($"Finished core simulation in {DateTime.Now - simulationStart}");


            var postprocessingStart = DateTime.Now;
            try
            {
                FinishSimulation();
            }
            catch (Exception e)
            {
                logger.Error($"Exception during postprocessing:\n{e}");
                throw;
            }
            comm.Barrier();
            logger.Info($"Finished postprocessing: {DateTime.Now - postprocessingStart}");
            logger.Info($"Finished city simulation: {DateTime.Now - start}");
        }

        private void InitSimulation(string inputPath)
        {
            // general settings
            // avoid MPI processes cluttering the console
            Config.OutputToConsole = false;

            // create scenario
            ScenarioPart[]? scenarioParts = null;
            if (rank == 0)
            {
                // determine simulation targets
                //scenario = TestScenarios.CreateDuplicateHousesScenario(inputPath, numWorkers);
                scenario = CityScenarioImport.ReadScenarioFromConfigDirectory(inputPath);
                scenarioParts = scenario.GetScenarioParts(numWorkers);
                int length = scenarioParts.Length;
                if (length < numWorkers)
                {
                    // not enough parts for all workers
                    throw new LPGException("Not enough work packages for all MPI processes (" + length + " work packages for " + numWorkers + " workers).");
                }
            }

            // distribute simulation targets
            scenarioPart = comm.Scatter(scenarioParts, 0);

            lpgSimulator = new(rank, scenarioPart);
            calcParameters = lpgSimulator.CalcParameters;

            lpgSimulator.Init();

            // initialize the transport simulator
            transportSimulator = new TransportSimulator(rank, scenarioPart.CalcSpecification);

            // initialize the point of interst simulators
            poiSimulators = scenarioPart.PointsOfInterest.Select(poi => new PointOfInterestSimulator(rank, poi.Id, scenarioPart.CalcSpecification)).ToList();
        }

        private void RunSimulation()
        {
            if (calcParameters is null)
            {
                throw new LPGException("CalcParameters are not set");
            }
            // define iteration variables
            var simulationTime = calcParameters.InternalStartTime;
            var timestep = new TimeStep(0, calcParameters);
            // initialize the variable for storing exchanged messages across iterations, starting with no messages
            SortedMessageCollection activityMessages = new([], [], []);

            // main simulation loop
            while (simulationTime < calcParameters.InternalEndTime)
            {
                // run all simulators for one timestep
                var messageDistributor = SimulateOneStep(timestep, simulationTime, activityMessages);

                // exchange messages via MPI; this calls MPI.AllToAll
                activityMessages = messageDistributor.DistributeMessages(comm);

                // increment timestep
                simulationTime += calcParameters.InternalStepsize;
                timestep = timestep.AddSteps(1);
            }
        }

        public MPIDistributor SimulateOneStep(TimeStep timestep, DateTime simulationTime, SortedMessageCollection activityMessages)
        {
            // run household simulators first and get newly started remote newTravels
            var remoteTravelsAndActivities = lpgSimulator.SimulateOneStep(timestep, simulationTime, activityMessages.finishedActivities);

            // create a new object for message collection and distribution
            var messageCollector = new MPIDistributor(numWorkers, scenarioPart.PoiRegister);
            messageCollector.AddNewActivities(remoteTravelsAndActivities);
            // remark: for consistency, these messages are only distributed after this timestep is finished

            // run transport simulation
            var finishedTravels = transportSimulator.SimulateOneStep(timestep, simulationTime, activityMessages.NewTravelActivities);
            messageCollector.AddFinishedActivities(finishedTravels);

            // run POI simulators
            Dictionary<PointOfInterestId, IEnumerable<RemoteActivityStart>> newActivities = [];
            foreach (var simulator in poiSimulators)
            {
                var relevantActivities = activityMessages.NewPoiActivities.GetValueOrDefault(simulator.PoiId, []);
                var finishedActivities = simulator.SimulateOneStep(timestep, simulationTime, relevantActivities);
                messageCollector.AddFinishedActivities(finishedActivities);
            }
            return messageCollector;
        }

        private void FinishSimulation()
        {
            lpgSimulator.FinishSimulation();
            transportSimulator.FinishSimulation();

            foreach (var simulator in poiSimulators)
            {
                simulator.FinishSimulation();
            }

            if (rank == 0)
            {
                // remove unneeded files and subdirectories
                SimulationEngineLib.HouseJobProcessor.JsonCalculator.CleanUpResultDirectory(scenario!.CalcSpecification);
            }
        }
    }
}

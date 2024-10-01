using Automation;
using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using Common;
using Common.JSON;
using MassSimulation.Simulators;
using MPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    /// <summary>
    /// MPI Worker class that is instantiated once per MPI process.
    /// </summary>
    internal class Worker
    {
        private readonly Intracommunicator comm;
        private readonly int rank;
        private readonly int numWorkers;
        private readonly string workerName;

        private LPGMassSimulator lpgSimulator;
        private List<PointOfInterestSimulator> poiSimulators = [];
        private TransportSimulator? transportSimulator;

        private PointOfInterestRegister poiRegister;

        private CalcParameters? calcParameters;
        private Scenario? scenario;

        private readonly MPILogger logger;

        public Worker(Intracommunicator comm)
        {
            this.comm = comm;
            rank = comm.Rank;
            numWorkers = comm.Size;
            workerName = MPI.Environment.ProcessorName;

            poiRegister = new(numWorkers);
            logger = new MPILogger(true, rank);
        }

        public void Run()
        {
            int numAgents = numWorkers; // TODO: for testing
            var houseJobFile = @"D:\Home\Homeoffice\Arbeit FzJ\Projekte\Große Projekte\03 - LPG\test.json";

            logger.Info("Starting mass simulation with " + numWorkers + " workers.");

            InitSimulation(houseJobFile, numAgents);
            logger.Info("Finished initialization");

            Stopwatch watch = Stopwatch.StartNew();
            RunSimulation();
            watch.Stop();
            logger.Info("Finished simulation");

            FinishSimulation();

            comm.Barrier();
            if (rank == 0)
                logger.Info("Simulation time: " + Math.Round((double)watch.ElapsedMilliseconds / 1000, 2) + " s");
        }

        public void InitSimulation(string houseJobFile, int numAgents)
        {
            // general settings
            // avoid MPI processes cluttering the console
            Config.OutputToConsole = false;

            // create scenario
            ScenarioPart[]? scenarioParts = null;
            if (rank == 0)
            {
                // determine simulation targets
                scenario = Scenario.CreateDuplicateHousesScenario(houseJobFile, numAgents);
                scenarioParts = scenario.GetScenarioParts(numWorkers);
                int length = scenarioParts.Length;
                if (length < numWorkers)
                {
                    // not enough parts for all workers
                    throw new LPGException("Not enough work packages for all MPI processes (" + length + " work packages for " + numWorkers + " workers).");
                }
            }

            // distribute simulation targets
            ScenarioPart partForThisWorker = comm.Scatter(scenarioParts, 0);

            lpgSimulator = new(rank, partForThisWorker);
            calcParameters = lpgSimulator.CalcParameters;

            lpgSimulator.Init();

            // initialize the transport simulator
            transportSimulator = new TransportSimulator();
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

            // sort new activity messages by target worker
            var messageCollector = poiRegister.SortActivityMessagesByWorker(remoteTravelsAndActivities);
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

        private void CollectResults(List<PersonIdentifier> agents)
        {
            // collect all agents
            int[] agentCounts = comm.Gather(agents.Count, 0);
            if (rank == 0)
                Console.WriteLine("Agent counts: " + string.Join(", ", agentCounts));
            var allAgents = comm.GatherFlattened(agents.ToArray(), agentCounts, 0);
            comm.Barrier();

            // result processing
            if (rank == 0)
            {
                //ProcessResults(allAgents);
            }
        }
    }
}

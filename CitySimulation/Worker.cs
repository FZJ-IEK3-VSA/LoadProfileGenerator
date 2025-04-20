using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using Common;
using Common.JSON;
using CitySimulation.CityGeneration;
using CitySimulation.Scenarios;
using CitySimulation.Simulators;
using MPI;
using System.Runtime.InteropServices;
using CitySimulation.SimulationTargets;

namespace CitySimulation
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
        private readonly CalculationProfiler? calculationProfiler;

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
            calculationProfiler = new();
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
            logger.Info($"Finished initialization in  {DateTime.Now - start}");

            var simulationStart = DateTime.Now;
            RunSimulation();
            MPIBarrierWithLog();
            logger.Info($"Finished main simulation in {DateTime.Now - simulationStart}");


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
            MPIBarrierWithLog();
            logger.Info($"Finished postprocessing in  {DateTime.Now - postprocessingStart}");
            logger.Info($"Finished city simulation in {DateTime.Now - start}");
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
                scenario = CityScenarioImport.ReadScenarioFromConfigDirectory(inputPath);
                scenarioParts = scenario.GetScenarioParts(numWorkers);
                int length = scenarioParts.Length;
                if (length < numWorkers)
                {
                    // not enough parts for all workers
                    throw new LPGException($"Not enough work packages for all MPI processes ({length} work packages for {numWorkers} workers).");
                }
            }

            // distribute simulation targets
            logger.Debug("Calling MPI Scatter to distribute scenario parts.");
            scenarioPart = comm.Scatter(scenarioParts, 0);

            // configure the logger
            string logFile = Path.Combine(scenarioPart.CalcSpecification.OutputDirectory, $"Log.CitySimulation.Worker{rank}.txt");
            logger.SetLogFilePath(logFile);
            logger.Info($"Worker {rank} on {workerName} is responsible for {scenarioPart.TargetReferences.Count} houses and {scenarioPart.PointsOfInterest.Count} POIs.");

            lpgSimulator = new(rank, scenarioPart);
            calcParameters = lpgSimulator.CalcParameters;
            int totalPersons = lpgSimulator.TotalNumberOfPersons();
            int totalHouseholds = lpgSimulator.TotalNumberOfHouseholds();
            logger.Info($"In total, this worker simulates {totalPersons} persons in {totalHouseholds} households.");

            lpgSimulator.Init();

            // initialize the transport simulator
            transportSimulator = new TransportSimulator(rank, scenarioPart.CalcSpecification);
            CreatePoiSimulators();
        }

        private void CreatePoiSimulators()
        {
            // initialize the point of interest simulators
            poiSimulators = [.. scenarioPart.PointsOfInterest.Select(CreatePoiSimulator)];
        }

        private PointOfInterestSimulator CreatePoiSimulator(PointOfInterestConfig poi)
        {
            return poi.LocationType.Name switch
            {
                //"Doctors Office" => new QueuePointOfInterestSimulator(rank, poi.Id, scenarioPart.CalcSpecification, 2),
                _ => new PointOfInterestSimulator(rank, poi.Id, scenarioPart.CalcSpecification),
            };
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

            // this barrier is not required, but it makes all workers start the main loop at the same time
            MPIBarrierWithLog();

            calculationProfiler?.StartPart("Main simulation loop", false);
            var startLoop = DateTime.UtcNow;
            TimeSpan totalDistribution = TimeSpan.Zero;
            // main simulation loop
            while (simulationTime < calcParameters.InternalEndTime)
            {
                // run all simulators for one timestep
                calculationProfiler?.StartPart("single simulation step", false);
                var messageDistributor = SimulateOneStep(timestep, simulationTime, activityMessages);
                calculationProfiler?.StopPart("single simulation step", false);

                // exchange messages via MPI; this calls MPI.AllToAll
                calculationProfiler?.StartPart("MPI message distribution", false);
                var startDistribution = DateTime.UtcNow;
                activityMessages = messageDistributor.DistributeMessages(comm);
                totalDistribution += DateTime.UtcNow - startDistribution;
                calculationProfiler?.StopPart("MPI message distribution", false);

                // increment timestep
                simulationTime += calcParameters.InternalStepsize;
                timestep = timestep.AddSteps(1);
            }
            TimeSpan totalLoop = DateTime.UtcNow - startLoop;
            calculationProfiler?.StopPart("Main simulation loop", false);
            double stepsPerSecond = timestep.InternalStep / totalLoop.TotalSeconds;
            int totalHouseholds = lpgSimulator.TotalNumberOfHouseholds();
            logger.Info($"Main loop: {totalLoop}, MPI distribution: {totalDistribution} ({100 * totalDistribution / totalLoop:f2} %), speed: {stepsPerSecond:f2} steps/second, {stepsPerSecond * totalHouseholds:f2} household steps/second");
        }

        private void MPIBarrierWithLog()
        {
            logger.Debug("Calling MPI Barrier");
            var startBarrier = DateTime.UtcNow;
            comm.Barrier();
            logger.Debug($"MPI Barrier is over after {DateTime.UtcNow - startBarrier}");
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

            // create an additional flame chart for the calculation profiler of this MPI worker
            if (calculationProfiler is not null && calcParameters.Options.Contains(Automation.CalcOption.CalculationFlameChart))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var profilerDirectory = Path.Combine(scenarioPart.CalcSpecification.OutputDirectory, "CalculationProfiler");
                    ChartCreator2.OxyCharts.ChartMaker.MakeFlameChart(new DirectoryInfo(profilerDirectory), calculationProfiler, $"Worker{rank}");
                }
                else
                {
                    logger.Warning("CalculationProfiler flame chart creation is only supported on Windows.");
                }
            }

            if (rank == 0)
            {
                // remove unneeded files and subdirectories
                SimulationEngineLib.HouseJobProcessor.JsonCalculator.CleanUpResultDirectory(scenario!.CalcSpecification);
            }
        }
    }
}

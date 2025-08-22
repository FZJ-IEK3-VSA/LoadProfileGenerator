using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using Common;
using CitySimulation.Scenarios;
using CitySimulation.Simulators;
using MPI;
using System.Runtime.InteropServices;
using CitySimulation.SimulationTargets;

namespace CitySimulation
{
    /// <summary>
    /// CitySimulation worker class that is instantiated once per MPI process and handles the whole simulation.
    /// </summary>
    internal class Worker
    {
        private readonly Intracommunicator comm;
        private readonly int rank;
        private readonly int numWorkers;
        private readonly string workerName;
        private readonly string inputPath;
        private readonly string outputPath;

        private readonly ScenarioPart scenarioPart;

        private readonly LPGMassSimulator lpgSimulator;
        private readonly List<PointOfInterestSimulator> poiSimulators = [];
        private readonly TransportSimulator transportSimulator;

        private readonly MPILogger logger;
        private readonly CalculationProfiler? calculationProfiler;
        private readonly DateTime start;

        public Worker(Intracommunicator comm, string[] args)
        {
            // parse command line arguments
            if (args.Length == 0)
                throw new LPGException("Did not receive any command line arguments.");
            if (args.Length > 1)
                throw new LPGException($"Received unexpected command line arguments: {args}");
            inputPath = args[0];

            // store general MPI information about the process
            this.comm = comm;
            rank = comm.Rank;
            numWorkers = comm.Size;
            workerName = MPI.Environment.ProcessorName;

            logger = new MPILogger(true, rank);
            calculationProfiler = new();

            logger.Info("Starting mass simulation with " + numWorkers + " workers.");
            start = DateTime.Now;

            // general settings
            // avoid MPI processes cluttering the console
            Config.OutputToConsole = false;
            Config.ResultLogger = Common.SQLResultLogging.ResultLoggerType.JSON;

            scenarioPart = LoadAndDistributeScenario(inputPath);
            outputPath = scenarioPart.CalcSpecification.OutputDirectory ?? throw new LPGPBadParameterException("Missing output path");

            InitLogger();

            // init all simulators
            try
            {
                lpgSimulator = InitLPGSimulator();
                transportSimulator = new TransportSimulator(rank, scenarioPart.CalcSpecification.OutputDirectory);
                poiSimulators = CreatePoiSimulators();
            }
            catch (Exception e)
            {
                logger.Error($"Exception during simulator initialization:\n{e}");
                throw;
            }
            logger.Info($"Finished initialization in  {DateTime.Now - start}");
        }

        /// <summary>
        /// Initializes the logger by setting the logfile path and logging an initial message
        /// </summary>
        private void InitLogger()
        {
            string logFile = Path.Combine(outputPath, $"Log.CitySimulation.Worker{rank}.txt");
            logger.SetLogFilePath(logFile);
            logger.Info($"Worker {rank} on {workerName} is responsible for {scenarioPart.TargetReferences.Count} houses and {scenarioPart.PointsOfInterest.Count} POIs.");
        }

        /// <summary>
        /// Loads the city scenario from the specified path, splits it and distributes it across all workers.
        /// </summary>
        /// <param name="inputPath">scenario path to load</param>
        /// <returns>the part of the scenario this worker is responsible for</returns>
        /// <exception cref="LPGException">if there is not enough work for all workers</exception>
        private ScenarioPart LoadAndDistributeScenario(string inputPath)
        {
            // create scenario
            ScenarioPart[]? scenarioParts = null;
            if (rank == 0)
            {
                // determine simulation targets
                var scenario = CityScenarioImport.ReadScenarioFromConfigDirectory(inputPath, numWorkers);
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
            return comm.Scatter(scenarioParts, 0);
        }

        /// <summary>
        /// Creates the LPG simulator and initializes it, if necessary creating and preparing
        /// all houses for simulation.
        /// </summary>
        /// <returns>the LPGMassSimulator for this worker</returns>
        private LPGMassSimulator InitLPGSimulator()
        {
            LPGMassSimulator lpgSimulator = new(comm, rank, scenarioPart);
            int totalPersons = lpgSimulator.TotalNumberOfPersons();
            int totalHouseholds = lpgSimulator.TotalNumberOfHouseholds();
            logger.Info($"In total, this worker simulates {totalPersons} persons in {totalHouseholds} households.");

            lpgSimulator.Init();
            return lpgSimulator;
        }

        /// <summary>
        /// Creates all POI simulators for this worker
        /// </summary>
        /// <returns>a list of POI simulators</returns>
        private List<PointOfInterestSimulator> CreatePoiSimulators()
        {
            // initialize the point of interest simulators
            return [.. scenarioPart.PointsOfInterest.Select(CreatePoiSimulator)];
        }

        /// <summary>
        /// Creates a POI simulator for a single POI, depending on the POI type.
        /// </summary>
        /// <param name="poi">the POI config</param>
        /// <returns>the created POI simulator</returns>
        private PointOfInterestSimulator CreatePoiSimulator(PointOfInterestConfig poi)
        {
            return poi.LocationType.Name switch
            {
                //"Doctors Office" => new QueuePointOfInterestSimulator(rank, poi.Id, scenarioPart.CalcSpecification, 2),
                _ => new PointOfInterestSimulator(rank, poi.Id, outputPath),
            };
        }

        /// <summary>
        /// Main method that runs the simulation, postprocessing.
        /// </summary>
        public void Run()
        {
            var simulationStart = DateTime.Now;
            RunMainSimulation();
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

        /// <summary>
        /// Runs the main simulation part of the city simulation, where every timestep is simulated individually.
        /// </summary>
        private void RunMainSimulation()
        {
            // define iteration variables
            var calcParameters = scenarioPart.CalcParams;
            var simulationTime = calcParameters.InternalStartTime;
            var timestep = new TimeStep(0, calcParameters);
            // initialize the variable for storing exchanged messages across iterations, starting with no messages
            SortedMessageCollection activityMessages = new([], [], []);

            // this barrier is not required, but it makes all workers start the main loop at the same time
            MPIBarrierWithLog();

            // prepare data for performance measurements
            int totalHouseholds = lpgSimulator.TotalNumberOfHouseholds();
            int totalPersons = lpgSimulator.TotalNumberOfPersons();
            calculationProfiler?.StartPart("Main simulation loop", false);
            var startLoop = DateTime.UtcNow;
            TimeSpan totalTimeMPI = TimeSpan.Zero;
            var lastLog = startLoop;
            var timestepLastLog = timestep.InternalStep;
            var logInterval = new TimeSpan(0, 1, 0);
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
                totalTimeMPI += DateTime.UtcNow - startDistribution;
                calculationProfiler?.StopPart("MPI message distribution", false);

                // increment timestep
                simulationTime += calcParameters.InternalStepsize;
                timestep = timestep.AddSteps(1);

                // log the simulation performance
                var timeSinceLastLog = startDistribution - lastLog;
                if (timeSinceLastLog > logInterval)
                {
                    var elapsedTime = startDistribution - startLoop;
                    double currentSpeed = (timestep.InternalStep - timestepLastLog) / timeSinceLastLog.TotalSeconds;
                    var estimatedRemainingTime = TimeSpan.FromSeconds((calcParameters.InternalTimesteps - timestep.InternalStep) / currentSpeed);
                    logger.Info($"Simulating timestep {timestep.InternalStep}, datetime: {simulationTime}, elapsed time: {elapsedTime}, speed: {currentSpeed:f2} steps/second, " +
                        $"{currentSpeed * totalHouseholds:f2} household steps/second, {currentSpeed * totalPersons:f2} person steps/second, time left: {estimatedRemainingTime}");
                    lastLog = startDistribution;
                    timestepLastLog = timestep.InternalStep;
                }
            }
            // main simulation is over, log speed and share of MPI communication
            TimeSpan totalLoop = DateTime.UtcNow - startLoop;
            calculationProfiler?.StopPart("Main simulation loop", false);
            double stepsPerSecond = timestep.InternalStep / totalLoop.TotalSeconds;
            logger.Info($"Main loop: {totalLoop}, MPI communication: {totalTimeMPI} ({100 * totalTimeMPI / totalLoop:f2} %), speed: {stepsPerSecond:f2} steps/second, " +
                $"{stepsPerSecond * totalHouseholds:f2} household steps/second, {stepsPerSecond * totalPersons:f2} person steps/second");
        }

        /// <summary>
        /// Does an MPI barrier with additional logging before and after.
        /// </summary>
        private void MPIBarrierWithLog()
        {
            logger.Debug("Calling MPI Barrier");
            var startBarrier = DateTime.UtcNow;
            comm.Barrier();
            logger.Debug($"MPI Barrier is over after {DateTime.UtcNow - startBarrier}");
        }

        /// <summary>
        /// Executes a single simulation step.
        /// </summary>
        /// <param name="timestep">the current timestep to simulate</param>
        /// <param name="simulationTime">the simulated datetime</param>
        /// <param name="activityMessages">the received messages for this worker from the previous timestep</param>
        /// <returns>an MPIDistributor object with all messages from this worker to others</returns>
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

        /// <summary>
        /// Finishes the simulation and runs all postprocessing tasks.
        /// </summary>
        private void FinishSimulation()
        {
            lpgSimulator.FinishSimulation();
            transportSimulator.FinishSimulation();

            foreach (var simulator in poiSimulators)
            {
                simulator.FinishSimulation();
            }

            // create an additional flame chart for the calculation profiler of this MPI worker
            if (calculationProfiler is not null && scenarioPart.CalcParams.Options.Contains(Automation.CalcOption.CalculationFlameChart))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var profilerDirectory = Path.Combine(outputPath, "CalculationProfiler");
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
                SimulationEngineLib.HouseJobProcessor.JsonCalculator.CleanUpResultDirectory(scenarioPart.CalcSpecification);
            }
        }
    }
}

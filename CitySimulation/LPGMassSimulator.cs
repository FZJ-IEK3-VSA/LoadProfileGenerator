using Automation;
using Automation.ResultFiles;
using CalcPostProcessor;
using CalculationController.CalcFactories;
using CalculationEngine;
using CalculationEngine.CitySimulation;
using CalculationEngine.HouseElements;
using ChartCreator2;
using ChartCreator2.OxyCharts;
using Common;
using Common.JSON;
using Database;
using CitySimulation.Scenarios;
using CitySimulation.SimulationTargets;
using Newtonsoft.Json;
using PowerArgs;
using SimulationEngineLib.HouseJobProcessor;
using System.Runtime.InteropServices;

namespace CitySimulation
{
    /// <summary>
    /// A class that simulates multiple LPG households simultaneously.
    /// </summary>
    internal class LPGMassSimulator
    {
        private readonly int rank;
        private readonly Simulator sim;
        private readonly ScenarioPart scenarioPart;
        private readonly List<CitySimulationHouse> simulationTargets;

        public CalcParameters CalcParameters;

        public LPGMassSimulator(int rank, ScenarioPart scenarioPart)
        {
            this.rank = rank;
            this.scenarioPart = scenarioPart;

            string baseResultDir = scenarioPart.CalcSpecification.OutputDirectory ?? throw new LPGPBadParameterException("No OutputDirectory specified");

            if (scenarioPart.TargetReferences.Count == 0)
                throw new LPGPBadParameterException($"LPGMassSimulator on worker {rank} received no simulation targets.");

            // configure logger so that each worker logs to a different file
            Logger.Get().StartCollectingAllMessages();
            JsonCalculator.LogCalcSpec(scenarioPart.CalcSpecification);

            // create a DB copy or load an existing one and open the DB connection
            string dbFilename = $"profilegenerator.worker_{rank}.db3";
            var databaseDirectory = Path.Combine(baseResultDir, Constants.DataBaseDirectory);
            var dbFilepath = Path.Combine(databaseDirectory, dbFilename);
            // check if the database file already exists
            if (File.Exists(dbFilepath))
            {
                // load the existing database and reuse it
                Logger.Info($"Reusing existing database file {dbFilename}");
                sim = HouseGenerator.OpenDatabase(dbFilepath);
            }
            else
            {
                // create a database copy for this worker and open it
                Logger.Info($"No reusable database found. Creating a new one: {dbFilename}");
                sim = HouseGenerator.CopyAndOpenDatabase(scenarioPart.DatabasePath, databaseDirectory, out _, dbFilename);
                // generate all houses according to the config files
                GenerateHouses();

                // TODO: reopening the database is necessary to ensure same results as when cached DBs are reused
                sim = HouseGenerator.OpenDatabase(dbFilepath);
            }

            simulationTargets = PrepareHousesForSimulation(baseResultDir, rank);

            // make the common CalcParameters accessible
            CalcParameters = simulationTargets[0].CalcManager.CalcRepo.CalcParameters;
        }

        /// <summary>
        /// Generates all houses for this LPG simulator from the house config files.
        /// </summary>
        private void GenerateHouses()
        {
            HouseGenerator houseGenerator = new();
            foreach (var target in scenarioPart.TargetReferences)
            {
                ReadAndGenerateHouse(target, houseGenerator);
            }
        }

        /// <summary>
        /// Reads a house config file and creates the respective house with its households. Uses the seed of the house config
        /// for generating households from templates.
        /// </summary>
        /// <param name="target">the target config to process</param>
        /// <param name="houseGenerator">a house generator object to create the house</param>
        /// <returns>the JsonReference of the created house</returns>
        /// <exception cref="LPGException">if the house job file was invalid</exception>
        /// <exception cref="CitySimWrapperException">if there was an error during house generation</exception>
        private JsonReference ReadAndGenerateHouse(ResidentialBuildingConfig target, HouseGenerator houseGenerator)
        {
            // read house job file for this target
            string houseJobStr = File.ReadAllText(target.ConfigFilePath).Trim(HouseGenerator.charsToTrim);
            var hcj = JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(houseJobStr) ?? throw new LPGPBadParameterException("housejob was null");

            // set the global Calcspec
            hcj.CalcSpec = scenarioPart.CalcSpecification;

            // copy information from the global city data object
            if (hcj.City is null)
            {
                Logger.Info($"City object of house {target.Id} was null, using the global city data object with all POIs instead.");
                hcj.City = scenarioPart.CityData;
            }
            else
            {
                // POIs are not copied, as each house already contains all relevant POIs for efficiency reasons
                hcj.City.TravelDefinition = scenarioPart.CityData.TravelDefinition;
            }
            try
            {
                // create the target house/household if necessary and get its JsonReference
                var calcObjectReference = houseGenerator.GetHouseReference(hcj, sim, new Random(target.Seed));
                return calcObjectReference;
            }
            catch (Exception ex)
            {
                throw new CitySimWrapperException(ex, rank, target.Id, "initialization (template generation)");
            }
        }

        /// <summary>
        /// Prepares the already generated houses for simulation by setting up the required LPG calculation objects,
        /// and returns the house objects for simulation.
        /// </summary>
        /// <param name="baseResultDir">base result directory for the simulation</param>
        /// <param name="rank">rank of the MPI worker</param>
        /// <returns>list of house objects for simulation</returns>
        /// <exception cref="LPGPBadParameterException">if a required house was missing in the database</exception>
        /// <exception cref="CitySimWrapperException">if an error occurred during preparation of a house</exception>
        private List<CitySimulationHouse> PrepareHousesForSimulation(string baseResultDir, int rank)
        {
            var simulationTargets = new List<CitySimulationHouse>(scenarioPart.TargetReferences.Count);
            var cmf = new CalcManagerFactory();

            foreach (var target in scenarioPart.TargetReferences)
            {
                // create a separate subdirectory for each simulation target
                string subdir = target.Id;
                string houseResultDir = Path.Combine(baseResultDir, Constants.HousesDirectory, subdir);
                Directory.CreateDirectory(houseResultDir);

                // get the JsonReference for the generated house
                var house = sim.Houses.FindFirstByName(target.Id) ?? throw new LPGPBadParameterException($"House {target.Id} is missing. If reusing existing databases, please delete the database directory and start again.");
                var calcObjectReference = house.GetJsonReference();

                try
                {
                    // create the CalcStartParameterSet containing all parameters for the calculation
                    var calcStartParameterSet = JsonCalculator.CreateCalcParametersFromCalcSpec(sim, scenarioPart.CalcSpecification, calcObjectReference, citySimulationEnabled: true);
                    calcStartParameterSet.ResultPath = houseResultDir;

                    // create a unique random seed for this target
                    calcStartParameterSet.SelectedRandomSeed = target.Seed;

                    // create a calcManager for each household
                    var calcManager = cmf.GetCalcManager(sim, calcStartParameterSet, false);
                    simulationTargets.Add(new CitySimulationHouse(target.Id, calcManager, houseResultDir));
                }
                catch (Exception ex)
                {
                    throw new CitySimWrapperException(ex, rank, target.Id, "initialization");
                }
            }
            return simulationTargets;
        }

        public void Init()
        {
            CalcManager.StartRunning();
        }

        public IEnumerable<RemoteActivityInfo> SimulateOneStep(TimeStep timeStep, DateTime dateTime,
            Dictionary<string, Dictionary<HouseholdKey, Dictionary<string, RemoteActivityFinished>>> finishedActivities)
        {
            // simulate each target for one timestep and collect all new activities
            return simulationTargets.SelectMany(target => SimulateOneStepOneTarget(timeStep, dateTime, finishedActivities, target));
        }

        private ICollection<RemoteActivityInfo> SimulateOneStepOneTarget(TimeStep timeStep, DateTime dateTime, Dictionary<string, Dictionary<HouseholdKey, Dictionary<string, RemoteActivityFinished>>> finishedActivities, CitySimulationHouse target)
        {
            try
            {
                var newActivities = target.CalcManager.RunOneStep(timeStep, dateTime, finishedActivities.GetValueOrDefault(target.Id, []));
                // set the missing target ID and worker rank to make the person identifier simulation-wide unique
                newActivities.ForEach(activity => activity.Person.AddMissingInfo(target.Id, rank));
                return [.. newActivities];
            }
            catch (Exception e)
            {
                // wrap the exception in a CitySimWrapperException contining more relevant information
                throw new CitySimWrapperException(e, rank, target.Id, $"timestep {timeStep.InternalStep}");
            }
        }

        public void FinishSimulation()
        {
            foreach (var target in simulationTargets)
            {
                target.CalcManager.CalcObject.FinishCalculation();
                var calcRepo = target.CalcManager.CalcRepo;
                calcRepo.Flush();
                calcRepo.Dispose();

                // postprocessing
                var ppm = new PostProcessingManager(calcRepo.CalculationProfiler, calcRepo.FileFactoryAndTracker);
                ppm.Run(target.ResultDirectory);
                calcRepo.Flush();

                // chart creation
                FileFactoryAndTracker.CheckExistingFilesFromSql(target.ResultDirectory);
                var cpm = new ChartProcessorManager(calcRepo.CalculationProfiler, calcRepo.FileFactoryAndTracker);
                cpm.Run(target.ResultDirectory);
                calcRepo.Flush();

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // create additional charts
                    ChartMaker.MakeChartsAndPDF(calcRepo.CalculationProfiler, target.ResultDirectory);

                    // create the calculation profiler flame chart if requested
                    if (calcRepo.CalcParameters.IsSet(CalcOption.CalculationFlameChart))
                        ChartMaker.MakeFlameChart(new DirectoryInfo(target.ResultDirectory), calcRepo.CalculationProfiler);
                }

                if (calcRepo.CalcParameters.IsSet(CalcOption.LogAllMessages) || calcRepo.CalcParameters.IsSet(CalcOption.LogErrorMessages))
                {
                    target.CalcManager.InitializeFileLogging(calcRepo.Srls);
                }
            }
        }

        /// <summary>
        /// Returns the total number of households this worker is simulating.
        /// </summary>
        /// <returns>total number of households in all houses of this simulator</returns>
        public int TotalNumberOfHouseholds()
        {
            return simulationTargets.Select(t => t.CalcManager.CalcObject).As<CalcHouse>().Sum(house => house.Households.Count);
        }

        /// <summary>
        /// Returns the total number of persons this worker is simulating.
        /// </summary>
        /// <returns>total number of persons in all houses of this simulator</returns>
        public int TotalNumberOfPersons()
        {
            return simulationTargets.Select(t => t.CalcManager.CalcObject).Sum(house => house!.CollectPersons().Count);
        }
    }
}

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
            var baseResultDirInfo = new DirectoryInfo(baseResultDir);
            Logger.Get().StartCollectingAllMessages();
            JsonCalculator.LogCalcSpec(scenarioPart.CalcSpecification);

            HouseGenerator houseGenerator = new();

            // create a DB copy for this worker and open a connection to it
            var databaseDirectory = Path.Combine(baseResultDir, "Databases");
            sim = houseGenerator.CopyAndOpenDatabase(scenarioPart.DatabasePath, databaseDirectory, out _, $"profilegenerator.worker_{rank}.db3");

            simulationTargets = new List<CitySimulationHouse>(scenarioPart.TargetReferences.Count);
            var cmf = new CalcManagerFactory();

            foreach (var target in scenarioPart.TargetReferences)
            {
                // create a separate subdirectory for each simulation target
                string subdir = target.Id;
                string resultDirectory = Path.Combine(baseResultDir, "Houses", subdir);
                Directory.CreateDirectory(resultDirectory);

                // read house job file for this target
                string houseJobStr = File.ReadAllText(target.ConfigFilePath).Trim(HouseGenerator.charsToTrim);
                var hcj = JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(houseJobStr) ?? throw new LPGException("housejob was null");

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

                    // create the CalcStartParameterSet containing all parameters for the calculation
                    var calcStartParameterSet = JsonCalculator.CreateCalcParametersFromCalcSpec(sim, scenarioPart.CalcSpecification, calcObjectReference, citySimulationEnabled: true);
                    calcStartParameterSet.ResultPath = resultDirectory;

                    // create a unique random seed for this target
                    calcStartParameterSet.SelectedRandomSeed = target.Seed;

                    // create a calcManager for each household
                    var calcManager = cmf.GetCalcManager(sim, calcStartParameterSet, false);
                    simulationTargets.Add(new CitySimulationHouse(target.Id, calcManager, resultDirectory));
                }
                catch (Exception ex)
                {
                    throw new CitySimWrapperException(ex, rank, target.Id, "initialization");
                }
            }

            // make the common CalcParameters accessible
            CalcParameters = simulationTargets[0].CalcManager.CalcRepo.CalcParameters;
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
                target.CalcManager.CalcObject!.FinishCalculation();
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

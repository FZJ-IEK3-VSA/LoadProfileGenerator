using Automation;
using Automation.ResultFiles;
using CalcPostProcessor;
using CalculationController.CalcFactories;
using CalculationEngine;
using CalculationEngine.CitySimulation;
using ChartCreator2;
using ChartCreator2.OxyCharts;
using Common;
using Common.JSON;
using Database;
using Newtonsoft.Json;
using SimulationEngineLib.HouseJobProcessor;
using System.Runtime.InteropServices;

namespace MassSimulation
{
    /// <summary>
    /// A class that simulates multiple LPG households simultaneously.
    /// </summary>
    internal class LPGMassSimulator
    {
        private readonly int rank;
        private readonly Simulator sim;
        private readonly ScenarioPart scenarioPart;
        private readonly List<MassSimulationTarget> simulationTargets;
        private readonly Random random;

        public CalcParameters CalcParameters;

        public LPGMassSimulator(int rank, ScenarioPart scenarioPart)
        {
            this.rank = rank;
            this.scenarioPart = scenarioPart;

            string baseResultDir = scenarioPart.CalcSpecification.OutputDirectory ?? throw new LPGPBadParameterException("No OutputDirectory specified");

            random = new Random(scenarioPart.RandomSeed);

            // configure logger so that each worker logs to a different file
            var baseResultDirInfo = new DirectoryInfo(baseResultDir);
            Logger.Get().StartCollectingAllMessages();
            JsonCalculator.InitLoggerAndLogCalcSpec(baseResultDirInfo, scenarioPart.CalcSpecification, "Log.CommandlineCalculation.Worker" + rank + ".txt");

            HouseGenerator houseGenerator = new();

            // create a DB copy for this worker and open a connection to it
            var databaseDirectory = Path.Combine(baseResultDir, "Databases");
            sim = houseGenerator.CopyAndOpenDatabase(scenarioPart.DatabasePath, databaseDirectory, out _, $"profilegenerator.worker_{rank}.db3");

            simulationTargets = new List<MassSimulationTarget>(scenarioPart.TargetReferences.Count);
            var cmf = new CalcManagerFactory();

            foreach (var target in scenarioPart.TargetReferences)
            {
                // create a separate subdirectory for each simulation target
                string subdir = target.Id.ToString();
                string resultDirectory = Path.Combine(baseResultDir, "Houses", subdir);
                Directory.CreateDirectory(resultDirectory);

                // read house job file for this target
                string houseJobStr = File.ReadAllText(target.ConfigFilePath).Trim(HouseGenerator.charsToTrim);
                var hcj = JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(houseJobStr) ?? throw new LPGException("housejob was null");

                // set the global Calcspec
                hcj.CalcSpec = scenarioPart.CalcSpecification;

                try
                {
                    // create the target house/household if necessary and get its JsonReference
                    var calcObjectReference = houseGenerator.GetHouseReference(hcj, sim, random);

                    // create the CalcStartParameterSet containing all parameters for the calculation
                    var calcStartParameterSet = JsonCalculator.CreateCalcParametersFromCalcSpec(sim, scenarioPart.CalcSpecification, calcObjectReference, citySimulationEnabled: true);
                    calcStartParameterSet.ResultPath = resultDirectory;

                    // create a calcManager for each household
                    var calcManager = cmf.GetCalcManager(sim, calcStartParameterSet, false);
                    simulationTargets.Add(new MassSimulationTarget(target.Id, calcManager, resultDirectory));
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
            var newRemoteActivities = new List<RemoteActivityInfo>();
            // simulate each target for one timestep
            foreach (var target in simulationTargets)
            {
                try
                {
                    var newActivities = target.CalcManager.RunOneStep(timeStep, dateTime, finishedActivities.GetValueOrDefault(target.Id, []));
                    // collect all new activity messages
                    foreach (var newActivityContext in newActivities)
                    {
                        // set the missing target ID and worker rank to make the person identifier simulation-wide unique
                        newActivityContext.Person.AddMissingInfo(target.Id, rank);
                        newRemoteActivities.Add(newActivityContext);
                    }
                }
                catch (Exception e)
                {
                    // wrap the exception in a CitySimWrapperException contining more relevant information
                    throw new CitySimWrapperException(e, rank, target.Id, $"timestep {timeStep.InternalStep}");
                }
            }
            return newRemoteActivities;
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
                }

                if (calcRepo.CalcParameters.IsSet(CalcOption.LogAllMessages) || calcRepo.CalcParameters.IsSet(CalcOption.LogErrorMessages))
                {
                    target.CalcManager.InitializeFileLogging(calcRepo.Srls);
                }
            }
        }
    }
}

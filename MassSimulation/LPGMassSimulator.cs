using Automation;
using Automation.ResultFiles;
using CalcPostProcessor;
using CalculationController.CalcFactories;
using CalculationController.Queue;
using CalculationEngine;
using CalculationEngine.CitySimulation;
using ChartCreator2;
using ChartCreator2.OxyCharts;
using Common;
using Common.JSON;
using Database;
using SimulationEngineLib.HouseJobProcessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    /// <summary>
    /// A class that simulates multiple LPG households simultaneously.
    /// </summary>
    internal class LPGMassSimulator
    {
        private int rank;
        private Simulator sim;
        private ScenarioPart scenarioPart;
        private List<MassSimulationTarget> simulationTargets;

        public CalcParameters CalcParameters;

        public LPGMassSimulator(int rank, ScenarioPart scenarioPart)
        {
            this.rank = rank;
            this.scenarioPart = scenarioPart;

            string baseResultDir = scenarioPart.CalcSpecification.OutputDirectory ?? throw new LPGPBadParameterException("No OutputDirectory specified");

            // configure logger so that each worker logs to a different file
            var baseResultDirInfo = new DirectoryInfo(baseResultDir);
            Logger.Get().StartCollectingAllMessages();
            JsonCalculator.InitLoggerAndLogCalcSpec(baseResultDirInfo, scenarioPart.CalcSpecification, "Log.CommandlineCalculation.Worker" + rank + ".txt");

            // TODO: copy DB for each worker
            //var sim = HouseGenerator.CopyAndOpenDatabase(PathToDatabase, resultDir);
            sim = new Simulator("Data Source=" + scenarioPart.DatabasePath);

            simulationTargets = new List<MassSimulationTarget>(scenarioPart.TargetReferences.Count);
            var cmf = new CalcManagerFactory();

            foreach (var calcObjectRef in scenarioPart.TargetReferences)
            {
                // create a separate subdirectory for each simulation target
                string subdir = calcObjectRef.Id.ToString();
                string resultDirectory = Path.Combine(baseResultDir, subdir);
                Directory.CreateDirectory(resultDirectory);

                // create the CalcStartParameterSet containing all parameters for the calculation
                var calcStartParameterSet = JsonCalculator.CreateCalcParametersFromCalcSpec(sim, scenarioPart.CalcSpecification, calcObjectRef.Reference);
                calcStartParameterSet.ResultPath = resultDirectory;

                // create a calcManager for each household
                var calcManager = cmf.GetCalcManager(sim, calcStartParameterSet, false);
                simulationTargets.Add(new MassSimulationTarget(calcObjectRef.Id, calcManager, resultDirectory));
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
            try
            {
                // simulate each target for one timestep
                foreach (var target in simulationTargets)
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
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occurred in timestep " + timeStep.ToString() + ":\n" + ex.ToString());
                throw;
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

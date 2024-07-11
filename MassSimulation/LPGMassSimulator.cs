using Automation;
using Automation.ResultFiles;
using CalcPostProcessor;
using CalculationController.CalcFactories;
using CalculationController.Queue;
using CalculationEngine;
using ChartCreator2;
using Common;
using Common.JSON;
using Database;
using SimulationEngineLib.HouseJobProcessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    /// <summary>
    /// A class that simulates multiple LPG households simultaneously.
    /// </summary>
    internal class LPGMassSimulator : ISimulator
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

            // TODO: copy DB for each worker
            //var sim = HouseGenerator.CopyAndOpenDatabase(PathToDatabase, resultDir);
            sim = new Simulator("Data Source=" + scenarioPart.DatabasePath);

            string baseResultDir = scenarioPart.CalcSpecification.OutputDirectory ?? throw new LPGPBadParameterException("No OutputDirectory specified");

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
                simulationTargets.Add(new MassSimulationTarget("TODO: id", calcManager, resultDirectory));
            }

            // make the common CalcParameters accessible
            CalcParameters = simulationTargets[0].CalcManager.CalcRepo.CalcParameters;

            // configure logger so that each worker logs to a different file
            var baseResultDirInfo = new DirectoryInfo(baseResultDir);
            Logger.Get().StartCollectingAllMessages();
            JsonCalculator.InitLoggerAndLogCalcSpec(baseResultDirInfo, scenarioPart.CalcSpecification, "Log.CommandlineCalculation.Worker" + rank + ".txt");
        }

        public void Init()
        {
            CalcManager.StartRunning();
        }

        public void SimulateOneStep(TimeStep timeStep, DateTime dateTime)
        {
            try
            {
                foreach (var target in simulationTargets)
                {
                    target.CalcManager.RunOneStep(timeStep, dateTime);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occurred in timestep " + timeStep.ToString() + ":\n" + ex.ToString());
                throw;
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
                var cpm = new ChartProcessorManager(calcRepo.CalculationProfiler, calcRepo.FileFactoryAndTracker);
                cpm.Run(target.ResultDirectory);
                calcRepo.Flush();


                if (calcRepo.CalcParameters.IsSet(CalcOption.LogAllMessages) || calcRepo.CalcParameters.IsSet(CalcOption.LogErrorMessages))
                {
                    target.CalcManager.InitializeFileLogging(calcRepo.Srls);
                }
            }
        }
    }
}

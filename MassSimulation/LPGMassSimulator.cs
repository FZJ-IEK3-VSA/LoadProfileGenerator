using Automation;
using CalculationController.CalcFactories;
using CalculationController.Queue;
using CalculationEngine;
using Common;
using Common.JSON;
using Database;
using SimulationEngineLib.HouseJobProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    /// <summary>
    /// A class that simulates multiple LPG households simultaneously.
    /// </summary>
    internal class LPGMassSimulator: ISimulator
    {
        private int rank;
        private Simulator sim;
        private ScenarioPart scenarioPart;
        private List<CalcManager> calcManagers;

        public CalcParameters CalcParameters;

        public LPGMassSimulator(int rank, ScenarioPart scenarioPart)
        {
            this.rank = rank;
            this.scenarioPart = scenarioPart;

            // TODO: copy DB for each worker
            //var sim = HouseGenerator.CopyAndOpenDatabase(PathToDatabase, resultDir);
            sim = new Simulator("Data Source=" + scenarioPart.DatabasePath);

            calcManagers = new List<CalcManager>(scenarioPart.CalcObjectReferences.Count);
            var cmf = new CalcManagerFactory();

            foreach (var calcObjectRef in scenarioPart.CalcObjectReferences)
            {
                // create the CalcStartParameterSet containing all parameters for the calculation
                var calcStartParameterSet = JsonCalculator.CreateCalcParametersFromCalcSpec(sim, scenarioPart.CalcSpecification, calcObjectRef);

                // create a calcManager for each household
                calcManagers.Add(cmf.GetCalcManager(sim, calcStartParameterSet, false));
            }

            // make the common CalcParameters accessible
            CalcParameters = calcManagers[0].CalcRepo.CalcParameters;

            foreach (var calcManager in calcManagers)
            {
                if (calcManager.CalcRepo.CalcParameters != CalcParameters)
                {
                    throw new Exception("Found different CalcParameters");
                }
            }

            // configure logger so that each worker logs to a different file
            var resultDir = new DirectoryInfo(scenarioPart.CalcSpecification.OutputDirectory);
            Logger.Get().StartCollectingAllMessages();
            JsonCalculator.InitLogger(resultDir, scenarioPart.CalcSpecification, "Log.CommandlineCalculation.Worker" + rank + ".txt");
        }

        public void Init()
        {
            CalcManager.StartRunning();
        }

        public void SimulateOneStep(TimeStep timeStep, DateTime dateTime)
        {
            foreach (var calcManager in calcManagers)
            {
                calcManager.RunOneStep(timeStep, dateTime);
            }
        }
    }
}

using Automation;
using Automation.ResultFiles;
using CalculationController.Queue;
using CalculationEngine;
using Common;
using Newtonsoft.Json;
using SimulationEngineLib.HouseJobProcessor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    /// <summary>
    /// Represents a simulation scenario with all objects that belong to that, including
    /// fully defined houses with households etc.
    /// </summary>
    public class Scenario
    {
        public string DatabasePath { get; private set; }
        public JsonCalcSpecification CalcSpecification { get; private set; }
        public MassSimTargetReference[] TargetReferences { get; private set; }

        // TODO: also store SMEs here

        public Scenario(string databasePath, JsonCalcSpecification calcSpec, MassSimTargetReference[] targetReferences)
        {
            DatabasePath = databasePath;
            CalcSpecification = calcSpec;
            TargetReferences = targetReferences;
        }

        /// <summary>
        /// Divide the scenario into scenario parts, one for each worker.
        /// </summary>
        /// <param name="numberOfParts"></param>
        /// <returns></returns>
        public ScenarioPart[] GetScenarioParts(int numberOfParts)
        {
            // divide the JsonReferences evenly
            var referencesSublists = TargetReferences.Split(numberOfParts);
            var parts = referencesSublists.Select(sublist => new ScenarioPart(DatabasePath, sublist.ToList(), CalcSpecification));
            return parts.ToArray();
        }
    }
}

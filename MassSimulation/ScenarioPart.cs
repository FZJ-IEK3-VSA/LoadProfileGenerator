using Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    /// <summary>
    /// Represents the part of a scenarion that a single worker simulates.
    /// Includes all simulation targets this worker is responsible for.
    /// </summary>
    internal class ScenarioPart
    {
        public string DatabasePath;
        public List<MassSimTargetReference> TargetReferences;
        public JsonCalcSpecification CalcSpecification;

        public ScenarioPart(string databasePath, List<MassSimTargetReference> targetReferences, JsonCalcSpecification calcSpecification)
        {
            DatabasePath = databasePath;
            TargetReferences = targetReferences;
            CalcSpecification = calcSpecification;
        }
    }
}

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
        public List<JsonReference> CalcObjectReferences;
        public JsonCalcSpecification CalcSpecification;

        public ScenarioPart(string databasePath, List<JsonReference> calcObjectReferences, JsonCalcSpecification calcSpecification)
        {
            DatabasePath = databasePath;
            CalcObjectReferences = calcObjectReferences;
            CalcSpecification = calcSpecification;
        }
    }
}

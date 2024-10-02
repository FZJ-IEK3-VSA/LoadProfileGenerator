using Automation;

namespace MassSimulation
{
    /// <summary>
    /// Represents the part of a scenarion that a single worker simulates.
    /// Includes all simulation targets this worker is responsible for.
    /// </summary>
    public class ScenarioPart
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

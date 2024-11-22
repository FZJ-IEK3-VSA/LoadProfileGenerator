using Automation;

namespace MassSimulation
{
    /// <summary>
    /// Represents the part of a scenarion that a single worker simulates.
    /// Includes all simulation targets this worker is responsible for.
    /// </summary>
    public class ScenarioPart(List<MassSimTargetReference> targetReferences, List<PointOfInterestConfig> pointsOfInterest, string databasePath,
        JsonCalcSpecification calcSpecification, PointOfInterestRegister poiRegister)
    {
        public string DatabasePath = databasePath;
        public List<MassSimTargetReference> TargetReferences = targetReferences;
        public List<PointOfInterestConfig> PointsOfInterest = pointsOfInterest;
        public JsonCalcSpecification CalcSpecification = calcSpecification;
        public PointOfInterestRegister PoiRegister = poiRegister;
    }
}

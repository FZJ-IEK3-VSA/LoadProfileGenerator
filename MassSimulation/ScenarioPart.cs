using Automation;

namespace MassSimulation
{
    /// <summary>
    /// Represents the part of a scenarion that a single worker simulates.
    /// Includes all simulation targets this worker is responsible for.
    /// </summary>
    public record ScenarioPart(List<MassSimTargetReference> TargetReferences, List<PointOfInterestConfig> PointsOfInterest, string DatabasePath,
        JsonCalcSpecification CalcSpecification, PointOfInterestRegister PoiRegister, int RandomSeed);
}

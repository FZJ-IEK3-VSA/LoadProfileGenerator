using Automation;
using CitySimulation.SimulationTargets;
using Common.JSON;

namespace CitySimulation.Scenarios
{
    /// <summary>
    /// Represents the part of a scenario that a single worker simulates.
    /// Includes all simulation targets this worker is responsible for.
    /// </summary>
    public record ScenarioPart(List<ResidentialBuildingConfig> TargetReferences, List<PointOfInterestConfig> PointsOfInterest, string DatabasePath,
        JsonCalcSpecification CalcSpecification, CalcParameters CalcParams, PointOfInterestRegister PoiRegister, CityData CityData);
}

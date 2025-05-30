using Automation;
using CalculationEngine.CitySimulation;

namespace CitySimulation.SimulationTargets
{
    public record PointOfInterestConfig(PointOfInterestId Id, JsonReference LocationType);
}
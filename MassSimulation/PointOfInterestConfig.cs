using CalculationEngine.CitySimulation;

namespace MassSimulation
{
    public class PointOfInterestConfig(PointOfInterestId id)
    {
        public PointOfInterestId Id { get; } = id;
    }
}
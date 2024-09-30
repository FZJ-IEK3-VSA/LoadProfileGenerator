using CalculationEngine.CitySimulation;

namespace MassSimulation
{
    internal class AgentTravelState(RemoteActivityStart travelActivity, double travelDistance)
    {
        public readonly RemoteActivityStart ActivityInfo = travelActivity;
        public double RemainingTravelDistance { get; set; } = travelDistance;
    }
}
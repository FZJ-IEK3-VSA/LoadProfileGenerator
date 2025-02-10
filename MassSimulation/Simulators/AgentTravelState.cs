using CalculationEngine.CitySimulation;

namespace MassSimulation.Simulators
{
    internal class AgentTravelState(RemoteActivityStart travelActivity, double travelDistance)
    {
        public readonly RemoteActivityStart ActivityInfo = travelActivity;
        public double RemainingTravelDistance { get; set; } = travelDistance;
    }
}
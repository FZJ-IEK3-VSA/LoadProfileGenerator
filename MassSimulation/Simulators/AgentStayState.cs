using CalculationEngine.CitySimulation;

namespace CitySimulation.Simulators
{
    internal class AgentStayState(RemoteActivityStart activity, double stayDuration)
    {
        public readonly RemoteActivityStart Activity = activity;
        public readonly double StayDuration = stayDuration;
        public double RemainingDuration { get; set; } = stayDuration;
    }
}
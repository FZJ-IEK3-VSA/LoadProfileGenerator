using CalculationEngine.CitySimulation;

namespace MassSimulation
{
    internal class AgentTravelState
    {
        public readonly PersonIdentifier Agent;
        public double remainingTravelDistance { get; set; }

        public AgentTravelState(PersonIdentifier agent, double travelDistance)
        {
            this.Agent = agent;
            this.remainingTravelDistance = travelDistance;
        }
    }
}
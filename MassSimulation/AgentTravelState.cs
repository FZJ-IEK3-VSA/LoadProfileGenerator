namespace MassSimulation
{
    internal class AgentTravelState
    {
        public readonly AgentInfo Agent;
        public double remainingTravelDistance { get; set; }

        public AgentTravelState(AgentInfo agent, double travelDistance)
        {
            this.Agent = agent;
            this.remainingTravelDistance = travelDistance;
        }
    }
}
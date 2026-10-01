using CalculationEngine.CitySimulation;
using Common;

namespace CitySimulation.Simulators
{
    /// <summary>
    /// Stores the state of a single person's visit to a POI.
    /// </summary>
    /// <param name="arrival">time step of arrival</param>
    /// <param name="activity">the start activity message</param>
    /// <param name="stayDuration">the stay duration in time steps</param>
    internal class AgentStayState(TimeStep arrival, RemoteActivityStart activity, int stayDuration)
    {
        public readonly TimeStep Arrival = arrival;
        public readonly RemoteActivityStart Activity = activity;
        public readonly int StayDuration = stayDuration;

        /// <summary>
        /// The remaining stay duration. Is updated continuously during the stay.
        /// </summary>
        public int RemainingDuration { get; set; } = stayDuration;
    }
}
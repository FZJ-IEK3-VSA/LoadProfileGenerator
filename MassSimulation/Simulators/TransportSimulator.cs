using CalculationEngine.CitySimulation;
using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation.Simulators
{
    /// <summary>
    /// Stores all currently traveling agents and simulates their travel times.
    /// </summary>
    internal class TransportSimulator : ISimulator
    {
        private List<AgentTravelState> travelStates = [];

        public IEnumerable<RemoteActivityFinished> SimulateOneStep(TimeStep timeStep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities)
        {
            // TODO: dummy implementation
            foreach (var state in travelStates)
            {
                // update travel progress
                UpdateRemainingTravelDistance(state);
            }
            return [];
        }

        private void UpdateRemainingTravelDistance(AgentTravelState state)
        {
            // TODO: update depending on the number of traveling agents
            state.remainingTravelDistance--;
        }

        public void AddAgents(IEnumerable<PersonIdentifier> newAgents)
        {
            foreach (var agent in newAgents)
            {
                double distance = DetermineTravelDistance(agent);
                travelStates.Add(new AgentTravelState(agent, distance));
            }
        }

        public IEnumerable<PersonIdentifier> GetArrivedAgents()
        {
            var arrived = travelStates.Where(t => t.remainingTravelDistance <= 0);
            foreach (var agentState in arrived)
            {
                travelStates.Remove(agentState);
            }
            return arrived.Select(t => t.Agent);
        }

        private double DetermineTravelDistance(PersonIdentifier agent)
        {
            return 100;
        }

        public void FinishSimulation()
        {
        }
    }
}

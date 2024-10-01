using CalculationEngine.CitySimulation;
using CalculationEngine.HouseholdElements;
using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation.Simulators
{
    /// <summary>
    /// Simulates agent stays in any point of interest, for example a small enterprise.
    /// </summary>
    internal class PointOfInterestSimulator(int rank, int id) : ISimulator
    {
        private Collection<AgentStayState> activityStates = [];
        public PointOfInterestId PoiId { get; } = new PointOfInterestId(id, rank);

        public IEnumerable<RemoteActivityFinished> SimulateOneStep(TimeStep timeStep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities)
        {
            AddNewPersons(newActivities);

            // TODO: dummy implementation
            foreach (var state in activityStates)
            {
                // update travel progress
                UpdateRemainingStayTime(state);
            }

            return GetFinishedAgents();
        }

        private void AddNewPersons(IEnumerable<RemoteActivityStart> newActivities)
        {
            foreach (var newActivity in newActivities)
            {
                Debug.Assert(!newActivity.IsTravel, "TransportSimulator received a non-travel activity.");

                // TODO: simple test implementation: take longer the more people are present
                double duration = 5 + Math.Max(30, activityStates.Count);

                activityStates.Add(new AgentStayState(newActivity, duration));
            }
        }

        private void UpdateRemainingStayTime(AgentStayState state)
        {
            state.RemainingDuration--;
        }

        private IEnumerable<RemoteActivityFinished> GetFinishedAgents()
        {
            var arrived = activityStates.Where(t => t.RemainingDuration <= 0);
            foreach (var travelState in arrived)
            {
                activityStates.Remove(travelState);
            }
            return arrived.Select(t => new RemoteActivityFinished(t.Activity.Person, PoiId));
        }

        public void FinishSimulation()
        {
        }
    }
}

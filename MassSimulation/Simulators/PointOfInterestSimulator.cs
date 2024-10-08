using CalculationEngine.CitySimulation;
using CalculationEngine.HouseholdElements;
using Common;
using System;
using System.Collections;
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
        private List<AgentStayState> activityStates = [];
        public PointOfInterestId PoiId { get; } = new PointOfInterestId(id, rank);

        private TestLogger logger = new();

        public IEnumerable<RemoteActivityFinished> SimulateOneStep(TimeStep timeStep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities)
        {
            AddNewPersons(newActivities);

            // TODO: dummy implementation
            foreach (var state in activityStates)
            {
                // update travel progress
                UpdateRemainingStayTime(state);
                LogArrivals(timeStep, dateTime, newActivities);
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

        private void LogArrivals(TimeStep timestep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities)
        {
            var newPersons = string.Join(", ", newActivities.Select(a => a.Person.PersonName));
            var message = $"Total persons: {activityStates.Count} - new arrivals: {newPersons}";
            logger.Log(timestep, dateTime, message);
        }

        private void UpdateRemainingStayTime(AgentStayState state)
        {
            state.RemainingDuration--;
        }

        private IEnumerable<RemoteActivityFinished> GetFinishedAgents()
        {
            // collect all persons that finished their activity in the current Timestep
            Predicate<AgentStayState> isFinished = t => t.RemainingDuration <= 0;
            var arrived = activityStates.FindAll(isFinished);
            // remove the finished persons from the list of present persons
            activityStates.RemoveAll(isFinished);
            // create the corresponding finished activity messages
            return arrived.Select(t => new RemoteActivityFinished(t.Activity.Person, PoiId));
        }

        public void FinishSimulation()
        {
            var filename = $"POI-{PoiId.WorkerId}-{PoiId.Id}.txt";
            logger.WriteToFile(filename);
        }
    }
}

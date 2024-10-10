using CalculationEngine.CitySimulation;
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
    /// Stores all currently traveling agents and simulates their travel times.
    /// </summary>
    internal class TransportSimulator(int rank) : ISimulator
    {
        public readonly int WorkerId = rank;
        private List<AgentTravelState> travelStates = [];

        private TestLogger logger = new();

        public IEnumerable<RemoteActivityFinished> SimulateOneStep(TimeStep timeStep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities)
        {
            AddNewTravelers(newActivities);

            // TODO: dummy implementation
            foreach (var state in travelStates)
            {
                // update travel progress
                UpdateRemainingTravelDistance(state);
            }

            var finishedTravels = GetArrivingAgents();
            LogState(timeStep, dateTime, newActivities, finishedTravels);
            return finishedTravels;
        }

        private void UpdateRemainingTravelDistance(AgentTravelState state)
        {
            // TODO: update depending on the number of traveling agents
            state.RemainingTravelDistance--;
        }

        public void AddNewTravelers(IEnumerable<RemoteActivityStart> newTravelActivities)
        {
            foreach (var travelActivity in newTravelActivities)
            {
                Debug.Assert(travelActivity.IsTravel, "TransportSimulator received a non-travel activity.");

                // TODO: dummy value; here, all travel route steps should be started succesively
                double distance = 10;

                travelStates.Add(new AgentTravelState(travelActivity, distance));
            }
        }

        private void LogState(TimeStep timestep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities, IEnumerable<RemoteActivityFinished> finishedActivities)
        {
            if (newActivities.Any() || finishedActivities.Any())
            {
                var newPersons = string.Join(", ", newActivities.Select(a => a.Person.PersonName));
                var finishedPersons = string.Join(", ", finishedActivities.Select(a => a.Person.PersonName));
                var message = $"Total persons: {travelStates.Count} - started: {newPersons}; arrived: {finishedPersons}";
                logger.Log(timestep, dateTime, message);
            }
        }

        public IEnumerable<RemoteActivityFinished> GetArrivingAgents()
        {
            // collect all persons that arrived in the current timestep
            Predicate<AgentTravelState> hasArrived = t => t.RemainingTravelDistance <= 0;
            var arrived = travelStates.FindAll(hasArrived);
            // remove the arrived persons from the collection of currently traveling persons
            travelStates.RemoveAll(hasArrived);
            // create the corresponding finished activity messages
            return arrived.Select(t => new RemoteActivityFinished(t.ActivityInfo.Person, t.ActivityInfo.Poi));
        }

        public void FinishSimulation()
        {
            var filename = $"Transport-{WorkerId}.txt";
            logger.WriteToFile(filename);
        }
    }
}

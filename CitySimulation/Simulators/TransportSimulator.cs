using CalculationEngine.CitySimulation;
using Common;
using System.Diagnostics;

namespace CitySimulation.Simulators
{
    /// <summary>
    /// Stores all currently traveling agents and simulates their travel times.
    /// Only keeps track of all travelers and does not change the travel
    /// times determined by the affordance transport decorator.
    /// </summary>
    internal class TransportSimulator : ISimulator
    {
        public readonly int WorkerId;
        private List<AgentTravelState> travelStates = [];

        private readonly TextLogger logger;
        private readonly TextLogger presenceLogger;

        public TransportSimulator(int rank, string outputDir)
        {
            WorkerId = rank;
            var filename = $"Worker{WorkerId}.txt";
            logger = new FreeTextLogger(filename, outputDir, "travel_events");
            presenceLogger = new CsvLogger(filename, outputDir, ["People traveling"], "traveling_persons");
        }

        public IEnumerable<RemoteActivityFinished> SimulateOneStep(TimeStep timeStep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities)
        {
            AddNewTravelers(newActivities);

            foreach (var state in travelStates)
            {
                // update travel progress
                UpdateRemainingTravelDistance(state);
            }

            var finishedTravels = GetArrivedAgents();
            LogState(timeStep, dateTime, newActivities, finishedTravels);
            return finishedTravels;
        }

        private void UpdateRemainingTravelDistance(AgentTravelState state)
        {
            state.RemainingTravelDistance--;
        }

        public void AddNewTravelers(IEnumerable<RemoteActivityStart> newTravelActivities)
        {
            foreach (var travelActivity in newTravelActivities)
            {
                Debug.Assert(travelActivity.IsTravel, "TransportSimulator received a non-travel activity.");

                double distance = DetermineDuration(travelActivity);
                travelStates.Add(new AgentTravelState(travelActivity, distance));
            }
        }

        private int DetermineDuration(RemoteActivityStart activity)
        {
            // -2 to account for the timesteps lost due to messaging until the CalcPerson receives the ActivityFinished message
            return activity.ExpectedDuration - 2 ?? throw new NotImplementedException("No default duration for traveling implemented");
        }

        private void LogState(TimeStep timestep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities, IEnumerable<RemoteActivityFinished> finishedActivities)
        {
            if (newActivities.Any() || finishedActivities.Any())
            {
                var message = $"Persons traveling: {travelStates.Count}";
                if (newActivities.Any())
                    message += "; started: " + string.Join(", ", newActivities.Select(a => a.Person.PersonName));
                if (finishedActivities.Any())
                    message += "; arrived: " + string.Join(", ", finishedActivities.Select(a => a.Person.PersonName));
                logger.Log(timestep, dateTime, message);
                presenceLogger.Log(timestep, dateTime, $"{travelStates.Count}");
            }
        }

        public IEnumerable<RemoteActivityFinished> GetArrivedAgents()
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
            logger.WriteToFile();
        }
    }
}

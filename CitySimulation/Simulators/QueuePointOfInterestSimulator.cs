using Automation;
using CalculationEngine.CitySimulation;
using Common;
using System.Diagnostics;

namespace CitySimulation.Simulators
{
    class QueuePointOfInterestSimulator(int rank, PointOfInterestId id, string outputDir, int concurrentActivities) : PointOfInterestSimulator(rank, id, outputDir)
    {
        readonly Queue<AgentStayState> waitingVisitors = [];

        private void AddNewPersons(IEnumerable<RemoteActivityStart> newActivities)
        {
            foreach (var newActivity in newActivities)
            {
                Debug.Assert(!newActivity.IsTravel, "TransportSimulator received a non-travel activity.");

                double duration = DetermineDuration(newActivity);
                waitingVisitors.Enqueue(new AgentStayState(newActivity, duration));
            }
        }

        public override IEnumerable<RemoteActivityFinished> SimulateOneStep(TimeStep timeStep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities)
        {
            AddNewPersons(newActivities);

            while (activeVisitors.Count < concurrentActivities && waitingVisitors.Any())
            {
                // the next agents can start their activity
                activeVisitors.Add(waitingVisitors.Dequeue());
            }

            foreach (var state in activeVisitors)
            {
                // update travel progress
                UpdateRemainingStayTime(state);
            }

            var finishedActivitites = GetFinishedAgents();
            LogState(timeStep, dateTime, newActivities, finishedActivitites);
            return finishedActivitites;
        }

        protected override void LogState(TimeStep timestep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities, IEnumerable<RemoteActivityFinished> finishedActivities)
        {
            if (newActivities.Any() || finishedActivities.Any())
            {
                int totalVisitors = activeVisitors.Count + waitingVisitors.Count;
                logger.Log(timestep, dateTime, $"Persons present: {totalVisitors}, active: {activeVisitors.Count}");
                // log each newly started activity
                foreach (var newActivity in newActivities)
                {
                    logger.Log(timestep, dateTime, $"{newActivity.Person.PersonName} arrived for {newActivity.Affordance}");
                }
                if (finishedActivities.Any())
                {
                    var finishedPersons = string.Join(", ", finishedActivities.Select(a => a.Person.PersonName));
                    logger.Log(timestep, dateTime, $"Finished activitites: {finishedPersons}");
                }
                presenceLogger.Log(timestep, dateTime, $"{totalVisitors}");
            }
        }
    }
}

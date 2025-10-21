using CalculationEngine.CitySimulation;
using Common;
using System.Diagnostics;

namespace CitySimulation.Simulators
{
    class QueuePointOfInterestSimulator : PointOfInterestSimulator
    {
        readonly Queue<AgentStayState> waitingVisitors = [];
        private readonly int concurrentActivities;

        public QueuePointOfInterestSimulator(int rank, PointOfInterestId id, string outputDir, int concurrentActivities) : base(rank, id, outputDir)
        {
            this.concurrentActivities = concurrentActivities;
            csvLogger.AddColumns([""]);
        }

        private void AddNewPersons(TimeStep timeStep, IEnumerable<RemoteActivityStart> newActivities)
        {
            foreach (var newActivity in newActivities)
            {
                Debug.Assert(!newActivity.IsTravel, "TransportSimulator received a non-travel activity.");

                int duration = DetermineDuration(newActivity);
                waitingVisitors.Enqueue(new AgentStayState(timeStep, newActivity, duration));
            }
        }

        public override IEnumerable<RemoteActivityFinished> SimulateOneStep(TimeStep timeStep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities)
        {
            AddNewPersons(timeStep, newActivities);

            while (activeVisitors.Count < concurrentActivities && waitingVisitors.Count != 0)
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
                csvLogger.Log(timestep, dateTime, [totalVisitors, newActivities.Count(), finishedActivities.Count()]);
            }
        }
    }
}

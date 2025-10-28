using CalculationEngine.CitySimulation;
using Common;
using System.Diagnostics;

namespace CitySimulation.Simulators
{
    class QueuePointOfInterestSimulator : PointOfInterestSimulator
    {
        /// <summary>
        /// Additional logger for waiting times
        /// </summary>
        CsvIndexDateLogger waitingTimeLogger;

        /// <summary>
        /// Stores visitors that are still waiting for their turn.
        /// </summary>
        readonly Queue<AgentStayState> waitingVisitors = [];

        /// <summary>
        /// Defines how many visitors can be served at once, i.e. how many 
        /// can carry out their activity in parallel.
        /// </summary>
        private readonly int concurrentActivities;

        /// <summary>
        /// Indicates how long a visitor is willing to wait, relative to
        /// their own activity duration. 2 means the person will wait up
        /// to twice as long as their own activity will last.
        /// </summary>
        private readonly int maxWaitingFactor;

        /// <summary>
        /// Indicates how long in timesteps any visitor is willing to wait,
        /// independent of their own activity duration.
        /// </summary>
        private readonly int minWaitingTime = 10;

        public QueuePointOfInterestSimulator(int rank, PointOfInterestId id, string outputDir, int concurrentActivities, int maxWaitingFactor = 4) : base(rank, id, outputDir)
        {
            this.concurrentActivities = concurrentActivities;
            this.maxWaitingFactor = maxWaitingFactor;
            csvLogger.AddColumns(["People cancelling"]);

            var filename = $"{PoiId.Id}.csv";
            waitingTimeLogger = new(filename, outputDir, ["Person ID", "Waiting time"], "poi_queue");
        }

        /// <summary>
        /// Estimates the current waiting time for newly arriving visitors.
        /// </summary>
        /// <returns>estimated waiting time</returns>
        private int CalcExpectedWaitingTime()
        {
            if (activeVisitors.Count < concurrentActivities && waitingVisitors.Count == 0)
                return 0; // no waiting time, visitor can immediately start

            int queueDuration = waitingVisitors.Sum(v => v.StayDuration);
            int activeDuration = activeVisitors.Sum(v => v.RemainingDuration);
            int expectedWait = (queueDuration + activeDuration) / concurrentActivities;
            return expectedWait;
        }

        /// <summary>
        /// Determines if the visitor will cancel the visit due to the long waiting time.
        /// </summary>
        /// <param name="expectedWaitingTime">the estimated waiting time</param>
        /// <param name="activityDuration">the activity duration of the visitor</param>
        /// <returns>true if the visitor cancels their visit; otherwise, false</returns>
        private bool DoesVisitorCancel(int expectedWaitingTime, int activityDuration)
        {
            return expectedWaitingTime > minWaitingTime && expectedWaitingTime > maxWaitingFactor * activityDuration;
        }

        private List<RemoteActivityFinished> AddNewPersons(TimeStep timeStep, IEnumerable<RemoteActivityStart> newActivities)
        {
            List<RemoteActivityFinished> cancelling = [];
            foreach (var newActivity in newActivities)
            {
                Debug.Assert(!newActivity.IsTravel, "TransportSimulator received a non-travel activity.");

                int expectedWaitingTime = CalcExpectedWaitingTime();
                int duration = DetermineDuration(newActivity);
                if (DoesVisitorCancel(expectedWaitingTime, duration))
                {
                    // waiting time is too long, the visitor leaves again
                    cancelling.Add(new RemoteActivityFinished(newActivity.Person, PoiId, false));
                    continue;
                }

                waitingVisitors.Enqueue(new AgentStayState(timeStep, newActivity, duration));
            }
            return cancelling;
        }

        public override IEnumerable<RemoteActivityFinished> SimulateOneStep(TimeStep timeStep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities)
        {

            // enqueue new arrivals and check if any of them cancel their activity
            var cancelling = AddNewPersons(timeStep, newActivities);

            while (activeVisitors.Count < concurrentActivities && waitingVisitors.Count != 0)
            {
                // the next agents can start their activity
                activeVisitors.Add(waitingVisitors.Dequeue());
            }

            foreach (var state in activeVisitors)
            {
                UpdateRemainingStayTime(state);
            }

            var finishedActivitites = GetFinishedAgents();
            LogState(timeStep, dateTime, newActivities, finishedActivitites, cancelling);

            // collect activity finished messages for finished and cancelling visitors
            var finishedMessages = GetFinishedMessages(finishedActivitites);
            var leaving = finishedMessages.Concat(cancelling);
            return leaving;
        }

        /// <summary>
        /// Logs the current state of this POI, if necessary.
        /// </summary>
        /// <param name="timestep">the current time step</param>
        /// <param name="dateTime">the current datetime</param>
        /// <param name="newActivities">new arrivals, if any</param>
        /// <param name="finishedActivities">visitors who finished their stay</param>
        /// <param name="cancelling">visitors that cancelled their activity and left immediately</param>
        protected void LogState(TimeStep timestep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities,
            IEnumerable<AgentStayState> finishedActivities, IEnumerable<RemoteActivityFinished> cancelling)
        {
            // leaving can only be not empty if there was also an arrival, so no need to check it
            if (newActivities.Any() || finishedActivities.Any())
            {
                int totalVisitors = activeVisitors.Count + waitingVisitors.Count;
                csvLogger.Log(timestep, dateTime, [totalVisitors, newActivities.Count(), finishedActivities.Count(), cancelling.Count()]);

                // log waiting time for all finished visitors
                foreach (var finished in finishedActivities)
                {
                    int waitingTime = timestep.InternalStep - finished.Arrival.InternalStep - finished.StayDuration + 1;
                    waitingTimeLogger.Log(timestep, dateTime, [finished.Activity.Person, waitingTime]);
                }
            }
        }

        public override void FinishSimulation()
        {
            base.FinishSimulation();
            waitingTimeLogger.WriteToFile();
        }
    }
}

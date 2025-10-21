using CalculationEngine.CitySimulation;
using Common;
using System.Diagnostics;

namespace CitySimulation.Simulators
{
    /// <summary>
    /// Simulates agent stays in any point of interest, for example a small enterprise.
    /// This is the most basic implementation of a POI simulator. It just keeps track of
    /// visitors, and every visitor stays for the duration sampled for the affordance.
    /// </summary>
    internal class PointOfInterestSimulator : ISimulator
    {
        protected List<AgentStayState> activeVisitors = [];

        protected readonly CsvLogger csvLogger;

        public PointOfInterestId PoiId { get; }

        public PointOfInterestSimulator(int rank, PointOfInterestId id, string outputDir)
        {
            PoiId = id;
            var filename = $"{PoiId.Id}.csv";
            csvLogger = new CsvIndexDateLogger(filename, outputDir, ["People present", "People arriving", "People leaving"], "poi_presence");
        }

        public virtual IEnumerable<RemoteActivityFinished> SimulateOneStep(TimeStep timeStep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities)
        {
            AddNewPersons(timeStep, newActivities);

            foreach (var state in activeVisitors)
            {
                UpdateRemainingStayTime(state);
            }

            var finishedActivitites = GetFinishedAgents();
            LogState(timeStep, dateTime, newActivities, finishedActivitites);
            var finishedMessages = GetFinishedMessages(finishedActivitites);
            return finishedMessages;
        }

        private void AddNewPersons(TimeStep timeStep, IEnumerable<RemoteActivityStart> newActivities)
        {
            foreach (var newActivity in newActivities)
            {
                Debug.Assert(!newActivity.IsTravel, "PointOfInterestSimulator received a travel activity.");

                int duration = DetermineDuration(newActivity);
                activeVisitors.Add(new AgentStayState(timeStep, newActivity, duration));
            }
        }

        /// <summary>
        /// Determines the stay duration of the person. The default implementation just uses the duration
        /// defined by the affordance.
        /// </summary>
        /// <param name="activity">the ActivityStart message</param>
        /// <returns>the duration in timesteps</returns>
        /// <exception cref="NotImplementedException">if no duration was specified in the message</exception>
        protected int DetermineDuration(RemoteActivityStart activity)
        {
            // -2 to account for the timesteps lost due to messaging until the CalcPerson receives the ActivityFinished message
            return activity.ExpectedDuration - 2 ?? throw new NotImplementedException("No default duration for remote activity implemented");
        }

        /// <summary>
        /// Logs the current state of this POI, if necessary.
        /// </summary>
        /// <param name="timestep">the current time step</param>
        /// <param name="dateTime">the current datetime</param>
        /// <param name="newActivities">new arrivals, if any</param>
        /// <param name="finishedActivities">visitors who finished their stay</param>
        protected virtual void LogState(TimeStep timestep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities, IEnumerable<AgentStayState> finishedActivities)
        {
            // only create a log entry if something changes
            if (newActivities.Any() || finishedActivities.Any())
            {
                csvLogger.Log(timestep, dateTime, [activeVisitors.Count, newActivities.Count(), finishedActivities.Count()]);
            }
        }

        /// <summary>
        /// Updates the remaining stay duration of a visitor.
        /// </summary>
        /// <param name="state">the stay state of the visitor to update</param>
        protected void UpdateRemainingStayTime(AgentStayState state)
        {
            state.RemainingDuration--;
        }

        /// <summary>
        /// Collects all stay state objects of visitors who finish their stay at the end
        /// of the current time step and removes them from the active visitors list.
        /// </summary>
        /// <returns>stay states of the finished visitors</returns>
        protected IEnumerable<AgentStayState> GetFinishedAgents()
        {
            // collect all persons that finished their activity in the current Timestep
            bool isFinished(AgentStayState t) => t.RemainingDuration <= 0;
            var finished = activeVisitors.FindAll(isFinished);
            // remove the finished persons from the list of present persons
            activeVisitors.RemoveAll(isFinished);
            return finished;
        }

        /// <summary>
        /// Creates an activity finished message for every visitor that
        /// finished their stay.
        /// </summary>
        /// <param name="finishedStates">state objects of all finished visitors</param>
        /// <returns>finished activity messages</returns>
        protected IEnumerable<RemoteActivityFinished> GetFinishedMessages(IEnumerable<AgentStayState> finishedStates)
        {
            // create the corresponding finished activity messages for all visitors
            return finishedStates.Select(t => new RemoteActivityFinished(t.Activity.Person, PoiId, true));
        }

        public virtual void FinishSimulation()
        {
            csvLogger.WriteToFile();
        }
    }
}

using Automation;
using CalculationEngine.CitySimulation;
using Common;
using System.Diagnostics;

namespace CitySimulation.Simulators
{
    /// <summary>
    /// Simulates agent stays in any point of interest, for example a small enterprise.
    /// </summary>
    internal class PointOfInterestSimulator : ISimulator
    {
        protected List<AgentStayState> activeVisitors = [];

        protected readonly TextLogger logger;
        protected readonly TextLogger presenceLogger;

        protected readonly JsonCalcSpecification calcSpec;

        public PointOfInterestId PoiId { get; }

        public PointOfInterestSimulator(int rank, PointOfInterestId id, JsonCalcSpecification calcSpec)
        {
            PoiId = id;
            this.calcSpec = calcSpec;
            var filename = $"{PoiId.Id}.txt";
            logger = new(filename, calcSpec.OutputDirectory, "poi_events");
            presenceLogger = new(filename, calcSpec.OutputDirectory, "poi_presence", true, "People present");
        }

        public virtual IEnumerable<RemoteActivityFinished> SimulateOneStep(TimeStep timeStep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities)
        {
            AddNewPersons(newActivities);

            // TODO: dummy implementation
            foreach (var state in activeVisitors)
            {
                // update travel progress
                UpdateRemainingStayTime(state);
            }

            var finishedActivitites = GetFinishedAgents();
            LogState(timeStep, dateTime, newActivities, finishedActivitites);
            return finishedActivitites;
        }

        private void AddNewPersons(IEnumerable<RemoteActivityStart> newActivities)
        {
            foreach (var newActivity in newActivities)
            {
                Debug.Assert(!newActivity.IsTravel, "PointOfInterestSimulator received a travel activity.");

                double duration = DetermineDuration(newActivity);
                activeVisitors.Add(new AgentStayState(newActivity, duration));
            }
        }

        protected int DetermineDuration(RemoteActivityStart activity)
        {
            // -2 to account for the timesteps lost due to messaging until the CalcPerson receives the ActivityFinished message
            return activity.ExpectedDuration - 2 ?? throw new NotImplementedException("No default duration for remote activity implemented");
        }

        protected virtual void LogState(TimeStep timestep, DateTime dateTime, IEnumerable<RemoteActivityStart> newActivities, IEnumerable<RemoteActivityFinished> finishedActivities)
        {
            if (newActivities.Any() || finishedActivities.Any())
            {
                logger.Log(timestep, dateTime, $"Persons present: {activeVisitors.Count}");
                // log each newly started activity
                foreach (var newActivity in newActivities)
                {
                    logger.Log(timestep, dateTime, $"{newActivity.Person.PersonName} started {newActivity.Affordance}");
                }
                if (finishedActivities.Any())
                {
                    var finishedPersons = string.Join(", ", finishedActivities.Select(a => a.Person.PersonName));
                    logger.Log(timestep, dateTime, $"Finished activitites: {finishedPersons}");
                }
                presenceLogger.Log(timestep, dateTime, $"{activeVisitors.Count}");
            }
        }

        protected void UpdateRemainingStayTime(AgentStayState state)
        {
            state.RemainingDuration--;
        }

        protected IEnumerable<RemoteActivityFinished> GetFinishedAgents()
        {
            // collect all persons that finished their activity in the current Timestep
            Predicate<AgentStayState> isFinished = t => t.RemainingDuration <= 0;
            var arrived = activeVisitors.FindAll(isFinished);
            // remove the finished persons from the list of present persons
            activeVisitors.RemoveAll(isFinished);
            // create the corresponding finished activity messages
            return arrived.Select(t => new RemoteActivityFinished(t.Activity.Person, PoiId));
        }

        public void FinishSimulation()
        {
            logger.WriteToFile();
        }
    }
}

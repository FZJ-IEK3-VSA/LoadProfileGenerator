using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using CalculationEngine.Transportation;
using Common;

namespace CalculationEngine.Activities
{
    /// <summary>
    /// Abstract base class for all affordance activations
    /// </summary>
    public abstract class Activity(string name, string dataSource, string personName, PointOfInterestId? destination) : IActivity
    {
        public string Name { get; } = name;
        public string DataSource { get; } = dataSource;
        public string PersonName { get; } = personName;
        public TimeStep? StartTime { get; protected set; }
        public PointOfInterestId? Destination { get; } = destination;
        public bool LightingSwitchedOn { get; protected set; }

        public abstract bool IsDetermined { get; }
        public abstract bool IsTravel { get; }
        public abstract ICalcAffordanceBase Affordance { get; }


        public virtual void Start(TimeStep timestep, DayLightStatus dayLightStatus, ICalcSite? currentSite)
        {
            if (StartTime is not null)
                throw new LPGException($"Activity {Name} was activated a second time.");

            StartTime = timestep;
            Affordance.StartActivation(timestep, PersonName, currentSite);
        }

        public virtual int Finish(TimeStep time, RemoteActivityFinished? remoteActivityResult)
        {
            if (StartTime is null)
                throw new LPGException($"Activity {Name} has not been activated yet.");

            Affordance.FinishActivation(time, PersonName);
            return time.InternalStep - StartTime.InternalStep;
        }


        public abstract string GetStartThought();

        public abstract bool IsFinished(TimeStep timestep, RemoteActivityFinished? remoteActivityResult);
    }
}

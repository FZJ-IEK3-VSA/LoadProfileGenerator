using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using Common;


namespace CalculationEngine.Activities
{
    /// <summary>
    /// Stores information about a single activation of a remote affordance, with unknown duration.
    /// </summary>
    /// <param name="dataSource">source of the affordance activation</param>
    /// <param name="personName">name of the person activating the affordance</param>
    /// <param name="destination">POI where the activity will take place</param>
    /// <param name="expectedDuration">the expected duration of the activity, if known</param>
    public abstract class DynamicActivity(string dataSource, string personName, PointOfInterestId? destination, int? expectedDuration)
        : Activity(dataSource, personName, destination)
    {
        public override bool IsDetermined => false;

        public override int? ExpectedDuration => expectedDuration;


        public override string GetStartThought()
        {
            string durationString = ExpectedDuration is null ? "unknown duration" : $"expected duration {ExpectedDuration}";
            return $"Starting to execute dynamic affordance {Name} with {durationString}.";
        }

        public override bool IsFinished(TimeStep timestep, RemoteActivityFinished? remoteActivityResult)
        {
            if (StartTime is null)
                throw new LPGException("Activity has not been activated yet.");

            return remoteActivityResult is not null;
        }

        public override int Finish(TimeStep time, RemoteActivityFinished? remoteActivityResult)
        {
            if (remoteActivityResult is null)
                throw new LPGException("A dynamic activity can only be finished with an activity finished object.");
            if (remoteActivityResult.NewLocation != Destination)
                throw new LPGException("Remote activity result specified a wrong new location.");
            return base.Finish(time, remoteActivityResult);
        }
    }
}

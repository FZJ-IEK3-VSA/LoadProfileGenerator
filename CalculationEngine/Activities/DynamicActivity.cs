using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using Common;


namespace CalculationEngine.Activities
{
    /// <summary>
    /// Stores information about a single activation of a remote affordance, with unknown duration.
    /// </summary>
    /// <param name="name">affordance name</param>
    /// <param name="dataSource">source of the affordance activation</param>
    /// <param name="personName">name of the person activating the affordance</param>
    /// <param name="destination">POI where the activity will take place</param>
    public abstract class DynamicActivity(string name, string dataSource, string personName, PointOfInterestId? destination)
        : Activity(name, dataSource, personName, destination)
    {
        public override bool IsDetermined => false;


        public override string GetStartThought()
        {
            return "Starting to execute dynamic affordance " + Name + " with unknown duration.";
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

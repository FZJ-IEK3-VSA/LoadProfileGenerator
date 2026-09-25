using CalculationEngine.Activities;

namespace CalculationEngine.CitySimulation
{
    /// <summary>
    /// Object encapsulating relevant information for a new remote activity
    /// </summary>
    /// <param name="person">the person carrying out the activity</param>
    /// <param name="activity">the corresponding activity object</param>
    /// <param name="currentLocation">ID of the point of interest the person is currently at</param>
    public class RemoteActivityInfo(PersonIdentifier person, DynamicActivity activity, PointOfInterestId? currentLocation)
    {
        public readonly PersonIdentifier Person = person;
        public readonly DynamicActivity Activity = activity;
        public readonly PointOfInterestId? CurrentLocation = currentLocation;

        /// <summary>
        /// Returns true if this activity is a travel activity, and false if it
        /// is an actual remote affordance.
        /// </summary>
        /// <returns>whether this activity is a travel activity</returns>
        public bool IsTravel()
        {
            return Activity.IsTravel;
        }
    }

    /// <summary>
    /// A message that a person is starting a new remote activity.
    /// </summary>
    /// <param name="person">the person starting the activity</param>
    /// <param name="isTravel">whether the activity is traveling or an activity at a point of interest</param>
    /// <param name="affordance">the name of the affordance being activated</param>
    /// <param name="poi">the ID of the point of interest where the activity will be carried out; null means home</param>
    /// <param name="currentLocation">the ID of the current location of the person; null means home</param>
    public class RemoteActivityStart(PersonIdentifier person, bool isTravel, string affordance, PointOfInterestId? poi, PointOfInterestId? currentLocation = null, int? expectedDuration = null)
    {
        public readonly PersonIdentifier Person = person;
        public readonly bool IsTravel = isTravel;
        public readonly string Affordance = affordance;
        public readonly PointOfInterestId? Poi = poi;
        public readonly PointOfInterestId? CurrentLocation = currentLocation;
        public readonly int? ExpectedDuration = expectedDuration;
    }

    /// <summary>
    /// A message that the current travel or remote activity of a person has been finished.
    /// </summary>
    /// <param name="person">the person whose activity is finished</param>
    /// <param name="newLocation">the location of the person at the end of the travel/activity; null means the person is at home</param>
    /// <param name="successful">whether the activity was finished successfully; false means it was cancelled</param>
    public class RemoteActivityFinished(PersonIdentifier person, PointOfInterestId? newLocation, bool successful)
    {
        public readonly PersonIdentifier Person = person;
        public readonly PointOfInterestId? NewLocation = newLocation;
        public readonly bool Successful = successful;
    }
}

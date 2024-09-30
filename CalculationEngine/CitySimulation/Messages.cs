using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;

namespace CalculationEngine.CitySimulation
{
    /// <summary>
    /// Object encapsulating relevant information for a new remote activity
    /// </summary>
    /// <param name="person">the person carrying out the activity</param>
    /// <param name="affordanceActivation">the corresponding RemoteAffordanceActivation object</param>
    public class RemoteActivityInfo(PersonIdentifier person, RemoteAffordanceActivation affordanceActivation)
    {
        public readonly PersonIdentifier Person = person;
        public readonly RemoteAffordanceActivation AffordanceActivation = affordanceActivation;
    }

    /// <summary>
    /// A message that the current remote activity of a person has been finished.
    /// </summary>
    /// <param name="person">the person whose activity is finished</param>
    public class RemoteActivityFinished(PersonIdentifier person)
    {
        public readonly PersonIdentifier Person = person;
    }

    /// <summary>
    /// A message that a person is starting a new remote activity.
    /// </summary>
    /// <param name="person">the person starting the activity</param>
    /// <param name="isTravel">whether the activity is traveling or an activity at a point of interest</param>
    /// <param name="affordance">the name of the affordance being activated</param>
    /// <param name="poi">the ID of the point of interest where the activity will be carried out</param>
    public class RemoteActivityStart(PersonIdentifier person, bool isTravel, string affordance, PointOfInterestId? poi = null)
    {
        public readonly PersonIdentifier Person = person;
        public readonly bool IsTravel = isTravel;
        public readonly string Affordance = affordance;
        public readonly PointOfInterestId? Poi = poi;
    }
}

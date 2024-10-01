using Automation.ResultFiles;
using CalculationEngine.CitySimulation;
using CalculationEngine.HouseholdElements;

namespace MassSimulation
{
    /// <summary>
    /// Register that stores all POIs
    /// TODO: is this even needed if each CalcPerson/CalcHousehold decides which POIs to use in advance?
    /// </summary>
    internal class PointOfInterestRegister(int numWorkers)
    {
        /// <summary>
        /// Register for all point of interests
        /// </summary>
        private readonly Dictionary<CalcLocation, IEnumerable<PointOfInterestId>> poiRegister = [];

        /// <summary>
        /// Get a random item out of an enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static T PickRandomElement<T>(IEnumerable<T> enumerable, Random? rand = null)
        {
            rand = rand ?? new Random();
            int index = rand.Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }

        private PointOfInterestId GetPOIForAffordance(ICalcAffordanceBase affordance)
        {
            if (!poiRegister.ContainsKey(affordance.ParentLocation))
            {
                throw new LPGException("No fitting POIs for location " + affordance.ParentLocation + " found.");
            }
            return PickRandomElement(poiRegister[affordance.ParentLocation]);
        }

        public MPIDistributor SortActivityMessagesByWorker(IEnumerable<RemoteActivityInfo> newActivities)
        {
            var activityMessages = new MPIDistributor(numWorkers);
            foreach (var activity in newActivities)
            {
                // determine whether the activity is a traveling or POI activity
                bool isTravel = activity.IsTravel();
                var poi = activity.AffordanceActivation.Destination;
                if (isTravel)
                {
                    var travelMessage = new RemoteActivityStart(activity.Person, true, activity.AffordanceActivation.Affordance.Name, poi, activity.CurrentLocation);
                    // determine the rank of the worker responsible for the person's current location
                    var currentLocationWorker = activity.CurrentLocation?.WorkerId ?? activity.Person.WorkerId;
                    activityMessages.AddNewActivity(currentLocationWorker, travelMessage);
                }
                else
                {
                    // the activity is a remote affordance; traveling has already happend and the person must be at the affordance location
                    if (poi is null || poi != activity.CurrentLocation)
                    {
                        throw new LPGException("A person wants to start a remote activity but is not at the correct point of interest.");
                    }
                    var activityMessage = new RemoteActivityStart(activity.Person, false, activity.AffordanceActivation.Affordance.Name, poi);
                    activityMessages.AddNewActivity(poi.WorkerId, activityMessage);
                }
            }
            return activityMessages;
        }
    }
}

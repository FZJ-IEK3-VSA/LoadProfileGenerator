using Automation;
using Database;
using Database.Tables.Transportation;
using System.Collections.Generic;
using Common;
using Automation.ResultFiles;

namespace SimulationEngineLib.HouseJobProcessor
{
    /// <summary>
    /// Builds a new travel route set based on the defined points of interests and
    /// the POI preferences of each person.
    /// </summary>
    internal class TravelRouteSetBuilderCity(Simulator simulator, IReadOnlyDictionary<string, PoiLocationReplacement> locationReplacements)
    {
        /// <summary>
        /// Database access object
        /// </summary>
        private readonly Simulator sim = simulator;

        /// <summary>
        /// Stores which location must be replaced with which new one, for each point of interest
        /// separately. Uses the POI-ID as key.
        /// </summary>
        public IReadOnlyDictionary<string, PoiLocationReplacement> LocationReplacements { get; } = locationReplacements;

        /// <summary>
        /// Checks if all required data is given to create a travel route set based
        /// on the point of interest preferences of each person.
        /// </summary>
        /// <param name="householdData">the HouseholdData object</param>
        /// <returns>true if all data is available; otherwise, false</returns>
        public static bool IsRequiredDataAvailable(HouseholdData householdData)
        {
            return householdData.PointOfInterestPreferences is not null;
        }

        internal TravelRouteSet CreateTravelRouteSetFromPoiPreferences(HouseholdData householdData)
        {
            // create a new empty travel route set
            var travelRouteSet = sim.TravelRouteSets.CreateNewItem(sim.ConnectionString);
            travelRouteSet.Name = $"Generated Travel Route Set for {householdData.Name}";
            travelRouteSet.Description = "This travel route set was generated using the point of interest preferences of all persons in this household.";

            foreach (var personPreference in householdData.PointOfInterestPreferences)
            {
                AddRoutesForPerson(personPreference.Key, personPreference.Value, travelRouteSet);
            }
            travelRouteSet.SaveToDB();
            return travelRouteSet;
        }

        private void AddRoutesForPerson(string personName, PersonPoiPreferences preferences, TravelRouteSet travelRouteSet)
        {
            var person = sim.Persons.FindFirstByNameNotNull(personName);
            foreach (var routeData in preferences.Routes)
            {
                // create the new travel route
                var route = sim.TravelRoutes.CreateNewItem(sim.ConnectionString);
                route.Description = "Generated from POI preferences";
                route.SiteA = GetSiteFromPoi(routeData.Start);
                route.SiteB = GetSiteFromPoi(routeData.Destination);
                route.RouteKey = "Generated";

                // create a single step with the specified transportation device category
                var deviceCategory = sim.TransportationDeviceCategories.FindWithException(routeData.TransportationDeviceCategory);
                var name = deviceCategory.Name;
                route.AddStep(name, deviceCategory, routeData.Distance, 1, name, true);

                SetRouteName(route, personName, deviceCategory.Name);
                route.SaveToDB();
                travelRouteSet.AddRoute(route, personID: person.IntID, weight: routeData.Weight);

                // if required, also create an identical route in the opposite direction
                if (preferences.MirrorRoutes)
                {
                    var mirroredRoute = route.MakeACopy(sim);
                    mirroredRoute.SiteA = route.SiteB;
                    mirroredRoute.SiteB = route.SiteA;
                    SetRouteName(mirroredRoute, personName, deviceCategory.Name);

                    mirroredRoute.SaveToDB();
                    travelRouteSet.AddRoute(mirroredRoute, personID: person.IntID, weight: routeData.Weight);
                }
            }
        }

        public Site GetSiteFromPoi(string poiId)
        {
            if (poiId == Constants.HomeSiteName)
            {
                // return the Home site
                return TravelRouteSetBuilderFromPersonData.GetHomeSite(sim);
            }
            if (!LocationReplacements.TryGetValue(poiId, out var routeData))
                throw new LPGException("The PointOfInterestPreferences specify a route with start/destination {poiId}, which is no known site or POI.");
            return routeData.NewSite;
        }

        /// <summary>
        /// Sets the name for a travel route depending on start and destination, and the name
        /// of the person the route is made for.
        /// </summary>
        /// <param name="route">the route to set the name for</param>
        /// <param name="personName">the name of the person who will use the route</param>
        private static void SetRouteName(TravelRoute route, string personName, string deviceCategoryName)
        {
            var distanceInKm = route.CalculateTotalDistance() / 1000;
            route.Name = $"Route for {personName} from {route.SiteA.Name} to {route.SiteB.Name} via {deviceCategoryName} {distanceInKm}km";
        }
    }
}

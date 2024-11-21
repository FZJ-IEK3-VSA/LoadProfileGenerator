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
                var route = sim.TravelRoutes.CreateNewItem(sim.ConnectionString);
                route.Name = $"Route for {personName} from {routeData.Start} to {routeData.Destination}";
                route.Description = "Generated";
                route.SiteA = GetSiteFromPoi(routeData.Start);
                route.SiteA = GetSiteFromPoi(routeData.Destination);
                
                var deviceCategory = sim.TransportationDeviceCategories.FindWithException(routeData.TransportationDeviceCategory);
                var name = deviceCategory.Name;
                route.AddStep(name, deviceCategory, routeData.Distance, 1, name, true);
                
                travelRouteSet.AddRoute(route, personID: person.IntID, weight: routeData.Weight);
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
    }
}

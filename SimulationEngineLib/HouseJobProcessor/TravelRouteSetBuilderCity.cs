using Automation;
using Database;
using Database.Tables.Transportation;
using System.Collections.Generic;
using Automation.ResultFiles;
using Database.Tables.ModularHouseholds;
using System.Linq;
using PowerArgs;

namespace SimulationEngineLib.HouseJobProcessor
{
    /// <summary>
    /// Builds a new travel route set based on the defined points of interests and
    /// the POI preferences of each person.
    /// </summary>
    internal class TravelRouteSetBuilderCity
    {
        /// <summary>
        /// Database access object
        /// </summary>
        private readonly Simulator sim;

        /// <summary>
        /// Stores which location must be replaced with which new one, for each point of interest
        /// separately. Uses the POI-ID as key.
        /// </summary>
        public IReadOnlyDictionary<string, PoiLocationReplacement> LocationReplacements { get; }

        /// <summary>
        /// The car transportation device category. Is used to apply a minimum driving age, if specified.
        /// </summary>
        private readonly TransportationDeviceCategory CarCategory;

        /// <summary>
        /// Maps each supported transport mode to the corresponding LPG transportation device category.
        /// </summary>
        private readonly Dictionary<string, TransportationDeviceCategory> TransportModes;

        public TravelRouteSetBuilderCity(Simulator simulator, IReadOnlyDictionary<string, PoiLocationReplacement> locationReplacements)
        {
            sim = simulator;
            LocationReplacements = locationReplacements;

            CarCategory = sim.TransportationDeviceCategories.FindFirstByName("Car Category");

            // initialize the mapping of transport modes to LPG device categories
            TransportModes = new Dictionary<string, TransportationDeviceCategory>
            {
                ["car"] = CarCategory,
                ["pt"] = sim.TransportationDeviceCategories.FindFirstByName("Bus Category"),
                ["bicycle"] = sim.TransportationDeviceCategories.FindFirstByName("Bicycle Category"),
                ["walk"] = sim.TransportationDeviceCategories.FindFirstByName("Walking Category")
            };
        }

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

        internal TravelRouteSet CreateTravelRouteSetFromPoiPreferences(HouseholdData householdData, ModularHousehold household, HouseCreationAndCalculationJob hj, TransportationDeviceSet transportationDeviceSet)
        {
            if (householdData.PointOfInterestPreferences.IsNullOrEmpty())
                throw new LPGPBadParameterException("Cannot create dynamic city routes without point of interest preferences for each person.");
            if (hj.City.Routes.Count == 0)
                throw new LPGPBadParameterException("Point of interest preferences were given, but no route data was provided.");

            // create a new empty travel route set
            var travelRouteSet = sim.TravelRouteSets.CreateNewItem(sim.ConnectionString);
            travelRouteSet.Name = $"Generated Travel Route Set for {householdData.Name}";
            travelRouteSet.Description = "This travel route set was generated using the point of interest preferences of all persons in this household.";

            foreach (var personPreference in householdData.PointOfInterestPreferences)
            {
                string personName = personPreference.Key;

                // determine the POIs that are actually relevant for this person
                var relevantLocations = household.Traits.Where(t => t.DstPerson.Name == personName).SelectMany(t => t.HouseholdTrait.Locations).Select(t => t.Location).ToHashSet();
                var relevantPOIs = LocationReplacements.Where(x => relevantLocations.Contains(x.Value.NewLocation)).Select(x => x.Key).ToHashSet();
                relevantPOIs.Add(hj.House.Name);

                AddRoutesForPerson(hj, travelRouteSet, relevantPOIs, transportationDeviceSet, personName);
            }
            travelRouteSet.SaveToDB();
            return travelRouteSet;
        }

        private void AddRoutesForPerson(HouseCreationAndCalculationJob hj, TravelRouteSet travelRouteSet, HashSet<string> relevantPOIs,
            TransportationDeviceSet transportationDeviceSet, string personName = null)
        {
            // use a single database connection to add all routes for a better performance
            using var con = new Database.Database.Connection(sim.ConnectionString);
            con.Open();
            using var tr = con.BeginTransaction();

            // determine the personId ID, if the routes are only for one person
            int? personId = string.IsNullOrEmpty(personName) ? null : sim.Persons.FindFirstByNameNotNull(personName).IntID;
            foreach (var routeData in hj.City.Routes)
            {
                if (!relevantPOIs.Contains(routeData.origin_id) || !relevantPOIs.Contains(routeData.destination_id))
                {
                    // start or destination of this route is not relevant for this person, so the route is not needed
                    continue;
                }

                // check if the household has a car
                bool hasCar = transportationDeviceSet.TransportationDeviceSetEntries.Any(x => x.TransportationDevice.TransportationDeviceCategory.Name == "Car Category");

                // select the correct weights for the household type
                var weights = hasCar ? routeData.prob_with_car_hh : routeData.prob_no_car_hh;

                foreach (var categoryDistancePair in routeData.mode_distances)
                {
                    // create the new travel route
                    var houseId = hj.House.Name;
                    var route = sim.TravelRoutes.CreateNewItem(sim.ConnectionString, con);
                    route.Description = "Generated from transport model data";
                    route.SiteA = GetSiteFromPoi(routeData.origin_id, houseId);
                    route.SiteB = GetSiteFromPoi(routeData.destination_id, houseId);
                    route.RouteKey = "Generated";

                    // create a single step with the specified transportation device category
                    var deviceCategory = TransportModes[categoryDistancePair.Key];
                    var deviceCategoryName = deviceCategory.Name;
                    SetRouteName(route, deviceCategoryName, personName);

                    // check if a duration is specified for this route and mode
                    double durationInS = routeData.mode_times.GetValueOrDefault(categoryDistancePair.Key, -1);
                    route.AddStep(deviceCategoryName, deviceCategory, categoryDistancePair.Value, 1, deviceCategoryName, durationInS, false);

                    route.SaveToDB(con);
                    var routeWeight = weights[categoryDistancePair.Key];
                    travelRouteSet.AddRoute(route, personID: personId, weight: routeWeight, savetodb: false);

                    // if required, also create an identical route in the opposite direction
                    if (hj.City.MirrorRoutes)
                    {
                        var mirroredRoute = sim.TravelRoutes.CreateNewItem(sim.ConnectionString, con);
                        mirroredRoute.SiteA = route.SiteB;
                        mirroredRoute.SiteB = route.SiteA;
                        mirroredRoute.Description = route.Description;
                        mirroredRoute.RouteKey = route.RouteKey;
                        SetRouteName(mirroredRoute, deviceCategoryName, personName);

                        mirroredRoute.AddStep(deviceCategoryName, deviceCategory, categoryDistancePair.Value, 1, deviceCategoryName, durationInS, false);

                        // set the specified minimum driving age for cars; -1 means no restriction
                        int minimumAge = deviceCategory == CarCategory ? hj.City.MinimumDrivingAge : -1;

                        mirroredRoute.SaveToDB(con);
                        travelRouteSet.AddRoute(mirroredRoute, personID: personId, minimumAge: minimumAge, weight: routeWeight, savetodb: false);
                    }
                }
            }
            tr.Commit();
        }

        /// <summary>
        /// Returns the corresponding site object for the given POI name. If the poiId is the same as the homeId, returns the home site.
        /// </summary>
        /// <param name="poiId">the name/ID of the POI</param>
        /// <param name="homeId">the building ID of the home of the affected person</param>
        /// <returns>the site object</returns>
        /// <exception cref="LPGException">if the POI name does not match a site object</exception>
        public Site GetSiteFromPoi(string poiId, string homeId)
        {
            if (poiId == homeId)
            {
                // return the Home site
                return TravelRouteSetBuilderFromPersonData.GetHomeSite(sim);
            }
            if (!LocationReplacements.TryGetValue(poiId, out var replacement))
                throw new LPGException($"Found a route with start/destination {poiId}, which is no known site or POI.");
            return replacement.NewSite;
        }

        /// <summary>
        /// Sets the name for a travel route depending on start and destination, and the name
        /// of the person the route is made for.
        /// </summary>
        /// <param name="route">the route to set the name for</param>
        /// <param name="deviceCategoryName">name of the transportation device category</param>
        /// <param name="personName">the name of the personId who will use the route</param>
        private static void SetRouteName(TravelRoute route, string deviceCategoryName, string personName = "")
        {
            var distanceInKm = route.CalculateTotalDistance() / 1000;
            var personHint = string.IsNullOrEmpty(personName) ? "" : $" for {personName}";
            route.Name = $"Route{personHint} from {route.SiteA.Name} to {route.SiteB.Name} via {deviceCategoryName} {distanceInKm}km";
        }
    }
}

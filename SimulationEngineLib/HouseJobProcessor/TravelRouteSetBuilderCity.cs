using Automation;
using Database;
using Database.Tables.Transportation;
using System.Collections.Generic;
using Automation.ResultFiles;
using Database.Tables.ModularHouseholds;
using System.Linq;
using PowerArgs;
using System;
using Database.Tables.BasicElements;

namespace SimulationEngineLib.HouseJobProcessor
{
    /// <summary>
    /// Auxiliary record to store origin and destination Building ID for a route.
    /// </summary>
    /// <param name="Origin">Building ID of the route start</param>
    /// <param name="Destination">Building ID of the route end</param>
    internal record RouteEndpoints(string Origin, string Destination);

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

        /// <summary>
        /// Maps each encountered time slot object to the corresponding timelimit.
        /// </summary>
        private readonly Dictionary<TimeSlot, TimeLimit> TimeLimitMap = [];

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

        /// <summary>
        /// Builds a new travel route set for the household, based on the POI preferences and the route data in the
        /// city specification.
        /// </summary>
        /// <param name="householdData">the household data</param>
        /// <param name="household">the ModularHousehold object</param>
        /// <param name="hj">the house job object</param>
        /// <param name="transportationDeviceSet">the transportation device set to use</param>
        /// <returns> the new travel route set</returns>
        /// <exception cref="LPGPBadParameterException"></exception>
        internal TravelRouteSet CreateTravelRouteSetFromPoiPreferences(HouseholdData householdData, ModularHousehold household, HouseCreationAndCalculationJob hj, TransportationDeviceSet transportationDeviceSet)
        {
            if (householdData.PointOfInterestPreferences.IsNullOrEmpty())
                throw new LPGPBadParameterException("Cannot create dynamic city routes without point of interest preferences for each person.");
            if (hj.City.TravelDefinition.TimeSlotRouteLists.Count == 0)
                throw new LPGPBadParameterException("Point of interest preferences were given, but no route data was provided.");

            // create a timelimit for every route data time slot
            foreach (var tlRouteData in hj.City.TravelDefinition.TimeSlotRouteLists)
            {
                var timeLimit = GenerateTimeLimitFromTimeSlot(tlRouteData.TimeSlot);
                // store the timelimit in the map to access it later
                TimeLimitMap.Add(tlRouteData.TimeSlot, timeLimit);
            }

            // check if the household has a car
            bool hasCar = transportationDeviceSet.TransportationDeviceSetEntries.Any(x => x.TransportationDevice.TransportationDeviceCategory.Name == "Car Category");

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

                AddRoutesForPerson(hj, travelRouteSet, relevantPOIs, hasCar, personName);
            }
            travelRouteSet.SaveToDB();
            return travelRouteSet;
        }

        /// <summary>
        /// Creates a new timelimit for the time slot, matching its day type and time frame
        /// </summary>
        /// <param name="timeSlot">the time slot to create a timelimit for</param>
        /// <returns>the new timelimit</returns>
        private TimeLimit GenerateTimeLimitFromTimeSlot(TimeSlot timeSlot)
        {
            string name = $"TimeLimit generated for Route {timeSlot}";
            var timeLimit = sim.TimeLimits.FindFirstByName(name);
            if (timeLimit is not null)
            {
                // a matching timelimit already exists
                return timeLimit;
            }

            timeLimit = sim.TimeLimits.CreateNewItem(sim.ConnectionString);
            timeLimit.Name = name;

            // create a timelimit entry for the time and day type of the time slot
            var entry = timeLimit.AddTimeLimitEntry(null, sim.DateBasedProfiles.Items);
            entry.RepeaterType = Database.Helpers.PermissionMode.EveryXWeeks;
            entry.RandomizeTimeAmount = 15;
            entry.WeeklyWeekCount = 1;
            entry.StartWeek = -3;

            // convert the timeslot numbers (seconds since midnight) to time spans
            entry.StartTimeTimeSpan = GetTimeSpanFromSecondsSinceMidnight(timeSlot.Start);
            entry.EndTimeTimeSpan = GetTimeSpanFromSecondsSinceMidnight(timeSlot.End);

            // set the correct days depending on the DayType of the timeSlot
            entry.WeeklyMonday = false;
            entry.WeeklyTuesday = false;
            entry.WeeklyWednesday = false;
            entry.WeeklyThursday = false;
            entry.WeeklyFriday = false;
            entry.WeeklySaturday = false;
            entry.WeeklySunday = false;
            foreach (var dayOfWeek in timeSlot.WeekDays)
            {
                switch (dayOfWeek)
                {
                    case DayOfWeek.Monday: entry.WeeklyMonday = true; break;
                    case DayOfWeek.Tuesday: entry.WeeklyTuesday = true; break;
                    case DayOfWeek.Wednesday: entry.WeeklyWednesday = true; break;
                    case DayOfWeek.Thursday: entry.WeeklyThursday = true; break;
                    case DayOfWeek.Friday: entry.WeeklyFriday = true; break;
                    case DayOfWeek.Saturday: entry.WeeklySaturday = true; break;
                    case DayOfWeek.Sunday: entry.WeeklySunday = true; break;
                }
            }

            timeLimit.SaveToDB();
            return timeLimit;
        }

        /// <summary>
        /// Turns the time specification from a time slot, which is in seconds since midnight,
        /// into a time span. Ensures that the time span is not longer than one day so
        /// that the timelimit calculation works.
        /// </summary>
        /// <param name="time">time slot time, in seconds since midnight</param>
        /// <returns>the corresponding time span</returns>
        /// <exception cref="LPGPBadParameterException">if the time span was longer than one day</exception>
        private static TimeSpan GetTimeSpanFromSecondsSinceMidnight(int time)
        {
            var timeSpan = TimeSpan.FromSeconds(time);
            if (timeSpan.TotalDays > 1)
                throw new LPGPBadParameterException($"Time declaration in a timeslot must be max. one day: {time} (as TimeSpan: {timeSpan})");
            return timeSpan;
        }

        private void AddRoutesForPerson(HouseCreationAndCalculationJob hj, TravelRouteSet travelRouteSet, HashSet<string> relevantPOIs,
            bool hasCar, string personName = null)
        {
            // use a single database connection to add all routes for a better performance
            using var con = new Database.Database.Connection(sim.ConnectionString);
            con.Open();
            using var tr = con.BeginTransaction();

            // determine the personId ID, if the routes are only for one person
            int? personId = string.IsNullOrEmpty(personName) ? null : sim.Persons.FindFirstByNameNotNull(personName).IntID;
            var houseId = hj.House.Name;
            // iterate through all routes in all RoutesForTimeSlot objects and identify the relevant ones
            foreach (var routesForOneTimeSlot in hj.City.TravelDefinition.TimeSlotRouteLists)
            {
                // get the timelimit that applies for these routes
                var timeLimit = TimeLimitMap[routesForOneTimeSlot.TimeSlot];
                foreach (var routeData in routesForOneTimeSlot.Routes)
                {
                    // get all combinations of origins and destinations that this route applies to
                    var routeEndpoints = GetAllSiteCombinationsForRoute(relevantPOIs, routeData, hj.City.TravelDefinition.PoiClusterMapping);
                    foreach (var endpoints in routeEndpoints)
                    {
                        CreateRoutesForOneOriginDestinatino(hj, travelRouteSet, hasCar, personName, personId, houseId, timeLimit, routeData, endpoints.Origin, endpoints.Destination, con);
                        // if required, also create an identical route in the opposite direction
                        if (hj.City.TravelDefinition.MirrorRoutes)
                        {
                            CreateRoutesForOneOriginDestinatino(hj, travelRouteSet, hasCar, personName, personId, houseId, timeLimit, routeData, endpoints.Destination, endpoints.Origin, con);
                        }
                    }
                }
            }
            tr.Commit();
        }

        /// <summary>
        /// Creates all routes for one person for one fixed origin and destination. One TravelRoute object per mode is created.
        /// </summary>
        /// <param name="hj">the house job</param>
        /// <param name="travelRouteSet">the travel route set to add the routes to</param>
        /// <param name="hasCar">whether the household has a car</param>
        /// <param name="personName">the name of the person the routes are for</param>
        /// <param name="personId">the ID of the person the routes are for</param>
        /// <param name="houseId">ID of the house the household belongs to</param>
        /// <param name="timeLimit">timelimit that applies for the routes</param>
        /// <param name="routeData">the route definition; origin and destination might be cluster IDs</param>
        /// <param name="origin">actual starting point of the route</param>
        /// <param name="destination">actual destination point of the route</param>
        /// <param name="con">database connection to use for faster storage</param>
        private void CreateRoutesForOneOriginDestinatino(HouseCreationAndCalculationJob hj, TravelRouteSet travelRouteSet, bool hasCar,
            string personName, int? personId, string houseId, TimeLimit timeLimit, RouteData routeData,
            string origin, string destination, Database.Database.Connection con)
        {
            // select the correct weights for the household type
            var weights = hasCar ? routeData.prob_with_car_hh : routeData.prob_no_car_hh;
            var originSite = GetSiteFromPoi(origin, houseId);
            var destinationSite = GetSiteFromPoi(destination, houseId);

            foreach (var categoryDistancePair in routeData.mode_distances)
            {
                // check if a duration is specified for this route and mode
                double? durationInMin = routeData.mode_times.GetValueOrDefault(categoryDistancePair.Key, -1);
                double? distanceInKm = categoryDistancePair.Value;
                if (!durationInMin.HasValue || !distanceInKm.HasValue)
                    continue; // skip this mode

                double durationInS = durationInMin.Value * 60;
                double distanceInM = distanceInKm.Value * 1000;

                // create a single step with the specified transportation device category
                var deviceCategory = TransportModes[categoryDistancePair.Key];
                var deviceCategoryName = deviceCategory.Name;

                // create the new travel route
                var personHint = string.IsNullOrEmpty(personName) ? "" : $" for {personName}";
                string routeName = $"Route{personHint} from {originSite.Name} to {destinationSite.Name} via {deviceCategoryName} {distanceInKm:f1}km";
                const string description = "Generated from transport model data";
                var route = new TravelRoute(null, sim.ConnectionString, routeName, description, originSite, destinationSite, StrGuid.New(), "Generated");
                sim.TravelRoutes.Items.Add(route);
                // save here already to receive an ID (necessary for adding steps)
                route.SaveToDB(con);

                route.AddStep(deviceCategoryName, deviceCategory, distanceInM, 1, deviceCategoryName, durationInS, false);
                route.SaveToDB(con);

                // set the specified minimum driving age for cars; -1 means no restriction
                int minimumAge = deviceCategory == CarCategory ? hj.City.TravelDefinition.MinimumDrivingAge : -1;
                var routeWeight = weights[categoryDistancePair.Key];
                travelRouteSet.AddRoute(route, minimumAge: minimumAge, personID: personId, weight: routeWeight, timeLimit: timeLimit, savetodb: false);
            }
        }

        /// <summary>
        /// Generates a list of all possible origin and destination site IDs that a RouteData object is relevant for.
        /// If no clustering is used, this is at most one combination, namely the origin and destination declared in the
        /// RouteData object. But if clustering is used and multiple POIs belong to the same cluster, this results in many
        /// combinations. For each combination, individual TravelRoute objects need to be generated.
        /// </summary>
        /// <param name="relevantPOIs">all POIs that are relevant for a single person</param>
        /// <param name="route">the route definition</param>
        /// <param name="poiClusterMapping">the dictionary mapping POI IDs to the respective clusters</param>
        /// <returns>the origin-destination combinations for which TravelRoutes must be generated</returns>
        private static IEnumerable<RouteEndpoints> GetAllSiteCombinationsForRoute(HashSet<string> relevantPOIs, RouteData route, Dictionary<string, string>? poiClusterMapping)
        {
            if (poiClusterMapping.IsNullOrEmpty())
            {
                // no clustering is used, origin and destination of each route are POI IDs
                if (!relevantPOIs.Contains(route.origin_id) || !relevantPOIs.Contains(route.destination_id))
                    return [];

                // both origin and destination are relevant POIs, so this route is relevant
                return [new(route.origin_id, route.destination_id)];
            }

            // A clustering is used, so origin and destination of each route are cluster IDs instead of POI IDs.
            // First, collect all POIs that belong to the origin or destination cluster of the route.
            var routeOrigins = relevantPOIs.Where(poi => poiClusterMapping[poi] == route.origin_id);
            var routeDestinations = relevantPOIs.Where(poi => poiClusterMapping[poi] == route.destination_id);
            // do a cartesian product to get all possible origin-destination combinations, excluding routes with identical start and end POI
            var combinations = from orig in routeOrigins from dest in routeDestinations where orig != dest select new RouteEndpoints(orig, dest);
            return combinations;
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
    }
}

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common.Extensions;
using Database;
using Database.Helpers;
using Database.Tables.Transportation;

namespace SimulationEngineLib.HouseJobProcessor
{
    /// <summary>
    /// Creates a new travel route set out of individual personal traveling preferences specified
    /// in the person data of the calc spec.
    /// </summary>
    /// <param name="simulator">database access object</param>
    internal class TravelRouteSetBuilderFromPersonData(Simulator simulator)
    {
        private readonly Simulator sim = simulator;

        /// <summary>
        /// Checks if all required data is set in order to create a travel route set from
        /// the traveling preferences of each person.
        /// </summary>
        /// <param name="householdData">the HouseholdData object</param>
        /// <returns>true if all data is available; otherwise, false</returns>
        public static bool IsRequiredDataAvailable(HouseholdData householdData)
        {
            // check if person data is given
            if (householdData.HouseholdDataPersonSpec?.Persons is null)
                return false;
            foreach (var person in householdData.HouseholdDataPersonSpec.Persons)
            {
                // check if the transportation preferences are set for each person in the household
                if (person?.TransportationPreferences is null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Alternatively to directly specifying a TravelRouteSet the transport preferences of each person can be specified.
        /// This function collects these preferences and uses them to create a new TravelRouteSet.
        /// </summary>
        /// <param name="householdData">The HouseholdData object for which the TravelRouteSet will be created</param>
        /// <returns>A new TravelRouteSet that contains all required routes.</returns>
        public TravelRouteSet CreateTravelRouteSetFromPersonPreferences(HouseholdData householdData)
        {
            // find the home site of this household because it has a special role when using transportation preferences of persons
            Site home = sim.Sites.FindFirstByName("Home", FindMode.IgnoreCase);
            if (home is null)
            {
                throw new LPGException("Could not find the \"Home\" site, which is necessary to build a new travel route set");
            }
            // create a new empty travel route set
            var name = "Generated TravelRouteSet " + "(" + householdData.Name + ")";
            var description = "This TravelRouteSet was generated using the transportation device preferences of all persons in this household.";
            var travelRouteSet = new TravelRouteSet(name, null, sim.ConnectionString, description, Guid.NewGuid().ToStrGuid(), null);
            travelRouteSet.SaveToDB();
            foreach (PersonData person in householdData.HouseholdDataPersonSpec!.Persons)
            {
                CreateTravelRoutesForPerson(person, home, travelRouteSet);
            }
            return travelRouteSet;
        }

        /// <summary>
        /// Creates new TravelRoutes based on the transportation preferences of a person and adds them to the TravelRouteSet.
        /// </summary>
        /// <param name="person">The person the TravelRoutes will be created for</param>
        /// <param name="home">The site that is used as home of the person</param>
        /// <param name="travelRouteSet">The TravelRouteSet to which the new routes will be added</param>
        private void CreateTravelRoutesForPerson(PersonData person, Site home, TravelRouteSet travelRouteSet)
        {
            var preferences = person.TransportationPreferences;
            if (preferences is null)
            {
                throw new LPGPBadParameterException($"No transportation preferences given for {person.PersonName}.");
            }
            Dictionary<Site, TransportationPreference> sites = [];
            // collect all sites (except home)
            foreach (var preference in preferences)
            {
                var site = sim.Sites.FindWithException(preference.DestinationSite);
                sites.Add(site, preference);
            }
            // create routes from each site to all others
            foreach (var destination in sites.Keys)
            {
                var preference = sites[destination];
                // add route from home
                CreateRoutesForDifferentCategories(person, home, destination, preference.DistanceFromHome, preference, travelRouteSet);
                // add route from this site to home, using the same transportation preference
                CreateRoutesForDifferentCategories(person, destination, home, preference.DistanceFromHome, preference, travelRouteSet);
                // add routes from all other sites
                foreach (var origin in sites.Keys.Where(site => site != destination))
                {
                    var preferenceOrigin = sites[origin];
                    double distance = CalcDistanceBetweenSites(preference.Angle, preference.DistanceFromHome, preferenceOrigin.Angle, preferenceOrigin.DistanceFromHome);
                    CreateRoutesForDifferentCategories(person, origin, destination, distance, preference, travelRouteSet);
                }
            }
        }

        /// <summary>
        /// Creates all necessary TravelRoutes for a specific origin and destination, one for each transportation device category.
        /// </summary>
        /// <param name="person">The person for that the TravelRoutes are created</param>
        /// <param name="origin">The origin site for the TravelRoutes</param>
        /// <param name="destination">The destination site for the TravelRoutes</param>
        /// <param name="length">The length of the TravelRoutes</param>
        /// <param name="preference">The transportation preference of the person for the destination site</param>
        /// <param name="travelRouteSet">The TravelRouteSet to which the new TravelRoutes will be added</param>
        private void CreateRoutesForDifferentCategories(PersonData person, Site origin, Site destination, double length, TransportationPreference preference, TravelRouteSet travelRouteSet)
        {
            for (int i = 0; i < preference.TransportationDeviceCategories.Count; i++)
            {
                // create the route for the specified transportation device category
                var category = sim.TransportationDeviceCategories.FindWithException(preference.TransportationDeviceCategories[i]);
                var name = "Generated (from " + origin.Name + " to " + destination.Name + " for " + person.PersonName + " using \"" + category.Name + "\")";
                var description = "This route was generated based on the transportation preferences of " + person.PersonName;
                var route = new TravelRoute(null, sim.ConnectionString, name, description, origin, destination, StrGuid.New(), "");
                route.SaveToDB();
                // add only a single step using the specified category
                var stepName = "Generated (" + category.Name + ")";
                route.AddStep(stepName, category, length, 1, "");
                // add an entry to the travel route set with the specified weight
                travelRouteSet.AddRoute(route, weight: preference.Weights[i]);
            }
        }

        /// <summary>
        /// Calculates the distance between two sites, using their respective distances and angles to the centre (home)
        /// </summary>
        /// <param name="angle1">Angle of the first site as viewed from the centre</param>
        /// <param name="distanceFromHome1">Distance from the first site to the centre</param>
        /// <param name="angle2">Angle of the second site as viewed from the centre</param>
        /// <param name="distanceFromHome2">Distance from the second site to the centre</param>
        /// <param name="rad">True, if the angles are specified in rad, else false</param>
        /// <returns>The distance between the two sites</returns>
        private static double CalcDistanceBetweenSites(double angle1, double distanceFromHome1, double angle2, double distanceFromHome2, bool rad = false)
        {
            double difference = CalcAngleDifference(angle1, angle2, rad);
            return CalcTriangleMissingSide(distanceFromHome1, distanceFromHome2, difference, rad);
        }

        /// <summary>
        /// Calculates the minimum difference between two angles.
        /// </summary>
        /// <param name="angle1">The first angle</param>
        /// <param name="angle2">The second angle</param>
        /// <param name="rad">True, if the angles are specified in rad, else false</param>
        /// <returns>The difference angle between the two input angles</returns>
        private static double CalcAngleDifference(double angle1, double angle2, bool rad = false)
        {
            double fullCircle = rad ? 2 * Math.PI : 360;
            double difference = Math.Abs(angle1 - angle2);
            return difference > fullCircle / 2.0 ? fullCircle - difference : difference;
        }

        /// <summary>
        /// Calculates the missing side of a triangle, given two sides and the angle between them.
        /// </summary>
        /// <param name="a">The length of the first side</param>
        /// <param name="b">The length of the second side</param>
        /// <param name="angle">The angle between the two known sides</param>
        /// <param name="rad">True, if the angle is specified in rad, else false</param>
        /// <returns>The length of the third side</returns>
        private static double CalcTriangleMissingSide(double a, double b, double angle, bool rad = false)
        {
            angle = rad ? angle : Math.PI * angle / 180.0;
            return Math.Sqrt(a * a + b * b - Math.Cos(angle) * 2 * a * b);
        }
    }
}

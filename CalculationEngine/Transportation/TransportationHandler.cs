using System;
using System.Collections.Generic;
using System.Linq;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.Enums;
using JetBrains.Annotations;

namespace CalculationEngine.Transportation {
    public class TransportationHandler {
        [NotNull]
        [ItemNotNull]
        public List<CalcSite> CalcSites { get; }= new List<CalcSite>();
        [NotNull]
        [ItemNotNull]
        public List<CalcTransportationDevice> VehicleDepot { get; } = new List<CalcTransportationDevice>();
        [NotNull]
        [ItemNotNull]
        public List<CalcTransportationDevice> LocationUnlimitedDevices { get; } = new List<CalcTransportationDevice>();
        [NotNull]
        [ItemNotNull]
        public List<CalcTravelRoute> TravelRoutes { get; }= new List<CalcTravelRoute>();

        [NotNull]
        [ItemNotNull]
        public List<CalcTransportationDevice> AllMoveableDevices { get; } = new List<CalcTransportationDevice>();

        [NotNull]
        public Dictionary<CalcSite, CalcTravelRoute> SameSiteRoutes { get; }= new Dictionary<CalcSite, CalcTravelRoute>();
        [NotNull]
        [ItemNotNull]
        public List<CalcTransportationDeviceCategory> DeviceCategories { get;  } = new List<CalcTransportationDeviceCategory>();

        /// maps the name of each AffordanceTaggingSet to the respective CalcAffordanceTaggingSetDto object
        [NotNull]
        [ItemNotNull]
        public Dictionary<string, CalcAffordanceTaggingSetDto> AffordanceTaggingSets { get; } = new Dictionary<string, CalcAffordanceTaggingSetDto>();

        /// <summary>
        /// Stores which transportation device (e.g., cars) is currently owned by which person
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public DeviceOwnershipMapping<string, CalcTransportationDevice> DeviceOwnerships { get; } = new DeviceOwnershipMapping<string, CalcTransportationDevice>();

        /// <summary>
        /// Randomly selects a route out of all available ones, taking the respective weights into account.
        /// Depending on the selected TravelRouteSet, routes can be filtered based on the person or 
        /// the target affordance.
        /// </summary>
        /// <param name="srcSite">source site for the route</param>
        /// <param name="dstSite">destination site for the route</param>
        /// <param name="startTimeStep">time step in which to start the trip</param>
        /// <param name="person">the traveling person</param>
        /// <param name="affordance">the affordance for which the person wants to travel</param>
        /// <param name="calcRepo">the CalcRepo instance</param>
        /// <returns>the chosen route, or null if no available route was found</returns>
        public CalcTravelRoute? GetTravelRouteFromSrcLoc(CalcSite srcSite, CalcSite dstSite, TimeStep startTimeStep,
            CalcPersonDto person, ICalcAffordanceBase affordance, CalcRepo calcRepo)
        {
            if (srcSite == dstSite) {
                // TODO: except for home, these should actually be selected like all other routes
                return SameSiteRoutes[srcSite];
            }
            if (srcSite.DeviceChangeAllowed)
            {
                // person is not bound to a device anymore
                DeviceOwnerships.RemoveOwnership(person.Name);
            }
            //first get the routes, no matter if busy
            var devicesAtSrc = AllMoveableDevices.Where(x => x.Currentsite == srcSite).ToList();
            var possibleRoutes = srcSite.GetAllRoutesTo(dstSite, devicesAtSrc, person, DeviceOwnerships);
            // filter routes based on the affordance tag
            var allowedRoutes = possibleRoutes
                .Where(route => route.PersonID == null || route.PersonID == person.ID)
                .Where(route => route.Gender == PermittedGender.All || person.Gender == PermittedGender.All || route.Gender == person.Gender)
                .Where(route => route.MinimumAge < 0 || route.MinimumAge <= person.Age)
                .Where(route => route.MaximumAge < 0 || route.MaximumAge >= person.Age)
                .Where(route => {
                    if (route.AffordanceTaggingSetName == null || route.AffordanceTagName == null)
                    {
                        // if no AffordanceTagging information is given for a route, then it is allowed for all affordances
                        return true;
                    }
                    var affordanceTaggingSet = AffordanceTaggingSets[route.AffordanceTaggingSetName];
                    if (!affordanceTaggingSet.ContainsAffordance(affordance.Name))
                    {
                        // if the affordance is not tagged, then all routes are allowed
                        return true;
                    }
                    return affordanceTaggingSet.GetAffordanceTag(affordance.Name) == route.AffordanceTagName;
                })
                .ToList();
            if (allowedRoutes.Count == 0) {
                return null;
            }

            //check if the route is busy by calculating the duration. If busy, duration will be null
            int? dur = null;
            CalcTravelRoute? selectedRoute = null;
            while (dur == null && allowedRoutes.Count > 0) {
                // select a route randomly, based on the weights
                double totalWeight = allowedRoutes.Sum(route => route.Weight);
                double randomNumber = calcRepo.Rnd.NextDouble() * totalWeight;
                selectedRoute = allowedRoutes.Last(); // default (in case of double errors); should normally be overwritten
                foreach (var route in allowedRoutes)
                {
                    if (randomNumber < route.Weight)
                    {
                        selectedRoute = route;
                        break;
                    }
                    randomNumber -= route.Weight;
                }
                allowedRoutes.Remove(selectedRoute);
                dur = selectedRoute.GetDuration(startTimeStep, person, AllMoveableDevices, DeviceOwnerships);
            }

            if (dur == null) {
                // no usable route found
                selectedRoute = null;
            }

            return selectedRoute;
        }

        public void AddVehicleDepotDevice([NotNull] CalcTransportationDevice dev)
        {
            VehicleDepot.Add(dev);
            AllMoveableDevices.Add(dev);
        }

        public void AddSite([NotNull] CalcSite srcSite)
        {
            CalcSites.Add(srcSite);
        }

        [NotNull]
        public CalcTransportationDeviceCategory GetCategory([NotNull] CalcTransportationDeviceCategoryDto catDto)
        {
            if (DeviceCategories.Any(x => x.Guid == catDto.Guid)) {
                return DeviceCategories.Single(x => x.Guid == catDto.Guid);
            }
            CalcTransportationDeviceCategory ct = new CalcTransportationDeviceCategory(catDto.Name,catDto.IsLimitedToSingleLocation,catDto.Guid);
            DeviceCategories.Add(ct);
            return ct;
        }

        public void AddAffordanceTaggingSets(List<CalcAffordanceTaggingSetDto> affordanceTaggingSets)
        {
            foreach (var set in affordanceTaggingSets) { 
                AffordanceTaggingSets.Add(set.Name, set);
            }
        }
  }
}
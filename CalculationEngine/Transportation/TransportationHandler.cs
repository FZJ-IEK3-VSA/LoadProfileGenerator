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
        private Dictionary<CalcLocation, CalcSite> LocationSiteLookup { get; } = new Dictionary<CalcLocation, CalcSite>();

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

        public CalcTravelRoute? GetTravelRouteFromSrcLoc([NotNull] CalcLocation srcLocation,
                                                             [NotNull] CalcSite dstSite, [NotNull] TimeStep startTimeStep,
                                                             [NotNull] CalcPersonDto calcPerson, [NotNull] ICalcAffordanceBase affordance, CalcRepo calcRepo)
        {
            CalcSite srcSite = LocationSiteLookup[srcLocation];
            if (srcSite == dstSite) {
                return SameSiteRoutes[srcSite];
            }
            //first get the routes, no matter if busy
            var devicesAtSrc = AllMoveableDevices.Where(x => x.Currentsite == srcSite).ToList();
            var possibleRoutes = srcSite.GetAllRoutesTo(dstSite,devicesAtSrc);
            // filter routes based on the affordance tag
            var allowedRoutes = possibleRoutes
                .Where(route => route.Gender == PermittedGender.All || calcPerson.Gender == PermittedGender.All || route.Gender == calcPerson.Gender)
                .Where(route => route.MinimumAge < 0 || route.MinimumAge <= calcPerson.Age)
                .Where(route => route.MaximumAge < 0 || route.MaximumAge >= calcPerson.Age)
                .Where(route => {
                    if (route.AffordanceTaggingSetName == null || route.AffordanceTagName == null)
                    {
                    // if no AffordanceTagging information is given for a route, then it is allowed for all affordances
                    return true;
                    }
                    var affordanceTaggingSet = AffordanceTaggingSets[route.AffordanceTaggingSetName];
                    if (!affordanceTaggingSet.AffordanceToTagDict.ContainsKey(affordance.Name))
                    {
                    // if the affordance is not tagged, then all routes are allowed
                    return true;
                    }
                    return affordanceTaggingSet.AffordanceToTagDict[affordance.Name] == route.AffordanceTagName;
                })
                .ToList();
            if (allowedRoutes.Count == 0) {
                return null;
            }

            //check if the route is busy by calculating the duration. If busy, duration will be null
            int? dur = null;
            CalcTravelRoute? selectedRoute = null;
            while (dur== null && allowedRoutes.Count > 0) {
                // select a route randomly, based on the weights
                double totalWeight = allowedRoutes.Sum(route => route.Weight);
                double randomNumber = calcRepo.Rnd.NextDouble() * totalWeight;
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
                dur = selectedRoute.GetDuration(startTimeStep, calcPerson.Name, AllMoveableDevices);
            }

            if (dur == null) {
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
            foreach (CalcLocation location in srcSite.Locations) {
                LocationSiteLookup.Add(location,srcSite);
            }
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
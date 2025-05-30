using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;

namespace CalculationEngine.Transportation
{
    public class TransportationHandler
    {
        public List<CalcSite> CalcSites { get; } = [];

        public List<CalcTransportationDevice> VehicleDepot { get; } = [];

        public List<CalcTransportationDevice> LocationUnlimitedDevices { get; } = [];

        public List<CalcTravelRoute> TravelRoutes { get; } = [];

        public List<CalcTransportationDevice> AllMoveableDevices { get; } = [];

        // TODO: remove SameSiteRoutes if not needed anymore
        public Dictionary<CalcSite, CalcTravelRoute> SameSiteRoutes { get; } = [];

        public List<CalcTransportationDeviceCategory> DeviceCategories { get; } = [];

        /// maps the name of each AffordanceTaggingSet to the respective CalcAffordanceTaggingSetDto object
        public Dictionary<string, CalcAffordanceTaggingSetDto> AffordanceTaggingSets { get; } = [];

        /// <summary>
        /// Stores which transportation device (e.g., cars) is currently owned by which person
        /// </summary>
        public DeviceOwnershipMapping<string, CalcTransportationDevice> DeviceOwnerships { get; } = new();

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
            var allowedRoutes = CollectPossibleRoutes(startTimeStep, srcSite, dstSite, person, affordance);
            return SelectRoute(startTimeStep, person, calcRepo, allowedRoutes);
        }

        /// <summary>
        /// Determines all travel routes that match the requirements and are available for the specified situation.
        /// </summary>
        /// <param name="timeStep">the starting timestep of the desired travel</param>
        /// <param name="srcSite">the source site of the travel</param>
        /// <param name="dstSite">the destination site of the travel</param>
        /// <param name="person">the person who wants to travel</param>
        /// <param name="affordance">the target affordance for which the travel should be made; this is
        /// the source affordance of the transport decorator</param>
        /// <returns>the available travel routes</returns>
        /// <exception cref="LPGException">if source and destination are the same</exception>
        public List<CalcTravelRoute> CollectPossibleRoutes(TimeStep timeStep, CalcSite srcSite, CalcSite dstSite, CalcPersonDto person, ICalcAffordanceBase affordance)
        {
            if (srcSite == dstSite)
                throw new LPGException($"Source and destination of a travel must not be the same site ({srcSite}).");

            if (srcSite.DeviceChangeAllowed)
            {
                // person is not bound to a device anymore
                DeviceOwnerships.RemoveOwnership(person.Name);
            }
            //first get the routes, no matter if busy
            List<CalcTransportationDevice> devicesAtSrc = GetDevicesAtSite(srcSite);
            var possibleRoutes = srcSite.GetAllRoutesTo(timeStep, dstSite, devicesAtSrc, person);
            // filter routes based on the affordance tag
            var allowedRoutes = possibleRoutes.Where(route => IsRouteAllowedForAffordance(route, affordance)).ToList();
            return allowedRoutes;
        }

        /// <summary>
        /// Collects all movable devices that are currently at the given location.
        /// </summary>
        /// <param name="site">the site to collect devices from</param>
        /// <returns>the devices at the site</returns>
        public List<CalcTransportationDevice> GetDevicesAtSite(CalcSite site)
        {
            return [.. AllMoveableDevices.Where(x => x.Currentsite == site)];
        }

        /// <summary>
        /// Checks if the route can be used for traveling to carry out the specified affordance.
        /// This is used to define routes with different weights depending on the purpose of the travel.
        /// </summary>
        /// <param name="route">the route to check</param>
        /// <param name="affordance">the affordance that is the reason for traveling</param>
        /// <returns>true if the route is available for the travel; otherwhise, false</returns>
        public bool IsRouteAllowedForAffordance(CalcTravelRoute route, ICalcAffordanceBase affordance)
        {
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
        }

        /// <summary>
        /// Randomly selects a route out of the list, taking the respective weights into account.
        /// Depending on the selected TravelRouteSet, routes can be filtered based on the person or 
        /// the target affordance.
        /// </summary>
        /// <param name="startTimeStep">the timestep for starting the route</param>
        /// <param name="person">the person who wants to travel</param>
        /// <param name="calcRepo">the CaclRepo object</param>
        /// <param name="allowedRoutes">the list of possible routes to choose from</param>
        /// <returns>the selected route, or null if no route was feasible</returns>
        private CalcTravelRoute? SelectRoute(TimeStep startTimeStep, CalcPersonDto person, CalcRepo calcRepo, List<CalcTravelRoute> allowedRoutes)
        {
            if (allowedRoutes.Count == 0)
            {
                return null;
            }

            //check if the route is busy by calculating the duration. If busy, duration will be null
            int? dur = null;
            CalcTravelRoute? selectedRoute = null;
            while (dur == null && allowedRoutes.Count > 0)
            {
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
                dur = selectedRoute.GetDuration(startTimeStep, person, AllMoveableDevices);
            }

            if (dur == null)
            {
                // no usable route found
                selectedRoute = null;
            }

            return selectedRoute;
        }

        public void AddVehicleDepotDevice(CalcTransportationDevice dev)
        {
            VehicleDepot.Add(dev);
            AllMoveableDevices.Add(dev);
        }

        public void AddSite(CalcSite srcSite)
        {
            CalcSites.Add(srcSite);
        }

        public CalcTransportationDeviceCategory GetCategory(CalcTransportationDeviceCategoryDto catDto)
        {
            if (DeviceCategories.Any(x => x.Guid == catDto.Guid))
            {
                return DeviceCategories.Single(x => x.Guid == catDto.Guid);
            }
            CalcTransportationDeviceCategory ct = new CalcTransportationDeviceCategory(catDto.Name, catDto.IsLimitedToSingleLocation, catDto.Guid);
            DeviceCategories.Add(ct);
            return ct;
        }

        public void AddAffordanceTaggingSets(List<CalcAffordanceTaggingSetDto> affordanceTaggingSets)
        {
            foreach (var set in affordanceTaggingSets)
            {
                AffordanceTaggingSets.Add(set.Name, set);
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using JetBrains.Annotations;

namespace CalculationEngine.Transportation {
    public class TransportationHandler {
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcSite> CalcSites { get; }= new List<CalcSite>();
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcTransportationDevice> VehicleDepot { get; } = new List<CalcTransportationDevice>();
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcTransportationDevice> LocationUnlimitedDevices { get; } = new List<CalcTransportationDevice>();
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcTravelRoute> TravelRoutes { get; }= new List<CalcTravelRoute>();
        [JetBrains.Annotations.NotNull]
        private Dictionary<CalcLocation, CalcSite> LocationSiteLookup { get; } = new Dictionary<CalcLocation, CalcSite>();

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcTransportationDevice> AllMoveableDevices { get; } = new List<CalcTransportationDevice>();

        [JetBrains.Annotations.NotNull]
        public Dictionary<CalcSite, CalcTravelRoute> SameSiteRoutes { get; }= new Dictionary<CalcSite, CalcTravelRoute>();
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcTransportationDeviceCategory> DeviceCategories { get;  } = new List<CalcTransportationDeviceCategory>();

        public CalcTravelRoute? GetTravelRouteFromSrcLoc([JetBrains.Annotations.NotNull] CalcLocation srcLocation,
                                                         [JetBrains.Annotations.NotNull] CalcSite dstSite, [JetBrains.Annotations.NotNull] TimeStep startTimeStep,
                                                         [JetBrains.Annotations.NotNull] string personName, CalcRepo calcRepo)
        {
            CalcSite srcSite = LocationSiteLookup[srcLocation];
            if (srcSite == dstSite) {
                return SameSiteRoutes[srcSite];
            }
            //first get the routes, no matter if busy
            var devicesAtSrc = AllMoveableDevices.Where(x => x.Currentsite == srcSite).ToList();
            var possibleRoutes = srcSite.GetAllRoutesTo(dstSite,devicesAtSrc);
            if (possibleRoutes.Count == 0) {
                return null;
            }

            //check if the route is busy by calculating the duration. If busy, duration will be null
            int? dur = null;
            CalcTravelRoute? ctr = null;
            while (dur== null && possibleRoutes.Count > 0) {
                ctr = possibleRoutes[calcRepo.Rnd.Next(possibleRoutes.Count)];
                possibleRoutes.Remove(ctr);
                dur = ctr.GetDuration(startTimeStep, personName, AllMoveableDevices);
            }

            if (dur == null) {
                ctr = null;
            }

            return ctr;
        }

        public void AddVehicleDepotDevice([JetBrains.Annotations.NotNull] CalcTransportationDevice dev)
        {
            VehicleDepot.Add(dev);
            AllMoveableDevices.Add(dev);
        }

        public void AddSite([JetBrains.Annotations.NotNull] CalcSite srcSite)
        {
            CalcSites.Add(srcSite);
            foreach (CalcLocation location in srcSite.Locations) {
                LocationSiteLookup.Add(location,srcSite);
            }
        }

        [JetBrains.Annotations.NotNull]
        public CalcTransportationDeviceCategory GetCategory([JetBrains.Annotations.NotNull] CalcTransportationDeviceCategoryDto catDto)
        {
            if (DeviceCategories.Any(x => x.Guid == catDto.Guid)) {
                return DeviceCategories.Single(x => x.Guid == catDto.Guid);
            }
            CalcTransportationDeviceCategory ct = new CalcTransportationDeviceCategory(catDto.Name,catDto.IsLimitedToSingleLocation,catDto.Guid);
            DeviceCategories.Add(ct);
            return ct;
        }
    }
}
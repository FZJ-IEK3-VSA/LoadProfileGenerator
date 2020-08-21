using System.Collections.Generic;
using System.Linq;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
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

        public CalcTravelRoute? GetTravelRouteFromSrcLoc([NotNull] CalcLocation srcLocation,
                                                         [NotNull] CalcSite dstSite, [NotNull] TimeStep startTimeStep,
                                                         [NotNull] string personName, CalcRepo calcRepo)
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
    }
}
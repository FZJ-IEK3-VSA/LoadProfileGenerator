using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;

namespace CalculationEngine.Transportation
{
    public class CalcSite : CalcBase, ICalcSite
    {
        public CalcSite(string pName, bool deviceChangeAllowed, StrGuid guid, HouseholdKey householdKey) : base(pName, guid)
        {
            DeviceChangeAllowed = deviceChangeAllowed;
            _householdKey = householdKey;
        }

        public bool DeviceChangeAllowed { get; }

        private readonly HouseholdKey _householdKey;

        public List<CalcChargingStation> ChargingDevices { get; } = [];

        private readonly List<CalcLocation> locations = [];

        public IReadOnlyCollection<CalcLocation> Locations => locations.AsReadOnly();

        private List<CalcTravelRoute> MyRoutes { get; } = [];

        /// <summary>
        /// Adds a new location to the list of locations, and sets the CalcSite property of the location to this site.
        /// </summary>
        /// <param name="location">the location to add</param>
        public void AddLocation(CalcLocation location)
        {
            locations.Add(location);
            Debug.Assert(location.CalcSite is null, $"Location {location.Name} was added to multiple sites");
            location.CalcSite = this;
        }

        public void AddRoute(CalcTravelRoute route)
        {
            if (MyRoutes.Contains(route))
            {
                return;
            }
            MyRoutes.Add(route);
        }

        public bool AreCategoriesAvailable(List<CalcTransportationDeviceCategory> neededDeviceCategories,
              List<CalcTransportationDevice> vehiclepool, List<CalcTransportationDevice> devicesAtLoc,
             CalcPersonDto person, DeviceOwnershipMapping<string, CalcTransportationDevice> deviceOwnerships)
        {
            //TODO: check for fuel on each transportation device
            foreach (var neededDeviceCategory in neededDeviceCategories)
            {
                bool foundDevice = false;
                foreach (var deviceAtSite in devicesAtLoc)
                {
                    if (deviceAtSite.Category == neededDeviceCategory && deviceOwnerships.CanUse(person.Name, deviceAtSite))
                    {
                        foundDevice = true;
                    }
                }

                if (!foundDevice)
                {
                    foreach (var poolDevice in vehiclepool)
                    {
                        if (poolDevice.Category == neededDeviceCategory)
                        {
                            foundDevice = true;
                        }
                    }
                }

                if (!foundDevice)
                {
                    return false;
                }
            }

            return true;
        }



        public List<CalcTravelRoute> GetAllRoutesTo(ICalcSite dstSite, List<CalcTransportationDevice> devicesAtSrc,
             CalcPersonDto person, DeviceOwnershipMapping<string, CalcTransportationDevice> deviceOwnerships)
        {
            return MyRoutes.Where(x => x.IsAvailableRouteFor(this, dstSite, devicesAtSrc, person, deviceOwnerships)).ToList();
        }



        public List<CalcChargingStation> CollectChargingDevicesFor(CalcTransportationDeviceCategory category, CalcLoadType carLoadType)
        {
            return ChargingDevices.Where(x => x.DeviceCategory == category && x.CarChargingLoadType == carLoadType).ToList();
        }

        public void AddChargingStation(CalcLoadType gridchargingLoadType, CalcTransportationDeviceCategory cat,
            double chargingDeviceMaxChargingPower, CalcLoadType carChargingLoadType, CalcRepo calcRepo, BitArray isBusy)
        {
            string name = Name + " - Charging station " + (ChargingDevices.Count + 1);

            CalcChargingStation station = new CalcChargingStation(cat,
                gridchargingLoadType, chargingDeviceMaxChargingPower, name,
                System.Guid.NewGuid().ToStrGuid(), _householdKey, carChargingLoadType, calcRepo, isBusy);
            ChargingDevices.Add(station);
        }

        public bool IsSameCategory(ICalcSite other)
        {
            return this == other;
        }
    }
}
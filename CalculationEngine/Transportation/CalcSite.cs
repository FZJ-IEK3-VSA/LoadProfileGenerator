using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using JetBrains.Annotations;

namespace CalculationEngine.Transportation {
    public class CalcSite : CalcBase {
        public CalcSite([NotNull] string pName, StrGuid guid, [NotNull] HouseholdKey householdKey) : base(pName, guid)
        {
            _householdKey = householdKey;
        }

        [NotNull] private readonly HouseholdKey _householdKey;
        [NotNull]
        [ItemNotNull]
        public List<CalcChargingStation> ChargingDevices {get;} = new List<CalcChargingStation>();
        /*[JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcTransportationDevice> Devices { get; } = new List<CalcTransportationDevice>();*/
        [NotNull]
        [ItemNotNull]
        public List<CalcLocation> Locations { get; } = new List<CalcLocation>();
        [NotNull]
        [ItemNotNull]
        private List<CalcTravelRoute> MyRoutes { get; } = new List<CalcTravelRoute>();

        public void AddRoute([NotNull] CalcTravelRoute route)
        {
            if (MyRoutes.Contains(route)) {
                return;
            }
            MyRoutes.Add(route);
        }

        public bool AreCategoriesAvailable([NotNull][ItemNotNull] List<CalcTransportationDeviceCategory> neededDeviceCategories,
            [NotNull] [ItemNotNull] List<CalcTransportationDevice> vehiclepool,
                                           [ItemNotNull] [NotNull] List<CalcTransportationDevice> devicesAtLoc)
        {
            //TODO: check for fuel on each transportation device
            foreach (var neededDeviceCategory in neededDeviceCategories) {
                bool foundDevice = false;
                foreach (var deviceAtSite in devicesAtLoc) {
                    if (deviceAtSite.Category == neededDeviceCategory) {
                        foundDevice = true;
                    }
                }

                if (!foundDevice) {
                    foreach (var poolDevice in vehiclepool) {
                        if (poolDevice.Category == neededDeviceCategory) {
                            foundDevice = true;
                        }
                    }
                }

                if (!foundDevice) {
                    return false;
                }
            }

            return true;
        }

        /*[JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcTravelRoute> GetViableTrafficRoutes([JetBrains.Annotations.NotNull] CalcSite destination, int startTimeStep)
        {
            List<CalcTravelRoute> viableRoutes = new List<CalcTravelRoute>();
            foreach (CalcTravelRoute myRoute in MyRoutes) {
                if (myRoute.IsAvailableRouteFor(this, destination,startTimeStep,stopTimeStep)) {
                    viableRoutes.Add(myRoute);
                }
            }

            return viableRoutes;
        }*/

        [NotNull]
        [ItemNotNull]
        public List<CalcTravelRoute> GetAllRoutesTo([NotNull] CalcSite dstSite, [ItemNotNull] [NotNull] List<CalcTransportationDevice> devicesAtSrc)
        {
            return MyRoutes.Where(x => x.IsAvailableRouteFor(this, dstSite,devicesAtSrc)).ToList();
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcChargingStation> CollectChargingDevicesFor([NotNull] CalcTransportationDeviceCategory category, [NotNull] CalcLoadType carLoadType)
        {
            return ChargingDevices.Where(x => x.DeviceCategory == category && x.CarChargingLoadType == carLoadType).ToList();
        }

        public void AddChargingStation([NotNull] CalcLoadType gridchargingLoadType,
                                       [NotNull] CalcTransportationDeviceCategory cat,
                                       double chargingDeviceMaxChargingPower,
                                       [NotNull] CalcLoadType carChargingLoadType,
                                       CalcRepo calcRepo, BitArray isBusy)
        {
            string name = Name + " - Charging station " + (ChargingDevices.Count + 1);

            CalcChargingStation station = new CalcChargingStation(cat,
                gridchargingLoadType, chargingDeviceMaxChargingPower,name,
                System.Guid.NewGuid().ToStrGuid(),_householdKey, carChargingLoadType, calcRepo, isBusy);
            ChargingDevices.Add(station);
        }
    }
}
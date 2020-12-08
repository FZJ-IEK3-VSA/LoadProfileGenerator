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
        public CalcSite([JetBrains.Annotations.NotNull] string pName, StrGuid guid, [JetBrains.Annotations.NotNull] HouseholdKey householdKey) : base(pName, guid)
        {
            _householdKey = householdKey;
        }

        [JetBrains.Annotations.NotNull] private readonly HouseholdKey _householdKey;
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcChargingStation> ChargingDevices {get;} = new List<CalcChargingStation>();
        /*[JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcTransportationDevice> Devices { get; } = new List<CalcTransportationDevice>();*/
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcLocation> Locations { get; } = new List<CalcLocation>();
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<CalcTravelRoute> MyRoutes { get; } = new List<CalcTravelRoute>();

        public void AddRoute([JetBrains.Annotations.NotNull] CalcTravelRoute route)
        {
            if (MyRoutes.Contains(route)) {
                return;
            }
            MyRoutes.Add(route);
        }

        public bool AreCategoriesAvailable([JetBrains.Annotations.NotNull][ItemNotNull] List<CalcTransportationDeviceCategory> neededDeviceCategories,
            [JetBrains.Annotations.NotNull] [ItemNotNull] List<CalcTransportationDevice> vehiclepool,
                                           [ItemNotNull] [JetBrains.Annotations.NotNull] List<CalcTransportationDevice> devicesAtLoc)
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

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcTravelRoute> GetAllRoutesTo([JetBrains.Annotations.NotNull] CalcSite dstSite, [ItemNotNull] [JetBrains.Annotations.NotNull] List<CalcTransportationDevice> devicesAtSrc)
        {
            return MyRoutes.Where(x => x.IsAvailableRouteFor(this, dstSite,devicesAtSrc)).ToList();
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcChargingStation> CollectChargingDevicesFor([JetBrains.Annotations.NotNull] CalcTransportationDeviceCategory category, [JetBrains.Annotations.NotNull] CalcLoadType carLoadType)
        {
            return ChargingDevices.Where(x => x.DeviceCategory == category && x.CarChargingLoadType == carLoadType).ToList();
        }

        public void AddChargingStation([JetBrains.Annotations.NotNull] CalcLoadType gridchargingLoadType,
                                       [JetBrains.Annotations.NotNull] CalcTransportationDeviceCategory cat,
                                       double chargingDeviceMaxChargingPower,
                                       [JetBrains.Annotations.NotNull] CalcLoadType carChargingLoadType,
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
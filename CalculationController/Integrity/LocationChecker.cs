using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Helpers;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class LocationChecker : BasicChecker {
        public LocationChecker(bool performCleanupChecks) : base("Locations", performCleanupChecks) {
        }

        private static void CheckDevicesAtLocation([NotNull] Location location) {
            var devices = location.LocationDevices.Select(x => x.Device).Distinct().ToList();
            foreach (var device in devices) {
                var selectedLocationDevices =
                    location.LocationDevices.Where(x => x.Device == device).ToList();
                // make list of possible real devices
                var realDevices = new List<RealDevice>();
                foreach (var locDevice in selectedLocationDevices) {
                    if(locDevice.Device == null) {
                        throw new LPGException("Device was null");
                    }
                    if (locDevice.Device.AssignableDeviceType == AssignableDeviceType.Device) {
                        realDevices.Add(locDevice.RealDevice);
                        if(locDevice.LoadType == null) {
                            throw new DataIntegrityException("The location " + location.PrettyName + " has a device without load type. This is not correct. Please fix.",location );
                        }
                    }
                    else {
                        var dc = locDevice.DeviceCategory;
                        if(dc == null) {
                            throw new LPGException("Device Category was null");
                        }
                        realDevices.AddRange(dc.GetRealDevices());
                    }
                }
                // make list of the load types of these real devices
                var loadTypes = new List<VLoadType>();
                foreach (var realDevice in realDevices) {
                    foreach (var loads in realDevice.Loads) {
                        if (!loadTypes.Contains(loads.LoadType)) {
                            loadTypes.Add(loads.LoadType);
                        }
                    }
                }
            }
        }

        private static void CheckForDuplicateDevices([NotNull] Location location) {
            var alldevices = location.LocationDevices.Select(x => x.Device).ToList();
            var distinctdevices = alldevices.Distinct().ToList();
            if (alldevices.Count != distinctdevices.Count) {
                throw new DataIntegrityException("The location " + location.PrettyName + " has duplicate devices.",
                    location);
            }
        }

        private static void CheckUses([NotNull][ItemNotNull] ObservableCollection<Location> locations,
            [NotNull][ItemNotNull] ObservableCollection<HouseholdTrait> traits, [NotNull][ItemNotNull] ObservableCollection<HouseType> houseTypes) {
            var usedLocations = new List<Location>();
            foreach (var trait in traits) {
                foreach (var location in trait.Locations) {
                    usedLocations.Add(location.Location);
                }
            }
            foreach (var type in houseTypes) {
                foreach (var device in type.HouseDevices) {
                    usedLocations.Add(device.Location);
                }
            }
            foreach (var location in locations) {
                if (!usedLocations.Contains(location)) {
                    throw new DataIntegrityException(
                        "The Location " + location.Name + " is not used in any trait or household. Please fix.",
                        location);
                }
            }
        }

        protected override void Run(Simulator sim) {
            if (!PerformCleanupChecks) {
                return;
            }
            CheckUses(sim.Locations.Items, sim.HouseholdTraits.Items, sim.HouseTypes.Items);
            foreach (var location in sim.Locations.Items) {
                CheckForDuplicateDevices(location);
                CheckDevicesAtLocation(location);
            }
        }
    }
}
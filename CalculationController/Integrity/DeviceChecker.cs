using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class DeviceChecker : BasicChecker {
        public DeviceChecker(bool performCleanupChecks) : base("Devices", performCleanupChecks) {
        }

        private static void BasicChecks([JetBrains.Annotations.NotNull] RealDevice device) {
            if (device.Loads.Count == 0) {
                throw new DataIntegrityException(
                    "The device " + device.Name + " has no loads. This isn't going to work!", device);
            }
            if (device.DeviceCategory == null) {
                throw new DataIntegrityException(
                    "The device " + device.Name + " has no device category. This is not allowed!", device);
            }
            if (device.Year < 1980 || device.Year > 2030) {
                throw new DataIntegrityException(
                    "The device " + device.PrettyName +
                    " has a year that is smaller than 1980 or larger than 2030. Please fix.", device);
            }
        }

        private static void CheckDeviceActionLoads([JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> actions, [JetBrains.Annotations.NotNull] RealDevice device) {
            foreach (var action in actions) {
                if (action.Device == device) {
                    foreach (var profile in action.Profiles) {
                        if (profile.Timeprofile?.TimeProfileType == TimeProfileType.Absolute) {
                            for (var i = 0; i < device.Loads.Count; i++) {
                                var load = device.Loads[i];
                                if (load.LoadType == profile.VLoadType) {
                                    var newMax = profile.Timeprofile.Maxiumum * profile.Multiplier;
                                    if (newMax > load.MaxPower) {
                                        if (profile.VLoadType == null) {
                                            throw new LPGException("Loadtype was null");
                                        }
                                        /*throw new DataIntegrityException("The device " + device.Name + " has a maximum power of " + load.MaxPower +
                                        " " + load.LoadType.UnitOfPower + "  for " +
                                        load.LoadType + " which is less than the " +
                                        "maxmium of " + profile.Timeprofile.Maxiumum.ToString("N2", CultureInfo.CurrentCulture) + " " +
                                        load.LoadType.UnitOfPower +
                                        " in the profile " + profile.Timeprofile.Name +
                                        " from the device action " + action.Name + ". Please fix.", device)*/
                                        var roundedmax = Math.Ceiling(newMax * 10) / 10.0;
                                        var mbr =
                                            MessageWindowHandler.Mw.ShowYesNoMessage(
                                                "The device " + device.Name + " has a maximum power of " +
                                                load.MaxPower +
                                                " " + load.LoadType?.UnitOfPower + "  for " + load.LoadType +
                                                " which is less than the " + "maximum of " +
                                                newMax.ToString("N2", CultureInfo.CurrentCulture) + " " +
                                                load.LoadType?.UnitOfPower + " in the profile " +
                                                profile.Timeprofile.Name + " from the device action \"" + action.Name +
                                                "\". Set to " + roundedmax + "?", "Change?");
                                        if (mbr == LPGMsgBoxResult.Yes) {
                                            device.AddLoad(profile.VLoadType, roundedmax, load.StandardDeviation,
                                                load.AverageYearlyConsumption);
                                            i = 0;
                                        }

                                        if (mbr == LPGMsgBoxResult.No) {
                                            List<BasicElement> elements = new List<BasicElement>();
                                            elements.Add(action);
                                            elements.Add(device);
                                            throw new DataIntegrityException("Please fix.",elements);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void CheckForUnusedDevices([JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<RealDevice> realDevices,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Affordance> affordances, [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> actions,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Location> locations,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<HouseholdTrait> traits, [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<HouseType> houses) {
            if (!PerformCleanupChecks) {
                return;
            }
            var count = 0;

            var allUsedDevices = new HashSet<RealDevice>();
            foreach (var affordance in affordances) {
                foreach (var device in affordance.AffordanceDevices) {
                    var devices = device.Device?.GetRealDevices(actions);
                    devices?.ForEach(x => {
                        if (!allUsedDevices.Contains(x)) {
                            allUsedDevices.Add(x);
                        }
                    });
                }
                foreach (var std in affordance.AffordanceStandbys) {
                    if(std.Device == null) {
                        throw new DataIntegrityException("Device was null");
                    }
                    var stds = std.Device.GetRealDevices(actions);
                    stds.ForEach(x => {
                        if (!allUsedDevices.Contains(x)) {
                            allUsedDevices.Add(x);
                        }
                    });
                }
            }
            foreach (var location in locations) {
                foreach (var device in location.LocationDevices) {
                    if(device.Device == null) {
                        throw new LPGException("Device was null");
                    }
                    var devices = device.Device.GetRealDevices(actions);
                    devices.ForEach(x => {
                        if (!allUsedDevices.Contains(x)) {
                            allUsedDevices.Add(x);
                        }
                    });
                }
            }
            foreach (var trait in traits) {
                foreach (var device in trait.Autodevs) {
                    var devices = device.Device?.GetRealDevices(actions);
                    if (devices != null) {
                        devices.ForEach(x => {
                            if (!allUsedDevices.Contains(x)) {
                                allUsedDevices.Add(x);
                            }
                        });
                    }
                }
            }
            foreach (var house in houses) {
                foreach (var device in house.HouseDevices) {
                    if (device.Device == null) {
                        throw new DataIntegrityException("Device was null");
                    }
                    var devices = device.Device.GetRealDevices(actions);
                    devices.ForEach(x => {
                        if (!allUsedDevices.Contains(x)) {
                            allUsedDevices.Add(x);
                        }
                    });
                }
            }

            foreach (var device in realDevices) {
                if (!allUsedDevices.Contains(device)) {
                    count++;
                    Logger.Error("found unused device " + count + ": " + device.Name);
                    throw new DataIntegrityException(
                        "Unused device " + device.Name +
                        ". It has not a single affordance associated with it. Please fix.", device);
                }
            }
        }

        private static void ElectricityChecks([JetBrains.Annotations.NotNull] RealDevice device) {
            if (device.Loads.Any(x => string.Equals(x.LoadType?.Name, "electricity",
                StringComparison.OrdinalIgnoreCase))) {
                if (
                    !device.Loads.Any(
                        x => string.Equals(x.LoadType?.Name, "apparent", StringComparison.OrdinalIgnoreCase))) {
                    throw new DataIntegrityException(
                        "The device " + device.PrettyName +
                        " is electric and missing the load type apparent. Please fix.", device);
                }
                if (
                    !device.Loads.Any(
                        x => string.Equals(x.LoadType?.Name, "reactive", StringComparison.OrdinalIgnoreCase))) {
                    throw new DataIntegrityException(
                        "The device " + device.PrettyName +
                        " is electric and missing the load type reactive. Please fix.", device);
                }
                var rdload =
                    device.Loads.First(
                        x => string.Equals(x.LoadType?.Name, "electricity", StringComparison.OrdinalIgnoreCase));
                var appLoad =
                    device.Loads.First(
                        x => string.Equals(x.LoadType?.Name, "apparent", StringComparison.OrdinalIgnoreCase));

                if (Math.Abs(appLoad.MaxPower - rdload.MaxPower) < Constants.Ebsilon ||
                    Math.Abs(appLoad.MaxPower) < Constants.Ebsilon || appLoad.MaxPower < rdload.MaxPower) {
                    throw new DataIntegrityException(
                        "The device " + device.Name + " has an invalid reactive power. Please fix.", device);
                }

                var reaLoad =
                    device.Loads.First(
                        x => string.Equals(x.LoadType?.Name, "reactive", StringComparison.OrdinalIgnoreCase));
                if (Math.Abs(reaLoad.MaxPower - rdload.MaxPower) < Constants.Ebsilon ||
                    Math.Abs(reaLoad.MaxPower) < Constants.Ebsilon || reaLoad.MaxPower > appLoad.MaxPower) {
                    throw new DataIntegrityException(
                        "The device " + device.Name + " has an invalid reactive power. Please fix.", device);
                }
            }
        }

        protected override void Run(Simulator sim, CheckingOptions options) {
            CheckForUnusedDevices(sim.RealDevices.Items, sim.Affordances.Items, sim.DeviceActions.Items, sim.Locations.Items,
                sim.HouseholdTraits.Items, sim.HouseTypes.Items);
            foreach (var device in sim.RealDevices.Items) {
                BasicChecks(device);
                if (PerformCleanupChecks) {
                    ElectricityChecks(device);
                    CheckDeviceActionLoads(sim.DeviceActions.Items, device);
                }
            }
            if(PerformCleanupChecks){
                List<RealDevice> devicesWithMissingLt = new List<RealDevice>();
                foreach (var device in sim.RealDevices.Items) {
                    RealDeviceLoadType electricityLoad = null;
                    RealDeviceLoadType innerLoad = null;
                    foreach (var load in device.Loads) {
                        if (load.LoadType?.Name == "Electricity") {
                            electricityLoad = load;
                        }

                        if (load.LoadType?.Name.Contains("Inner") == true) {
                            innerLoad = load;
                        }
                    }

                    if (electricityLoad != null && innerLoad == null) {
                        devicesWithMissingLt.Add(device);
                    }

                    if (devicesWithMissingLt.Count > 10) {
                        break;
                    }
                }

                if (devicesWithMissingLt.Count > 0) {
                    throw new DataIntegrityException("Found devices with electricity that don't have inner heat gains set.", devicesWithMissingLt.Select(x=>(BasicElement) x).ToList());
                }
            }
        }
    }
}
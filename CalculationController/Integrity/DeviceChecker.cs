using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
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

        private static void BasicChecks([NotNull] RealDevice device) {
            if (device.Loads.Count == 0) {
                throw new DataIntegrityException(
                    "The device " + device.Name + " has no loads. This isn't going to work!", device);
            }
            if (device.DeviceCategory == null) {
                throw new DataIntegrityException(
                    "The device " + device.Name + " has no device category. This is not allowed!", device);
            }
            if (device.Year < 1980 || device.Year > 2020) {
                throw new DataIntegrityException(
                    "The device " + device.PrettyName +
                    " has a year that is smaller than 1980 or larger than 2020. Please fix.", device);
            }
        }

        private static void CheckDeviceActionLoads([NotNull][ItemNotNull] ObservableCollection<DeviceAction> actions, [NotNull] RealDevice device) {
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
                                            MessageWindows.ShowYesNoMessage(
                                                "The device " + device.Name + " has a maximum power of " +
                                                load.MaxPower +
                                                " " + load.LoadType?.UnitOfPower + "  for " + load.LoadType +
                                                " which is less than the " + "maximum of " +
                                                newMax.ToString("N2", CultureInfo.CurrentCulture) + " " +
                                                load.LoadType?.UnitOfPower + " in the profile " +
                                                profile.Timeprofile.Name + " from the device action \"" + action.Name +
                                                "\". Set to " + roundedmax + "?", "Change?");
                                        if (mbr == MessageBoxResult.Yes) {
                                            device.AddLoad(profile.VLoadType, roundedmax, load.StandardDeviation,
                                                load.AverageYearlyConsumption);
                                            i = 0;
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
        private void CheckForUnusedDevices([NotNull][ItemNotNull] ObservableCollection<RealDevice> realDevices,
            [NotNull][ItemNotNull] ObservableCollection<Affordance> affordances, [NotNull][ItemNotNull] ObservableCollection<DeviceAction> actions,
            [NotNull][ItemNotNull] ObservableCollection<Location> locations,
            [NotNull][ItemNotNull] ObservableCollection<HouseholdTrait> traits, [NotNull][ItemNotNull] ObservableCollection<HouseType> houses) {
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

        private static void ElectricityChecks([NotNull] RealDevice device) {
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

        protected override void Run([NotNull] Simulator sim) {
            CheckForUnusedDevices(sim.RealDevices.It, sim.Affordances.It, sim.DeviceActions.It, sim.Locations.It,
                sim.HouseholdTraits.It, sim.HouseTypes.It);
            foreach (var device in sim.RealDevices.It) {
                BasicChecks(device);
                if (PerformCleanupChecks) {
                    ElectricityChecks(device);
                    CheckDeviceActionLoads(sim.DeviceActions.It, device);
                }
            }
        }
    }
}
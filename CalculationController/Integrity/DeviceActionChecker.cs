using System;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    public class DeviceActionChecker : BasicChecker {
        [CanBeNull] private TimeBasedProfile _placeholder;

        public DeviceActionChecker(bool performCleanupChecks) : base("Device Actions", performCleanupChecks) {
        }

        private static void CheckDeviceActionGroup([NotNull] DeviceAction action) {
            if (action.DeviceActionGroup == null) {
                throw new DataIntegrityException(
                    "The device action " + action.Name + " is missing a device action group. Please fix.", action);
            }
        }

        private void CheckElectricity([NotNull] DeviceAction action) {
            var elec = action.Profiles.FirstOrDefault(p => p.VLoadType?.Name == "Electricity");
            if (elec != null) {
                var apparent = action.Profiles.FirstOrDefault(p => p.VLoadType?.Name == "Apparent");
                var reactive = action.Profiles.FirstOrDefault(p => p.VLoadType?.Name == "Reactive");
                if (PerformCleanupChecks) {
                    if (apparent == null) {
                        throw new DataIntegrityException("No apparent power. Please fix.", action);
                    }
                    if (reactive == null) {
                        throw new DataIntegrityException("No reactive power. Please fix.", action);
                    }
                    if (apparent.Timeprofile?.Name.ToUpperInvariant().Contains("REACTIVE")== true) {
                        throw new DataIntegrityException("Switched apparent and reactive profiles!", action);
                    }
                    if (elec.Timeprofile == _placeholder || apparent.Timeprofile == _placeholder ||
                        reactive.Timeprofile == _placeholder) {
                        throw new DataIntegrityException(
                            "The device action " + action.Name + " has place holder profiles! Please fix!", action);
                    }

                    if (elec.Timeprofile?.TimeProfileType == TimeProfileType.Absolute) {
                        if (apparent.Timeprofile == elec.Timeprofile && Math.Abs(apparent.Multiplier - 1) < 0.000001) {
                            throw new DataIntegrityException(
                                "Can't have the same measured profile for apparent and electricity", action);
                        }
                        if (reactive.Timeprofile == elec.Timeprofile &&
                            Math.Abs(reactive.Multiplier - 1) < Constants.Ebsilon) {
                            throw new DataIntegrityException(
                                "Can't have the same measured profile for reactive and electricity", action);
                        }
                    }
                    if (reactive.Timeprofile?.Name.ToUpperInvariant().Contains("APPARENT")==true) {
                        throw new DataIntegrityException("Switched apparent and reactive profiles!", action);
                    }
                }
            }
        }

        private static void CheckforAllPlaceholders([NotNull] DeviceAction action) {
            var areAllPlaceholder = true;
            foreach (var profile in action.Profiles) {
                if (profile.Timeprofile?.Name.ToUpperInvariant().Contains("PLACEHOLDER") == false) {
                    areAllPlaceholder = false;
                }
            }
            if (areAllPlaceholder) {
                if (!action.Description.Contains("Place holder profile on purpose")) {
                    throw new DataIntegrityException(
                        "The device action " + action.Name +
                        " has only placeholder profiles. This is pointless. Please fix.", action);
                }
            }
        }

        private static void CheckForDuplicateTimeProfiles([NotNull] DeviceAction action) {
            foreach (var profile1 in action.Profiles) {
                foreach (var profile2 in action.Profiles) {
                    if (profile1 != profile2) {
                        if (profile1.VLoadType == profile2.VLoadType && profile1.TimeOffset == profile2.TimeOffset) {
                            throw new DataIntegrityException(
                                "The device action " + action.Name +
                                " has the same load type twice with the same time offset. Please fix.", action);
                        }
                    }
                }
            }
        }

        private void CheckForUsage([NotNull] DeviceAction action, [NotNull] Simulator sim) {
            if (!PerformCleanupChecks) {
                return;
            }
            var usedIns = action.CalculateUsedIns(sim);
            if (usedIns.Count == 0) {
                throw new DataIntegrityException(
                    "It seems the device action " + action.Name +
                    " is not used in a single affordance. Please fix or delete.", action);
            }
        }

        private static void CheckValidLoadtypesOnDeviceAction([NotNull] DeviceAction da) {
            if (da.Device == null) {
                throw new DataIntegrityException(
                    "The Device Action " + da.Name + " has no device selected! Please fix.", da);
            }
            // check if all the load types on the realdevice are set on the deviceaction
            if (da.Device.ForceAllLoadTypesToBeSet) {
                foreach (var load in da.Device.Loads) {
                    var hasDeviceActionSet = da.Profiles.Any(prof => prof.VLoadType == load.LoadType);
                    if (!hasDeviceActionSet) {
                        throw new DataIntegrityException(
                            "The device " + da.Device.Name + " has the load type " + load.LoadType?.Name +
                            " set, but the device action " + da.Name + " is missing that load type. Please fix.", da);
                    }
                }
            }

            foreach (var profile in da.Profiles) {
                var hasRealDevice = da.Device.Loads.Any(load => load.LoadType == profile.VLoadType);
                if (!hasRealDevice) {
                    throw new DataIntegrityException(
                        "The device action " + da.Name + " has set the load type " + profile.VLoadType +
                        " but the device is missing that load type. Please fix.", da);
                }
            }
        }

        protected override void Run(Simulator sim, CheckingOptions options) {
            var placeholder = sim.Timeprofiles.FindFirstByName("placeholder", FindMode.Partial);
            _placeholder = placeholder ?? throw new LPGException("Placeholder was null");
            foreach (var action in sim.DeviceActions.Items) {
                CheckForUsage(action, sim);
                CheckElectricity(action);
                CheckDeviceActionGroup(action);
                CheckValidLoadtypesOnDeviceAction(action);
                CheckForDuplicateTimeProfiles(action);
                CheckforAllPlaceholders(action);
            }
        }
    }
}
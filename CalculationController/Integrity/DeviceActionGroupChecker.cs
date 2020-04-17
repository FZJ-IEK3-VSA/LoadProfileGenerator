using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class DeviceActionGroupChecker : BasicChecker {
        public DeviceActionGroupChecker(bool performCleanupChecks) :
            base("Device Action Groups", performCleanupChecks) {
        }

        private static void CheckAndDeleteEmptyDeviceActionGroups([NotNull] Simulator sim) {
            // do both checks here to avoid doing the getdeviceactions twice
            for (var index = 0; index < sim.DeviceActionGroups.It.Count; index++) {
                var actionGroup = sim.DeviceActionGroups[index];
                var actions = actionGroup.GetDeviceActions(sim.DeviceActions.It);
                if (actions.Count > 0) {
                    if (actions[0].Device == null)
                    {
                        throw new LPGException("Device was null");
                    }

                    var category = actions[0].Device.DeviceCategory;
                    foreach (var action in actions) {
                        if (action.Device== null)
                        {
                            throw new LPGException("Device was null");
                        }
                        if (action.Device.DeviceCategory != category) {
                            throw new DataIntegrityException(
                                "The device of the device action " + action.Name +
                                " has a different device category from the other devices in the group. " +
                                "The other category is " + category?.Name + ", this device action has " +
                                action.Device.DeviceCategory + ". Please fix.", actionGroup);
                        }
                    }
                }
                else {
                    if (!Config.IsInUnitTesting ) {
                        var mgr =
                            MessageWindowHandler.Mw.ShowYesNoMessage(
                                "The device action group " + actionGroup.Name +
                                " has not a single device action. Delete?",
                                "Delete?");
                        if (mgr == LPGMsgBoxResult.Yes) {
                            var index1 = index;
                            Logger.Get()
                                .SafeExecuteWithWait(
                                    () => sim.DeviceActionGroups.DeleteItem(sim.DeviceActionGroups[index1]));
                            index = 0;
                        }
                        else {
                            throw new DataIntegrityException(
                                "The device action group " + actionGroup.Name +
                                " has not a single device action. Please fix or delete!", actionGroup);
                        }
                    }
                    else {
                            throw new DataIntegrityException(
                                "The device action group " + actionGroup.Name +
                                " has not a single device action. Please fix or delete!", actionGroup);
                    }
                }
            }
        }

        private void CheckDeviceActionsAllInSameCategory([NotNull] DeviceActionGroup actionGroup,
            [NotNull][ItemNotNull] ObservableCollection<DeviceAction> allActions, [NotNull] Simulator sim) {
            // collect all used groups

            var devicesInActions = actionGroup.GetRealDevices(allActions);
            if (devicesInActions.Count == 0) {
                throw new LPGException("Bug in device action check. Please report!");
            }
            var cat = devicesInActions[0].DeviceCategory;
            if (devicesInActions[0].DeviceCategory == null)
            {
                throw new LPGException("Device Category was null");
            }
            var devicesInCategory = devicesInActions[0].DeviceCategory.GetRealDevices(allActions);
            if (cat == null)
            {
                throw new LPGException("Category was null");
            }
            foreach (var realDevice in devicesInActions) {
                if (!devicesInCategory.Contains(realDevice)) {
                    FindMissingDevice(cat, actionGroup.GetDeviceActions(allActions), actionGroup, sim);
                }
            }
            foreach (var realDevice in devicesInCategory) {
                if (!devicesInActions.Contains(realDevice)) {
                    FindMissingDevice(cat, actionGroup.GetDeviceActions(allActions), actionGroup, sim);
                }
            }
        }

        private void CheckGroupUsage([NotNull] Simulator sim, [NotNull] DeviceActionGroup actionGroup) {
            if (!PerformCleanupChecks) {
                return;
            }

            var result = actionGroup.CalculateUsedIns(sim);
            if (result.Count == 0) {
                throw new DataIntegrityException(
                    "The device action group " + actionGroup + " is not used. Please fix or delete!", actionGroup);
            }
        }

        private void FindMissingDevice([NotNull] DeviceCategory category, [NotNull][ItemNotNull] List<DeviceAction> actionsInGroup,
            [NotNull] DeviceActionGroup deviceActionGroup, [NotNull] Simulator sim) {
            if (!PerformCleanupChecks) {
                return;
            }
            foreach (var device in category.SubDevicesWithoutRefresh) {
                var action = actionsInGroup.FirstOrDefault(a => a.Device == device);

                if (action == null) {
                    DeviceAction a;
                    Logger.Get().SafeExecuteWithWait(() => {
                        a = device.MakeDeviceAction(sim);
                        a.DeviceActionGroup = deviceActionGroup;
                        var other = actionsInGroup.FirstOrDefault();
                        if (other != null) {
                            foreach (var profile in other.Profiles) {
                                if(profile.Timeprofile == null) {
                                    throw  new LPGException("Time Profile was null");
                                }
                                if(profile.VLoadType == null) {
                                    throw new LPGException("LoadType was null");
                                }
                                a.AddDeviceProfile(profile.Timeprofile, profile.TimeOffset, profile.VLoadType,
                                    profile.Multiplier);
                            }
                        }
                        a.SaveToDB();
                    });
                    throw new DataIntegrityException(
                        "A device action group always needs to have all the devices in a device category." +
                        "The device action group "+ Environment.NewLine + deviceActionGroup +
                        Environment.NewLine +" is missing at least one device. This device is " + Environment.NewLine + device.Name +
                        Environment.NewLine+" from the device category " + Environment.NewLine + device.DeviceCategory?.Name + Environment.NewLine+ Environment.NewLine +
                        "A new device action was created for it. Please check if everything is correct and adjust as needed.",
                        deviceActionGroup);
                }
            }
        }

        protected override void Run([NotNull] Simulator sim) {
            CheckAndDeleteEmptyDeviceActionGroups(sim);
            foreach (var deviceActionGroup in sim.DeviceActionGroups.It) {
                CheckGroupUsage(sim, deviceActionGroup);
                CheckDeviceActionsAllInSameCategory(deviceActionGroup, sim.DeviceActions.It, sim);
            }
        }
    }
}
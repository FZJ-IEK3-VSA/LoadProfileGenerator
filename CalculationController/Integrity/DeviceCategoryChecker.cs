using System.Collections.Generic;
using Automation.ResultFiles;
using Common;
using Database;

namespace CalculationController.Integrity {
    internal class DeviceCategoryChecker : BasicChecker {
        public DeviceCategoryChecker(bool performCleanupChecks) : base("Device Categories", performCleanupChecks) {
        }

        protected override void Run(Simulator sim) {
            var categoryShortNames = new List<string>();
            if (sim.DeviceCategories.DeviceCategoryNone == null) {
                throw new LPGException("Device category none was null.");
            }
            var none = sim.DeviceCategories.DeviceCategoryNone;
            if (none.SubDevices.Count > 0) {
                throw new DataIntegrityException(
                    "There are devices with the device category none: " + none.SubDevices[0] + ". Please fix.",
                    none.SubDevices[0]);
            }
            foreach (var category in sim.DeviceCategories.Items) {
                if (categoryShortNames.Contains(category.ShortName.ToUpperInvariant())) {
                    throw new DataIntegrityException(
                        "There are two device AffordanceToCategories with the name " + category.ShortName +
                        ". Please fix.", category);
                }
                categoryShortNames.Add(category.ShortName.ToUpperInvariant());
                var isStandby = false;
                foreach (var realDevice in category.SubDevicesWithoutRefresh) {
                    if (realDevice.IsStandbyDevice) {
                        isStandby = true;
                        break;
                    }
                }
                foreach (var realDevice in category.SubDevicesWithoutRefresh) {
                    if (isStandby) {
                        if (!realDevice.IsStandbyDevice) {
                            throw new DataIntegrityException(
                                "The device " + realDevice.Name +
                                " is not a standby device, but other devices in the device category " + category +
                                " are. Please fix!", realDevice);
                        }
                    }
                }
                // only cleanup checks
                if (!PerformCleanupChecks) {
                    continue;
                }

                if (category.Children.Count == 0 && category.SubDevicesWithoutRefresh.Count == 0 &&
                    category != none) {
                    throw new DataIntegrityException(
                        "The device category " + category.Name +
                        " seems to be completely empty. It has neither devices nor subcategories. Please fix or delete.",
                        category);
                }
            }
        }
    }
}
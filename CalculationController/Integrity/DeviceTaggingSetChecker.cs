﻿using System.Linq;
using Database;

namespace CalculationController.Integrity {
    internal class DeviceTaggingSetChecker : BasicChecker {
        public DeviceTaggingSetChecker(bool performCleanupChecks) : base("Device Tagging Sets", performCleanupChecks) {
        }

        protected override void Run(Simulator sim, CheckingOptions options) {
            foreach (var set in sim.DeviceTaggingSets.Items) {
                var isrefrehsed = false;
                foreach (var entry in set.Entries) {
                    if (entry.Device == null) {
                        set.RefreshDevices(sim.RealDevices.Items);
                        isrefrehsed = true;
                        break;
                    }
                }
                if (!isrefrehsed) {
                    foreach (var affordance in sim.RealDevices.Items) {
                        var entry =
                            set.Entries.FirstOrDefault(myentry => myentry.Device?.Name == affordance.Name);
                        if (entry == null) {
                            set.RefreshDevices(sim.RealDevices.Items);
                            break;
                        }
                    }
                }
            }
        }
    }
}
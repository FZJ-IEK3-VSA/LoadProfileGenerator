using System.Linq;
using Database;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class DeviceTaggingSetChecker : BasicChecker {
        public DeviceTaggingSetChecker(bool performCleanupChecks) : base("Device Tagging Sets", performCleanupChecks) {
        }

        protected override void Run([NotNull] Simulator sim) {
            foreach (var set in sim.DeviceTaggingSets.It) {
                var isrefrehsed = false;
                foreach (var entry in set.Entries) {
                    if (entry.Device == null) {
                        set.RefreshDevices(sim.RealDevices.It);
                        isrefrehsed = true;
                        break;
                    }
                }
                if (!isrefrehsed) {
                    foreach (var affordance in sim.RealDevices.It) {
                        var entry =
                            set.Entries.FirstOrDefault(myentry => myentry.Device?.Name == affordance.Name);
                        if (entry == null) {
                            set.RefreshDevices(sim.RealDevices.It);
                            break;
                        }
                    }
                }
            }
        }
    }
}
using System.Linq;
using Common;
using Database;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class DeviceSelectionChecker : BasicChecker {
        public DeviceSelectionChecker(bool performCleanupChecks) : base("Device Selections", performCleanupChecks) {
        }

        protected override void Run([NotNull] Simulator sim) {
            foreach (var deviceSelection in sim.DeviceSelections.It) {
                var devices = deviceSelection.Items.Select(x => x.DeviceCategory).ToList();
                var distinctDevices = devices.Distinct();
                if (devices.Count != distinctDevices.Count()) {
                    throw new DataIntegrityException(
                        "Duplicate choices for the same device category in the device selection " +
                        deviceSelection.PrettyName, deviceSelection);
                }
            }
        }
    }
}
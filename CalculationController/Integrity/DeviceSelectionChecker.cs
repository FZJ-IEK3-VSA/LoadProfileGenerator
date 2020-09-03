using System.Linq;
using Common;
using Database;

namespace CalculationController.Integrity {
    internal class DeviceSelectionChecker : BasicChecker {
        public DeviceSelectionChecker(bool performCleanupChecks) : base("Device Selections", performCleanupChecks) {
        }

        protected override void Run(Simulator sim, CheckingOptions options) {
            foreach (var deviceSelection in sim.DeviceSelections.Items) {
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
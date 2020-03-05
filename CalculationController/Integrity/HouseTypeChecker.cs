using Common;
using Database;
using Database.Tables.Houses;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class HouseTypeChecker : BasicChecker {
        public HouseTypeChecker(bool performCleanupChecks) : base("House Types", performCleanupChecks) {
        }

        private static void CheckStandbyDevicesHouses([NotNull] HouseType ht) {
            foreach (var hhAutonomousDevice in ht.HouseDevices) {
                if (hhAutonomousDevice.Location == null) {
                    throw new DataIntegrityException(
                        "In the house type " + ht.Name + " the autonomous device " + hhAutonomousDevice.Name +
                        " has no location. Please fix.", ht);
                }
                if (hhAutonomousDevice.TimeLimit == null)
                {
                    throw new DataIntegrityException(
                        "In the house type " + ht.Name + " the autonomous device " + hhAutonomousDevice.Name +
                        " has no time limit. Please fix.", ht);
                }
            }
        }

        protected override void Run([NotNull] Simulator sim) {
            foreach (var houseType in sim.HouseTypes.It) {
                CheckStandbyDevicesHouses(houseType);
                if (houseType.MinimumHouseholdCount == 0 || houseType.MaximumHouseholdCount == 0) {
                    throw new DataIntegrityException(
                        "Please check the minimum / maximum household count in the house type " + houseType.PrettyName +
                        ". 0 is not allowed.", houseType);
                }
            }
        }
    }
}
using Common;
using Database;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class HouseChecker : BasicChecker {
        public HouseChecker(bool performCleanupChecks) : base("Houses", performCleanupChecks) {
        }

        protected override void Run([NotNull] Simulator sim) {
            foreach (var house in sim.Houses.It) {
                if (Config.AllowEmptyHouses) {
                    if (house.Households.Count == 0) {
                        throw new DataIntegrityException("The house " + house.PrettyName + " has no households. Please fix.", house);
                    }
                }

                if (house.HouseType == null) {
                    throw new DataIntegrityException(
                        "The house " + house.PrettyName + " has no housetype. Please fix.", house);
                }
                if (house.GeographicLocation == null) {
                    throw new DataIntegrityException(
                        "The house " + house.PrettyName + " has no geographic Location. Please fix.", house);
                }
                if (house.TemperatureProfile == null) {
                    throw new DataIntegrityException(
                        "The house " + house.PrettyName + " has no temperature profile. Please fix.", house);
                }
            }
        }
    }
}
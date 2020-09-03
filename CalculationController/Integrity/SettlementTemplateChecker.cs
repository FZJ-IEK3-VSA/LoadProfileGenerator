using Common;
using Database;

namespace CalculationController.Integrity {
    internal class SettlementTemplateChecker : BasicChecker {
        public SettlementTemplateChecker(bool performCleanupChecks) :
            base("Settlement Templates", performCleanupChecks) {
        }

        protected override void Run(Simulator sim, CheckingOptions options) {
            foreach (var settemp in sim.SettlementTemplates.Items) {
                if (string.IsNullOrEmpty(settemp.NewName)) {
                    throw new DataIntegrityException(
                        "The settlement template " + settemp.Name + " has no name for the new settlements. Please fix.",
                        settemp);
                }
                if (settemp.GeographicLocation == null) {
                    throw new DataIntegrityException(
                        "The settlement template " + settemp.Name +
                        " has no geographic location for the new settlements. Please fix.", settemp);
                }
                if (settemp.TemperatureProfile == null) {
                    throw new DataIntegrityException(
                        "The settlement template " + settemp.Name +
                        " has no temperature profile for the new settlements. Please fix.", settemp);
                }
                if (settemp.HouseSizes.Count == 0) {
                    throw new DataIntegrityException(
                        "The settlement template " + settemp.Name + " has no house sizes defined. Please fix.",
                        settemp);
                }
                if (settemp.HouseholdDistributions.Count == 0) {
                    throw new DataIntegrityException(
                        "The settlement template " + settemp.Name +
                        " has no household distribution defined. Please fix.", settemp);
                }
                if (settemp.HouseTypes.Count == 0) {
                    throw new DataIntegrityException(
                        "The settlement template " + settemp.Name + " has no house types defined. Please fix.",
                        settemp);
                }

                if (PerformCleanupChecks) {
                    if (settemp.HouseholdTemplates.Count == 0) {
                        throw new DataIntegrityException("The settlement template " + settemp.Name + " has no household templates.", settemp);
                    }
                    if (settemp.ChargingStationSets.Count == 0)
                    {
                        throw new DataIntegrityException("Can't have a settlement with 0 charging stations.", settemp);
                    }
                    if (settemp.TransportationDeviceSets.Count == 0)
                    {
                        throw new DataIntegrityException("Can't have a settlement template with 0 transportation device sets.", settemp);
                    }
                    if (settemp.TravelRouteSets.Count == 0)
                    {
                        throw new DataIntegrityException("Can't have a settlement with 0 travel route sets.", settemp);
                    }
                }
            }
        }
    }
}
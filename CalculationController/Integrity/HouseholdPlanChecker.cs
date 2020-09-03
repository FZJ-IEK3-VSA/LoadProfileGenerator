using Common;
using Database;

namespace CalculationController.Integrity {
    internal class HouseholdPlanChecker : BasicChecker {
        public HouseholdPlanChecker(bool performCleanupChecks) : base("Household Plans", performCleanupChecks) {
        }

        protected override void Run(Simulator sim, CheckingOptions options) {
            foreach (var householdPlan in sim.HouseholdPlans.Items) {
                if (householdPlan.AffordanceTaggingSet == null) {
                    throw new DataIntegrityException(
                        "The household plan " + householdPlan.Name +
                        " has no affordance tagging set selected. This is not allowed", householdPlan);
                }
                if (householdPlan.CalcObject == null) {
                    throw new DataIntegrityException(
                        "The household plan " + householdPlan.Name + " has no household selected. This is not allowed",
                        householdPlan);
                }
            }
        }
    }
}
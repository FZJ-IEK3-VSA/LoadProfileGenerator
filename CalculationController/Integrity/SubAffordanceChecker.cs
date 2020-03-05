using Common;
using Database;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class SubAffordanceChecker : BasicChecker {
        public SubAffordanceChecker(bool performCleanupChecks) : base("Subaffordances", performCleanupChecks) {
        }

        protected override void Run([NotNull] Simulator sim) {
            foreach (var subaff in sim.SubAffordances.It) {
                foreach (var des1 in subaff.SubAffordanceDesires) {
                    if (des1.SatisfactionValue <= 0 || des1.SatisfactionValue > 1) {
                        throw new DataIntegrityException(
                            "The satisfaction value for the desire " + des1.Desire?.Name +
                            " is over 100% or below 0%. Please fix.", subaff);
                    }
                    foreach (var des2 in subaff.SubAffordanceDesires) {
                        if (des1 != des2 && des1.Desire == des2.Desire) {
                            throw new DataIntegrityException(
                                "In the sub-affordance " + subaff.Name + " the desire " + des1.Desire +
                                " exists twice. This is not right.", subaff);
                        }
                    }
                }
                if (subaff.SubAffordanceDesires.Count == 0) {
                    throw new DataIntegrityException(
                        "The sub-affordance " + subaff.Name + " contains no desires. Please fix.", subaff);
                }
                if (subaff.MinimumAge >= subaff.MaximumAge) {
                    throw new DataIntegrityException(
                        "The sub-affordance " + subaff.Name +
                        " has a minimum age bigger than the maximum age. Please fix", subaff);
                }
                if (PerformCleanupChecks) {
                    var uses = subaff.CalculateUsedIns(sim);
                    if (uses.Count == 0) {
                        throw new DataIntegrityException(
                            "The sub-affordance " + subaff.Name +
                            " doesn't seem to be used anywhere. Please fix or delete.", subaff);
                    }
                }
            }
        }
    }
}
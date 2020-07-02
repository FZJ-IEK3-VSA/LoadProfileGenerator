using System.Linq;
using Database;

namespace CalculationController.Integrity {
    public class AffordanceTaggingSetChecker : BasicChecker {
        public AffordanceTaggingSetChecker(bool performCleanupChecks)
            : base("Affordance Tagging Sets", performCleanupChecks) {
        }

        protected override void Run(Simulator sim) {
            foreach (var set in sim.AffordanceTaggingSets.Items) {
                var isrefrehsed = false;
                foreach (var entry in set.Entries) {
                    if (entry.Affordance == null) {
                        set.RefreshAffordances(sim.Affordances.Items);
                        isrefrehsed = true;
                        break;
                    }
                }
                if (!isrefrehsed) {
                    foreach (var affordance in sim.Affordances.Items) {
                        var entry =
                            set.Entries.FirstOrDefault(myentry => myentry.Affordance?.Name == affordance.Name);
                        if (entry == null) {
                            set.RefreshAffordances(sim.Affordances.Items);
                            break;
                        }
                    }
                }
            }
        }
    }
}
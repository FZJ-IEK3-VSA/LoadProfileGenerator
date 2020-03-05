using System.Linq;
using Database;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    public class AffordanceTaggingSetChecker : BasicChecker {
        public AffordanceTaggingSetChecker(bool performCleanupChecks)
            : base("Affordance Tagging Sets", performCleanupChecks) {
        }

        protected override void Run([NotNull] Simulator sim) {
            foreach (var set in sim.AffordanceTaggingSets.It) {
                var isrefrehsed = false;
                foreach (var entry in set.Entries) {
                    if (entry.Affordance == null) {
                        set.RefreshAffordances(sim.Affordances.It);
                        isrefrehsed = true;
                        break;
                    }
                }
                if (!isrefrehsed) {
                    foreach (var affordance in sim.Affordances.It) {
                        var entry =
                            set.Entries.FirstOrDefault(myentry => myentry.Affordance?.Name == affordance.Name);
                        if (entry == null) {
                            set.RefreshAffordances(sim.Affordances.It);
                            break;
                        }
                    }
                }
            }
        }
    }
}
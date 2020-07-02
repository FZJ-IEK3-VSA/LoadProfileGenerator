using Common;
using Database;

namespace CalculationController.Integrity {
    internal class TraitTagChecker : BasicChecker {
        public TraitTagChecker(bool performCleanupChecks) : base("Trait Tags", performCleanupChecks) {
        }

        protected override void Run(Simulator sim) {
            foreach (var tag in sim.TraitTags.Items) {
                if (!tag.Name.Contains(" / ") && PerformCleanupChecks) {
                    throw new DataIntegrityException(
                        "The trait tag " + tag.Name + " has no slash ('/'). Please delete or fix.", tag);
                }
                var uses = tag.CalculateUsedIns(sim);
                if (uses.Count == 0) {
                    throw new DataIntegrityException(
                        "The trait tag " + tag.Name + " has not a single entry. Please delete or fix.", tag);
                }
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Common;
using Database;

namespace CalculationController.Integrity {
    internal class HouseholdTagChecker : BasicChecker {
        public HouseholdTagChecker(bool performCleanupChecks) : base("Household Tags", performCleanupChecks) {
        }

        protected override void Run(Simulator sim) {
            var noClass =
                sim.HouseholdTags.Items.Where(x => string.IsNullOrWhiteSpace(x.Classification)).ToList();
            if (noClass.Count > 0) {
                var be = new List<BasicElement>();
                be.AddRange(noClass);
                throw new DataIntegrityException("The opened template tags have no classification. Please fix.", be);
            }
        }
    }
}
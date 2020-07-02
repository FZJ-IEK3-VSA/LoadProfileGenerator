using Common;
using Database;

namespace CalculationController.Integrity {
    internal class GeneratorChecker : BasicChecker {
        public GeneratorChecker(bool performCleanupChecks) : base("Generators", performCleanupChecks) {
        }

        protected override void Run(Simulator sim) {
            foreach (var generator in sim.Generators.Items) {
                if (generator.DateBasedProfile == null) {
                    throw new DataIntegrityException("The generator " + generator.Name +
                                                     " is missing a date based profile.");
                }
            }
        }
    }
}
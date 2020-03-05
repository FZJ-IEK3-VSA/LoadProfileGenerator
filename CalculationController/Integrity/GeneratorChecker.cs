using Common;
using Database;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    internal class GeneratorChecker : BasicChecker {
        public GeneratorChecker(bool performCleanupChecks) : base("Generators", performCleanupChecks) {
        }

        protected override void Run([NotNull] Simulator sim) {
            foreach (var generator in sim.Generators.It) {
                if (generator.DateBasedProfile == null) {
                    throw new DataIntegrityException("The generator " + generator.Name +
                                                     " is missing a date based profile.");
                }
            }
        }
    }
}
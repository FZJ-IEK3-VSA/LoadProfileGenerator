using Database;
using JetBrains.Annotations;

namespace CalculationController.Integrity
{
    internal class SiteChecker : BasicChecker
    {
        public SiteChecker(bool performCleanupChecks)
            : base("Sites", performCleanupChecks)
        {
        }

        protected override void Run([NotNull] Simulator sim)
        {
        }
    }
}

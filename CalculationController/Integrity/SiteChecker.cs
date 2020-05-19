using Database;

namespace CalculationController.Integrity
{
    internal class SiteChecker : BasicChecker
    {
        public SiteChecker(bool performCleanupChecks)
            : base("Sites", performCleanupChecks)
        {
        }

        protected override void Run(Simulator sim)
        {
        }
    }
}

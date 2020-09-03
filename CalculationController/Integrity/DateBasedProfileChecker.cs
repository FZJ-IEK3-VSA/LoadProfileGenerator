using Common;
using Database;

namespace CalculationController.Integrity
{
    internal class DateBasedProfileChecker : BasicChecker
    {
        public DateBasedProfileChecker(bool performCleanupChecks) : base("Date Based Profile", performCleanupChecks) {
        }

        protected override void Run(Simulator sim, CheckingOptions options)
        {
            foreach (var profile in sim.DateBasedProfiles.Items) {
                if(profile.Datapoints.Count == 0) {
                    throw new DataIntegrityException("The date based profile " + profile.PrettyName + " seems to be empty. " +
                                                     "Please fix.",profile);
                }
                if (PerformCleanupChecks) {
                    var usedIns = profile.CalculateUsedIns(sim);
                    if (usedIns.Count == 0) {
                        throw new DataIntegrityException(
                            "The date based profile " + profile.PrettyName +
                            " does not seem to be used anywhere. Please fix.", profile);
                    }
                }
            }
        }
    }
}

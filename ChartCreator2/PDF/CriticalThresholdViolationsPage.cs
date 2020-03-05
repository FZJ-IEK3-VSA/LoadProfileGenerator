using Common;
using JetBrains.Annotations;

namespace ChartCreator2.PDF {
    internal class CriticalThresholdViolationsPage : ChartPageBase {
        public CriticalThresholdViolationsPage():base(
            "This shows desire threshold violations beyond the defined critical level. The idea is to find out if for " +
            "example people are starving, because there is no food in the household. If this is always 0, it shows that everything is perfect.",
            "CriticalThresholdViolations", "CriticalThresholdViolations.*.png",
            "Critical Desire Threshold Violations"
            ) {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        [NotNull]
        protected override string GetGraphTitle([NotNull] string filename) {
            var arr = filename.Split('.');

            return "Curve for the  for " + arr[1] + " from " + filename;
        }
    }
}
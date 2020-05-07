using Common;
using JetBrains.Annotations;

namespace ChartCreator2.PDF {
    internal class TimeOfUseEnergyProfilePages : ChartPageBase {
        public TimeOfUseEnergyProfilePages():base("The time of use energy profiles show when each device was used and how much power it used.",
            "TimeOfUseEnergyProfiles",
            "TimeOfUseEnergyProfiles.*.png",
            "Overview of the time and power of the use per load type per device"
            ) {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        [JetBrains.Annotations.NotNull]
        protected override string GetGraphTitle([JetBrains.Annotations.NotNull] string filename) {
            var str = filename.Split('.');
            var lt = str[1];
            return lt;
        }
    }
}
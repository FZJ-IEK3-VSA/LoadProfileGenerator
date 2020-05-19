using Common;

namespace ChartCreator2.PDF {
    internal class TimeOfUseProfilesPages : ChartPageBase {
        public TimeOfUseProfilesPages():base("The time of use energy profiles shows when each device was used.",
            "TimeOfUseEnergyProfiles",
            "TimeOfUseProfiles.*.png",
            "Overview of the time of the use per load type per device"
            ) {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        protected override string GetGraphTitle(string filename) {
            var str = filename.Split('.');
            var lt = str[1];
            return lt;
        }
    }
}
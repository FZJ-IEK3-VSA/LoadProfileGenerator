using Common;
using JetBrains.Annotations;

namespace ChartCreator2.PDF {
    internal class WeekdayProfilesPages : ChartPageBase {
        public WeekdayProfilesPages():base(
            "This graph shows for each load type the average power consumption per day grouped by" +
            "season and weekday/saturday/sunday.",
            "WeekdayProfiles",
            "WeekdayProfiles.*.png",
            "Energy use per load type during different seasons, split by weekday/saturday/sunday"
            ) {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        protected override string GetGraphTitle([NotNull] string filename) {
            var str = filename.Split('.');
            var lt = str[1];
            return lt;
        }
    }
}
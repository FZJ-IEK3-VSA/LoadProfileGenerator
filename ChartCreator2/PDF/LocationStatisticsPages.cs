using Common;
using JetBrains.Annotations;

namespace ChartCreator2.PDF {
    internal class LocationStatisticsPages : ChartPageBase {
        public LocationStatisticsPages():base(
            "These charts show where the persons spend their time.",
            "LocationStatistics",
            "LocationStatistics.*.png",
            "Location Distribution per Person"
            ) {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        [NotNull]
        protected override string GetGraphTitle([NotNull] string filename) {
            var str = filename.Split('.');
            var person = str[1];
            return person;
        }
    }
}
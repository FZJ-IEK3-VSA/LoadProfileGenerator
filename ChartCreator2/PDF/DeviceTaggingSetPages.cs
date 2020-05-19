using Common;

namespace ChartCreator2.PDF {
    internal class DeviceTaggingSetPages : ChartPageBase {
        public DeviceTaggingSetPages():base(
            "The devices in the LPG can be grouped with various criteria by the device " +
            "tagging sets. These charts show the results.",
            "DeviceTaggingSet",
            "DeviceTaggingSet.*.png",
            "Grouped energy use for each load type for each device"
            ) {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        protected override string GetGraphTitle(string filename) {
            var str = filename.Split('.');
            var lt = str[1];
            var hh = str[2];
            var taggingset = str[3];
            return hh + " - " + taggingset + " - " + lt;
        }
    }
}
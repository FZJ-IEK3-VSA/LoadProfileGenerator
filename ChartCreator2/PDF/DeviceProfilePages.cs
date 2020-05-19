using Common;

namespace ChartCreator2.PDF {
    internal class DeviceProfilePages : ChartPageBase {
        public DeviceProfilePages():base("The device profile files are the reason for the LPG. They show " +
                                         "the power consumption of each device.",
            "DeviceProfiles", "DeviceProfiles.*.png",
            "Example of the device profiles for each load type"
            ) {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        protected override string GetGraphTitle(string filename) {
            var str = filename.Split('.');
            var name = "";
            for (var i = 2; i < str.Length - 4; i++) {
                name += str[i] + ".";
            }
            name = name.Substring(0, name.Length - 1);
            var max = str.Length;
            var lt = str[1] + ", Coloring Scheme: " + name + ", Date " + str[max - 4] + "." + str[max - 3] + "." +
                     str[max - 2];
            return lt;
        }
    }
}
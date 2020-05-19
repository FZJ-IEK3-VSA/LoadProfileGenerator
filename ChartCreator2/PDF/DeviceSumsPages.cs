using Common;

namespace ChartCreator2.PDF {
    internal class DeviceSumsPages : ChartPageBase {
        public DeviceSumsPages():base("These pie charts show the energy use for each invidividual device in " +
                                      "each load type.",
            "DeviceSums",
            "DeviceSums.*.png",
            "Energy use for each load type for each device"
            ) {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        protected override string GetGraphTitle(string filename) {
            var str = filename.Split('.');
            if (str.Length >3) {
                var name = str[1] + " - ";
                for (int i = 2; i <str.Length-1; i++) {
                    name += str[i] + ".";
                }

                return  name.Substring(0, name.Length - 1);
            }
            return str[1];
        }
    }
}
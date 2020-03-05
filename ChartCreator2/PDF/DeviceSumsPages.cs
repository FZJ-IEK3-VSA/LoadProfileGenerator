using Common;
using JetBrains.Annotations;

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

        [NotNull]
        protected override string GetGraphTitle([NotNull] string filename) {
            var str = filename.Split('.');
            string name;
            if (str.Length >3) {
                name = str[1] + " - ";
                for (int i = 2; i <str.Length-1; i++) {
                    name += str[i] + ".";
                }

                return  name.Substring(0, name.Length - 1);
            }
            return str[1];
        }
    }
}
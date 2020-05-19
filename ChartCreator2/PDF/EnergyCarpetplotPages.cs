using Common;

namespace ChartCreator2.PDF {
    internal class EnergyCarpetplotPages : ChartPageBase {
        public EnergyCarpetplotPages() :base(
            "The energy carpet plots show energy use as a carpet plot.",
            "EnergyCarpetplot",
            "EnergyCarpetplot.*.7.png",
            "Carpet plot of the energy use for each load type"
            )
        {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        protected override string GetGraphTitle(string filename) {
            var str = filename.Split('.');
            var lt = str[1];
            return lt;
        }
    }
}
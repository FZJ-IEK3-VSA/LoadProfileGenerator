using Common;
using JetBrains.Annotations;

namespace ChartCreator2.PDF {
    internal class TemperaturePages : ChartPageBase {
        public TemperaturePages():
            base("This shows the temperature curve used for the simulation.",
                "Temperatures",
                "Temperatures.png",
                "Temperature profile used in the calculation"
                )
        {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        [JetBrains.Annotations.NotNull]
        protected override string GetGraphTitle([JetBrains.Annotations.NotNull] string filename) => "Temperatures";
    }
}
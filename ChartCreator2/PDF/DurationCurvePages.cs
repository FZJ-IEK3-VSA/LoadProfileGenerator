using Common;
using JetBrains.Annotations;

namespace ChartCreator2.PDF {
    internal class DurationCurvePages : ChartPageBase {
        public DurationCurvePages() :base(
            "The duration curve show the duration curve for the entire household to " +
            " give an overview of the power consumption.",
            "DurationCurve",
            "DurationCurve.*.png",
            "Duration curve for each load type"
            )
        {
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
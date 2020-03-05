using Common;
using JetBrains.Annotations;

namespace ChartCreator2.PDF {
    internal class DeviceDurationCurvesPages : ChartPageBase {
        public DeviceDurationCurvesPages():base("The device duration curve show the duration curve of each device to " +
                                         " give an overview of the power consumption.", "DeviceDurationCurves", "DeviceDurationCurves.*.png",
            "Duration curve for each device for each load type"
            )
        {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        [NotNull]
        protected override string GetGraphTitle([NotNull] string filename) {
            var str = filename.Split('.');
            var lt = str[1];
            return lt;
        }
    }
}
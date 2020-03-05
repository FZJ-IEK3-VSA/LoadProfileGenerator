using Common;
using JetBrains.Annotations;

namespace ChartCreator2.PDF {
    internal class ExecutedActionsOverviewPages : ChartPageBase {
        public ExecutedActionsOverviewPages():base("These charts show how often each affordance was executed.",
            "ExecutedActionsOverviewCount",
            "ExecutedActionsOverviewCount.*.png",
            "Overview of the actions of each member of the household"
            )
        {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        [NotNull]
        protected override string GetGraphTitle([NotNull] string filename) {
            var str = filename.Split('.');
            var hh = str[1];
            var person = str[2];
            return hh + " - " + person;
        }
    }
}
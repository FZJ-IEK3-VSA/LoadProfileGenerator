using Common;

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

        [JetBrains.Annotations.NotNull]
        protected override string GetGraphTitle([JetBrains.Annotations.NotNull] string filename) {
            var str = filename.Split('.');
            var hh = str[1];
            var person = str[2];
            return hh + " - " + person;
        }
    }
}
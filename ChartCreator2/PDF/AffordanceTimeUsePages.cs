using Common;
using JetBrains.Annotations;

namespace ChartCreator2.PDF {
    internal class AffordanceTimeUsePages : ChartPageBase {
        public AffordanceTimeUsePages():base(
            "These charts show how the people in the household use their time. This shows the individual affordances " +
            "to help find problems in the household definition.", "AffordanceTimeUse",
            "AffordanceTimeUse.*.png",
            "Time Use per Person per Affordance Per Person"
            )
        {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        [NotNull]
        protected override string GetGraphTitle([NotNull] string filename) {
            var str = filename.Split('.');
            var hh = str[1];
            var taggingset = str[2];
            return hh + " -  " + taggingset;
        }
    }
}
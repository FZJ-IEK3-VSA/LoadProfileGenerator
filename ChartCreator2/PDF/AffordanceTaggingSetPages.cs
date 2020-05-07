using Common;
using JetBrains.Annotations;

namespace ChartCreator2.PDF {
    internal class AffordanceTaggingSetPages : ChartPageBase {
        public AffordanceTaggingSetPages():base("These charts show how the people in the household use their time. To help " +
                                                "with analysis, the activities can be grouped by various criteria. This is " +
                                                "done with the affordance tagging sets in the LPG.",
            "AffordanceTaggingSet", "AffordanceTaggingSet.*.png", "Time Use per Person Per Affordance according to different category definitions") {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        [JetBrains.Annotations.NotNull]
        protected override string GetGraphTitle([JetBrains.Annotations.NotNull] string filename) {
            var str = filename.Split('.');
            var hh = str[1];
            var taggingset = str[2];
            return hh + " -  " + taggingset;
        }
    }
}
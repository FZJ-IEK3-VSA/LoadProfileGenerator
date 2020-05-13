using Common;

namespace ChartCreator2.PDF {
    internal class ActivityFrequencyPages : ChartPageBase {
        public ActivityFrequencyPages():base("These charts show an ordered distribution of times of the activities of each person." +
                                             "This helps with judging quickly if a person is sleeping correctly and if they are going to work regularly.",
            "ActivityFrequenciesPerMinute", "ActivityFrequencies*.png", "Activity Frequency Charts") {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        [JetBrains.Annotations.NotNull]
        protected override string GetGraphTitle([JetBrains.Annotations.NotNull] string filename) {
            var str = filename.Split('.');
            var hh = str[1];
            var person = str[2];
            return hh + " -  " + person;
        }
    }
}
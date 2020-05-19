using Common;

namespace ChartCreator2.PDF {
    internal class VariablePages : ChartPageBase {
        public VariablePages():base("The variables are used to keep track of things like dirty laundry," +
                                    " dirty dishes and the amount of laundry to iron. They are used to ensure that for example " +
                                    "the dishwasher is only turned on if there are sufficient dirty dishes. " +
                                    "One chart shows the first 25000 timesteps of the contents of all variables, the other shows the entire time span.",
            "Variablelogfile", "Variablelogfile.*.png", "Variables")

        {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        protected override string GetGraphTitle(string filename) => "Variables";
    }
}
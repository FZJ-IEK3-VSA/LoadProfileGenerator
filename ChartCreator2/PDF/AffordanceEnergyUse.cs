using Common;

namespace ChartCreator2.PDF {
    internal class AffordanceEnergyUse : ChartPageBase {
        public AffordanceEnergyUse():base("This shows the distribution of the energy/ressource use to each affordance by load type.",
            "AffordanceEnergyUse", "AffordanceEnergyUse.*.png", "Energy/Resource use distribution per load type per affordance") {
            MyTargetDirectory = TargetDirectory.Charts;
        }

        [JetBrains.Annotations.NotNull]
        protected override string GetGraphTitle([JetBrains.Annotations.NotNull] string filename) {
            var str = filename.Split('.');
            var hh = str[1];
            var loadtype = str[2];
            return hh + " -  " + loadtype;
        }
    }
}
//using Common;

//namespace ChartCreator2.PDF {
//    internal class AffordanceEnergyUsePerPersonPages : ChartPageBase {
//        public AffordanceEnergyUsePerPersonPages():base ("This shows the distribution of the energy/ressource use to each affordance by load type " +
//                                                         "and by person. This helps with figuring out if a person is using too much electricity.", "AffordanceEnergyUsePerPerson",
//            "AffordanceEnergyUsePerPerson.*.png","Energy use per person per affordance")
//        {
//            MyTargetDirectory = TargetDirectory.Charts;
//        }

//        protected override string GetGraphTitle(string filename) {
//            var str = filename.Split('.');
//            var hh = str[1];
//            var person = str[2];
//            return hh + " -  " + person;
//        }
//    }
//}
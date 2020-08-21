//using Common;

//namespace ChartCreator2.PDF {
//    internal class DesirePages : ChartPageBase {
//        public DesirePages():base("The desires determine the actions of the people in the household. These charts " +
//                                  "show a section of the file.", "Desires", "Desires.*.png", "Example from the desire values for each person") {
//            MyTargetDirectory = TargetDirectory.Charts;
//        }

//        protected override string GetGraphTitle(string filename) {
//            var str = filename.Split('.');
//            var hh = str[1];
//            var taggingset = str[2];
//            return hh + " -  " + taggingset;
//        }
//    }
//}
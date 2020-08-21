//using Common;

//namespace ChartCreator2.PDF {
//    internal class DeviceProfileExternalPages : ChartPageBase {
//        public DeviceProfileExternalPages():base("The device profile files are the same data as the device profile files, but " +
//                                                 " summed up to the external time resolution. They show " +
//                                                 "the power consumption of each device.", "DeviceProfiles_", "DeviceProfiles_*.png", "Example of the device profiles for each load type in the external time resolution.") {
//            MyTargetDirectory = TargetDirectory.Charts;
//        }

//        protected override string GetGraphTitle(string filename) {
//            var str = filename.Split('.');
//            var lt = str[1];
//            return lt;
//        }
//    }
//}
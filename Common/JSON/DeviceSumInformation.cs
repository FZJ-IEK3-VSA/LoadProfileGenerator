using Common.CalcDto;
using JetBrains.Annotations;

namespace Common.JSON {
    public class DeviceSumInformation {
        public DeviceSumInformation([NotNull] string deviceName, double totalConsumption, [NotNull] CalcLoadTypeDto loadTypeInformation)
        {
            DeviceName = deviceName;
            TotalConsumption = totalConsumption;
            LoadTypeInformation = loadTypeInformation;
        }

        [UsedImplicitly]
        [NotNull]
        public string DeviceName { get; set; }
        [UsedImplicitly]
        [NotNull]
        public CalcLoadTypeDto LoadTypeInformation { get; set; }
        [UsedImplicitly]
        public double TotalConsumption { get; set; }
    }
    /*
    public class DeviceSumInformationList {
        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<DeviceSumInformation> DeviceSums { get; set; } = new List<DeviceSumInformation>();

        [JetBrains.Annotations.NotNull]
        public static DeviceSumInformationList Read([JetBrains.Annotations.NotNull] string fullFilePath)
        {
            string json;
            using (var sw = new StreamReader(fullFilePath)) {
                json = sw.ReadToEnd();
            }
            var o = JsonConvert.DeserializeObject<DeviceSumInformationList>(json);
            return o;
        }

        public void WriteJson([JetBrains.Annotations.NotNull] FileFactoryAndTracker fft, TimeSpan resolution)
        {
            var sw = fft.MakeFile<StreamWriter>(Constants.DevicesumsJsonFileName, "Totals per device as JSON", false,
                ResultFileID.DeviceSumsJson, Constants.GeneralHouseholdKey, TargetDirectory.Reports, resolution);
            WriteResultEntries(sw);
            sw.Close();
        }

        private void WriteResultEntries([JetBrains.Annotations.NotNull] StreamWriter sw)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Include
            });
            sw.WriteLine(json);
        }

        public void AddDeviceSum([JetBrains.Annotations.NotNull] string name, double devicesum, [JetBrains.Annotations.NotNull] CalcLoadTypeDto lti)
        {
            DeviceSumInformation dsi = new DeviceSumInformation(name,devicesum,lti);
            DeviceSums.Add(dsi);
        }
    }*/
}
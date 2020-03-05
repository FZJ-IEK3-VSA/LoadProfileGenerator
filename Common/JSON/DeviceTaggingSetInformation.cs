using System.Collections.Generic;
using Automation.ResultFiles;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;

namespace Common.JSON {
    public class CalcDeviceTaggingSets
    {
        [NotNull]
        [ItemNotNull]
        public List<DeviceTaggingSetInformation> AllCalcDeviceTaggingSets { get; set; } = new List<DeviceTaggingSetInformation>();
    }
    public class DeviceTaggingSetList {
        [UsedImplicitly]
        [NotNull]
        [ItemNotNull]
        public List<DeviceTaggingSetInformation> TaggingSets { get; set; } = new List<DeviceTaggingSetInformation>();

        [NotNull]
        public static DeviceTaggingSetList Read([NotNull] SqlResultLoggingService srls)
        {
            DeviceTaggingSetLogger dtsl = new DeviceTaggingSetLogger(srls);
            var list = dtsl.Load();
            var dtlist = new DeviceTaggingSetList
            {
                TaggingSets = list
            };
            return dtlist;
        }
        /*
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Include
            });
        }*/
    }

    public class DeviceTaggingSetInformation {
        public DeviceTaggingSetInformation([NotNull] string name) => Name = name;

        [UsedImplicitly]
        [NotNull]
        [ItemNotNull]
        public List<LoadTypeInformation> LoadTypesForThisSet { get; set; } = new List<LoadTypeInformation>();

        [NotNull]
        [UsedImplicitly]
        public string Name { get; set; }
        [UsedImplicitly]
        [NotNull]
        public Dictionary<string, string> TagByDeviceName { get; set; } = new Dictionary<string, string>();

        [UsedImplicitly]
        [NotNull]
        public Dictionary<string, double> ReferenceValues { get; set; } = new Dictionary<string, double>();

        public void AddTag([NotNull] string deviceName, [NotNull] string tagName)
        {
            TagByDeviceName.Add(deviceName, tagName);
        }
        public void AddRefValue([NotNull] string referenceValue, double value, [NotNull] string loadtypename)
        {
            ReferenceValues.Add(MakeKey(loadtypename, referenceValue), value);
        }

        [NotNull]
        public static string MakeKey([NotNull] string loadType, [NotNull] string referenceValue) => loadType + "#" + referenceValue;

        public void AddLoadType([NotNull] LoadTypeInformation clt)
        {
            LoadTypesForThisSet.Add(clt);
        }
    }
}
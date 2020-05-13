using Automation;
using Automation.ResultFiles;

namespace Common.JSON {
    using System;
    using CalcDto;
    using JetBrains.Annotations;
    using Newtonsoft.Json;
    using SQLResultLogging.Loggers;

    public class ColumnEntry : IHouseholdKey
    {
        [NotNull]
        [JsonProperty]
        public CalcDeviceDto CalcDeviceDto { get; set; }
        public ColumnEntry([NotNull] string name, int dstcolumn, [NotNull] string locationName, [NotNull] StrGuid deviceGuid, [NotNull] HouseholdKey householdKey,
                           [NotNull] CalcLoadTypeDto loadType, [NotNull] string oefckey, [NotNull] string deviceCategory,
                           [NotNull] CalcDeviceDto calcDeviceDto)
        {
            Name = name;
            // Key = key,
            Column = dstcolumn;
            LocationName = locationName;
            DeviceGuid = deviceGuid;
            HouseholdKey = householdKey;
            LoadType = loadType;
            Oefckey = oefckey;
            DeviceCategory = deviceCategory;
            CalcDeviceDto = calcDeviceDto;
        }
        [JsonProperty]
        public int Column { get; private set; }
        [NotNull]
        [JsonProperty]
        public StrGuid DeviceGuid { get; private set; }

        [NotNull]
        [JsonProperty]
        public HouseholdKey HouseholdKey { get; private set; }

        [NotNull]
        [JsonProperty]
        public CalcLoadTypeDto LoadType { get; [UsedImplicitly] private set; }

        [NotNull]
        public string Oefckey { get; private set; }

        [NotNull]
        public string DeviceCategory { get; private set; }

        [NotNull]
        [JsonProperty]
        public string LocationName { get; private set; }

        [NotNull]
        [JsonProperty]
        public string Name { get; private set; }

        [NotNull]
        public string HeaderString([NotNull] CalcLoadTypeDto lt, [NotNull] CalcParameters calcParameters)
        {
            if (calcParameters.DeviceProfileHeaderMode == DeviceProfileHeaderMode.Standard) {
                while (Name.EndsWith(calcParameters.CSVCharacter, StringComparison.Ordinal)) {
                    Name = Name.Substring(0, Name.Length - 1);
                }

                var housenumber = HouseholdKey + " - ";
                if (LocationName.Length > 0 && Name.Length > 0) {
                    return housenumber + LocationName + " - " + Name + " [" + lt.UnitOfSum + "]" + calcParameters.CSVCharacter;
                }

                if (LocationName.Length == 0 && Name.Length > 0) {
                    return Name + " [" + lt.UnitOfSum + "]" + calcParameters.CSVCharacter;
                }

                return housenumber + LocationName + " - (none) [" + lt.UnitOfSum + "]" + calcParameters.CSVCharacter;
            }

            if (calcParameters.DeviceProfileHeaderMode == DeviceProfileHeaderMode.OnlyDeviceCategories) {
                return DeviceCategory + " [" + lt.UnitOfSum + "]" + calcParameters.CSVCharacter;
            }
            throw new LPGException("Forgotten Device Profile Header Mode");
        }

        [NotNull]
        public override string ToString() => HouseholdKey + " - " + LocationName + " - " + Name;
    }
}
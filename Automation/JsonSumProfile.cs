using System;
using System.Collections.Generic;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Automation
{

    public class SingleDeviceProfile {
        public SingleDeviceProfile(string? name) => Name = name;
        [Obsolete("json only")]
        public SingleDeviceProfile()
        {

        }

        public string? Name { get; set; }
        public List<double> Values { get; set; } = new List<double>();

    }

    public class JsonDeviceProfiles
    {
        public JsonDeviceProfiles(TimeSpan timeResolution, DateTime startTime, string loadTypeName, string unit, LoadTypeInformation loadTypeInformation)
        {
            TimeResolution = timeResolution;
            StartTime = startTime;
            LoadTypeName = loadTypeName;
            Unit = unit;
            LoadTypeInformation = loadTypeInformation;
        }
        [Obsolete("only for json")]
#pragma warning disable 8618
        public JsonDeviceProfiles()
#pragma warning restore 8618
        {
        }

        public List<SingleDeviceProfile> DeviceProfiles { get; set; } = new List<SingleDeviceProfile>();
        public TimeSpan TimeResolution { get; set; }
        public DateTime StartTime { get; set; }

        public string? LoadTypeName { get; set; }

        public LoadTypeInformation LoadTypeInformation { get; set; }
        public string? Unit { get; set; }
    }
    public class JsonSumProfile
    {
        public JsonSumProfile(string name, TimeSpan timeResolution, DateTime startTime, string loadTypeName, string unit, LoadTypeInformation? loadTypeInformation, [NotNull] HouseholdKeyEntry houseKey)
        {
            Name = name;
            TimeResolution = timeResolution;
            StartTime = startTime;
            LoadTypeName = loadTypeName;
            Unit = unit;
            LoadTypeInformation = loadTypeInformation;
            HouseKey = houseKey;
        }
        [Obsolete("only for json")]
#pragma warning disable 8618
        public JsonSumProfile()
#pragma warning restore 8618
        {
        }

        public string? Name { get; set; }
        public TimeSpan TimeResolution { get; set; }
        public List<double> Values { get; set; } = new List<double>();
        public DateTime StartTime { get; set; }

        public string? LoadTypeName { get; set; }

        public LoadTypeInformation? LoadTypeInformation { get; set; }
        public string? Unit { get; set; }

        public HouseholdKeyEntry HouseKey { get; set; }
    }

    public class JsonEnumProfile
    {
        public JsonEnumProfile(string name, TimeSpan timeResolution, DateTime startTime, string loadTypeName, string unit, LoadTypeInformation? loadTypeInformation, HouseholdKeyEntry houseKey)
        {
            Name = name;
            TimeResolution = timeResolution;
            StartTime = startTime;
            LoadTypeName = loadTypeName;
            Unit = unit;
            LoadTypeInformation = loadTypeInformation;
            HouseKey = houseKey;
        }
        [Obsolete("only for json")]
#pragma warning disable 8618
        public JsonEnumProfile()
#pragma warning restore 8618
        {
        }

        public string? Name { get; set; }
        public TimeSpan TimeResolution { get; set; }
        public List<string> Values { get; set; } = new List<string>();
        public DateTime StartTime { get; set; }

        public string? LoadTypeName { get; set; }

        public LoadTypeInformation? LoadTypeInformation { get; set; }
        public string? Unit { get; set; }

        public HouseholdKeyEntry HouseKey { get; set; }
    }
}

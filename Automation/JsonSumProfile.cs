﻿using System;
using System.Collections.Generic;
using Automation.ResultFiles;

namespace Automation
{

    public class SingleDeviceProfile {
        public SingleDeviceProfile(string? name, string? guid, Dictionary<string, string> tagsBySet, string deviceType)
        {
            Name = name;
            Guid = guid;
            TagsBySet = tagsBySet;
            DeviceType = deviceType;
        }

        [Obsolete("json only")]
#pragma warning disable 8618
        public SingleDeviceProfile()
#pragma warning restore 8618
        {
        }

        public string? Name { get; set; }
        public string? Guid { get; set; }
        public Dictionary<string, string>? TagsBySet { get; set; }
        public string DeviceType { get; set; }
    }

    public class JsonDeviceProfiles
    {
        public JsonDeviceProfiles(TimeSpan timeResolution, DateTime startTime, string loadTypeName, string unit, LoadTypeInformation loadTypeDefinition)
        {
            TimeResolution = timeResolution;
            StartTime = startTime;
            LoadTypeName = loadTypeName;
            Unit = unit;
            LoadTypeDefinition = loadTypeDefinition;
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

        public LoadTypeInformation LoadTypeDefinition { get; set; }
        public string? Unit { get; set; }
    }
    public class JsonSumProfile
    {
        public JsonSumProfile(string name, TimeSpan timeResolution, DateTime startTime, string loadTypeName, string unit, LoadTypeInformation? loadTypeDefinition, [JetBrains.Annotations.NotNull] HouseholdKeyEntry houseKey)
        {
            Name = name;
            TimeResolution = timeResolution;
            StartTime = startTime;
            LoadTypeName = loadTypeName;
            Unit = unit;
            LoadTypeDefinition = loadTypeDefinition;
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

        public LoadTypeInformation? LoadTypeDefinition { get; set; }
        public string? Unit { get; set; }

        public HouseholdKeyEntry HouseKey { get; set; }
    }

    public class JsonEnumProfile
    {
        public JsonEnumProfile(string name, TimeSpan timeResolution, DateTime startTime, string loadTypeName, string unit, LoadTypeInformation? loadTypeDefinition, HouseholdKeyEntry houseKey)
        {
            Name = name;
            TimeResolution = timeResolution;
            StartTime = startTime;
            LoadTypeName = loadTypeName;
            Unit = unit;
            LoadTypeDefinition = loadTypeDefinition;
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

        public LoadTypeInformation? LoadTypeDefinition { get; set; }
        public string? Unit { get; set; }

        public HouseholdKeyEntry HouseKey { get; set; }
    }
}

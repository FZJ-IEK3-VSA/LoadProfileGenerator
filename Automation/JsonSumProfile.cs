using System;
using System.Collections.Generic;
using Automation.ResultFiles;

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
        public JsonDeviceProfiles()
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
        public JsonSumProfile(string name, TimeSpan timeResolution, DateTime startTime, string loadTypeName, string unit, LoadTypeInformation loadTypeInformation)
        {
            Name = name;
            TimeResolution = timeResolution;
            StartTime = startTime;
            LoadTypeName = loadTypeName;
            Unit = unit;
            LoadTypeInformation = loadTypeInformation;
        }
        [Obsolete("only for json")]
        public JsonSumProfile()
        {
        }

        public string? Name { get; set; }
        public TimeSpan TimeResolution { get; set; }
        public List<double> Values { get; set; } = new List<double>();
        public DateTime StartTime { get; set; }

        public string? LoadTypeName { get; set; }

        public LoadTypeInformation LoadTypeInformation { get; set; }
        public string? Unit { get; set; }
    }
}

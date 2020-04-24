using System;
using System.Collections.Generic;

namespace Automation
{
    public class JsonResultFile
    {
        public JsonResultFile(string name, TimeSpan timeResolution, DateTime startTime, string loadTypeName, string unit)
        {
            Name = name;
            TimeResolution = timeResolution;
            StartTime = startTime;
            LoadTypeName = loadTypeName;
            Unit = unit;
        }
        [Obsolete("only for json")]
        public JsonResultFile()
        {
        }

        public string Name { get; set; }
        public TimeSpan TimeResolution { get; set; }
        public List<double> Values { get; set; } = new List<double>();
        public DateTime StartTime { get; set; }

        public string LoadTypeName { get; set; }
        public string Unit { get; set; }
    }
}

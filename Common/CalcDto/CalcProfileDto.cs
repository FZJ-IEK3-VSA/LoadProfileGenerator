using System;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcProfileDto {
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        public StrGuid Guid { get; }
        public ProfileType ProfileType { get; }
        [NotNull]
        public string DataSource { get; }

        [ItemNotNull]
        [NotNull]
        public List<CalcTimeDataPointDto> Datapoints { get; } = new List<CalcTimeDataPointDto>();

        public CalcProfileDto([NotNull] string name, int id,  ProfileType profileType, [NotNull] string dataSource, StrGuid guid)
        {
            Name = name;
            ID = id;
            ProfileType = profileType;
            DataSource = dataSource;
            Guid = guid;
        }

        public void AddNewTimepoint(TimeSpan ts, double value)
        {
            var tp = new CalcTimeDataPointDto(ts, value);
            if (Datapoints == null)
            {
                throw new LPGException("Datapoints was null");
            }
            Datapoints.Add(tp);
        }

        public class CalcTimeDataPointDto
        {
            private readonly TimeSpan _time;

            public CalcTimeDataPointDto(TimeSpan ts, double pvalue)
            {
                _time = ts;
                Value = pvalue;
            }

            public TimeSpan Time => _time;

            public double Value { get; }

            [NotNull]
            public override string ToString() => "[" + _time + "] " + Value.ToString(Config.CultureInfo);
        }
    }
}
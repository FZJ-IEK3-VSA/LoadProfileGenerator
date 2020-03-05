using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcVariableDto: IHouseholdKey
    {
        public CalcVariableDto([NotNull]string name, [NotNull] string guid, double value, [NotNull]string locationName, [NotNull]string locationGuid, [NotNull]HouseholdKey householdKey)
        {
            Name = name;
            Guid = guid;
            Value = value;
            LocationName = locationName;
            LocationGuid = locationGuid;
            HouseholdKey = householdKey;
        }
        [NotNull]
        public string Name { get; }
        [NotNull]
        public string Guid { get; }
        public double Value { get;  }
        [NotNull]
        public string LocationName { get; }
        [NotNull]
        public string LocationGuid { get; }
        [NotNull]
        public HouseholdKey HouseholdKey { get; }
    }
}
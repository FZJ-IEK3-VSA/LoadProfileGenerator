using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;

namespace Common.CalcDto {
    public class CalcVariableDto: IHouseholdKey
    {
        public CalcVariableDto([JetBrains.Annotations.NotNull]string name, StrGuid guid, double value,
                               [JetBrains.Annotations.NotNull]string locationName, StrGuid locationGuid, [JetBrains.Annotations.NotNull]HouseholdKey householdKey)
        {
            Name = name;
            Guid = guid;
            Value = value;
            LocationName = locationName;
            LocationGuid = locationGuid;
            HouseholdKey = householdKey;
        }
        [JetBrains.Annotations.NotNull]
        public string Name { get; }
        public StrGuid Guid { get; }
        public double Value { get;  }
        [JetBrains.Annotations.NotNull]
        public string LocationName { get; }
        public StrGuid LocationGuid { get; }
        public HouseholdKey HouseholdKey { get; }
    }
}
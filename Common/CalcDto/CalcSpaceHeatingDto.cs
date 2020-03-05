using System.Collections.Generic;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcSpaceHeatingDto {
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        [ItemNotNull]
        [NotNull]
        public List<CalcDeviceLoadDto> PowerUsage { get; }
        [ItemNotNull]
        [NotNull]
        public List<CalcDegreeDayDto> CalcDegreeDays { get; }
        [NotNull]
        public HouseholdKey HouseholdKey { get; }
        [NotNull]
        public string CalcLocationName { get; }
        [NotNull]
        public string CalcLocationGuid { get; }
        [NotNull]
        public string Guid { get; }

        public CalcSpaceHeatingDto([NotNull]string name, int id, [ItemNotNull] [NotNull]List<CalcDeviceLoadDto> powerUsage,
                                   [ItemNotNull] [NotNull]List<CalcDegreeDayDto> calcDegreeDays,
                                   [NotNull]HouseholdKey householdKey, [NotNull]string calcLocationName, [NotNull] string calcLocationGuid,
                                   [NotNull]string guid)
        {
            if (powerUsage.Count != 1) {
                throw new LPGException("there should be exactly one loadtype for space heating, not more or less.");
            }

            Name = name;
            ID = id;
            PowerUsage = powerUsage;
            CalcDegreeDays = calcDegreeDays;
            HouseholdKey = householdKey;
            CalcLocationName = calcLocationName;
            CalcLocationGuid = calcLocationGuid;
            Guid = guid;
        }
    }
}
using System;
using System.Collections.Generic;
using Automation;
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
        public StrGuid CalcLocationGuid { get; }
        public StrGuid Guid { get; }

        public CalcSpaceHeatingDto([NotNull]string name, int id, [ItemNotNull] [NotNull]List<CalcDeviceLoadDto> powerUsage,
                                   [ItemNotNull] [NotNull]List<CalcDegreeDayDto> calcDegreeDays,
                                   [NotNull]HouseholdKey householdKey, [NotNull]string calcLocationName, StrGuid calcLocationGuid,
                                   StrGuid guid)
        {
            if (powerUsage.Count != 1) {
                throw new LPGException("there should be exactly one loadtype for space heating, not more or less.");
            }
            if(Math.Abs(powerUsage[0].MaxPower) < 0.00000001) { throw new LPGException("Trying to initialize heating with a max power of 0.");
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
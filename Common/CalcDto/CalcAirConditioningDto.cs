using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcAirConditioningDto {
        [NotNull] public string Name { get; }
        public int ID { get; }
        [ItemNotNull]
        [NotNull] public List<CalcDeviceLoadDto> DeviceLoads { get; }
        [NotNull]
        public HouseholdKey HouseholdKey { get; }
        [NotNull] public string CalcLocationName { get; }
        public StrGuid CalcLocationGuid { get; }
        public StrGuid Guid { get; }
        [ItemNotNull]
        [NotNull] public List<CalcDegreeHourDto> CalcDegreeHours { get; }

        public CalcAirConditioningDto([NotNull]string name, int id, [ItemNotNull] [NotNull]List<CalcDeviceLoadDto> deviceLoads,
                                      [ItemNotNull] [NotNull] List<CalcDegreeHourDto> calcDegreeHours,
                                      [NotNull] HouseholdKey householdKey, [NotNull] string calcLocationName, StrGuid calcLocationGuid,
                                      StrGuid guid)
        {
            if (deviceLoads.Count != 1) {
                throw new LPGException("there should be exactly one loadtype for air conditioning, not more or less.");
            }

            Name = name;
            ID = id;
            DeviceLoads = deviceLoads;
            HouseholdKey = householdKey;
            CalcLocationName = calcLocationName;
            CalcLocationGuid = calcLocationGuid;
            Guid = guid;
            CalcDegreeHours = calcDegreeHours;
        }
    }
}
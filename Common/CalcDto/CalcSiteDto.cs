using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcSiteDto : IHouseholdKey {
        public CalcSiteDto([NotNull] string name, int id, StrGuid guid, [NotNull] HouseholdKey householdKey)
        {
            Name = name;
            ID = id;
            Guid = guid;
            HouseholdKey = householdKey;
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcChargingStationDto> ChargingStations { get; } = new List<CalcChargingStationDto>();

        public StrGuid Guid { get; }

        public int ID { get; }

        [NotNull]
        public List<StrGuid> LocationGuid { get; } = new List<StrGuid>();

        [NotNull]
        [ItemNotNull]
        public List<string> LocationNames { get; } = new List<string>();

        [NotNull]
        public string Name { get; }

        public HouseholdKey HouseholdKey { get; }

        public void AddChargingStation([NotNull] CalcLoadTypeDto gridLoadType, [NotNull] CalcTransportationDeviceCategoryDto cat,
                                       double chargingDeviceMaxChargingPower, [NotNull] CalcLoadTypeDto carLoadType)
        {
            ChargingStations.Add(new CalcChargingStationDto(cat, gridLoadType, chargingDeviceMaxChargingPower, carLoadType));
        }

        public void AddLocation([NotNull] CalcLocationDto calcloc)
        {
            LocationNames.Add(calcloc.Name);
            LocationGuid.Add(calcloc.Guid);
        }
    }
}
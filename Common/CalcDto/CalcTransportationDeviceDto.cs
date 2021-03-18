using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcTransportationDeviceDto :IHouseholdKey {
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        [NotNull]
        public CalcTransportationDeviceCategoryDto Category { get; }
        public double AverageSpeedInMPerS { get; }
        [NotNull][ItemNotNull]
        public List<CalcDeviceLoadDto> Loads { get; }
        public HouseholdKey HouseholdKey { get; }
        public double FullRangeInMeters { get; }
        public double EnergyToDistanceFactor { get; }
        public double MaxChargingPower { get; }
        [CanBeNull]
        public string ChargingCalcLoadTypeName { get; }
        [CanBeNull]
        public StrGuid? ChargingCalcLoadTypeGuid { get; }
        public StrGuid Guid { get; }

        public bool IsLimitedToSingleLocation { get; }

        public CalcTransportationDeviceDto([NotNull]string name, int id, [NotNull] CalcTransportationDeviceCategoryDto category,
                                           double averageSpeedInMPerS, [NotNull][ItemNotNull] List<CalcDeviceLoadDto> loads,
                                           [NotNull] HouseholdKey householdKey, double fullRangeInMeters,
                                           double energyToDistanceFactor,
                                           double maxChargingPower,
                                           [CanBeNull]string chargingCalcLoadTypeName, [CanBeNull] StrGuid? chargingCalcLoadTypeGuid,
                                           StrGuid guid, bool isLimitedToSingleLocation)
        {
            Name = name;
            ID = id;
            Category = category;
            AverageSpeedInMPerS = averageSpeedInMPerS;
            Loads = loads;
            HouseholdKey = householdKey;
            FullRangeInMeters = fullRangeInMeters;
            EnergyToDistanceFactor = energyToDistanceFactor;
            MaxChargingPower = maxChargingPower;
            ChargingCalcLoadTypeName = chargingCalcLoadTypeName;
            ChargingCalcLoadTypeGuid = chargingCalcLoadTypeGuid;
            Guid = guid;
            IsLimitedToSingleLocation = isLimitedToSingleLocation;
        }
    }
}
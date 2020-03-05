using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Automation {
    public class HouseData {
        public HouseData([NotNull] string houseGuid, [NotNull] string houseTypeCode, double targetHeatDemand,
                         double targetCoolingDemand, [NotNull] string name)
        {
            HouseGuid = houseGuid;
            HouseTypeCode = houseTypeCode;
            TargetHeatDemand = targetHeatDemand;
            TargetCoolingDemand = targetCoolingDemand;
            Name = name;
        }

        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public HouseData()
        {
        }
        [CanBeNull]
        public string Name { get; set; }
        [NotNull]
        public string HouseGuid { get; set; }

        [NotNull]
        [ItemNotNull]
        public List<HouseholdData> Households { get; set; } = new List<HouseholdData>();

        [NotNull]
        public string HouseTypeCode { get; set; }

        public double TargetCoolingDemand { get; set; }
        public double TargetHeatDemand { get; set; }

        public bool IsHeatingProfileCalculated { get; set; }
        public bool IsAirConditioningProfileCalculated { get; set; }
    }
}
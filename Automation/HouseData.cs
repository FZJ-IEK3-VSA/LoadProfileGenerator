using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Automation {
    public class HouseData {

        public HouseData([NotNull] string houseGuid, [CanBeNull] string houseTypeCode, double? targetHeatDemand,
                         double? targetCoolingDemand, [NotNull] string name)
        {
            HouseGuid = houseGuid;
            HouseTypeCode = houseTypeCode;
            TargetHeatDemand = targetHeatDemand;
            TargetCoolingDemand = targetCoolingDemand;
            Name = name;
        }

        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        [Obsolete("Only for json")]
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

        [CanBeNull]
        public string HouseTypeCode { get; set; }

        public double? TargetCoolingDemand { get; set; }
        public double? TargetHeatDemand { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Automation {
    public class HouseData {

        public HouseData(StrGuid houseGuid, string? houseTypeCode, double? targetHeatDemand,
                         double? targetCoolingDemand, [JetBrains.Annotations.NotNull] string name)
        {
            HouseGuid = houseGuid;
            HouseTypeCode = houseTypeCode;
            TargetHeatDemand = targetHeatDemand;
            TargetCoolingDemand = targetCoolingDemand;
            Name = name;
        }

        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        [Obsolete("Only for json")]
#pragma warning disable 8618
        public HouseData()
#pragma warning restore 8618
        {
        }
        [CanBeNull]
        public string? Name { get; set; }
        public StrGuid? HouseGuid { get; set; }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<HouseholdData> Households { get; set; } = new List<HouseholdData>();

        [CanBeNull]
        public string? HouseTypeCode { get; set; }

        public double? TargetCoolingDemand { get; set; }
        public double? TargetHeatDemand { get; set; }
    }
}
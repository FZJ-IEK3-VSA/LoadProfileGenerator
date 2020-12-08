using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Automation.ResultFiles {
    public class HouseholdKeyEntry {
        public HouseholdKeyEntry([JetBrains.Annotations.NotNull] HouseholdKey hhKey, [JetBrains.Annotations.NotNull] string householdName, HouseholdKeyType type,
                                 [JetBrains.Annotations.NotNull] string householdDescription, [CanBeNull] string? houseName,
                                 [CanBeNull] string? houseDescription)
        {
            HHKey = hhKey;
            HouseholdName = householdName;
            KeyType = type;
            HouseholdDescription = householdDescription;
            HouseName = houseName;
            HouseDescription = houseDescription;
        }
        [JsonProperty]
        public string? HouseDescription { get; set; }

        [JetBrains.Annotations.NotNull]
        [JsonProperty]
        public string HouseholdDescription { get; private set; }

        [JetBrains.Annotations.NotNull]
        [JsonProperty]
        public HouseholdKey HHKey { get; private set; }

        [JetBrains.Annotations.NotNull]
        [JsonProperty]
        public string HouseholdName { get; private set; }
        [JsonProperty]
        public string? HouseName { get; set; }

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public HouseholdKeyType KeyType { get; private set; }

        [JetBrains.Annotations.NotNull]
        public override string ToString() => HHKey.Key + " " + HouseholdName + " (" + KeyType + ")";
    }
}
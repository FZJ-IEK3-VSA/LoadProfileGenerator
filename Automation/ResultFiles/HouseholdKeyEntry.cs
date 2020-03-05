using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Automation.ResultFiles {
    public class HouseholdKeyEntry {
        public HouseholdKeyEntry([NotNull] HouseholdKey householdKey, [NotNull] string householdName, HouseholdKeyType type, [NotNull] string householdDescription, [CanBeNull] string houseName,
                                 [CanBeNull] string houseDescription)
        {
            HouseholdKey = householdKey;
            HouseholdName = householdName;
            KeyType = type;
            HouseholdDescription = householdDescription;
            HouseName = houseName;
            HouseDescription = houseDescription;
        }
        [CanBeNull]
        [JsonProperty]
        public string HouseDescription { get; set; }

        [NotNull]
        [JsonProperty]
        public string HouseholdDescription { get; private set; }

        [NotNull]
        [JsonProperty]
        public HouseholdKey HouseholdKey { get; private set; }

        [NotNull]
        [JsonProperty]
        public string HouseholdName { get; private set; }
        [CanBeNull]
        [JsonProperty]
        public string HouseName { get; set; }

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public HouseholdKeyType KeyType { get; private set; }

        [NotNull]
        public override string ToString() => HouseholdKey.Key + " " + HouseholdName + " (" + KeyType + ")";
    }
}
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.JSON
{
    public class AffordanceEnergyUseEntry : IHouseholdKey
    {
        //for json
        // ReSharper disable once NotNullMemberIsNotInitialized
        public AffordanceEnergyUseEntry()
        {
        }
        public AffordanceEnergyUseEntry([NotNull] HouseholdKey hhkey, [NotNull] string loadTypeGuid, [NotNull] string affordanceName, [NotNull] string personName, [NotNull] string loadTypeName)
        {
            HouseholdKey = hhkey;
            LoadTypeGuid = loadTypeGuid;
            AffordanceName = affordanceName;
            PersonName = personName;
            LoadTypeName = loadTypeName;
            DictKey = MakeKey(hhkey, loadTypeGuid, affordanceName, personName);
        }
        [NotNull]
        [UsedImplicitly]
        public string LoadTypeName { get; set; }

        [NotNull]
        public static string MakeKey([NotNull] HouseholdKey hhkey, [NotNull] string loadTypeGuid, [NotNull] string affordanceName, [NotNull] string personName)
        {
            return hhkey.Key + "#" + loadTypeGuid + "#" + affordanceName + "#" + personName;
        }

        [NotNull]
        [JsonIgnore]
        public string DictKey { get; }

        [NotNull]
        [JsonProperty]
        [UsedImplicitly]
        public string LoadTypeGuid { get; private set; }
        [JsonProperty]
        [UsedImplicitly]
        public HouseholdKey HouseholdKey { get; private set; }
        [NotNull]
        [JsonProperty]
        [UsedImplicitly]
        public string AffordanceName { get; private set; }
        public double EnergySum { get; set; }
        [NotNull]
        [JsonProperty]
        [UsedImplicitly]
        public string PersonName { get; private set; }
        public int NumberOfActivations { get; set; }
        public int TotalActivationDurationInSteps { get; set; }
    }
}

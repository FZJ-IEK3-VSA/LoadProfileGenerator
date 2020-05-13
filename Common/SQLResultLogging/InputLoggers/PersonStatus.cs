using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class PersonStatus: IHouseholdKey
    {
        public PersonStatus([NotNull] HouseholdKey householdKey, [CanBeNull] string personName,
                            [NotNull] StrGuid personGuid, [NotNull] string locationName,
                            [NotNull] StrGuid locationGuid, [CanBeNull] string siteName, [CanBeNull] StrGuid siteGuid,
                            [CanBeNull] string activeAffordance, [CanBeNull] StrGuid activeAffordanceGuid, [NotNull] TimeStep timeStep)
        {
            HouseholdKey = householdKey;
            PersonName = personName;
            PersonGuid = personGuid;
            LocationName = locationName;
            LocationGuid = locationGuid;
            SiteName = siteName;
            SiteGuid = siteGuid;
            ActiveAffordance = activeAffordance;
            ActiveAffordanceGuid = activeAffordanceGuid;
            TimeStep = timeStep;
        }
        [JsonProperty]
        [NotNull]
        public TimeStep TimeStep { get; set; }
        [JsonProperty]
        public HouseholdKey HouseholdKey { get; set; }
        [CanBeNull]
        [JsonProperty]
        public string PersonName { get; set; }
        [NotNull]
        [JsonProperty]
        public StrGuid PersonGuid { get; set; }
        [NotNull]
        [JsonProperty]
        public string LocationName { get; set; }
        [NotNull]
        [JsonProperty]
        public StrGuid LocationGuid { get; set; }
        [CanBeNull]
        [JsonProperty]
        public string SiteName { get; set; }
        [CanBeNull]
        [JsonProperty]
        public StrGuid SiteGuid { get; set; }
        [CanBeNull]
        [JsonProperty]
        public string ActiveAffordance { get; set; }
        [CanBeNull]
        [JsonProperty]
        public StrGuid ActiveAffordanceGuid { get; set; }
    }
}

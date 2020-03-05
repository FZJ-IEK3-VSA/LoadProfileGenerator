using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.InputLoggers
{
    public class PersonStatus: IHouseholdKey
    {
        public PersonStatus([NotNull] HouseholdKey householdKey, [CanBeNull] string personName,
                            [NotNull] string personGuid, [NotNull] string locationName,
                            [NotNull] string locationGuid, [CanBeNull] string siteName, [CanBeNull] string siteGuid,
                            [CanBeNull] string activeAffordance, [CanBeNull] string activeAffordanceGuid, [NotNull] TimeStep timeStep)
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
        public string PersonGuid { get; set; }
        [NotNull]
        [JsonProperty]
        public string LocationName { get; set; }
        [NotNull]
        [JsonProperty]
        public string LocationGuid { get; set; }
        [CanBeNull]
        [JsonProperty]
        public string SiteName { get; set; }
        [CanBeNull]
        [JsonProperty]
        public string SiteGuid { get; set; }
        [CanBeNull]
        [JsonProperty]
        public string ActiveAffordance { get; set; }
        [CanBeNull]
        [JsonProperty]
        public string ActiveAffordanceGuid { get; set; }
    }
}

using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcTravelRouteDto: IHouseholdKey {
        [NotNull]
        public string Name { get; }
        public int MinimumAge { get; }
        public int MaximumAge { get; }
        public Enums.PermittedGender Gender { get; }
        public string AffordanceTaggingSetName { get; }
        public string AffordanceTagName { get; }
        public int? PersonID { get; }
        public double Weight { get; }
        public int ID { get; }
        [NotNull]
        public string SiteAName { get; }
        public StrGuid SiteAGuid { get; }
        [NotNull]
        public string SiteBName { get; }
        public StrGuid SiteBGuid { get; }
        public HouseholdKey HouseholdKey { get; }
        public StrGuid Guid { get; }

        public CalcTravelRouteDto([NotNull]string name, int minimumAge, int maximumAge, Enums.PermittedGender gender, string affordanceTaggingSetName, string affordanceTagName, int? personID,
            double weight, int id, [NotNull] HouseholdKey householdkey, StrGuid guid, [NotNull]string siteAName, StrGuid siteAGuid, [NotNull] string siteBName, StrGuid siteBGuid)
        {
            Name = name;
            MinimumAge = minimumAge;
            MaximumAge = maximumAge;
            Gender = gender;
            AffordanceTaggingSetName = affordanceTaggingSetName;
            AffordanceTagName = affordanceTagName;
            PersonID = personID;
            Weight = weight;
            ID = id;
            HouseholdKey = householdkey;
            Guid = guid;
            SiteAName = siteAName;
            SiteAGuid = siteAGuid;
            SiteBName = siteBName;
            SiteBGuid = siteBGuid;
        }
        [NotNull][ItemNotNull]
        public List<CalcTravelRouteStepDto> Steps { get; } = new List<CalcTravelRouteStepDto>();
        public void AddTravelRouteStep([NotNull]string stepName, int stepIntID, [NotNull]CalcTransportationDeviceCategoryDto deviceCategory, int stepNumber, double distanceInM, StrGuid guid)
        {
            CalcTravelRouteStepDto trs = new CalcTravelRouteStepDto(stepName, stepIntID, deviceCategory, stepNumber, distanceInM, guid);
            Steps.Add(trs);
        }
    }
}
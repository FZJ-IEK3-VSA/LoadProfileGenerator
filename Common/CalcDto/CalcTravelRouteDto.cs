using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcTravelRouteDto: IHouseholdKey {
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        [NotNull]
        public string SiteAName { get; }
        [NotNull]
        public StrGuid SiteAGuid { get; }
        [NotNull]
        public string SiteBName { get; }
        [NotNull]
        public StrGuid SiteBGuid { get; }
        [NotNull]
        public HouseholdKey HouseholdKey { get; }
        [NotNull]
        public StrGuid Guid { get; }

        public CalcTravelRouteDto([NotNull]string name, int id, [NotNull] HouseholdKey householdkey, [NotNull] StrGuid guid,
                                  [NotNull]string siteAName, [NotNull]StrGuid siteAGuid, [NotNull] string siteBName, [NotNull]StrGuid siteBGuid)
        {
            Name = name;
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
        public void AddTravelRouteStep([NotNull]string stepName, int stepIntID, [NotNull]CalcTransportationDeviceCategoryDto deviceCategory, int stepNumber, double distanceInM, [NotNull]StrGuid guid)
        {
            CalcTravelRouteStepDto trs = new CalcTravelRouteStepDto(stepName, stepIntID, deviceCategory, stepNumber, distanceInM, guid);
            Steps.Add(trs);
        }
    }
}
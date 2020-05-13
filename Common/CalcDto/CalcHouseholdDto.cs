using System;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcHouseholdDto
    {
        //public Dictionary<HouseholdKey, string> GetHouseholdNamesByKey { get; }
        [NotNull]
        public string Name { get; }
        [NotNull]
        public HouseholdKey HouseholdKey { get; }
        public int ID { get; }
        [NotNull]
        public string TemperatureprofileName { get; }
        [NotNull]
        public StrGuid Guid { get; }
        [NotNull][ItemNotNull]
        public List<CalcLocationDto> LocationDtos { get;  }
        [NotNull]
        [ItemNotNull]
        public List<CalcPersonDto> Persons { get;  }
        [NotNull]
        [ItemNotNull]
        public List<CalcDeviceDto> DeviceDtos { get;  }
        [NotNull]
        [ItemNotNull]
        public List<CalcAffordanceDto> Affordances { get;  }
        [NotNull]
        public List<DateTime> BridgeDays { get;  }
        [NotNull]
        [ItemNotNull]
        public List<VacationTimeframe> Vacation { get;  }
        [NotNull]
        public string GeographicLocationName { get;  }

        [NotNull]
        public string Description { get; }

        public CalcHouseholdDto([NotNull]string name, int id,
                                [NotNull] string temperatureprofileName, [NotNull]HouseholdKey householdkey, [NotNull]StrGuid guid, [NotNull] string geographicLocationName,
                                [NotNull]List<DateTime> bridgeDays, [NotNull][ItemNotNull]List<CalcAutoDevDto> autoDevices,
                                [NotNull][ItemNotNull] List<CalcLocationDto> locationDtos, [NotNull][ItemNotNull]List<CalcPersonDto> persons,[NotNull][ItemNotNull]List<CalcDeviceDto> deviceDtos,
                                [NotNull][ItemNotNull] List<CalcAffordanceDto> affordances,
                                [NotNull][ItemNotNull]List<VacationTimeframe> vacation, [CanBeNull][ItemNotNull] List<CalcSiteDto> calcSites,
                                [CanBeNull][ItemNotNull]List<CalcTravelRouteDto> calcTravelRoutes, [CanBeNull][ItemNotNull]List<CalcTransportationDeviceDto> calcTransportationDevices, [NotNull] string description)
        {
            Name = name;
            ID = id;
            TemperatureprofileName = temperatureprofileName;
            HouseholdKey = householdkey;
            Guid = guid;
            GeographicLocationName = geographicLocationName;
            BridgeDays = bridgeDays;
            AutoDevices = autoDevices;
            LocationDtos = locationDtos;
            Persons = persons;
            DeviceDtos = deviceDtos;
            Affordances = affordances;
            Vacation = vacation;
            CalcSites = calcSites;
            CalcTravelRoutes = calcTravelRoutes;
            CalcTransportationDevices = calcTransportationDevices;
            Description = description;
        }
        [ItemNotNull]
        [NotNull]
        public List<CalcAutoDevDto> AutoDevices { get; }
        [CanBeNull]
        [ItemNotNull]
        public List<CalcSiteDto> CalcSites { get;  }
        [CanBeNull]
        [ItemNotNull]
        public List<CalcTravelRouteDto> CalcTravelRoutes { get;  }
        [CanBeNull]
        [ItemNotNull]
        public List<CalcTransportationDeviceDto> CalcTransportationDevices { get;  }
        //public const string TableName = "HouseholdDefinition";
        [ItemNotNull]
        [NotNull]
        public List<HouseholdKeyEntry> GetHouseholdKeyEntries()
        {
            List<HouseholdKeyEntry> list = new List<HouseholdKeyEntry>
            {
                new HouseholdKeyEntry(HouseholdKey, Name, HouseholdKeyType.Household, Description,null,null)
            };
            //list.Add(new HouseholdKeyEntry(Constants.GeneralHouseholdKey, "General Information", HouseholdKeyType.General));
            return list;
        }
    }
}
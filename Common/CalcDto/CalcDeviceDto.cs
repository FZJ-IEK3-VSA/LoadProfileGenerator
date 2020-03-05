using System;
using System.Collections.Generic;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.CalcDto {
    using JSON;

    [Serializable]
    public class CalcDeviceDto : ICalcDeviceDto, IHouseholdKey
    {
        public CalcDeviceDto([NotNull]string name, [NotNull] string deviceCategoryGuid, [NotNull] HouseholdKey householdKey, OefcDeviceType deviceType,
                             [NotNull] string deviceCategoryName, [NotNull]string additionalName, [NotNull] string guid, [NotNull] string locationGuid,
                             [NotNull]string locationName)
        {
            Name = name;
            DeviceCategoryGuid = deviceCategoryGuid;
            HouseholdKey = householdKey;
            DeviceType = deviceType;
            DeviceCategoryName = deviceCategoryName;
            AdditionalName = additionalName;
            Guid = guid;
            LocationGuid = locationGuid;
            LocationName = locationName;
        }
        [NotNull]
        public string Name { get; }
        [NotNull]
        public string DeviceCategoryGuid { get; }
        [NotNull]
        public HouseholdKey HouseholdKey
        {
            get;
        }
        public OefcDeviceType DeviceType { get; }
        [NotNull]
        public string DeviceCategoryName { get; }
        [NotNull]
        public string AdditionalName { get; }
        [NotNull]
        public string Guid { get; }
        [NotNull]
        public string LocationGuid { get; }
        [NotNull]
        public string LocationName { get; }
        [ItemNotNull]
        [NotNull]
        public List<CalcDeviceLoadDto> Loads { get; } = new List<CalcDeviceLoadDto>();

        public void AddLoads([ItemNotNull] [NotNull]List<CalcDeviceLoadDto> load)
        {
            Loads.AddRange(load);
        }
    }
}
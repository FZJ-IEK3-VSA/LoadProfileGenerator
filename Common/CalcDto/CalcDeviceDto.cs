using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.CalcDto {
    using JSON;

    public class CalcDeviceArchiveDto : IHouseholdKey
    {
        public CalcDeviceArchiveDto(CalcDeviceDto device) => Device = device;

        public CalcDeviceDto Device { get; set; }
        public HouseholdKey HouseholdKey => Device.HouseholdKey;
    }

    [Serializable]
    public class CalcDeviceDto : ICalcDeviceDto, IHouseholdKey
    {
        [Obsolete("json only")]
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public CalcDeviceDto()
        {

        }

        [JetBrains.Annotations.NotNull]
        public CalcDeviceDto Clone()
        {
            return (CalcDeviceDto)MemberwiseClone();
        }
        public CalcDeviceDto([JetBrains.Annotations.NotNull]string name, StrGuid deviceCategoryGuid, [JetBrains.Annotations.NotNull] HouseholdKey householdKey, OefcDeviceType deviceType,
                             [JetBrains.Annotations.NotNull] string deviceCategoryName, [JetBrains.Annotations.NotNull]string additionalName, StrGuid guid, StrGuid locationGuid,
                             [JetBrains.Annotations.NotNull]string locationName)
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
        [JetBrains.Annotations.NotNull]
        public string Name { get; set; }
        public StrGuid DeviceCategoryGuid { get; set; }
        public HouseholdKey HouseholdKey
        {
            get;
            set;
        }
        public OefcDeviceType DeviceType { get; set; }
        [JetBrains.Annotations.NotNull]
        public string DeviceCategoryName { get; set; }
        [JetBrains.Annotations.NotNull]
        public string AdditionalName { get; set; }
        public StrGuid Guid { get; set; }
        public StrGuid LocationGuid { get; set; }
        [JetBrains.Annotations.NotNull]
        public string LocationName { get; set; }
        public List<CalcDeviceLoadDto> Loads { get; set; } = new List<CalcDeviceLoadDto>();

        public void AddLoads([ItemNotNull] [JetBrains.Annotations.NotNull]List<CalcDeviceLoadDto> load)
        {
            Loads.AddRange(load);
        }
    }
}
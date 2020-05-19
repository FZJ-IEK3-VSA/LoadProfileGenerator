using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.CalcDto {
    using JSON;

    [Serializable]
    public class CalcDeviceDto : ICalcDeviceDto, IHouseholdKey
    {
        [Obsolete("json only")]
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public CalcDeviceDto()
        {

        }

        [NotNull]
        public CalcDeviceDto Clone()
        {
            return (CalcDeviceDto)this.MemberwiseClone();
        }
        public CalcDeviceDto([NotNull]string name, StrGuid deviceCategoryGuid, [NotNull] HouseholdKey householdKey, OefcDeviceType deviceType,
                             [NotNull] string deviceCategoryName, [NotNull]string additionalName, StrGuid guid, StrGuid locationGuid,
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
        public string Name { get; set; }
        public StrGuid DeviceCategoryGuid { get; set; }
        public HouseholdKey HouseholdKey
        {
            get;
            set;
        }
        public OefcDeviceType DeviceType { get; set; }
        [NotNull]
        public string DeviceCategoryName { get; set; }
        [NotNull]
        public string AdditionalName { get; set; }
        public StrGuid Guid { get; set; }
        public StrGuid LocationGuid { get; set; }
        [NotNull]
        public string LocationName { get; set; }
        public List<CalcDeviceLoadDto> Loads { get; set; } = new List<CalcDeviceLoadDto>();

        public void AddLoads([ItemNotNull] [NotNull]List<CalcDeviceLoadDto> load)
        {
            Loads.AddRange(load);
        }
    }
}
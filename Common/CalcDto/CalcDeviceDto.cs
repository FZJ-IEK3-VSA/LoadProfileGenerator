using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace Common.CalcDto {
    using JSON;

    public record CalcDeviceArchiveDto : IHouseholdKey
    {
        public CalcDeviceArchiveDto(CalcDeviceDto device) => Device = device;

        public CalcDeviceDto Device { get; set; }
        public HouseholdKey HouseholdKey => Device.HouseholdKey;
    }


    [Serializable]
    public record CalcDeviceDto : ICalcDeviceDto, IHouseholdKey
    {
        private FlexibilityType _flexibilityMode;

        [Obsolete("json only")]
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        public CalcDeviceDto()
        {

        }

        public CalcDeviceDto([JetBrains.Annotations.NotNull]string name, StrGuid deviceCategoryGuid, [JetBrains.Annotations.NotNull] HouseholdKey householdKey, OefcDeviceType deviceType,
                             [JetBrains.Annotations.NotNull] string deviceCategoryName, [JetBrains.Annotations.NotNull]string additionalName, StrGuid guid, StrGuid locationGuid,
                             [JetBrains.Annotations.NotNull]string locationName, FlexibilityType flexibilityMode, int maxTimeShiftInMinutes)
        {
            Name = name;
            DeviceCategoryGuid = deviceCategoryGuid;
            HouseholdKey = householdKey;
            DeviceType = deviceType;
            DeviceCategoryName = deviceCategoryName;
            AdditionalName = additionalName;
            DeviceClassGuid = guid;
            LocationGuid = locationGuid;
            LocationName = locationName;
            FlexibilityMode = flexibilityMode;
            MaxTimeShiftInMinutes = maxTimeShiftInMinutes;
            DeviceInstanceGuid = StrGuid.New();
        }

        [JetBrains.Annotations.NotNull]
        public StrGuid DeviceInstanceGuid { get; set; }

        public int MaxTimeShiftInMinutes { get; set; }

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
        public StrGuid DeviceClassGuid { get; set; }
        public StrGuid LocationGuid { get; set; }
        [JetBrains.Annotations.NotNull]
        public string LocationName { get; set; }

        public FlexibilityType FlexibilityMode {
            get => _flexibilityMode;
            set {
                _flexibilityMode = value;
                if(!Enum.IsDefined(typeof(FlexibilityType), value)) {
                    throw new LPGException("Invalid Flexibilty");
                }
            }
        }

        public List<CalcDeviceLoadDto> Loads { get; set; } = new List<CalcDeviceLoadDto>();

        public void AddLoads([ItemNotNull] [JetBrains.Annotations.NotNull]List<CalcDeviceLoadDto> load)
        {
            Loads.AddRange(load);
        }
    }
}
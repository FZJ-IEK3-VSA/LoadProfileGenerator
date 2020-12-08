using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.Helpers;
using Common;
using Common.CalcDto;
using Common.JSON;
using Database.Helpers;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace CalculationController.DtoFactories
{
    public class DeviceCategoryDto {
        public DeviceCategoryDto([JetBrains.Annotations.NotNull] string fullCategoryName, StrGuid guid)
        {
            FullCategoryName = fullCategoryName;
            Guid = guid;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string FullCategoryName { get; set; }
        [UsedImplicitly]
        public StrGuid Guid { get; set; }
    }

    public class CalcLocationDtoFactory {
        [JetBrains.Annotations.NotNull]
        private readonly IDeviceCategoryPicker _picker;
        [JetBrains.Annotations.NotNull]
        private readonly CalcLoadTypeDtoDictionary _calcLoadTypeDict;
        [JetBrains.Annotations.NotNull]
        private readonly CalcParameters _calcParameters;

        public CalcLocationDtoFactory([JetBrains.Annotations.NotNull] CalcParameters calcParameters, [JetBrains.Annotations.NotNull] IDeviceCategoryPicker picker, [JetBrains.Annotations.NotNull] CalcLoadTypeDtoDictionary calcLoadTypeDict)
        {
            _picker = picker;
            _calcLoadTypeDict = calcLoadTypeDict;
            _calcParameters = calcParameters;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcLocationDto> MakeCalcLocations([JetBrains.Annotations.NotNull][ItemNotNull] List<Location> locations,
            [JetBrains.Annotations.NotNull] HouseholdKey householdKey, EnergyIntensityType et,
            [JetBrains.Annotations.NotNull] Dictionary<CalcLocationDto, List<IAssignableDevice>> deviceLocationDict,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> deviceActions,
            [JetBrains.Annotations.NotNull] LocationDtoDict locdict,
                                                       [ItemNotNull] [JetBrains.Annotations.NotNull] List<DeviceCategoryDto> deviceCategoryDtos)
        {
            var locs = new List<CalcLocationDto>();
            foreach (var t in locations)
            {
                // loc anlegen
                var cloc = new CalcLocationDto(t.Name, t.IntID, Guid.NewGuid().ToStrGuid());
                foreach (var locdev in t.LocationDevices)
                {
                    RealDevice rd;
                    if (locdev.Device == null)
                    {
                        throw new LPGException("Device was null");
                    }
                    if (locdev.Device.AssignableDeviceType == AssignableDeviceType.DeviceCategory)
                    {
                        rd = _picker.PickDeviceFromCategory((DeviceCategory)locdev.Device, et,
                            deviceActions);
                    }
                    else
                    {
                        rd = (RealDevice)locdev.Device;
                    }

                    var deviceName = CalcAffordanceFactory.FixAffordanceName(rd.Name, _calcParameters.CSVCharacter);
                    if (rd.DeviceCategory == null)
                    {
                        throw new LPGException("Device Category was null");
                    }

                    var devcatdto = deviceCategoryDtos.Single(x => x.FullCategoryName == rd.DeviceCategory.FullPath);
                    var clightdevice = new CalcDeviceDto(deviceName,  devcatdto.Guid,
                        householdKey, OefcDeviceType.Light, rd.DeviceCategory.FullPath,
                        string.Empty, Guid.NewGuid().ToStrGuid(),cloc.Guid,cloc.Name);
                    clightdevice.AddLoads(CalcDeviceDtoFactory.MakeCalcDeviceLoads(rd, _calcLoadTypeDict));
                    cloc.AddLightDevice(clightdevice);
                }
                deviceLocationDict.Add(cloc, new List<IAssignableDevice>());
                locs.Add(cloc);
                locdict.LocationDict.Add(t, cloc);
            }
            return locs;
        }
    }
}
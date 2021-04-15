using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.Helpers;
using Common;
using Common.CalcDto;
using Common.JSON;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace CalculationController.DtoFactories
{
    public class CalcDeviceDtoFactory {
        [JetBrains.Annotations.NotNull] private readonly IDeviceCategoryPicker _picker;
        [JetBrains.Annotations.NotNull]
        private readonly CalcParameters _calcParameters;
        [JetBrains.Annotations.NotNull]
        private readonly Random _rnd;
        [JetBrains.Annotations.NotNull]
        private readonly CalcLoadTypeDtoDictionary _loadTypeDictionary;
        [JetBrains.Annotations.NotNull]
        private readonly CalcVariableDtoFactory _calcVariableRepositoryDtoFactory;
        [JetBrains.Annotations.NotNull]
        private readonly AvailabilityDtoRepository _availabilityDtoRepository;

        public CalcDeviceDtoFactory([JetBrains.Annotations.NotNull] IDeviceCategoryPicker picker, [JetBrains.Annotations.NotNull] CalcParameters calcParameters,
            [JetBrains.Annotations.NotNull] Random rnd, [JetBrains.Annotations.NotNull] CalcLoadTypeDtoDictionary loadTypeDictionary,
            [JetBrains.Annotations.NotNull] CalcVariableDtoFactory calcVariableRepositoryDtoFactory,
                                    [JetBrains.Annotations.NotNull] AvailabilityDtoRepository availabilityDtoRepository)
        {
            _picker = picker;
            _calcParameters = calcParameters;
            _rnd = rnd;
            _loadTypeDictionary = loadTypeDictionary;
            _calcVariableRepositoryDtoFactory = calcVariableRepositoryDtoFactory;
            _availabilityDtoRepository = availabilityDtoRepository;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        [SuppressMessage("ReSharper", "FunctionComplexityOverflow")]
        public List<CalcAutoDevDto> MakeCalcAutoDevDtos(
            [JetBrains.Annotations.NotNull][ItemNotNull] List<IAutonomousDevice> autoDevices,
            EnergyIntensityType energyIntensity,
            [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
            [JetBrains.Annotations.NotNull][ItemNotNull] List<VacationTimeframe> vacationTimeframes,
            [JetBrains.Annotations.NotNull] string holidayKey,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> deviceActions,
            [JetBrains.Annotations.NotNull] LocationDtoDict locationDict,
            [JetBrains.Annotations.NotNull] TemperatureProfile temperatureProfile, [JetBrains.Annotations.NotNull] GeographicLocation geographicLocation,
            [ItemNotNull] [JetBrains.Annotations.NotNull] List<DeviceCategoryDto> deviceCategoryDtos)
        {
            var autodevs = new List<CalcAutoDevDto>(autoDevices.Count);
            //// zur kategorien zuordnung
            var allAutonomousDevices = new List<IAssignableDevice>();
            allAutonomousDevices.AddRange(autoDevices.Select(x=> x.Device));

            foreach (var hhautodev in autoDevices) {
                var busyarr = new BitArray(_calcParameters.InternalTimesteps);
                busyarr.SetAll(false);
                Logger.Debug(
                    "Determining the permitted times for each autonomous device. Device: " + hhautodev.Name);
                if (hhautodev.TimeLimit?.RootEntry == null) {
                    throw new DataIntegrityException("Time limit was null");
                }

                busyarr =
                    hhautodev.TimeLimit.RootEntry.GetOneYearArray(
                        _calcParameters.InternalStepsize,
                        _calcParameters.InternalStartTime,
                        _calcParameters.InternalEndTime, temperatureProfile, geographicLocation,
                        _rnd, vacationTimeframes, holidayKey, out _, 0, 0, 0, 0);
                // invertieren von erlaubten zu verbotenen zeiten
                busyarr = busyarr.Not();
                var timeprofilereference =
                    _availabilityDtoRepository.MakeNewReference(hhautodev.TimeLimit.Name, busyarr);
                if (hhautodev.Location == null) {
                    throw new DataIntegrityException("Location was null");
                }

                var calcLocation = locationDict.LocationDict[hhautodev.Location];
                if (hhautodev.Device == null) {
                    throw new DataIntegrityException("Device was null");
                }

                switch (hhautodev.Device.AssignableDeviceType) {
                    case AssignableDeviceType.Device:
                    case AssignableDeviceType.DeviceCategory: {
                        if (hhautodev.LoadType == null || hhautodev.TimeProfile == null) {
                            throw new LPGException("load type was null");
                        }

                        if (_loadTypeDictionary.SimulateLoadtype(hhautodev.LoadType)) {
                            var profile = GetCalcProfileDto(hhautodev.TimeProfile);
                            var rd = _picker.GetAutoDeviceDeviceFromDeviceCategoryOrDevice(hhautodev.Device, allAutonomousDevices, energyIntensity,
                                deviceActions, hhautodev.Location.IntID);
                            var cdl = MakeCalcDeviceLoads(rd, _loadTypeDictionary);
                            var ltdto = _loadTypeDictionary.GetLoadtypeDtoByLoadType(hhautodev.LoadType);
                            List<VariableRequirementDto> requirementDtos = new List<VariableRequirementDto>();
                            if (hhautodev.Variable != null) {
                                var myVariable = _calcVariableRepositoryDtoFactory.RegisterVariableIfNotRegistered(hhautodev.Variable,
                                    hhautodev.Location, householdKey, locationDict);
                                VariableRequirementDto req = new VariableRequirementDto(hhautodev.Variable.Name, hhautodev.VariableValue,
                                    calcLocation.Name, calcLocation.Guid, hhautodev.VariableCondition, myVariable.Guid);
                                requirementDtos.Add(req);
                            }

                            if (rd.DeviceCategory == null) {
                                throw new LPGException("Device category was null");
                            }

                            var deviceCategoryDto = deviceCategoryDtos.Single(x => x.FullCategoryName == rd.DeviceCategory.FullPath);
                            CalcAutoDevProfileDto cadp = new CalcAutoDevProfileDto(profile, ltdto.Name, ltdto.Guid, 1);
                            var cadps = new List<CalcAutoDevProfileDto>();
                            cadps.Add(cadp);
                            var cautodev = new CalcAutoDevDto(rd.Name, cadps, cdl, (double)hhautodev.TimeStandardDeviation, deviceCategoryDto.Guid,
                                householdKey,  calcLocation.Name, calcLocation.Guid, deviceCategoryDto.FullCategoryName, Guid.NewGuid().ToStrGuid(),
                                timeprofilereference, requirementDtos, deviceCategoryDto.FullCategoryName, rd.FlexibilityType, rd.MaxTimeShiftInMinutes);
                            autodevs.Add(cautodev);
                        }
                    }
                        break;
                    case AssignableDeviceType.DeviceAction:
                    case AssignableDeviceType.DeviceActionGroup: {
                        var deviceAction = _picker.GetAutoDeviceActionFromGroup(hhautodev.Device, allAutonomousDevices, energyIntensity,
                            deviceActions, hhautodev.Location.IntID);

                        List<VariableRequirementDto> requirementDtos = new List<VariableRequirementDto>();
                        if (hhautodev.Variable != null) {
                            var myVariable = _calcVariableRepositoryDtoFactory.RegisterVariableIfNotRegistered(hhautodev.Variable, hhautodev.Location,
                                householdKey, locationDict);
                            VariableRequirementDto req = new VariableRequirementDto(hhautodev.Variable?.Name??"", hhautodev.VariableValue,
                                calcLocation.Name, calcLocation.Guid, hhautodev.VariableCondition, myVariable.Guid);
                            requirementDtos.Add(req);
                        }

                        if (deviceAction.Device?.DeviceCategory == null) {
                            throw new LPGException("device category was null");
                        }

                        var deviceCategoryDto = deviceCategoryDtos.Single(x => x.FullCategoryName == deviceAction.Device.DeviceCategory.FullPath);
                        var cdaps = new List<CalcAutoDevProfileDto>();
                        var cdl = MakeCalcDeviceLoads(deviceAction.Device, _loadTypeDictionary);
                        foreach (var actionProfile in deviceAction.Profiles) {
                            if (actionProfile.VLoadType == null) {
                                throw new DataIntegrityException("Vloadtype  was null");
                            }

                            if (actionProfile.Timeprofile == null) {
                                throw new DataIntegrityException("Timeprofile  was null");
                            }

                            if (_loadTypeDictionary.SimulateLoadtype(actionProfile.VLoadType)) {
                                var profile = GetCalcProfileDto(actionProfile.Timeprofile);
                                if (deviceAction.Device == null) {
                                    throw new LPGException("Device was null");
                                }

                                var lt = _loadTypeDictionary.GetLoadtypeDtoByLoadType(actionProfile.VLoadType);
                                var cdap = new CalcAutoDevProfileDto(profile, lt.Name, lt.Guid, actionProfile.Multiplier);
                                cdaps.Add(cdap);
                            }
                        }

                        var cautodev = new CalcAutoDevDto(deviceAction.Device.Name, cdaps, cdl, (double)hhautodev.TimeStandardDeviation,
                                deviceCategoryDto.Guid, householdKey, calcLocation.Name, calcLocation.Guid,
                                deviceAction.Device.DeviceCategory.FullPath, Guid.NewGuid().ToStrGuid(), timeprofilereference, requirementDtos,
                                deviceCategoryDto.FullCategoryName, deviceAction.Device.FlexibilityType, deviceAction.Device.MaxTimeShiftInMinutes);
                            autodevs.Add(cautodev);
                    }
                        break;
                    default:
                        throw new LPGException("Forgotten AssignableDeviceType. Please Report!");
                }
            }

            return autodevs;
        }

        [JetBrains.Annotations.NotNull]
        public static CalcProfileDto GetCalcProfileDto([JetBrains.Annotations.NotNull] TimeBasedProfile tp)
        {
            var cp = new CalcProfileDto(tp.Name, tp.IntID, (ProfileType)tp.TimeProfileType,
                tp.DataSource, Guid.NewGuid().ToStrGuid());
            foreach (var timeDataPoint in tp.ObservableDatapoints)
            {
                cp.AddNewTimepoint(timeDataPoint.Time, timeDataPoint.Value);
            }
            return cp;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public static List<CalcDeviceLoadDto> MakeCalcDeviceLoads([JetBrains.Annotations.NotNull] RealDevice device,
            [JetBrains.Annotations.NotNull] CalcLoadTypeDtoDictionary ltdtodict)
        {
            var deviceLoads = new List<CalcDeviceLoadDto>();
            foreach (var realDeviceLoadType in device.Loads) {
                if (realDeviceLoadType.LoadType == null) {
                    throw new LPGException("Loadtype was null");
                }

                if (ltdtodict.SimulateLoadtype(realDeviceLoadType.LoadType)) {
                    var cdl = new CalcDeviceLoadDto(realDeviceLoadType.Name, realDeviceLoadType.IntID,
                        ltdtodict.Ltdtodict[realDeviceLoadType.LoadType].Name, ltdtodict.Ltdtodict[realDeviceLoadType.LoadType].Guid,
                        realDeviceLoadType.AverageYearlyConsumption, realDeviceLoadType.StandardDeviation,
                        Guid.NewGuid().ToStrGuid(), realDeviceLoadType.MaxPower);
                    deviceLoads.Add(cdl);
                }
            }

            return deviceLoads;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcDeviceDto> MakeCalcDevices([JetBrains.Annotations.NotNull][ItemNotNull] List<CalcLocationDto> locs,
            [ItemNotNull] [JetBrains.Annotations.NotNull] List<DeviceLocationTuple> devlocs,
            EnergyIntensityType energyIntensity,
            [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
            [JetBrains.Annotations.NotNull]
            Dictionary<CalcLocationDto, List<IAssignableDevice>>
                allreadyAssigendDeviceLocationDict,
            [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<DeviceAction> deviceActions,
            [JetBrains.Annotations.NotNull] CalcLoadTypeDtoDictionary loadtypes,
            [ItemNotNull] [JetBrains.Annotations.NotNull] List<DeviceCategoryDto> deviceCategories)
        {
            List<CalcDeviceDto> calcDeviceDtos = new List<CalcDeviceDto>();
            foreach (var devloc in devlocs) {
                //if(devloc.Device.Name.Contains("Dishwasher")) {
                //    Logger.Info("Figuring out dishwasher");
                //}

                var locdto = locs.First(x => x.ID == devloc.Location.IntID);
                // ggf dev category / device action in dev umwandeln
                var devicesAtLocation =
                    devlocs.Where(x => x.Location == devloc.Location).Select(x => x.Device).ToList();
                var rd = _picker.GetOrPickDevice(devloc.Device, devloc.Location, energyIntensity,
                    devicesAtLocation, deviceActions);
                //save the picked device so that next time it will be picked again
                if (!allreadyAssigendDeviceLocationDict[locdto].Contains(devloc.Device)) {
                    allreadyAssigendDeviceLocationDict[locdto].Add(devloc.Device);
                }

                if (!allreadyAssigendDeviceLocationDict[locdto].Contains(rd)) {
                    allreadyAssigendDeviceLocationDict[locdto].Add(rd);
                }

                if (rd == null) // null means that no device needed to be picked since it already exists
                {
                    continue;
                }

                bool found = calcDeviceDtos.Any(x => rd.Name == x.Name && locdto.Guid == x.LocationGuid);
                if (found) {
                    Logger.Info(
                        "Device " + rd.Name + " existed already in the Location " + locdto.Name +
                        ". Skipping...");
                }
                else {
                    if (rd.DeviceCategory== null)
                    {
                        throw new LPGException("Device Category was null");
                    }

                    DeviceCategoryDto dcdto =
                        deviceCategories.Single(x => x.FullCategoryName == rd.DeviceCategory.FullPath);
                    CalcDeviceDto cdd = new CalcDeviceDto(rd.Name, dcdto.Guid, householdKey, OefcDeviceType.Device,
                        rd.DeviceCategory.FullPath, string.Empty, Guid.NewGuid().ToStrGuid(),
                         locdto.Guid, locdto.Name, rd.FlexibilityType, rd.MaxTimeShiftInMinutes);
                    cdd.AddLoads(MakeCalcDeviceLoads(rd, loadtypes));
                    calcDeviceDtos.Add(cdd);
                }
            }

            return calcDeviceDtos;
        }
    }
}
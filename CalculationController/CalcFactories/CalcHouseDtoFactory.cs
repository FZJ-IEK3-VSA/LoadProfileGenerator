using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationController.Helpers;
using CalculationController.Integrity;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace CalculationController.CalcFactories {
    [UsedImplicitly]
    public class CalcHouseDtoFactory {
        [NotNull] private readonly AvailabilityDtoRepository _availabilityDtoRepository;

        [NotNull] private readonly CalcParameters _calcParameters;

        [NotNull] private readonly CalcVariableDtoFactory _calcVariableDtoFactory;

        [NotNull] private readonly CalcModularHouseholdDtoFactory _hhDtoFactory;

        [NotNull] private readonly CalcLoadTypeDtoDictionary _ltDict;

        [NotNull] private readonly IDeviceCategoryPicker _picker;

        [NotNull] private readonly Random _random;

        public CalcHouseDtoFactory([NotNull] CalcLoadTypeDtoDictionary ltDict,
                                   [NotNull] Random random,
                                   [NotNull] IDeviceCategoryPicker picker,
                                   [NotNull] CalcParameters calcParameters,
                                   [NotNull] CalcModularHouseholdDtoFactory hhDtoFactory,
                                   [NotNull] AvailabilityDtoRepository availabilityDtoRepository,
                                   [NotNull] CalcVariableDtoFactory calcVariableDtoFactory)
        {
            _ltDict = ltDict;
            _random = random;
            _picker = picker;
            _calcParameters = calcParameters;
            _hhDtoFactory = hhDtoFactory;
            _availabilityDtoRepository = availabilityDtoRepository;
            _calcVariableDtoFactory = calcVariableDtoFactory;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht")]
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        [NotNull]
        public CalcHouseDto MakeHouse([NotNull] Simulator sim,
                                      [NotNull] House house,
                                      [NotNull] TemperatureProfile temperatureProfile,
                                      [NotNull] GeographicLocation geographicLocation, //List<CalcDeviceTaggingSet> taggingSets,
                                      EnergyIntensityType energyIntensity)
        {
            if (house.HouseType == null) {
                throw new LPGException("Housetype was null");
            }

            if (energyIntensity == EnergyIntensityType.AsOriginal) {
                energyIntensity = house.EnergyIntensityType;
            }

            if (energyIntensity == EnergyIntensityType.AsOriginal) {
                var calcObject = house.Households[0].CalcObject;
                if (calcObject == null) {
                    throw new LPGException("House was null");
                }

                energyIntensity = calcObject.EnergyIntensityType;
            }

            var houseLocations = new List<CalcLocationDto>();
            HouseholdKey houseKey = Constants.HouseKey;
            List<DeviceCategoryDto> deviceCategoryDtos = new List<DeviceCategoryDto>();
            foreach (var deviceCategory in sim.DeviceCategories.It) {
                deviceCategoryDtos.Add(new DeviceCategoryDto(deviceCategory.FullPath, Guid.NewGuid().ToString()));
            }

            HouseIntegrityChecker.Run(house, sim);
            var householdIndex = 1;
            var calcAbleObjects = new List<CalcHouseholdDto>();
            var globalLocationDict = new Dictionary<Location, CalcLocationDto>();
            foreach (var household in house.Households) {
                if (household.CalcObject == null) {
                    throw new LPGException("Calcobject was null");
                }

                if (household.CalcObject.CalcObjectType == CalcObjectType.ModularHousehold) {
                    CalcHouseholdDto hhdto = _hhDtoFactory.MakeCalcModularHouseholdDto(sim,
                        (ModularHousehold)household.CalcObject,
                        temperatureProfile,
                        new HouseholdKey("HH" + householdIndex),
                        geographicLocation,
                        out var locationDtoDict,
                        household.TransportationDeviceSet,
                        household.TravelRouteSet,
                        energyIntensity,
                        household.ChargingStationSet,
                        _calcParameters);
                    calcAbleObjects.Add(hhdto);
                    foreach (var pair in locationDtoDict.LocationDict) {
                        if (!globalLocationDict.ContainsKey(pair.Key)) {
                            globalLocationDict.Add(pair.Key, pair.Value);
                        }
                    }
                }
                else {
                    throw new LPGException("Unknown Calc Object Type in the house! This is a bug.");
                }

                householdIndex++;
            }

            var spaceHeating = CreateSpaceHeatingObject(house, temperatureProfile, 
                houseKey, out var heatingLocation,_calcParameters.InternalStartTime,_calcParameters.InternalEndTime
                ,_ltDict); //, taggingSets);
            if (heatingLocation != null) {
                houseLocations.Add(heatingLocation);
            }

            var airconditioning = MakeAirConditioning(temperatureProfile, house.HouseType, houseKey, out var airconditioningLocation);
            if (airconditioningLocation != null) {
                houseLocations.Add(airconditioningLocation);
            }

            List<CalcAutoDevDto> autoDevs = new List<CalcAutoDevDto>();

            var autodevs2 = MakeCalcAutoDevsFromHouse(temperatureProfile,
                geographicLocation,
                house.HouseType.HouseDevices,
                energyIntensity,
                houseKey,
                house.Name,
                sim.DeviceActions.It,
                house,
                out var devLocations,
                deviceCategoryDtos);
            houseLocations.AddRange(devLocations);
            autoDevs.AddRange(autodevs2);
            // energy Storage
            var calcEnergyStorages = MakeEnergyStorages(house, houseKey); //, taggingSets);
            // transformation devices
            var transformationDevices = MakeAllTransformationDevices(house.HouseType, calcEnergyStorages, houseKey); //taggingSets,

            // generators
            var generators = MakeGenerators(house.HouseType.HouseGenerators.Select(x => x.Generator).ToList(), houseKey); //taggingSets,
            var calchouse = new CalcHouseDto(house.Name,
                autoDevs,
                airconditioning,
                spaceHeating,
                calcEnergyStorages,
                generators,
                transformationDevices,
                calcAbleObjects,
                houseKey,
                houseLocations,
                house.Description);
            //check the calc variables
            return calchouse;
        }

        private void CreateCalcConditions([NotNull] [ItemNotNull] List<CalcEnergyStorageDto> storages,
                                          [NotNull] HouseType houseType,
                                          [NotNull] TransformationDevice trafo,
                                          [NotNull] [ItemNotNull] List<CalcTransformationConditionDto> calcconditions)
        {
            foreach (var condition in trafo.Conditions) {
                var type = DetermineTransformationConditionType(condition);

                CalcEnergyStorageDto storage = null;
                if (type == CalcTransformationConditionType.StorageBetweenValues && condition.Storage == null) {
                    throw new DataIntegrityException("The storage of the condition on the transformation device " + trafo.Name +
                                                     " is empty, but that's not allowed",
                        trafo);
                }

                if (condition.Storage != null) {
                    foreach (var energyStorage in storages) {
                        if (condition.Storage.ID == energyStorage.ID) {
                            storage = energyStorage;
                        }
                    }

                    if (storage == null) {
                        throw new DataIntegrityException("The storage device " + condition.Storage.Name +
                                                         " in the condition of the transformation device " + trafo + " is not in the housetype " +
                                                         houseType.Name + ". Please fix.",
                            trafo);
                    }
                }

                CalcLoadTypeDto conditionLoadType = null;
                if (condition.ConditionLoadType != null) {
                    conditionLoadType = _ltDict.GetLoadtypeDtoByLoadType(condition.ConditionLoadType);
                }

                double divider = 1;
                if (type == CalcTransformationConditionType.StorageBetweenValues) {
                    divider = 100;
                }

                CalcTransformationConditionDto trafocondition = new CalcTransformationConditionDto(condition.Name,
                    condition.IntID,
                    type,
                    conditionLoadType,
                    condition.MinValue / divider,
                    condition.MaxValue / divider,
                    Guid.NewGuid().ToString(),
                    storage?.Name,
                    storage?.Guid);
                calcconditions.Add(trafocondition);
            }
        }

        [NotNull]
        [ItemNotNull]
        private static List<CalcEnergyStorageDto> CreateEnergyStorageDtos([NotNull] [ItemNotNull] List<EnergyStorage> energyStorages,
                                                                          [NotNull] CalcLoadTypeDtoDictionary ltdict,
                                                                          [NotNull] HouseholdKey householdKey)
        {
            var calcEnergyStorages = new List<CalcEnergyStorageDto>();
            foreach (var es in energyStorages) {
                if (es.LoadType == null) {
                    throw new LPGException("Energy storage load type was null");
                }

                if (ltdict.SimulateLoadtype(es.LoadType)) {
                    List<CalcEnergyStorageSignalDto> signals = new List<CalcEnergyStorageSignalDto>();
                    foreach (var signal in es.Signals) {
                        if (signal.LoadType == null) {
                            throw new DataIntegrityException("Signal loadtype was null");
                        }

                        var cessig = new CalcEnergyStorageSignalDto(signal.Name,
                            signal.IntID,
                            signal.TriggerLevelOff,
                            signal.TriggerLevelOn,
                            signal.Value,
                            ltdict.GetLoadtypeDtoByLoadType(signal.LoadType),
                            Guid.NewGuid().ToString());
                        signals.Add(cessig);
                    }

                    //foreach (DeviceTaggingSet set in deviceTaggingSets) {
                    //set.AddTag(es.Name,"House Device");
                    //}
                    var lt = ltdict.GetLoadtypeDtoByLoadType(es.LoadType);
                    var ces = new CalcEnergyStorageDto(es.Name,
                        es.IntID,
                        lt,
                        es.MaximumStorageRate,
                        es.MaximumWithdrawRate,
                        es.MinimumStorageRate,
                        es.MinimumWithdrawRate,
                        es.InitialFill,
                        es.StorageCapacity,
                        householdKey,
                        Guid.NewGuid().ToString(),
                        signals);
                    calcEnergyStorages.Add(ces);
                }
                else {
                    throw new DataIntegrityException("You are trying to run a calculation with a house type that uses an energy storage, " +
                                                     " that references an load type you excluded. This will not work. Please enable the load type " +
                                                     es.LoadType + " by for example chosing All Loadtypes");
                }
            }

            return calcEnergyStorages;
        }

        [CanBeNull]
        public static CalcSpaceHeatingDto CreateSpaceHeatingObject([NotNull] House house,
                                                             [NotNull] TemperatureProfile temperatureProfile,
                                                             [NotNull] HouseholdKey householdKey,
                                                             [CanBeNull]
                                                             out CalcLocationDto heatingLocation, DateTime
                                                                 startTime, DateTime endTime,
                                                             CalcLoadTypeDtoDictionary ltDict) //, List<CalcDeviceTaggingSet> deviceTaggingSets)
        {
            if (house.HouseType == null) {
                throw new LPGException("Housetype was null");
            }

            if (house.HouseType.HeatingLoadType != null && Math.Abs(house.HouseType.HeatingYearlyTotal) > 0.0001 &&
                !double.IsNaN(house.HouseType.HeatingYearlyTotal)) {
                var degreeDays = MakeDegreeDaysClass.MakeDegreeDays(temperatureProfile,
                    startTime, endTime,
                    house.HouseType.HeatingTemperature,
                    house.HouseType.RoomTemperature,
                    house.HouseType.HeatingYearlyTotal,
                    house.HouseType.AdjustYearlyEnergy,
                    house.HouseType.ReferenceDegreeDays);
                foreach (var degreeHour in degreeDays) {
                    if (double.IsNaN(degreeHour.HeatingAmount)) {
                        throw new LPGException("Heating Amount was NaN");
                    }
                }

                var heatingParameter = new HeatingParameter(degreeDays, house.HouseType.HeatingLoadType, house.HouseType.HeatingYearlyTotal);
                var spaceheating = MakeSpaceHeatingDto(heatingParameter, ltDict, householdKey, out heatingLocation);
                return spaceheating; //,deviceTaggingSets
            }

            heatingLocation = null;
            return null;
        }

        private static CalcTransformationConditionType DetermineTransformationConditionType([NotNull] TransformationDeviceCondition condition)
        {
            CalcTransformationConditionType type;
            switch (condition.ConditionType) {
                case TransformationConditionType.MinMaxValue:
                    type = CalcTransformationConditionType.LoadtypeBalanceBetweenValues;
                    break;
                case TransformationConditionType.StorageContent:
                    type = CalcTransformationConditionType.StorageBetweenValues;
                    break;
                default:
                    throw new LPGException("unknown Type");
            }

            return type;
        }

        [CanBeNull]
        private CalcAirConditioningDto MakeAirConditioning([NotNull] TemperatureProfile temperatureProfile,
                                                           [NotNull] HouseType houseType,
                                                           [NotNull] HouseholdKey householdKey,
                                                           [CanBeNull] out CalcLocationDto airConditioningLocation)
        {
            var coolingParameter = MakeCoolingParameters(temperatureProfile, houseType);
            if (coolingParameter == null) {
                airConditioningLocation = null;
                return null;
            }

            var degreeHourDict = new List<CalcDegreeHourDto>();
            foreach (var degreeHour in coolingParameter.DegreeHours) {
                var cdd = new CalcDegreeHourDto(degreeHour.Date.Year,
                    degreeHour.Date.Month,
                    degreeHour.Date.Day,
                    degreeHour.Date.Hour,
                    degreeHour.CoolingAmount);
                degreeHourDict.Add(cdd);
            }

            CalcLoadTypeDto lt = _ltDict.GetLoadtypeDtoByLoadType(coolingParameter.CoolingLoadType);
            var cdl = new CalcDeviceLoadDto(coolingParameter.CoolingLoadType.Name,
                -1,
                lt.Name,
                lt.Guid,
                coolingParameter.YearlyConsumption,
                0,
                Guid.NewGuid().ToString(),
                1);
            var cdls = new List<CalcDeviceLoadDto> {
                cdl
            };
            airConditioningLocation = new CalcLocationDto("Air Conditioning", -100, Guid.NewGuid().ToString());
            var csh = new CalcAirConditioningDto("Air Conditioning",
                -1,
                cdls,
                degreeHourDict,
                householdKey,
                airConditioningLocation.Name,
                airConditioningLocation.Guid,
                Guid.NewGuid().ToString());
            return csh;
        }

        [NotNull]
        [ItemNotNull]
        private List<CalcTransformationDeviceDto> MakeAllTransformationDevices([NotNull] HouseType houseType,
                                                                               [NotNull] [ItemNotNull] List<CalcEnergyStorageDto> calcEnergyStorages,
                                                                               [NotNull]
                                                                               HouseholdKey householdKey) //List<CalcDeviceTaggingSet> taggingSets,
        {
            if (houseType == null) {
                throw new LPGException("Housetype was null");
            }

            var transformationDevices = new List<TransformationDevice>();
            foreach (var housetransformationDevice in houseType.HouseTransformationDevices) {
                transformationDevices.Add(housetransformationDevice.TransformationDevice);
            }

            return MakeTransformationDeviceDtos(transformationDevices, calcEnergyStorages, householdKey, houseType); //taggingSets,
        }

        private void MakeAutoDevFromDevice(EnergyIntensityType energyIntensity,
                                           [NotNull] HouseholdKey householdKey,
                                           [ItemNotNull] [NotNull] ObservableCollection<DeviceAction> deviceActions,
                                           [ItemNotNull] [NotNull] List<DeviceCategoryDto> deviceCategoryDtos,
                                           [NotNull] HouseTypeDevice hhautodev,
                                           [ItemNotNull] [NotNull] List<IAssignableDevice> allAutonomousDevices,
                                           [NotNull] CalcLocationDto calcLocation,
                                           [NotNull] AvailabilityDataReferenceDto timearray,
                                           [ItemNotNull] [NotNull] List<CalcAutoDevDto> autodevs)
        {
            if (hhautodev.LoadType == null) {
                throw new LPGException("Loadtype was null");
            }

            if (hhautodev.TimeProfile == null) {
                throw new LPGException("TimeProfile was null");
            }

            if (_ltDict.SimulateLoadtype(hhautodev.LoadType)) {
                var profile = CalcDeviceDtoFactory.GetCalcProfileDto(hhautodev.TimeProfile);
                if (hhautodev.Location == null) {
                    throw new LPGException("Location was null");
                }

                var rd = _picker.GetAutoDeviceDeviceFromDeviceCategoryOrDevice(hhautodev.Device,
                    allAutonomousDevices,
                    energyIntensity,
                    deviceActions,
                    hhautodev.Location.IntID);
                var cdl = CalcDeviceDtoFactory.MakeCalcDeviceLoads(rd, _ltDict);
                var lt = _ltDict.Ltdtodict[hhautodev.LoadType];
                CalcVariableDto variable = null;
                if (hhautodev.Variable != null) {
                    var v = hhautodev.Variable;
                    variable = _calcVariableDtoFactory.RegisterVariableIfNotRegistered(v, calcLocation.Name, calcLocation.Guid, householdKey);
                }

                List<VariableRequirementDto> reqDtos = new List<VariableRequirementDto>();
                if (variable?.Name != null) {
                    VariableRequirementDto req = new VariableRequirementDto(variable.Name,
                        hhautodev.VariableValue,
                        calcLocation.Name,
                        calcLocation.Guid,
                        hhautodev.VariableCondition,
                        variable.Guid);
                    reqDtos.Add(req);
                }

                //TODO: xxx missing probability?
                if (rd.DeviceCategory == null) {
                    throw new LPGException("Device category was null");
                }

                DeviceCategoryDto deviceCategoryDto = deviceCategoryDtos.Single(x => x.FullCategoryName == rd.DeviceCategory.FullPath);
                var cautodev = new CalcAutoDevDto(rd.Name,
                    profile,
                    lt.Name,
                    lt.Guid,
                    cdl,
                    (double)hhautodev.TimeStandardDeviation,
                    deviceCategoryDto.Guid,
                    householdKey,
                    1,
                    calcLocation.Name,
                    calcLocation.Guid,
                    deviceCategoryDto.FullCategoryName,
                    Guid.NewGuid().ToString(),
                    timearray,
                    reqDtos);
                //cautodev.ApplyBitArry(busyarr, _ltDict.LtDict[hhautodev.LoadType]);
                autodevs.Add(cautodev);
            }
        }

        private void MakeAutoDevFromDeviceAction(EnergyIntensityType energyIntensity,
                                                 [NotNull] HouseholdKey householdKey,
                                                 [ItemNotNull] [NotNull] ObservableCollection<DeviceAction> deviceActions,
                                                 [ItemNotNull] [NotNull] List<DeviceCategoryDto> deviceCategoryDtos,
                                                 [NotNull] HouseTypeDevice hhautodev,
                                                 [ItemNotNull] [NotNull] List<IAssignableDevice> allAutonomousDevices,
                                                 [NotNull] CalcLocationDto calcLocation,
                                                 [NotNull] AvailabilityDataReferenceDto availref,
                                                 [ItemNotNull] [NotNull] List<CalcAutoDevDto> autodevs)
        {
            if (hhautodev.Location == null) {
                throw new LPGException("Location was null");
            }

            if (hhautodev.Device == null) {
                throw new LPGException("Device was null");
            }

            var deviceAction = _picker.GetAutoDeviceActionFromGroup(hhautodev.Device,
                allAutonomousDevices,
                energyIntensity,
                deviceActions,
                hhautodev.Location.IntID);
            foreach (var actionProfile in deviceAction.Profiles) {
                if (actionProfile.VLoadType == null) {
                    throw new LPGException("Loadtype was null");
                }

                if (actionProfile.Timeprofile == null) {
                    throw new LPGException("Timeprofile was null");
                }

                if (_ltDict.SimulateLoadtype(actionProfile.VLoadType)) {
                    var profile = CalcDeviceDtoFactory.GetCalcProfileDto(actionProfile.Timeprofile);
                    if (deviceAction.Device == null) {
                        throw new LPGException("Device was null");
                    }

                    var cdl = CalcDeviceDtoFactory.MakeCalcDeviceLoads(deviceAction.Device, _ltDict);
                    var lt = _ltDict.Ltdtodict[actionProfile.VLoadType];
                    CalcVariableDto variable = null;
                    if (hhautodev.Variable != null) {
                        var v = hhautodev.Variable;
                        variable = _calcVariableDtoFactory.RegisterVariableIfNotRegistered(v, calcLocation.Name, calcLocation.Guid, householdKey);
                    }

                    List<VariableRequirementDto> reqDtos = new List<VariableRequirementDto>();
                    if (variable?.Name != null) {
                        VariableRequirementDto req = new VariableRequirementDto(variable.Name,
                            hhautodev.VariableValue,
                            calcLocation.Name,
                            calcLocation.Guid,
                            hhautodev.VariableCondition,
                            variable.Guid);
                        reqDtos.Add(req);
                    }

                    if (deviceAction.Device.DeviceCategory == null) {
                        throw new LPGException("Device was null");
                    }

                    DeviceCategoryDto devcat = deviceCategoryDtos.Single(x => x.FullCategoryName == deviceAction.Device?.DeviceCategory?.FullPath);
                    var cautodev = new CalcAutoDevDto(deviceAction.Device.Name,
                        profile,
                        lt.Name,
                        lt.Guid,
                        cdl,
                        (double)hhautodev.TimeStandardDeviation,
                        devcat.Guid,
                        householdKey,
                        actionProfile.Multiplier,
                        calcLocation.Name,
                        calcLocation.Guid,
                        devcat.FullCategoryName,
                        Guid.NewGuid().ToString(),
                        availref,
                        reqDtos);
                    autodevs.Add(cautodev);
                    //cautodev.ApplyBitArry(, _ltDict.LtDict[actionProfile.VLoadType]);
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [NotNull]
        [ItemNotNull]
        private List<CalcAutoDevDto> MakeCalcAutoDevsFromHouse([NotNull] TemperatureProfile temperatureProfile,
                                                               [NotNull] GeographicLocation geographicLocation,
                                                               [NotNull] [ItemNotNull] ObservableCollection<HouseTypeDevice> houseDevices,
                                                               EnergyIntensityType energyIntensity,
                                                               [NotNull] HouseholdKey householdKey,
                                                               [NotNull] string holidayKey,
                                                               [NotNull] [ItemNotNull] ObservableCollection<DeviceAction> deviceActions,
                                                               [NotNull] House house,
                                                               [NotNull] [ItemNotNull] out List<CalcLocationDto> deviceLocations,
                                                               [ItemNotNull] [NotNull] List<DeviceCategoryDto> deviceCategoryDtos)
        {
            deviceLocations = new List<CalcLocationDto>();
            var vacationTimeframes = new List<VacationTimeframe>();
            if (house.HouseType?.HouseDevices != null) {
                if (house.Households.Count == 1) {
                    vacationTimeframes.AddRange(house.Households[0].VacationTimeframes);
                }
            }

            var autodevs = new List<CalcAutoDevDto>(houseDevices.Count);
            // zur kategorien zuordnung
            var allAutonomousDevices = new List<IAssignableDevice>();
            foreach (var hhautodev in houseDevices) {
                allAutonomousDevices.Add(hhautodev.Device);
            }

            foreach (var hhautodev in houseDevices) {
                var busyarr = new BitArray(_calcParameters.InternalTimesteps);
                busyarr.SetAll(false);

                if (hhautodev.TimeLimit != null) {
                    Logger.Info("Determining the permitted times for each autonomous device. Device: " + hhautodev.Name);
                    if (hhautodev.TimeLimit.RootEntry == null) {
                        throw new LPGException(" was null");
                    }

                    busyarr = hhautodev.TimeLimit.RootEntry.GetOneYearArray(_calcParameters.InternalStepsize,
                        _calcParameters.InternalStartTime,
                        _calcParameters.InternalEndTime,
                        temperatureProfile,
                        geographicLocation,
                        _random,
                        vacationTimeframes,
                        holidayKey,
                        out _,
                        0,
                        0,
                        0,
                        0);

                    // invertieren von erlaubten zu verbotenen zeiten
                    busyarr = busyarr.Not();
                }

                var availref = _availabilityDtoRepository.MakeNewReference(hhautodev.TimeLimit?.Name ?? "No Name", busyarr);
                if (hhautodev.TimeLimit == null) {
                    throw new LPGException("Time limit was null on the autonomous device in the house " + house.Name + " for the device " +
                                           hhautodev.Name);
                }

                var timearray = _availabilityDtoRepository.MakeNewReference(hhautodev.TimeLimit.Name, busyarr);
                if (hhautodev.Location == null) {
                    throw new LPGException("Location was null");
                }

                if (hhautodev.Device == null) {
                    throw new LPGException("Device was null");
                }

                if (deviceLocations.All(x => x.Name != hhautodev.Location.Name)) {
                    deviceLocations.Add(new CalcLocationDto(hhautodev.Location.Name, hhautodev.Location.IntID, Guid.NewGuid().ToString()));
                }

                var calcLocation = deviceLocations.Single(x => x.Name == hhautodev.Location.Name);

                switch (hhautodev.Device.AssignableDeviceType) {
                    case AssignableDeviceType.Device:
                    case AssignableDeviceType.DeviceCategory:
                        MakeAutoDevFromDevice(energyIntensity,
                            householdKey,
                            deviceActions,
                            deviceCategoryDtos,
                            hhautodev,
                            allAutonomousDevices,
                            calcLocation,
                            timearray,
                            autodevs);
                        break;
                    case AssignableDeviceType.DeviceAction:
                    case AssignableDeviceType.DeviceActionGroup:
                        MakeAutoDevFromDeviceAction(energyIntensity,
                            householdKey,
                            deviceActions,
                            deviceCategoryDtos,
                            hhautodev,
                            allAutonomousDevices,
                            calcLocation,
                            availref,
                            autodevs);
                        break;
                    default:
                        throw new LPGException("Forgotten AssignableDeviceType. Please Report!");
                }
            }

            return autodevs;
        }

        [CanBeNull]
        private CoolingParameter MakeCoolingParameters([NotNull] TemperatureProfile temperatureProfile, [NotNull] HouseType houseType)
        {
            if (houseType == null) {
                throw new LPGException("Housetype was null");
            }

            if (houseType.CoolingLoadType != null && Math.Abs(houseType.CoolingYearlyTotal) > 0.000001 &&
                !double.IsNaN(houseType.CoolingYearlyTotal)) {
                var conversionFactor = _ltDict.GetLoadtypeDtoByLoadType(houseType.CoolingLoadType).ConversionFactor;
                var degreeHours = DbCalcDegreeHour.GetCalcDegreeHours(temperatureProfile,
                    _calcParameters.InternalStartTime,
                    _calcParameters.InternalEndTime,
                    houseType.CoolingTemperature,
                    houseType.CoolingYearlyTotal / conversionFactor,
                    houseType.AdjustYearlyCooling,
                    houseType.ReferenceCoolingHours);
                var isNan = false;
                foreach (var degreeHour in degreeHours) {
                    if (double.IsNaN(degreeHour.CoolingAmount)) {
                        isNan = true;
                    }
                }

                if (!isNan) {
                    return new CoolingParameter(degreeHours, houseType.CoolingLoadType, houseType.CoolingYearlyTotal);
                }
            }

            return null;
        }

        [NotNull]
        [ItemNotNull]
        private List<CalcEnergyStorageDto>
            MakeEnergyStorages([NotNull] House house, [NotNull] HouseholdKey householdKey) //, List<CalcDeviceTaggingSet> deviceTaggingSets)
        {
            var energyStorages = new List<EnergyStorage>();
            if (house.HouseType == null) {
                throw new LPGException("Housetype was null");
            }

            foreach (var houseEnergyStorage in house.HouseType.HouseEnergyStorages) {
                energyStorages.Add(houseEnergyStorage.EnergyStorage);
            }

            var calcEnergyStorages = CreateEnergyStorageDtos(energyStorages, _ltDict, householdKey); //,deviceTaggingSets);
            return calcEnergyStorages;
        }

        [NotNull]
        [ItemNotNull]
        private List<CalcGeneratorDto>
            MakeGenerators([NotNull] [ItemNotNull] List<Generator> generators,
                           [NotNull] HouseholdKey householdKey) //List<CalcDeviceTaggingSet> calcDeviceTaggingSets,
        {
            var cgens = new List<CalcGeneratorDto>();
            foreach (var gen in generators) {
                if (gen.LoadType == null) {
                    throw new LPGException("Loadtype for generator was null");
                }

                var lt = gen.LoadType;
                if (_ltDict.SimulateLoadtype(gen.LoadType)) {
                    var values = gen.GetValues(_calcParameters.InternalStartTime, _calcParameters.InternalEndTime, _calcParameters.InternalStepsize);

                    var cgen = new CalcGeneratorDto(gen.Name,
                        gen.IntID,
                        _ltDict.GetLoadtypeDtoByLoadType(lt),
                        values.ToList(),
                        householdKey,
                        Guid.NewGuid().ToString());
                    cgens.Add(cgen);
                    //TODO: add to tagging sets
                    /*foreach (CalcDeviceTaggingSet set in calcDeviceTaggingSets) {
                        set.AddTag(gen.Name, "House Devices");
                    }*/
                }
            }

            return cgens;
        }

        [NotNull]
        [ItemNotNull]
        private List<OutputLoadTypeDto> MakeOutputLoadTypeDtos([NotNull] TransformationDevice trafo)
        {
            List<OutputLoadTypeDto> outputs = new List<OutputLoadTypeDto>();
            foreach (var outlt in trafo.LoadTypesOut) {
                if (outlt.VLoadType == null) {
                    throw new LPGException("Loadtype was null");
                }

                if (_ltDict.SimulateLoadtype(outlt.VLoadType)) {
                    TransformationOutputFactorType factorType;
                    switch (outlt.FactorType) {
                        case TransformationFactorType.Fixed:
                            factorType = TransformationOutputFactorType.Fixed;
                            break;
                        case TransformationFactorType.Interpolated:
                            factorType = TransformationOutputFactorType.Interpolated;
                            break;
                        default:
                            throw new LPGException("Forgotten FactorType. Please report.");
                    }

                    outputs.Add(new OutputLoadTypeDto(_ltDict.GetLoadtypeDtoByLoadType(outlt.VLoadType), outlt.Factor, factorType));
                }
            }

            return outputs;
        }

        [CanBeNull]
        private static CalcSpaceHeatingDto MakeSpaceHeatingDto([NotNull] HeatingParameter heatingparameter,
                                                        [NotNull] CalcLoadTypeDtoDictionary ltDict,
                                                        [NotNull] HouseholdKey householdKey,
                                                        [CanBeNull] out CalcLocationDto heatingLocation)
            //, List<CalcDeviceTaggingSet> taggingSets)
        {
            if (!ltDict.SimulateLoadtype(heatingparameter.HeatingLoadType)) {
                heatingLocation = null;
                return null;
            }

            var degreeDayDict = new List<CalcDegreeDayDto>();
            foreach (var degreeDay in heatingparameter.DegreeDays) {
                var cdd = new CalcDegreeDayDto(degreeDay.Date.Year, degreeDay.Date.Month, degreeDay.Date.Day, degreeDay.HeatingAmount);
                degreeDayDict.Add(cdd);
            }

            var lt = ltDict.GetLoadtypeDtoByLoadType(heatingparameter.HeatingLoadType);
            var cdl = new CalcDeviceLoadDto(heatingparameter.HeatingLoadType.Name,
                -1,
                lt.Name,
                lt.Guid,
                heatingparameter.YearlyConsumption,
                0,
                Guid.NewGuid().ToString(),
                1);
            var cdls = new List<CalcDeviceLoadDto> {
                cdl
            };
            heatingLocation = new CalcLocationDto("Air Conditioning", -101, Guid.NewGuid().ToString());
            var csh = new CalcSpaceHeatingDto("Space Heating",
                -1,
                cdls,
                degreeDayDict,
                householdKey,
                heatingLocation.Name,
                heatingLocation.Guid,
                Guid.NewGuid().ToString());
            //foreach (var calcDeviceTaggingSet in taggingSets) {
            //calcDeviceTaggingSet.AddTag("Space Heating","House Device");
            //}
            return csh;
        }

        [NotNull]
        [ItemNotNull]
        private List<CalcTransformationDeviceDto> MakeTransformationDeviceDtos(
            [NotNull] [ItemNotNull] List<TransformationDevice> transformationDevices,
            [NotNull] [ItemNotNull] List<CalcEnergyStorageDto> storages,
            //List<CalcDeviceTaggingSet> taggingSets,
            [NotNull] HouseholdKey householdKey,
            [NotNull] HouseType houseType)
        {
            var ctds = new List<CalcTransformationDeviceDto>();
            foreach (var trafo in transformationDevices) {
                //foreach (CalcDeviceTaggingSet set in taggingSets) {
                //set.AddTag(trafo.Name,"House Device");
                //}
                if (trafo.LoadTypeIn == null) {
                    throw new LPGException("Transformation device loadtype was null");
                }

                if (!_ltDict.SimulateLoadtype(trafo.LoadTypeIn)) {
                    continue;
                }

                var inputLoadType = _ltDict.GetLoadtypeDtoByLoadType(trafo.LoadTypeIn);

                var outputs = MakeOutputLoadTypeDtos(trafo);

                List<DataPointDto> datapoints = null;
                List<CalcTransformationConditionDto> calcconditions = new List<CalcTransformationConditionDto>();
                if (outputs.Any()) {
                    datapoints = new List<DataPointDto>();
                    foreach (var datapoint in trafo.FactorDatapoints) {
                        datapoints.Add(new DataPointDto(datapoint.ReferenceValue, datapoint.Factor));
                    }

                    CreateCalcConditions(storages, houseType, trafo, calcconditions);
                }

                var ctd = new CalcTransformationDeviceDto(trafo.Name,
                    trafo.IntID,
                    trafo.MinValue,
                    trafo.MaxValue,
                    trafo.MinimumInputPower,
                    trafo.MaximumInputPower,
                    householdKey,
                    Guid.NewGuid().ToString(),
                    calcconditions,
                    datapoints,
                    outputs,
                    inputLoadType);
                ctds.Add(ctd);
            }

            return ctds;
        }
    }
}
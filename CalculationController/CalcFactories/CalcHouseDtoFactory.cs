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
        [JetBrains.Annotations.NotNull] private readonly AvailabilityDtoRepository _availabilityDtoRepository;

        [JetBrains.Annotations.NotNull] private readonly CalcParameters _calcParameters;

        [JetBrains.Annotations.NotNull] private readonly CalcVariableDtoFactory _calcVariableDtoFactory;

        [JetBrains.Annotations.NotNull] private readonly CalcModularHouseholdDtoFactory _hhDtoFactory;

        [JetBrains.Annotations.NotNull] private readonly CalcLoadTypeDtoDictionary _ltDict;

        [JetBrains.Annotations.NotNull] private readonly IDeviceCategoryPicker _picker;

        [JetBrains.Annotations.NotNull] private readonly Random _random;

        public CalcHouseDtoFactory([JetBrains.Annotations.NotNull] CalcLoadTypeDtoDictionary ltDict,
                                   [JetBrains.Annotations.NotNull] Random random,
                                   [JetBrains.Annotations.NotNull] IDeviceCategoryPicker picker,
                                   [JetBrains.Annotations.NotNull] CalcParameters calcParameters,
                                   [JetBrains.Annotations.NotNull] CalcModularHouseholdDtoFactory hhDtoFactory,
                                   [JetBrains.Annotations.NotNull] AvailabilityDtoRepository availabilityDtoRepository,
                                   [JetBrains.Annotations.NotNull] CalcVariableDtoFactory calcVariableDtoFactory)
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
        [JetBrains.Annotations.NotNull]
        public CalcHouseDto MakeHouseDto([JetBrains.Annotations.NotNull] Simulator sim,
                                      [JetBrains.Annotations.NotNull] House house,
                                      [JetBrains.Annotations.NotNull] TemperatureProfile temperatureProfile,
                                      [JetBrains.Annotations.NotNull] GeographicLocation geographicLocation, //List<CalcDeviceTaggingSet> taggingSets,
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
            foreach (var deviceCategory in sim.DeviceCategories.Items) {
                deviceCategoryDtos.Add(new DeviceCategoryDto(deviceCategory.FullPath, Guid.NewGuid().ToStrGuid()));
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
                        household.ChargingStationSet);
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
                sim.DeviceActions.Items,
                house,
                out var devLocations,
                deviceCategoryDtos);
            houseLocations.AddRange(devLocations);
            autoDevs.AddRange(autodevs2);
            // energy Storage
            var calcEnergyStorages = MakeEnergyStorages(house, houseKey); //, taggingSets);
            // transformation devices
            var transformationDevices = MakeAllTransformationDevices(house.HouseType,  houseKey); //taggingSets,

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

        private void CreateCalcConditions([JetBrains.Annotations.NotNull] TransformationDevice trafo,
                                          [JetBrains.Annotations.NotNull] [ItemNotNull] List<CalcTransformationConditionDto> calcconditions)
        {
            foreach (var condition in trafo.Conditions) {
                CalcVariableDto variableDto = _calcVariableDtoFactory.RegisterVariableIfNotRegistered(
                    condition.Variable,
                    "House", Constants.HouseLocationGuid, Constants.HouseKey);

                CalcTransformationConditionDto trafocondition = new CalcTransformationConditionDto(condition.Name,
                    condition.IntID,
                    variableDto,
                    condition.MinValue,
                    condition.MaxValue,
                    Guid.NewGuid().ToStrGuid());
                calcconditions.Add(trafocondition);
            }
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private static List<CalcEnergyStorageDto> CreateEnergyStorageDtos([JetBrains.Annotations.NotNull] [ItemNotNull] List<EnergyStorage> energyStorages,
                                                                          [JetBrains.Annotations.NotNull] CalcLoadTypeDtoDictionary ltdict,
                                                                          [JetBrains.Annotations.NotNull] HouseholdKey householdKey, CalcVariableDtoFactory calcVariableDtoFactory)
        {
            var calcEnergyStorages = new List<CalcEnergyStorageDto>();
            foreach (var es in energyStorages) {
                if (es.LoadType == null) {
                    throw new LPGException("Energy storage load type was null");
                }

                if (ltdict.SimulateLoadtype(es.LoadType)) {
                    List<CalcEnergyStorageSignalDto> signals = new List<CalcEnergyStorageSignalDto>();
                    foreach (var signal in es.Signals) {
                        if (signal.Variable == null) {
                            throw new DataIntegrityException("Signal variable was null");
                        }

                        var cvdto = calcVariableDtoFactory.RegisterVariableIfNotRegistered(signal.Variable, "House",
                            Constants.HouseLocationGuid, Constants.HouseKey);
                        var cessig = new CalcEnergyStorageSignalDto(signal.Name,
                            signal.IntID,
                            signal.TriggerLevelOff,
                            signal.TriggerLevelOn,
                            signal.Value,
                            cvdto,
                            Guid.NewGuid().ToStrGuid());
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
                        Guid.NewGuid().ToStrGuid(),
                        signals);
                    calcEnergyStorages.Add(ces);
                    if (es.Signals.Count != ces.Signals.Count) {
                        throw new LPGException("Signals for energy storage were not correctly initialized");
                    }
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
        public static CalcSpaceHeatingDto CreateSpaceHeatingObject([JetBrains.Annotations.NotNull] House house,
                                                             [JetBrains.Annotations.NotNull] TemperatureProfile temperatureProfile,
                                                             [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
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

        [CanBeNull]
        private CalcAirConditioningDto MakeAirConditioning([JetBrains.Annotations.NotNull] TemperatureProfile temperatureProfile,
                                                           [JetBrains.Annotations.NotNull] HouseType houseType,
                                                           [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
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
                Guid.NewGuid().ToStrGuid(),
                1);
            var cdls = new List<CalcDeviceLoadDto> {
                cdl
            };
            airConditioningLocation = new CalcLocationDto("Air Conditioning", -100, Guid.NewGuid().ToStrGuid());
            var csh = new CalcAirConditioningDto("Air Conditioning",
                -1,
                cdls,
                degreeHourDict,
                householdKey,
                airConditioningLocation.Name,
                airConditioningLocation.Guid,
                Guid.NewGuid().ToStrGuid());
            return csh;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<CalcTransformationDeviceDto> MakeAllTransformationDevices([JetBrains.Annotations.NotNull] HouseType houseType,
                                                                               [JetBrains.Annotations.NotNull]
                                                                               HouseholdKey householdKey) //List<CalcDeviceTaggingSet> taggingSets,
        {
            if (houseType == null) {
                throw new LPGException("Housetype was null");
            }

            var transformationDevices = new List<TransformationDevice>();
            foreach (var housetransformationDevice in houseType.HouseTransformationDevices) {
                transformationDevices.Add(housetransformationDevice.TransformationDevice);
            }

            return MakeTransformationDeviceDtos(transformationDevices, householdKey);
        }

        [CanBeNull]
        private CalcAutoDevDto MakeAutoDevFromDevice(EnergyIntensityType energyIntensity,
                                                     [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
                                                     [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceAction> deviceActions,
                                                     [ItemNotNull] [JetBrains.Annotations.NotNull] List<DeviceCategoryDto> deviceCategoryDtos,
                                                     [JetBrains.Annotations.NotNull] HouseTypeDevice hhautodev,
                                                     [ItemNotNull] [JetBrains.Annotations.NotNull] List<IAssignableDevice> allAutonomousDevices,
                                                     [JetBrains.Annotations.NotNull] CalcLocationDto calcLocation,
                                                     [JetBrains.Annotations.NotNull] AvailabilityDataReferenceDto timearray
                                                     )
        {
            if (hhautodev.LoadType == null) {
                throw new LPGException("Loadtype was null");
            }

            if (hhautodev.TimeProfile == null) {
                throw new LPGException("TimeProfile was null");
            }

            if (!_ltDict.SimulateLoadtype(hhautodev.LoadType)) {
                return null;
            }
            var profile = CalcDeviceDtoFactory.GetCalcProfileDto(hhautodev.TimeProfile);
            var lt = _ltDict.Ltdtodict[hhautodev.LoadType];

            if (hhautodev.Location == null) {
                throw new LPGException("Location was null");
            }

            var rd = _picker.GetAutoDeviceDeviceFromDeviceCategoryOrDevice(hhautodev.Device,
                allAutonomousDevices,
                energyIntensity,
                deviceActions,
                hhautodev.Location.IntID);
            var cdl = CalcDeviceDtoFactory.MakeCalcDeviceLoads(rd, _ltDict);
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
            List<CalcAutoDevProfileDto> cadps = new List<CalcAutoDevProfileDto>();
            CalcAutoDevProfileDto cadp = new CalcAutoDevProfileDto(profile, lt.Name,lt.Guid,1);
            cadps.Add(cadp);
            DeviceCategoryDto deviceCategoryDto = deviceCategoryDtos.Single(x => x.FullCategoryName == rd.DeviceCategory.FullPath);
            var cautodev = new CalcAutoDevDto(rd.Name,
                cadps,
                cdl,
                (double)hhautodev.TimeStandardDeviation,
                deviceCategoryDto.Guid,
                householdKey,
                calcLocation.Name,
                calcLocation.Guid,
                deviceCategoryDto.FullCategoryName,
                Guid.NewGuid().ToStrGuid(),
                timearray,
                reqDtos,deviceCategoryDto.FullCategoryName);
            //cautodev.ApplyBitArry(busyarr, _ltDict.LtDict[hhautodev.LoadType]);
            return cautodev;

        }

        private void MakeAutoDevFromDeviceAction(EnergyIntensityType energyIntensity,
                                                 [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
                                                 [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<DeviceAction> deviceActions,
                                                 [ItemNotNull] [JetBrains.Annotations.NotNull] List<DeviceCategoryDto> deviceCategoryDtos,
                                                 [JetBrains.Annotations.NotNull] HouseTypeDevice hhautodev,
                                                 [ItemNotNull] [JetBrains.Annotations.NotNull] List<IAssignableDevice> allAutonomousDevices,
                                                 [JetBrains.Annotations.NotNull] CalcLocationDto calcLocation,
                                                 [JetBrains.Annotations.NotNull] AvailabilityDataReferenceDto availref, [JetBrains.Annotations.NotNull] List<CalcAutoDevDto> autodevs
                                                 )
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
            List<CalcAutoDevProfileDto> cadpdtos = new List<CalcAutoDevProfileDto>();
            foreach (var actionProfile in deviceAction.Profiles) {
                if (actionProfile.VLoadType == null) {
                    throw new LPGException("Loadtype was null");
                }

                if (actionProfile.Timeprofile == null) {
                    throw new LPGException("Timeprofile was null");
                }

                if (!_ltDict.SimulateLoadtype(actionProfile.VLoadType)) {
                    continue;
                }

                var profile = CalcDeviceDtoFactory.GetCalcProfileDto(actionProfile.Timeprofile);
                if (deviceAction.Device == null) {
                    throw new LPGException("Device was null");
                }


                var lt = _ltDict.Ltdtodict[actionProfile.VLoadType];
                CalcAutoDevProfileDto cadpdto =
                    new CalcAutoDevProfileDto(profile, lt.Name, lt.Guid, actionProfile.Multiplier);
                cadpdtos.Add(cadpdto);
            }

            var cdl = CalcDeviceDtoFactory.MakeCalcDeviceLoads(deviceAction.Device ?? throw new LPGException("device action was null"), _ltDict);
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
                cadpdtos,
                cdl,
                (double)hhautodev.TimeStandardDeviation,
                devcat.Guid,
                householdKey,
                calcLocation.Name,
                calcLocation.Guid,
                devcat.FullCategoryName,
                Guid.NewGuid().ToStrGuid(),
                availref,
                reqDtos, devcat.FullCategoryName);
            autodevs.Add(cautodev);
            //cautodev.ApplyBitArry(, _ltDict.LtDict[actionProfile.VLoadType]);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<CalcAutoDevDto> MakeCalcAutoDevsFromHouse([JetBrains.Annotations.NotNull] TemperatureProfile temperatureProfile,
                                                               [JetBrains.Annotations.NotNull] GeographicLocation geographicLocation,
                                                               [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<HouseTypeDevice> houseDevices,
                                                               EnergyIntensityType energyIntensity,
                                                               [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
                                                               [JetBrains.Annotations.NotNull] string holidayKey,
                                                               [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<DeviceAction> deviceActions,
                                                               [JetBrains.Annotations.NotNull] House house,
                                                               [JetBrains.Annotations.NotNull] [ItemNotNull] out List<CalcLocationDto> deviceLocations,
                                                               [ItemNotNull] [JetBrains.Annotations.NotNull] List<DeviceCategoryDto> deviceCategoryDtos)
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
                    Logger.Debug("Determining the permitted times for each autonomous device. Device: " + hhautodev.Name);
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
                    deviceLocations.Add(new CalcLocationDto(hhautodev.Location.Name, hhautodev.Location.IntID, Guid.NewGuid().ToStrGuid()));
                }

                var calcLocation = deviceLocations.Single(x => x.Name == hhautodev.Location.Name);

                switch (hhautodev.Device.AssignableDeviceType) {
                    case AssignableDeviceType.Device:
                    case AssignableDeviceType.DeviceCategory:
                        var adev1 = MakeAutoDevFromDevice(energyIntensity,
                            householdKey,
                            deviceActions,
                            deviceCategoryDtos,
                            hhautodev,
                            allAutonomousDevices,
                            calcLocation,
                            timearray);
                        autodevs.Add(adev1);
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
                            availref,autodevs);
                        break;
                    default:
                        throw new LPGException("Forgotten AssignableDeviceType. Please Report!");
                }
            }

            if (autodevs.Count > houseDevices.Count) {
                throw new LPGException("Made too many autonomous devices in the house. This is a bug.");
            }
            return autodevs;
        }

        [CanBeNull]
        private CoolingParameter MakeCoolingParameters([JetBrains.Annotations.NotNull] TemperatureProfile temperatureProfile, [JetBrains.Annotations.NotNull] HouseType houseType)
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

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<CalcEnergyStorageDto>  MakeEnergyStorages([JetBrains.Annotations.NotNull] House house, [JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            var energyStorages = new List<EnergyStorage>();
            if (house.HouseType == null) {
                throw new LPGException("Housetype was null");
            }

            foreach (var houseEnergyStorage in house.HouseType.HouseEnergyStorages) {
                energyStorages.Add(houseEnergyStorage.EnergyStorage);
            }

            var calcEnergyStorages = CreateEnergyStorageDtos(energyStorages, _ltDict, householdKey, _calcVariableDtoFactory); //,deviceTaggingSets);
            return calcEnergyStorages;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<CalcGeneratorDto>
            MakeGenerators([JetBrains.Annotations.NotNull] [ItemNotNull] List<Generator> generators,
                           [JetBrains.Annotations.NotNull] HouseholdKey householdKey) //List<CalcDeviceTaggingSet> calcDeviceTaggingSets,
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
                        Guid.NewGuid().ToStrGuid());
                    cgens.Add(cgen);
                    //TODO: add to tagging sets
                    /*foreach (CalcDeviceTaggingSet set in calcDeviceTaggingSets) {
                        set.AddTag(gen.Name, "House Devices");
                    }*/
                }
            }

            return cgens;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<OutputLoadTypeDto> MakeOutputLoadTypeDtos([JetBrains.Annotations.NotNull] TransformationDevice trafo)
        {
            List<OutputLoadTypeDto> outputs = new List<OutputLoadTypeDto>();
            foreach (var outlt in trafo.LoadTypesOut) {
                if (outlt.VLoadType == null) {
                    throw new LPGException("Loadtype was null");
                }

                if (_ltDict.SimulateLoadtype(outlt.VLoadType)) {
                    TransformationOutputFactorType factorType;
                    switch (outlt.FactorType) {
                        case TransformationFactorType.FixedFactor:
                            factorType = TransformationOutputFactorType.FixedFactor;
                            break;
                        case TransformationFactorType.Interpolated:
                            factorType = TransformationOutputFactorType.Interpolated;
                            break;
                        case TransformationFactorType.FixedValue:
                            factorType = TransformationOutputFactorType.FixedValue;
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
        private static CalcSpaceHeatingDto MakeSpaceHeatingDto([JetBrains.Annotations.NotNull] HeatingParameter heatingparameter,
                                                        [JetBrains.Annotations.NotNull] CalcLoadTypeDtoDictionary ltDict,
                                                        [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
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
                Guid.NewGuid().ToStrGuid(),
                1);
            var cdls = new List<CalcDeviceLoadDto> {
                cdl
            };
            heatingLocation = new CalcLocationDto("Space Heating Location", -102, Guid.NewGuid().ToStrGuid());
            var csh = new CalcSpaceHeatingDto("Space Heating",
                -1,
                cdls,
                degreeDayDict,
                householdKey,
                heatingLocation.Name,
                heatingLocation.Guid,
                Guid.NewGuid().ToStrGuid());
            //foreach (var calcDeviceTaggingSet in taggingSets) {
            //calcDeviceTaggingSet.AddTag("Space Heating","House Device");
            //}
            return csh;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<CalcTransformationDeviceDto> MakeTransformationDeviceDtos(
            [JetBrains.Annotations.NotNull] [ItemNotNull] List<TransformationDevice> transformationDevices,
            [JetBrains.Annotations.NotNull] HouseholdKey householdKey)
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

                    CreateCalcConditions( trafo, calcconditions);
                }

                var ctd = new CalcTransformationDeviceDto(trafo.Name,
                    trafo.IntID,
                    trafo.MinValue,
                    trafo.MaxValue,
                    trafo.MinimumInputPower,
                    trafo.MaximumInputPower,
                    householdKey,
                    Guid.NewGuid().ToStrGuid(),
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.Helpers;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace CalculationController.DtoFactories
{
    public class CalcAffordanceDtoFactory {
        [NotNull]
        private readonly CalcParameters _cp;
        [NotNull]
        private readonly IDeviceCategoryPicker _picker;
        [NotNull]
        private readonly CalcVariableDtoFactory _variableRepository;
        [NotNull]
        private readonly AvailabilityDtoRepository _availabilityDtoRepository;

        public CalcAffordanceDtoFactory([NotNull] CalcParameters cp, [NotNull] IDeviceCategoryPicker picker,
            [NotNull] CalcVariableDtoFactory variableRepository, [NotNull] AvailabilityDtoRepository availabilityDtoRepository)
        {
            _cp = cp;
            _picker = picker;
            _variableRepository = variableRepository;
            _availabilityDtoRepository = availabilityDtoRepository;
        }

        /// <summary>
        ///     Sets all calc affordances
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public List<CalcAffordanceDto> SetCalcAffordances([NotNull][ItemNotNull] IEnumerable<CalcLocationDto> locs,
            [NotNull] TemperatureProfile temperatureProfile,
            [NotNull] CalcLoadTypeDtoDictionary ltdict,
            [NotNull] GeographicLocation geographicLocation,
            [NotNull] Random rnd,
            int timeStepsPerHour, TimeSpan internalStepSize,
            [NotNull][ItemNotNull] List<VacationTimeframe> vacationTimeframes, [NotNull] string holidayKey,
            [NotNull][ItemNotNull] ObservableCollection<DeviceAction> deviceActions,
            [NotNull] Dictionary<CalcLocationDto, List<AffordanceWithTimeLimit>> affordanceDict,
            [NotNull] LocationDtoDict locDict,
            [NotNull] out List<DateTime> bridgeDays,
            [NotNull] HouseholdKey householdKey, [NotNull][ItemNotNull] List<CalcDeviceDto> allCalcDeviceDtos,
            [ItemNotNull] [NotNull] List<DeviceCategoryDto> deviceCategoryDtos )
        {
            List<CalcAffordanceDto> allCalcAffordances = new List<CalcAffordanceDto>();
            bridgeDays = new List<DateTime>();
            // get affordances
            foreach (var calcLocation in locs)
            {
                var affs = affordanceDict[calcLocation];
                var tmp = affs.Distinct();
                if (affs.Count != tmp.Count())
                {
                    throw new LPGException("double affordances!?!");
                }

                var devicesAtLoc = allCalcDeviceDtos.Where(x => x.LocationGuid == calcLocation.Guid).ToList();
                var affordances= GetCalcAffordancesAtLocation(calcLocation, affs, internalStepSize, timeStepsPerHour, temperatureProfile,
                    ltdict, geographicLocation, rnd, vacationTimeframes, holidayKey, deviceActions,
                    locDict,
                    out var tmpBridgeDays, householdKey,devicesAtLoc, deviceCategoryDtos);
                allCalcAffordances.AddRange(affordances);
                foreach (var tmpBridgeDay in tmpBridgeDays)
                {
                    if (!bridgeDays.Contains(tmpBridgeDay))
                    {
                        bridgeDays.Add(tmpBridgeDay);
                    }
                }

                //calcLocation.SortAffordances();
            }

            return allCalcAffordances;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [NotNull]
        [ItemNotNull]
        private List<CalcAffordanceDto> GetCalcAffordancesAtLocation([NotNull] CalcLocationDto calcloc,
            [NotNull] List<AffordanceWithTimeLimit> affordancesAtLocation,
            TimeSpan internalStepSize, int timeStepsPerHour,
            [NotNull] TemperatureProfile temperatureProfile,
            [NotNull] CalcLoadTypeDtoDictionary ltdict,
            [NotNull] GeographicLocation geographicLocation, [NotNull] Random rnd,
            [NotNull][ItemNotNull] List<VacationTimeframe> vacationTimeframes,
            [NotNull] string holidayKey,
            [NotNull][ItemNotNull] ObservableCollection<DeviceAction> allDeviceActions,
            [NotNull]
            LocationDtoDict locDict,
            [NotNull] out List<DateTime> bridgeDays, [NotNull] HouseholdKey householdKey,
            [NotNull][ItemNotNull] List<CalcDeviceDto> deviceDtosAtLocation,
            [ItemNotNull] [NotNull] List<DeviceCategoryDto> deviceCategoryDtos)
        {
            List<CalcAffordanceDto> createdAffordances = new List<CalcAffordanceDto>();
            bridgeDays = new List<DateTime>();
            foreach (AffordanceWithTimeLimit aff in affordancesAtLocation)
            {
                var affordanceName =CalcAffordanceFactory.FixAffordanceName(aff.Affordance.Name, _cp.CSVCharacter);
                if(aff.Affordance.PersonProfile == null) {
                    throw new DataIntegrityException("Person profile on " + aff.Affordance.PrettyName + " was null. Please fix.");
                }

                var cp1 = CalcDeviceDtoFactory.GetCalcProfileDto(aff.Affordance.PersonProfile);
                List<CalcDesireDto> calcDesires = MakeCalcDesires(timeStepsPerHour, aff, affordanceName);
                List<CalcAffordanceVariableOpDto> variableOps = MakeVariableOps(calcloc, locDict, householdKey, aff);
                List<VariableRequirementDto> variableRequirements =
                    MakeVariableRequirements(calcloc, locDict, householdKey, aff);

                MakeAffordanceTimelimit(temperatureProfile, geographicLocation, rnd, vacationTimeframes,
                    holidayKey, bridgeDays, aff, out var availabilityDataReference);

                //make the affordance
                Logger.Info("Converting the time limit to a bitarray for the affordance " + aff.Affordance.Name);
                //caff.IsBusyArray = tmparr;
                string timeLimitName = "";

                if (aff.TimeLimit != null) {
                    timeLimitName = aff.TimeLimit.Name;
                }

                var caff = new CalcAffordanceDto(affordanceName, aff.Affordance.IntID, cp1, calcloc.Name,calcloc.Guid,
                    aff.Affordance.RandomDesireResults, calcDesires, aff.Affordance.MinimumAge,
                    aff.Affordance.MaximumAge,
                    aff.Affordance.PermittedGender,
                    aff.Affordance.NeedsLight, (double)aff.Affordance.TimeStandardDeviation,
                    aff.Affordance.CarpetPlotColor.R, aff.Affordance.CarpetPlotColor.G, aff.Affordance.CarpetPlotColor.B,
                    aff.Affordance.AffCategory,
                    aff.Affordance.IsInterruptable, aff.Affordance.IsInterrupting, variableOps, variableRequirements,
                    aff.Affordance.ActionAfterInterruption, timeLimitName, aff.Weight,
                    aff.Affordance.RequireAllDesires, aff.SrcTraitName,
                    Guid.NewGuid().ToString(), availabilityDataReference,householdKey);
                foreach (var devtup in aff.Affordance.AffordanceDevices)
                {
                    MakeAffordanceDevices(calcloc, internalStepSize, ltdict, allDeviceActions, aff, caff, devtup, deviceDtosAtLocation,
                        deviceCategoryDtos);
                }

                MakeSubAffordances(caff, aff.Affordance, timeStepsPerHour, internalStepSize, calcloc,
                    locDict, _cp.CSVCharacter, aff.Weight, aff.SrcTraitName, householdKey);
                createdAffordances.Add(caff);
            }
            return createdAffordances;
        }

        private void MakeAffordanceDevices([NotNull] CalcLocationDto calcloc, TimeSpan internalStepSize,
            [NotNull] CalcLoadTypeDtoDictionary ltdict,
            [NotNull][ItemNotNull] ObservableCollection<DeviceAction> allDeviceActions,
            AffordanceWithTimeLimit aff, [NotNull] CalcAffordanceDto caff, [NotNull] AffordanceDevice devtup,
            [NotNull][ItemNotNull] List<CalcDeviceDto> devicesAtLocation,
            [ItemNotNull] [NotNull] List<DeviceCategoryDto> deviceCategoryDtos)
        {
            if (devtup.Device == null) {
                throw new LPGException("Device was null");
            }
            // pick the device itself
            var pickedDevice = _picker.GetDeviceDtoForAffordance(devtup.Device, devicesAtLocation,
                calcloc.ID, allDeviceActions,deviceCategoryDtos);
            if (pickedDevice == null)
            {
                throw new DataIntegrityException(
                    "Affordance " + aff.Affordance.Name + " has broken devices. Please fix",
                    aff.Affordance);
            }

            // find the device in the calc devices
            var dev = devicesAtLocation.Single(calcDevice => calcDevice.Name == pickedDevice.Name);
            if (dev == null)
            {
                throw new DataIntegrityException(
                    "Affordance " + aff.Affordance.Name + " has broken devices. Please fix",
                    aff.Affordance);
            }

            if (devtup.Device == null)
            {
                throw new LPGException("Device was null");
            }

            switch (devtup.Device.AssignableDeviceType)
            {
                case AssignableDeviceType.Device:
                case AssignableDeviceType.DeviceCategory:
                {
                    if (devtup.LoadType == null) {
                        throw new LPGException("No load type set");
                    }
                    if (ltdict.SimulateLoadtype(devtup.LoadType))
                    {
                        if (devtup.TimeProfile == null)
                        {
                            throw new DataIntegrityException(
                                "Affordance " + aff.Affordance.Name + " has broken time profiles. Please fix.",
                                aff.Affordance);
                        }

                        var newprof =
                            CalcDeviceDtoFactory.GetCalcProfileDto(devtup.TimeProfile);
                        CalcLoadTypeDto loadtype = ltdict.GetLoadtypeDtoByLoadType(devtup.LoadType);
                        caff.AddDeviceTuple(dev.Name, dev.Guid, newprof,loadtype.Name,loadtype.Guid,
                            devtup.TimeOffset,internalStepSize, 1, devtup.Probability);
                    }
                }
                    break;
                case AssignableDeviceType.DeviceAction:
                case AssignableDeviceType.DeviceActionGroup:
                {
                    DeviceAction da;
                    // if it's a device action group, then go back to the picker to select a specific
                    // device action based on the available devices
                    if (devtup.Device.AssignableDeviceType == AssignableDeviceType.DeviceActionGroup)
                    {
                        da =
                            _picker.GetDeviceActionFromGroup(devtup.Device, devicesAtLocation,
                                allDeviceActions);
                    }
                    else
                    {
                        da = (DeviceAction)devtup.Device;
                    }
                    if(da == null) {
                        throw new LPGException("Device action was null");
                    }

                    if (da.Profiles.Count == 0)
                    {
                        throw new DataIntegrityException(
                            "The device action " + da.Name + " has no time profiles. Please fix", da);
                    }

                    foreach (var profile in da.Profiles)
                    {
                        if(profile.VLoadType==null) {
                            throw new LPGException("Profile was null");
                        }

                        if (ltdict.SimulateLoadtype(profile.VLoadType))
                        {
                            if (profile.Timeprofile == null)
                            {
                                throw new DataIntegrityException(
                                    "The device action " + da.Name + " has broken time profiles. Please fix.",
                                    da);
                            }

                            if (profile.VLoadType == null)
                            {
                                throw new DataIntegrityException(
                                    "The device action " + da.Name + " has broken load types. Please fix.", da);
                            }

                            var newprof =
                                CalcDeviceDtoFactory.GetCalcProfileDto(profile.Timeprofile);
                            CalcLoadTypeDto loadType = ltdict.GetLoadtypeDtoByLoadType(profile.VLoadType);
                            caff.AddDeviceTuple(dev.Name,dev.Guid, newprof, loadType.Name,loadType.Guid,
                                profile.TimeOffset + devtup.TimeOffset, internalStepSize, profile.Multiplier,
                                devtup.Probability);
                        }
                    }
                }
                    break;
                default:
                    throw new LPGException(
                        "Missing an AssignableDeviceType at GetCalcAffordancesAtLocation! Please report to the programmer.");
            }
        }

        private void MakeAffordanceTimelimit([NotNull] TemperatureProfile temperatureProfile,
            [NotNull] GeographicLocation geographicLocation,
            [NotNull] Random rnd, [NotNull][ItemNotNull] List<VacationTimeframe> vacationTimeframes,
            [NotNull] string holidayKey, [NotNull] List<DateTime> bridgeDays,
            AffordanceWithTimeLimit aff,
                                                 [NotNull] out AvailabilityDataReferenceDto availabilityDataReference)
        {
            //time limit stuff
            if (aff.Affordance.TimeLimit == null)
            {
                throw new DataIntegrityException("The time limit on the affordance was null. Please fix",
                    aff.Affordance);
            }

            var tl = aff.Affordance.TimeLimit;
            if (aff.TimeLimit != null)
            {
                tl = aff.TimeLimit;
            }
            if(tl.RootEntry == null) {
                throw new LPGException("Root Entry was null");
            }

            var tmparr = tl.RootEntry.GetOneYearArray(
                _cp.InternalStepsize,
                _cp.InternalStartTime,
                _cp.InternalEndTime, temperatureProfile, geographicLocation, rnd,
                vacationTimeframes, holidayKey, out var tmpBridgeDays, aff.StartMinusTime, aff.StartPlusTime,
                aff.EndMinusTime,
                aff.EndPlusTime);
            foreach (var tmpBridgeDay in tmpBridgeDays)
            {
                if (!bridgeDays.Contains(tmpBridgeDay))
                {
                    bridgeDays.Add(tmpBridgeDay);
                }
            }

            // invertieren von erlaubten zu verbotenen zeiten
            tmparr = tmparr.Not();
            availabilityDataReference = _availabilityDtoRepository.MakeNewReference(tl.Name, tmparr);
        }

        [NotNull]
        [ItemNotNull]
        private static List<CalcDesireDto> MakeCalcDesires(int timeStepsPerHour, AffordanceWithTimeLimit aff,
            [NotNull] string affordanceName)
        {
            var calcDesires = new List<CalcDesireDto>();
            foreach (var affDesire in aff.Affordance.AffordanceDesires)
            {
                var cd = new CalcDesireDto(affordanceName + " - " + affDesire.Name, affDesire.Desire.IntID, 0,
                    0, affDesire.SatisfactionValue, 0, timeStepsPerHour, -1, null, aff.SrcTraitName, "");
                calcDesires.Add(cd);
            }

            return calcDesires;
        }

        private void MakeSubAffordances([NotNull] CalcAffordanceDto caff, [NotNull] Affordance aff, int timeStepsPerHour,
            TimeSpan internalStepSize, [NotNull] CalcLocationDto calcloc,
            [NotNull] LocationDtoDict locDict,
            [NotNull] string csvCharacter, int weight, [NotNull] string srcTrait,
            [NotNull] HouseholdKey householdKey)
        {
            // Subaffordanzen durchgehen
            foreach (var affsubaff in aff.SubAffordances)
            {
                var calcDesires = new List<CalcDesireDto>();
                if (affsubaff.SubAffordance == null)
                {
                    throw new LPGException("SubAffordance was null");
                }

                var name = CalcAffordanceFactory.FixAffordanceName(affsubaff.SubAffordance.Name, csvCharacter);
                // f�r alle desires calcdesires anlegen
                foreach (var subaffDesire in affsubaff.SubAffordance.SubAffordanceDesires)
                {
                    if (subaffDesire.Desire == null)
                    {
                        throw new LPGException("Desire was null");
                    }

                    var cd = new CalcDesireDto(name + " - " + subaffDesire.Name, subaffDesire.Desire.IntID, 0, 0,
                        subaffDesire.SatisfactionValue, 0, timeStepsPerHour, subaffDesire.Desire.CriticalThreshold,
                        null, srcTrait, "");
                    calcDesires.Add(cd);
                }

                var minutesperstep = (decimal)internalStepSize.TotalMinutes;
                var delayTimeInSteps = (int)(affsubaff.DelayTime / minutesperstep);
                // variables
                var calcAffordanceVariableOps = new List<CalcAffordanceVariableOpDto>();
                foreach (var affVariableOp in affsubaff.SubAffordance.SubAffordanceVariableOps)
                {
                    CalcLocationDto loc = null;
                    if (affVariableOp.LocationMode == VariableLocationMode.OtherLocation)
                    {
                        if (affVariableOp.Location != null && locDict.SimulateLocation(affVariableOp.Location))
                        {
                            loc = locDict.GetDtoForLocation(affVariableOp.Location);
                        }
                    }
                    else
                    {
                        loc = calcloc;
                    }

                    if (loc != null && affVariableOp.Variable != null)
                    {
                        if (affVariableOp.Location == null) {
                            throw new LPGException("Variable Location was null");
                        }
                        var variable =
                            _variableRepository.RegisterVariableIfNotRegistered(affVariableOp.Variable,
                                affVariableOp.Location, householdKey, locDict);
                        calcAffordanceVariableOps.Add(new CalcAffordanceVariableOpDto(affVariableOp.Variable.Name,
                            affVariableOp.Value, loc.Name,loc.Guid, affVariableOp.VariableAction, affVariableOp.ExecutionTime,
                            variable.Guid));
                    }
                }

                // CalcSubaffordanz anlegen
                var affName = CalcAffordanceFactory.FixAffordanceName(aff.Name, csvCharacter);
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                var csubaff = new CalcSubAffordanceDto(name + " (" + affName + ")", affsubaff.IntID, calcloc.Name,calcloc.Guid,
                    calcDesires, affsubaff.SubAffordance.MinimumAge, affsubaff.SubAffordance.MaximumAge,
                    delayTimeInSteps, affsubaff.SubAffordance.PermittedGender, aff.AffCategory,
                    affsubaff.SubAffordance.IsInterruptable, affsubaff.SubAffordance.IsInterrupting,
                    calcAffordanceVariableOps, weight, srcTrait, Guid.NewGuid().ToString());
                caff.SubAffordance.Add(csubaff);
            }
        }

        [NotNull]
        [ItemNotNull]
        private List<CalcAffordanceVariableOpDto> MakeVariableOps([NotNull] CalcLocationDto calcloc,
            [NotNull] LocationDtoDict
                locDict, [NotNull] HouseholdKey householdKey,
            AffordanceWithTimeLimit aff)
        {
            var variableOps = new List<CalcAffordanceVariableOpDto>();
            foreach (var affVariableOperation in aff.Affordance.ExecutedVariables)
            {
                CalcLocationDto loc = null;
                if (affVariableOperation.LocationMode == VariableLocationMode.OtherLocation)
                {
                    if (affVariableOperation.Location == null) {
                        throw new LPGException("Location was null");
                    }
                    if (locDict.SimulateLocation(affVariableOperation.Location))
                    {
                        loc = locDict.GetDtoForLocation(affVariableOperation.Location);
                    }

                    // if the Location does not exist, don't do anything.
                }
                else
                {
                    loc = calcloc;
                }

                if (loc != null)
                {
                    if (affVariableOperation.Location == null)
                    {
                        throw new LPGException("Location was null");
                    }
                    var variable = _variableRepository.RegisterVariableIfNotRegistered(
                        affVariableOperation.Variable, affVariableOperation.Location, householdKey, locDict);

                    variableOps.Add(new CalcAffordanceVariableOpDto(affVariableOperation.Variable.Name,
                        affVariableOperation.Value, loc.Name,loc.Guid, affVariableOperation.Action,
                        affVariableOperation.ExecutionTime, variable.Guid));
                }
            }

            return variableOps;
        }

        [NotNull]
        [ItemNotNull]
        private List<VariableRequirementDto> MakeVariableRequirements([NotNull] CalcLocationDto calcloc,
            [NotNull] LocationDtoDict locDict,
            [NotNull] HouseholdKey householdKey,
            AffordanceWithTimeLimit aff)
        {
            var variableRequirements =
                new List<VariableRequirementDto>();
            foreach (var affVariableRequirement in aff.Affordance.RequiredVariables)
            {
                CalcLocationDto loc = null;
                if (affVariableRequirement.LocationMode == VariableLocationMode.OtherLocation)
                {
                    if (affVariableRequirement.Location == null)
                    {
                        throw new LPGException("Location was null");
                    }
                    if (locDict.SimulateLocation(affVariableRequirement.Location))
                    {
                        loc = locDict.GetDtoForLocation(affVariableRequirement.Location);
                    }
                }
                else
                {
                    loc = calcloc;
                }

                if (loc != null)
                {
                    if (affVariableRequirement.Location == null)
                    {
                        throw new LPGException("Location was null");
                    }
                    var variable = _variableRepository.RegisterVariableIfNotRegistered
                        (affVariableRequirement.Variable, affVariableRequirement.Location, householdKey, locDict);
                    variableRequirements.Add(new VariableRequirementDto(affVariableRequirement.Variable.Name,
                        affVariableRequirement.Value, loc.Name, loc.Guid, affVariableRequirement.Condition,
                        variable.Guid));
                }
            }

            return variableRequirements;
        }
    }
}
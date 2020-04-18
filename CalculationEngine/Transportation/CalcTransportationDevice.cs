﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalculationEngine.Transportation {
    public class CalcTransportationDevice : CalcBase {
        [NotNull] private readonly CalcParameters _calcParameters;

        [CanBeNull] private readonly CalcLoadType _chargingCalcLoadType1;
        private readonly double _currentSOC;
        [NotNull] private readonly DateStampCreator _dsc;

        private readonly double _energyToDistanceFactor;
        private readonly double _fullRangeInMeters;


        [ItemNotNull] [NotNull] private readonly BitArray _isBusyArray;

        [ItemNotNull] [NotNull] private readonly List<CalcDeviceLoad> _loads;

        private readonly double _maxChargingPower;

        [NotNull] private readonly IOnlineDeviceActivationProcessor _odap;
        [NotNull] private readonly Dictionary<int, CalcSite> _targetSiteByTimeStep = new Dictionary<int, CalcSite>();

        [NotNull] private TimeStep _activationStartTimestep = new TimeStep(-1, 0, false);
        [NotNull] private TimeStep _activationStopTimestep = new TimeStep(-1, 0, false);
        private double _availableRangeInMeters;
        [CanBeNull] private CalcSite _currentSite;

        [CanBeNull] private CalcChargingStation _lastChargingStation;

        [CanBeNull] private string _lastUsingPerson;
        private readonly Dictionary<CalcLoadType, OefcKey> _keysByLoadtype = new Dictionary<CalcLoadType, OefcKey>();

        [NotNull] private readonly CalcDeviceDto _calcDeviceDto;
        public CalcTransportationDevice(
                                        [NotNull] CalcTransportationDeviceCategory category,
                                        double averageSpeedInMPerS, [NotNull] [ItemNotNull] List<CalcDeviceLoad> loads,
                                        [NotNull] IOnlineDeviceActivationProcessor odap,
                                        double fullRangeInMeters,
                                        double energyToDistanceFactor,
                                        double maxChargingPower,
                                        [CanBeNull] CalcLoadType chargingCalcLoadType,
                                        [NotNull] [ItemNotNull] List<CalcSite> allCalcSites,
                                        [NotNull] CalcParameters calcParameters,
                                        [NotNull] IOnlineLoggingData onlineLoggingData,
                                        [NotNull] CalcDeviceDto calcDeviceDto)
            : base(calcDeviceDto.Name, calcDeviceDto.Guid)
        {
            _dsc = new DateStampCreator(calcParameters);
            _odap = odap;
            _loads = loads;
            _fullRangeInMeters = fullRangeInMeters;
            _energyToDistanceFactor = energyToDistanceFactor;
            _maxChargingPower = maxChargingPower;
            _chargingCalcLoadType1 = chargingCalcLoadType;
            _calcParameters = calcParameters;
            _availableRangeInMeters = fullRangeInMeters;
            Category = category;
            AverageSpeedInMPerS = averageSpeedInMPerS;
            OnlineLoggingData = onlineLoggingData;
            _currentSOC = 100;
            _calcDeviceDto = calcDeviceDto;
            if (_calcParameters.InternalTimesteps == 0) {
                throw new LPGException("Time steps were not initialized.");
            }

            _isBusyArray = new BitArray(_calcParameters.InternalTimesteps);
            //not all devices need to have load types. busses for example have no load
            //if (loads.Count == 0) {
            //throw new DataIntegrityException("The transportation device " + pName +
            //" seems to have no load types. That is not very useful. Please fix.");
            //}

            foreach (CalcDeviceLoad deviceLoad in loads) {
                //TODO: check if -1 is a good location guid
                //OefcKey key = new OefcKey(_calcDeviceDto.HouseholdKey, OefcDeviceType.Transportation, Guid, "-1", deviceLoad.LoadType.Guid, "Transportation");
                var key = odap.RegisterDevice(deviceLoad.LoadType.ConvertToDto(), calcDeviceDto);
                _keysByLoadtype.Add(deviceLoad.LoadType,key);
            }

            if (chargingCalcLoadType != null) {
                var key2 = odap.RegisterDevice(chargingCalcLoadType.ConvertToDto(), calcDeviceDto);
                if (!_keysByLoadtype.ContainsKey(chargingCalcLoadType)) {
                    _keysByLoadtype.Add(chargingCalcLoadType, key2);
                }
            }

            RegisterForAllCalcSites(allCalcSites);
        }

        public double AvailableRangeInMeters => _availableRangeInMeters;

        [NotNull]
        public CalcTransportationDeviceCategory Category { get; }

        [CanBeNull]
        public CalcSite Currentsite => _currentSite;

        public double LastChargingPower { get; set; }

        [NotNull]
        public IOnlineLoggingData OnlineLoggingData { get; }

        private double AverageSpeedInMPerS { get; }

        public void Activate([NotNull] TimeStep startTimeStep, int durationInTimesteps, [NotNull] CalcSite srcSite,
                             [NotNull] CalcSite dstSite,
                             [NotNull] string travelRouteName,
                             [NotNull] string personName, [NotNull] TimeStep transportationEventStartTimeStep,
                             [NotNull] TimeStep transportationEventEndTimeStep)
        {
            if (startTimeStep < transportationEventStartTimeStep || transportationEventEndTimeStep < startTimeStep) {
                throw new LPGException("Bug in the transportation module. Start time earlier than possible.");
            }

            if (durationInTimesteps == 0) {
                throw new LPGException("Can't activate with a duration of 0 timesteps");
            }

            _lastUsingPerson = personName;
            if (startTimeStep < _activationStopTimestep && Category.IsLimitedToSingleLocation) {
                throw new LPGException(
                    "Double activation of a transportation device. This seems to be a bug. Please fix.");
            }

            if (_currentSite != srcSite && _currentSite != null) {
                throw new LPGException("Trying to activate a device that is not at the source location");
            }

            _activationStartTimestep = startTimeStep;
            _activationStopTimestep = startTimeStep.AddSteps(durationInTimesteps);
            if (Category.IsLimitedToSingleLocation) {
                _targetSiteByTimeStep.Add(startTimeStep.InternalStep, null);
                _targetSiteByTimeStep.Add(_activationStopTimestep.InternalStep, dstSite);
            }

            //_currentSite = dstSite;
            for (int i = transportationEventStartTimeStep.InternalStep;
                i < transportationEventEndTimeStep.InternalStep && i < _isBusyArray.Length;
                i++) {
                _isBusyArray[i] = true;
            }

            foreach (CalcDeviceLoad load in _loads) {
                var transportationDeviceProfile = new List<double>(durationInTimesteps);
                for (var i = 0; i < durationInTimesteps; i++) {
                    transportationDeviceProfile.Add(load.Value);
                }

                var cp = new CalcProfile(srcSite.Name + " - " + dstSite.Name + " - " + Name,
                    System.Guid.NewGuid().ToString(),
                    transportationDeviceProfile,
                    ProfileType.Relative,
                    "Synthetic for " + Name);
                SetTimeprofile(cp, startTimeStep, load.LoadType, "Transport via " + Name,
                    personName + " - " + travelRouteName);
            }
        }

        public int CalculateDurationOfTimestepsForDistance(double distanceInM) =>
            CalculateDurationOfTimestepsForDistance(distanceInM, AverageSpeedInMPerS, _calcParameters.InternalStepsize);

        public static int CalculateDurationOfTimestepsForDistance(double distanceInM, double speed,
                                                                  TimeSpan internalStepSize)
        {
            double durationInSeconds = distanceInM / speed;
            double numberOfTimesteps =
                durationInSeconds / internalStepSize.TotalSeconds;
            return (int)Math.Ceiling(numberOfTimesteps);
        }

        public void DriveAndCharge([NotNull] TimeStep currentTimeStep)
        {
            AdjustCurrentsiteByTimestep(currentTimeStep);
            LastChargingPower = 0;
            //first the undefined state
            // geräte, die nicht geladen werden müssen, haben eine negative range.
            //geräte im vehicle depot / in transit müssen nicht geladen werden.

            if (_fullRangeInMeters < 0) {
                DisconnectCar();
                OnlineLoggingData.AddTransportationDeviceState(new TransportationDeviceStateEntry(
                    Name, Guid, currentTimeStep, TransportationDeviceState.Undefined,
                    _currentSOC, _calcDeviceDto.HouseholdKey, _availableRangeInMeters, _currentSite?.Name,
                    _lastUsingPerson, _dsc.MakeDateStringFromTimeStep(currentTimeStep)));
                return;
            }

            //currently driving
            if (currentTimeStep >= _activationStartTimestep && currentTimeStep < _activationStopTimestep) {
                DisconnectCar();

                if (_currentSite != null) {
                    throw new LPGException("transportation device was assigned to a site, even though it is driving");
                }

                double distancePerTimestep = AverageSpeedInMPerS *
                                             _calcParameters.InternalStepsize.TotalSeconds;
                _availableRangeInMeters -= distancePerTimestep;
                if (_availableRangeInMeters <= 0) {
                    _availableRangeInMeters = 0;
                }

                OnlineLoggingData.AddTransportationDeviceState(new TransportationDeviceStateEntry(
                    Name, Guid, currentTimeStep, TransportationDeviceState.Driving,
                    _currentSOC, _calcDeviceDto.HouseholdKey, _availableRangeInMeters,
                    _currentSite?.Name,
                    _lastUsingPerson, _dsc.MakeDateStringFromTimeStep(currentTimeStep)));
                return;
            }

            //car is fully charged
            if (_availableRangeInMeters >= _fullRangeInMeters) {
                OnlineLoggingData.AddTransportationDeviceState(new TransportationDeviceStateEntry(
                    Name, Guid, currentTimeStep, TransportationDeviceState.ParkingAndFullyCharged,
                    _currentSOC, _calcDeviceDto.HouseholdKey, _availableRangeInMeters, _currentSite?.Name,
                    null
                    , _dsc.MakeDateStringFromTimeStep(currentTimeStep)));
                DisconnectCar();
                //TODO: different disconnect strategies
                return;
            }

            if (_currentSite != null) {
                //needs charging && is at charging station
                var chargingStations =
                    _currentSite.ChargingDevices.Where(x =>
                        x.CarChargingLoadType == _chargingCalcLoadType1 &&
                        x.DeviceCategory == Category).ToList();
                if (chargingStations.Count > 0) {
                    if (chargingStations.All(x => !x.IsAvailable) && _lastChargingStation == null) {
                        OnlineLoggingData.AddTransportationDeviceState(new TransportationDeviceStateEntry(
                            Name, Guid, currentTimeStep, TransportationDeviceState.ParkingAndWaitingForCharging,
                            _currentSOC, _calcDeviceDto.HouseholdKey, _availableRangeInMeters, _currentSite.Name,
                            null, _dsc.MakeDateStringFromTimeStep(currentTimeStep)));
                        DisconnectCar();
                        return;
                    }

                    //use the first one that is available
                    //TODO: recycle the last one used
                    var chargingStation = _lastChargingStation;
                    if (_lastChargingStation == null) {
                        chargingStation = chargingStations.First(x => x.IsAvailable);
                        ConnectCar(chargingStation);
                    }

                    if (_currentSite == null) {
                        throw new LPGException("Current site was null while trying to charge.");
                    }

                    if (chargingStation == null) {
                        throw new LPGException("Charging station for charging was null");
                    }

                    OnlineLoggingData.AddTransportationDeviceState(new TransportationDeviceStateEntry(
                        Name, Guid, currentTimeStep, TransportationDeviceState.ParkingAndCharging,
                        _currentSOC, _calcDeviceDto.HouseholdKey, _availableRangeInMeters, _currentSite.Name,
                        null, _dsc.MakeDateStringFromTimeStep(currentTimeStep)));
                    double maxChargingPower = Math.Min(_maxChargingPower, chargingStation.MaxChargingPower);

                    List<double> chargingProfile = new List<double> {
                        maxChargingPower
                    };

                    var cp = new CalcProfile("Profile for " + _currentSite.Name + " - Charging - " + Name,
                        System.Guid.NewGuid().ToString(),
                        chargingProfile, ProfileType.Absolute,
                        "Synthetic Charging for " + Name + " @ " + _currentSite.Name);
                    var dstLoadType = chargingStation.GridChargingLoadType;
                    var key = _keysByLoadtype[dstLoadType];
                    //OefcKey key = new OefcKey(_householdKey, OefcDeviceType.Charging, Guid, _currentSite.Guid, dstLoadType.Guid, "Transportation");
                    //if (dstLoadType == null) {
                    //    throw new Exception("???");
                    //}

                    _odap.AddNewStateMachine(cp, currentTimeStep, 0, 1,
                         dstLoadType.ConvertToDto(),
                        "Charging for " + Name + " @ " + _currentSite,
                        "(autonomous)", cp.Name, "Synthetic", key, _calcDeviceDto);
                    double gainedDistance = maxChargingPower * _energyToDistanceFactor *
                                            _calcParameters.InternalStepsize.TotalSeconds;
                    _availableRangeInMeters += gainedDistance;
                    LastChargingPower = maxChargingPower;
                    if (_availableRangeInMeters > _fullRangeInMeters) {
                        //TODO: do this properly: reduce the max charging power
                        _availableRangeInMeters = _fullRangeInMeters;
                    }

                    return;
                }
            }

            DisconnectCar();
            OnlineLoggingData.AddTransportationDeviceState(new TransportationDeviceStateEntry(
                Name, Guid, currentTimeStep, TransportationDeviceState.ParkingAndNoChargingAvailableHere,
                _currentSOC, _calcDeviceDto.HouseholdKey, _availableRangeInMeters, _currentSite?.Name, null
                , _dsc.MakeDateStringFromTimeStep(currentTimeStep)));
        }

        public bool IsBusy([NotNull] TimeStep startTimeStep, int durationInTimesteps)
        {
            int endTimeStep = startTimeStep.InternalStep + durationInTimesteps;
            for (int i = startTimeStep.InternalStep; i < endTimeStep && i < _calcParameters.InternalTimesteps; i++) {
                if (_isBusyArray[i]) {
                    return true;
                }
            }

            return false;
        }

        private void AdjustCurrentsiteByTimestep([NotNull] TimeStep timestep)
        {
            if (Category.IsLimitedToSingleLocation) {
                if (_targetSiteByTimeStep.ContainsKey(timestep.InternalStep)) {
                    _currentSite = _targetSiteByTimeStep[timestep.InternalStep];
                    _targetSiteByTimeStep.Remove(timestep.InternalStep);
                }

                foreach (var ts in _targetSiteByTimeStep.Keys) {
                    if (ts < timestep.InternalStep) {
                        throw new LPGException("Leftover old timestep");
                    }
                }
            }
        }

        private void ConnectCar([NotNull] CalcChargingStation station)
        {
            if (_lastChargingStation != null) {
                _lastChargingStation.DisconnectCar();
            }

            _lastChargingStation = station;
            _lastChargingStation.SetConnectedCar(this);
        }

        private void DisconnectCar()
        {
            _lastChargingStation?.DisconnectCar();
            _lastChargingStation = null;
        }

        //readonly Dictionary<CalcChargingStation,OefcKey> _keysByChargingStation = new Dictionary<CalcChargingStation, OefcKey>();
        private void RegisterForAllCalcSites([NotNull] [ItemNotNull] List<CalcSite> calcSites)
        {
            //if the car doesnt need charging, it doesn't need to register as charging device
            if (_chargingCalcLoadType1 == null) {
                return;
            }

            //this registers possible charging events and basically reserves a column in the output files
            foreach (CalcSite site in calcSites) {
                var chargingStations = site.CollectChargingDevicesFor(Category, _chargingCalcLoadType1);
                foreach (CalcChargingStation station in chargingStations) {
                    //
                    //new OefcKey(_householdKey, OefcDeviceType.Charging, Guid, site.Guid, station.GridChargingLoadType.Guid, "Transportation")
                    var  clone = _calcDeviceDto.Clone();
                    clone.Name = "Charging " + Name + " @ " + station.ChargingStationName;
                    clone.LocationGuid = site.Guid;
                    //var key =
                        _odap.RegisterDevice(station.GridChargingLoadType.ConvertToDto(),_calcDeviceDto);
                    //_keysByChargingStation.Add(station,key);

                        //, site.Name, station.GridChargingLoadType.ConvertToDto());
                }
            }
        }

        private void SetTimeprofile([NotNull] CalcProfile calcProfile, [NotNull] TimeStep startidx,
                                    [NotNull] CalcLoadType loadType,
                                    [NotNull] string affordanceName, [NotNull] string activatingPersonName)
        {
            CalcDeviceLoad cdl = null;
            foreach (var calcDeviceLoad in _loads) {
                if (calcDeviceLoad.LoadType == loadType) {
                    cdl = calcDeviceLoad;
                }
            }

            if (cdl == null) {
                throw new LPGException("It was tried to activate the loadtype " + loadType.Name +
                                       " even though that one is not set for the device " + Name);
            }

            /*   var factor = cdl.Value * multiplier;
               if (calcProfile.ProfileType == ProfileType.Absolute)
               {
                   factor = 1 * multiplier;
               }*/
            if (_odap == null && !Config.IsInUnitTesting) {
                throw new LPGException("ODAP was null. Please report");
            }

            //   var totalDuration = calcProfile.GetNewLengthAfterCompressExpand(timefactor);
            //OefcKey key = new OefcKey(_calcDeviceDto.HouseholdKey, OefcDeviceType.Transportation, Guid, "-1", cdl.LoadType.Guid, "Transportation");
            var key = _keysByLoadtype[cdl.LoadType];
            _odap.AddNewStateMachine(calcProfile, startidx, cdl.PowerStandardDeviation, 1,
                 cdl.LoadType.ConvertToDto(), affordanceName, activatingPersonName,
                calcProfile.Name, calcProfile.DataSource, key, _calcDeviceDto);
            //SetBusy(startidx, totalDuration, loadType, activateDespiteBeingBusy);
            // return totalDuration + startidx;
        }
    }
}
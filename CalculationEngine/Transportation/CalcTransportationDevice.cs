using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalculationEngine.Transportation
{
    public class CalcTransportationDevice : CalcBase
    {

        private readonly CalcLoadType? _chargingCalcLoadType1;
        private readonly DateStampCreator _dsc;

        private readonly double _energyToDistanceFactor;
        private readonly double _fullRangeInMeters;


        private readonly BitArray _isBusyArray;

        private readonly List<CalcDeviceLoad> _loads;

        private readonly double _maxChargingPower;

        private readonly Dictionary<int, CalcSite?> _targetSiteByTimeStep = [];

        private TimeStep _activationStartTimestep = new(-1, 0, false);
        private TimeStep _activationStopTimestep = new(-1, 0, false);
        private double _availableRangeInMeters;
        private ICalcSite? _currentSite;

        private CalcChargingStation? _lastChargingStation;

        private string? _lastUsingPerson;
        private readonly Dictionary<StrGuid, Dictionary<CalcLoadType, OefcKey>> _keysByLocGuidAndLoadtype = [];

        private readonly CalcDeviceDto _calcDeviceDto;
        private readonly CalcRepo _calcRepo;

        public CalcTransportationDevice(CalcTransportationDeviceCategory category, double averageSpeedInMPerS,
            List<CalcDeviceLoad> loads, double fullRangeInMeters, double energyToDistanceFactor, double maxChargingPower,
            CalcLoadType? chargingCalcLoadType, List<CalcSite> allCalcSites, CalcDeviceDto calcDeviceDto,
            CalcRepo calcRepo)
            : base(calcDeviceDto.Name, calcDeviceDto.DeviceInstanceGuid)
        {
            _dsc = new DateStampCreator(calcRepo.CalcParameters);
            _loads = loads;
            _fullRangeInMeters = fullRangeInMeters;
            _energyToDistanceFactor = energyToDistanceFactor;
            _maxChargingPower = maxChargingPower;
            _chargingCalcLoadType1 = chargingCalcLoadType;
            _availableRangeInMeters = fullRangeInMeters;
            Category = category;
            AverageSpeedInMPerS = averageSpeedInMPerS;
            _calcDeviceDto = calcDeviceDto;
            _calcRepo = calcRepo;
            if (_calcRepo.CalcParameters.InternalTimesteps == 0)
            {
                throw new LPGException("Time steps were not initialized.");
            }

            _isBusyArray = new BitArray(_calcRepo.CalcParameters.InternalTimesteps);
            var vehiclePoolGuid = "8C426E95-B269-402E-9806-C3785D6C8433".ToStrGuid();
            _calcDeviceDto.LocationGuid = vehiclePoolGuid;
            _calcDeviceDto.LocationName = "Vehicle Pool";

            // remark: not all devices need to have load types. busses for example have no load
            foreach (CalcDeviceLoad deviceLoad in loads)
            {
                var key = _calcRepo.Odap.RegisterDevice(deviceLoad.LoadType.ConvertToDto(), calcDeviceDto);
                _keysByLocGuidAndLoadtype.Add(calcDeviceDto.LocationGuid, []);
                _keysByLocGuidAndLoadtype[calcDeviceDto.LocationGuid].Add(deviceLoad.LoadType, key);
            }

            if (chargingCalcLoadType != null)
            {
                var key2 = _calcRepo.Odap.RegisterDevice(chargingCalcLoadType.ConvertToDto(), calcDeviceDto);
                if (!_keysByLocGuidAndLoadtype.ContainsKey(calcDeviceDto.LocationGuid))
                {
                    _keysByLocGuidAndLoadtype.Add(calcDeviceDto.LocationGuid, []);
                }
                var dict = _keysByLocGuidAndLoadtype[calcDeviceDto.LocationGuid];
                if (!dict.ContainsKey(chargingCalcLoadType))
                {
                    dict.Add(chargingCalcLoadType, key2);
                }
            }

            RegisterForAllCalcSites(allCalcSites);
        }

        public double AvailableRangeInMeters => _availableRangeInMeters;


        public CalcTransportationDeviceCategory Category { get; }

        public CalcSite? Currentsite => _currentSite;

        public double LastChargingPower { get; set; }


        private double AverageSpeedInMPerS { get; }

        public void Activate(TimeStep startTimeStep, int durationInTimesteps, CalcSite srcSite, CalcSite dstSite,
            string travelRouteName, string personName, TimeStep transportationEventStartTimeStep,
            TimeStep transportationEventEndTimeStep)
        {
            if (startTimeStep < transportationEventStartTimeStep || transportationEventEndTimeStep < startTimeStep)
            {
                throw new LPGException("Bug in the transportation module. Start time earlier than possible.");
            }
            if (durationInTimesteps == 0)
            {
                throw new LPGException("Can't activate with a duration of 0 timesteps");
            }
            if (startTimeStep < _activationStopTimestep && Category.IsLimitedToSingleLocation)
            {
                throw new LPGException(
                    "Double activation of a transportation device. This seems to be a bug. Please fix.");
            }
            if (_currentSite != srcSite && _currentSite != null)
            {
                throw new LPGException("Trying to activate a device that is not at the source location");
            }

            // mark start and end time steps of traveling with this device
            _lastUsingPerson = personName;
            _activationStartTimestep = startTimeStep;
            _activationStopTimestep = startTimeStep.AddSteps(durationInTimesteps);
            if (Category.IsLimitedToSingleLocation)
            {
                _targetSiteByTimeStep.Add(startTimeStep.InternalStep, null);
                _targetSiteByTimeStep.Add(_activationStopTimestep.InternalStep, dstSite);
            }

            // mark the device as busy for the duration of this usage
            for (int i = transportationEventStartTimeStep.InternalStep;
                i < transportationEventEndTimeStep.InternalStep && i < _isBusyArray.Length;
                i++)
            {
                _isBusyArray[i] = true;
            }

            // create load profiles for all load types
            foreach (CalcDeviceLoad load in _loads)
            {
                var transportationDeviceProfile = new List<double>(durationInTimesteps);
                for (var i = 0; i < durationInTimesteps; i++)
                {
                    transportationDeviceProfile.Add(load.Value);
                }

                var cp = new CalcProfile($"{srcSite.Name} - {dstSite.Name} - {Name}",
                    System.Guid.NewGuid().ToStrGuid(),
                    transportationDeviceProfile,
                    ProfileType.Relative,
                    "Synthetic for " + Name);
                SetTimeprofile(cp, startTimeStep, load.LoadType, $"Transport via {Name}",
                    $"{personName} - {travelRouteName}", _calcDeviceDto.LocationGuid);
            }
        }

        public int CalculateDurationOfTimestepsForDistance(double distanceInM) =>
            CalculateDurationOfTimestepsForDistance(distanceInM, AverageSpeedInMPerS, _calcRepo.CalcParameters.InternalStepsize);

        public static int CalculateDurationOfTimestepsForDistance(double distanceInM, double speed,
                                                                  TimeSpan internalStepSize)
        {
            double durationInSeconds = distanceInM / speed;
            double numberOfTimesteps = durationInSeconds / internalStepSize.TotalSeconds;
            return (int)Math.Ceiling(numberOfTimesteps);
        }

        public double CurrentSoc => _availableRangeInMeters / _fullRangeInMeters;

        public void DriveAndCharge(TimeStep currentTimeStep)
        {
            AdjustCurrentsiteByTimestep(currentTimeStep);
            LastChargingPower = 0;
            
            // geräte, die nicht geladen werden müssen, haben eine negative range.
            // geräte im vehicle depot / in transit müssen nicht geladen werden.

            if (_fullRangeInMeters < 0)
            {
                // devices does not need to be charged
                DisconnectCar();
                _calcRepo.OnlineLoggingData.AddTransportationDeviceState(new TransportationDeviceStateEntry(
                    Name, Guid, currentTimeStep, TransportationDeviceState.Undefined,
                    CurrentSoc, _calcDeviceDto.HouseholdKey, _availableRangeInMeters, _currentSite?.Name,
                    _lastUsingPerson, _dsc.MakeDateStringFromTimeStep(currentTimeStep), 0));
                return;
            }

            if (currentTimeStep >= _activationStartTimestep && currentTimeStep < _activationStopTimestep)
            {
                // device is currently driving
                DisconnectCar();

                if (_currentSite != null)
                {
                    throw new LPGException("transportation device was assigned to a site, even though it is driving");
                }

                // calculate and log the distance driven in this timestep
                double distancePerTimestep = AverageSpeedInMPerS *
                                             _calcRepo.CalcParameters.InternalStepsize.TotalSeconds;
                _availableRangeInMeters -= distancePerTimestep;
                if (_availableRangeInMeters <= 0)
                {
                    _availableRangeInMeters = 0;
                }
                _calcRepo.OnlineLoggingData.AddTransportationDeviceState(new TransportationDeviceStateEntry(
                    Name, Guid, currentTimeStep, TransportationDeviceState.Driving,
                    CurrentSoc, _calcDeviceDto.HouseholdKey, _availableRangeInMeters,
                    _currentSite?.Name,
                    _lastUsingPerson, _dsc.MakeDateStringFromTimeStep(currentTimeStep), distancePerTimestep));
                return;
            }

            if (_availableRangeInMeters >= _fullRangeInMeters)
            {
                // car is fully charged
                _calcRepo.OnlineLoggingData.AddTransportationDeviceState(new TransportationDeviceStateEntry(
                    Name, Guid, currentTimeStep, TransportationDeviceState.ParkingAndFullyCharged,
                    CurrentSoc, _calcDeviceDto.HouseholdKey, _availableRangeInMeters, _currentSite?.Name,
                    null
                    , _dsc.MakeDateStringFromTimeStep(currentTimeStep), 0));
                DisconnectCar();
                //TODO: different disconnect strategies
                return;
            }

            if (_currentSite != null)
            {
                // needs charging && is at charging station
                var chargingStations =
                    _currentSite.ChargingDevices.Where(x =>
                        x.CarChargingLoadType == _chargingCalcLoadType1 &&
                        x.DeviceCategory == Category).ToList();
                if (chargingStations.Count > 0)
                {
                    if (chargingStations.All(x => !x.IsAvailable) && _lastChargingStation == null)
                    {
                        _calcRepo.OnlineLoggingData.AddTransportationDeviceState(new TransportationDeviceStateEntry(
                            Name, Guid, currentTimeStep, TransportationDeviceState.ParkingAndWaitingForCharging,
                            CurrentSoc, _calcDeviceDto.HouseholdKey, _availableRangeInMeters, _currentSite.Name,
                            null, _dsc.MakeDateStringFromTimeStep(currentTimeStep), 0));
                        DisconnectCar();
                        return;
                    }

                    //use the first one that is available
                    //TODO: recycle the last one used
                    var chargingStation = _lastChargingStation;
                    if (_lastChargingStation == null)
                    {
                        chargingStation = chargingStations.First(x => x.IsAvailable);
                        ConnectCar(chargingStation);
                    }

                    if (_currentSite == null)
                    {
                        throw new LPGException("Current site was null while trying to charge.");
                    }

                    if (chargingStation == null)
                    {
                        throw new LPGException("Charging station for charging was null");
                    }

                    _calcRepo.OnlineLoggingData.AddTransportationDeviceState(new TransportationDeviceStateEntry(
                        Name, Guid, currentTimeStep, TransportationDeviceState.ParkingAndCharging,
                        CurrentSoc, _calcDeviceDto.HouseholdKey, _availableRangeInMeters, _currentSite.Name,
                        null, _dsc.MakeDateStringFromTimeStep(currentTimeStep), 0));
                    double maxChargingPower = Math.Min(_maxChargingPower, chargingStation.MaxChargingPower);

                    List<double> chargingProfile = [maxChargingPower];

                    var cp = new CalcProfile("Profile for " + _currentSite.Name + " - Charging - " + Name,
                        System.Guid.NewGuid().ToStrGuid(),
                        chargingProfile, ProfileType.Absolute,
                        "Synthetic Charging for " + Name + " @ " + _currentSite.Name);
                    var dstLoadType = chargingStation.GridChargingLoadType;
                    var key = _keysByLocGuidAndLoadtype[_currentSite.Guid][dstLoadType];

                    CalcDeviceLoad cdl = new CalcDeviceLoad("", 1, dstLoadType, 0, 0);

                    var rsv = RandomValueProfile.MakeStepValues(cp.StepValues.Count, _calcRepo.NormalRandom, cdl.PowerStandardDeviation);
                    var sv = StepValues.MakeStepValues(cp, 1, rsv, cdl);
                    _calcRepo.Odap.AddNewStateMachine(currentTimeStep,
                         dstLoadType.ConvertToDto(),
                        "Charging for " + Name + " @ " + _currentSite,
                        "(autonomous)", key, _calcDeviceDto, sv);
                    double gainedDistance = maxChargingPower * _energyToDistanceFactor *
                                            _calcRepo.CalcParameters.InternalStepsize.TotalSeconds;
                    _availableRangeInMeters += gainedDistance;
                    LastChargingPower = maxChargingPower;
                    if (_availableRangeInMeters > _fullRangeInMeters)
                    {
                        //TODO: do this properly: reduce the max charging power
                        _availableRangeInMeters = _fullRangeInMeters;
                    }

                    return;
                }
            }

            DisconnectCar();
            _calcRepo.OnlineLoggingData.AddTransportationDeviceState(new TransportationDeviceStateEntry(
                Name, Guid, currentTimeStep, TransportationDeviceState.ParkingAndNoChargingAvailableHere,
                CurrentSoc, _calcDeviceDto.HouseholdKey, _availableRangeInMeters, _currentSite?.Name, null
                , _dsc.MakeDateStringFromTimeStep(currentTimeStep), 0));
        }

        public bool IsBusy(TimeStep startTimeStep, int durationInTimesteps)
        {
            int endTimeStep = startTimeStep.InternalStep + durationInTimesteps;
            for (int i = startTimeStep.InternalStep; i < endTimeStep && i < _calcRepo.CalcParameters.InternalTimesteps; i++)
            {
                if (_isBusyArray[i])
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// If necessary updates the site of a device during when it is used.
        /// </summary>
        /// <param name="timestep">the current timestep</param>
        /// <exception cref="LPGException">if a site change was missed</exception>
        private void AdjustCurrentsiteByTimestep(TimeStep timestep)
        {
            if (Category.IsLimitedToSingleLocation)
            {
                if (_targetSiteByTimeStep.TryGetValue(timestep.InternalStep, out CalcSite? value))
                {
                    // update to the new site and remove the update entry
                    _currentSite = value;
                    _targetSiteByTimeStep.Remove(timestep.InternalStep);
                }

                // double-check if no site updates were missed
                foreach (var ts in _targetSiteByTimeStep.Keys)
                {
                    if (ts < timestep.InternalStep)
                    {
                        throw new LPGException("Leftover old timestep");
                    }
                }
            }
        }

        /// <summary>
        /// Connect to a new charging station.
        /// </summary>
        /// <param name="station">the new station to connect to</param>
        private void ConnectCar(CalcChargingStation station)
        {
            if (_lastChargingStation != null)
            {
                _lastChargingStation.DisconnectCar();
            }

            _lastChargingStation = station;
            _lastChargingStation.SetConnectedCar(this);
        }

        /// <summary>
        /// Disconnect from the current charging station.
        /// </summary>
        private void DisconnectCar()
        {
            _lastChargingStation?.DisconnectCar();
            _lastChargingStation = null;
        }

        private void RegisterForAllCalcSites(List<CalcSite> calcSites)
        {
            //if the car doesnt need charging, it doesn't need to register as charging device
            if (_chargingCalcLoadType1 == null)
            {
                return;
            }

            //this registers possible charging events and basically reserves a column in the output files
            foreach (CalcSite site in calcSites)
            {
                var chargingStations = site.CollectChargingDevicesFor(Category, _chargingCalcLoadType1);
                foreach (CalcChargingStation station in chargingStations)
                {
                    var clone = _calcDeviceDto with { };
                    clone.Name = "Charging " + Name + " @ " + station.ChargingStationName;
                    clone.LocationGuid = site.Guid;
                    var key = _calcRepo.Odap.RegisterDevice(station.GridChargingLoadType.ConvertToDto(), _calcDeviceDto);
                    _keysByLocGuidAndLoadtype.Add(clone.LocationGuid, new Dictionary<CalcLoadType, OefcKey>());
                    var lt = station.GridChargingLoadType;
                    _keysByLocGuidAndLoadtype[clone.LocationGuid].Add(lt, key);
                }
            }
        }

        private void SetTimeprofile(CalcProfile calcProfile, TimeStep startidx, CalcLoadType loadType,
            string affordanceName, string activatingPersonName, StrGuid locationGuid)
        {
            CalcDeviceLoad? cdl = null;
            foreach (var calcDeviceLoad in _loads)
            {
                if (calcDeviceLoad.LoadType == loadType)
                {
                    cdl = calcDeviceLoad;
                }
            }

            if (cdl == null)
            {
                throw new LPGException($"It was tried to activate the loadtype {loadType.Name} even though that " +
                                       $"one is not set for the device {Name}");
            }

            if (_calcRepo.Odap == null)
            {
                throw new LPGException("ODAP was null. Please report");
            }

            var key = _keysByLocGuidAndLoadtype[locationGuid][cdl.LoadType];
            var rvp = RandomValueProfile.MakeStepValues(calcProfile.StepValues.Count, _calcRepo.NormalRandom, 0);
            var sv = StepValues.MakeStepValues(calcProfile, 1, rvp, cdl);
            _calcRepo.Odap.AddNewStateMachine(startidx, cdl.LoadType.ConvertToDto(), affordanceName, activatingPersonName,
                 key, _calcDeviceDto, sv);
        }
    }
}

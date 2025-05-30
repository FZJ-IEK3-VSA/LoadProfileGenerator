using System;
using System.Collections.Generic;
using System.Linq;
using Automation;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using JetBrains.Annotations;

namespace CalculationEngine.Transportation {
    public class CalcTravelRouteStep : CalcBase {
        private readonly double _distanceOfStepInM;
        private readonly double _durationInS;
        [ItemNotNull] [NotNull] private readonly List<CalcTransportationDevice> _vehiclePool;
        private readonly CalcRepo _calcRepo;

        public CalcTravelRouteStep([NotNull] string pName,
                                   [NotNull] CalcTransportationDeviceCategory transportationDeviceCategory, int stepNumber, double distanceInM,
                                   StrGuid guid, [NotNull] [ItemNotNull] List<CalcTransportationDevice> vehiclePool,
                                   CalcRepo calcRepo, double durationInS = -1) : base(pName, guid)
        {
            TransportationDeviceCategory = transportationDeviceCategory;
            StepNumber = stepNumber;
            _distanceOfStepInM = distanceInM;
            _vehiclePool = vehiclePool;
            _calcRepo = calcRepo;
            _durationInS = durationInS;
        }

        [NotNull]
        public CalcTransportationDeviceCategory TransportationDeviceCategory { get;  }
        public int StepNumber { get; }

        public double DistanceOfStepInM => _distanceOfStepInM;

        /// <summary>
        /// Optionally stores the duration for this timestep, if specified externally.
        /// A negative number means no duration was specified.
        /// </summary>
        public double DurationInS => _durationInS;

        public void ActivateStep([NotNull] TimeStep startTimeStep, [NotNull] CalcTransportationDevice pickedDevice,
            int pickeddurationInTimesteps, [NotNull] CalcSite srcSite, [NotNull] CalcSite dstSite,
            [NotNull] string travelRouteName, [NotNull] string personName
            , [NotNull] TimeStep transportationEventStartTimeStep, [NotNull] TimeStep transportationEventEndTimeStep)
        {
            pickedDevice.Activate(startTimeStep, pickeddurationInTimesteps,srcSite,dstSite,
                travelRouteName,personName, transportationEventStartTimeStep, transportationEventEndTimeStep);
            if (_vehiclePool.Contains(pickedDevice)) {
                _vehiclePool.Remove(pickedDevice);
            }
        }

        /// <summary>
        /// Determine the duration of this step, based on the selected device.
        /// </summary>
        /// <param name="td">the selected transportation device</param>
        /// <returns>the duration of this step in timesteps</returns>
        private int GetDurationInTimeSteps(CalcTransportationDevice td)
        {
            // if the travel route step has a fixed duration, return that
            int duration;
            if (DurationInS >= 0)
            {
                duration = td.CalculateDurationinTimeSteps(DurationInS);
            }else
            {
                // otherwise, calculate the duration using the vehicle speed
                duration = td.CalculateDurationOfTimestepsForDistance(_distanceOfStepInM);
            }
            // every step must take at least one timestep
            return Math.Max(duration, 1);
        }

        public bool CalculateDurationInTimestepsAndPickDevice([NotNull] TimeStep timestepOfThisStep,
            out CalcTransportationDevice? pickedDevice,
            [CanBeNull] out int? pickeddurationInTimesteps,
            [NotNull][ItemNotNull] List<CalcTransportationDevice> vehiclepool,
            [NotNull][ItemNotNull] List<CalcTransportationDevice> locationUnlimitedDevices,
            [ItemNotNull] [NotNull] List<CalcTransportationDevice> devicesAtSrcLoc, [NotNull] CalcPersonDto person,
            [NotNull] DeviceOwnershipMapping<string, CalcTransportationDevice> deviceOwnerships)
        {
            int durationInTimesteps;
            if (TransportationDeviceCategory.IsLimitedToSingleLocation) {
                //pick a limited device
                //first check if the person currently owns a device
                var ownedDevice = deviceOwnerships.GetDevice(person.Name);
                List<CalcTransportationDevice> srcdevices;
                if (ownedDevice != null && ownedDevice.Category == TransportationDeviceCategory)
                {
                    // it can be assumed that each route has at most one ownable device
                    // --> simply select the owned device if the category fits
                    srcdevices = [ownedDevice];
                } else
                {
                    // if no matching device is owned, try the other unowned devices at the src site
                    srcdevices = GetUsableDevices(devicesAtSrcLoc, person, deviceOwnerships);
                }
                bool addedVehiclePoolAlready = false;
                if (srcdevices.Count == 0)
                {
                    srcdevices.AddRange(vehiclepool.Where(x=> x.Category == TransportationDeviceCategory));
                    addedVehiclePoolAlready = true;
                }
                while (srcdevices.Count > 0)
                {
                    //pick a random one and try it out
                    CalcTransportationDevice td = srcdevices[_calcRepo.Rnd.Next(srcdevices.Count)];
                    durationInTimesteps = GetDurationInTimeSteps(td);
                    if (td.IsBusy(timestepOfThisStep, durationInTimesteps))
                    {
                        srcdevices.Remove(td);
                    }
                    else
                    {
                        pickedDevice = td;
                        pickeddurationInTimesteps = durationInTimesteps;
                        return true;
                    }
                    if (srcdevices.Count == 0 && !addedVehiclePoolAlready)
                    {
                        srcdevices.AddRange(vehiclepool);
                        addedVehiclePoolAlready = true;
                    }
                }

                pickeddurationInTimesteps = null;
                pickedDevice = null;
                return false;
                //throw new DataIntegrityException("No transportation device was found for " + Name);
            }
            //pick an unlimited device
            //TODO: maybe have some kind of person preference list
            var correctcategoryDevices =
                locationUnlimitedDevices.Where(x => x.Category == TransportationDeviceCategory).ToList();
            if (correctcategoryDevices.Count == 0) {
                throw new DataIntegrityException("No transportation device for the category " + TransportationDeviceCategory.Name + " could be found.");
            }
            pickedDevice = correctcategoryDevices[_calcRepo.Rnd.Next(correctcategoryDevices.Count)];
            //maybe put in some kind of time limits for busses for example to not run on the weekend
            durationInTimesteps = GetDurationInTimeSteps(pickedDevice);
            pickeddurationInTimesteps = durationInTimesteps;
            return true;
        }

        /// <summary>
        /// Returns those devices out of the passed list that can be used for this step.
        /// </summary>
        /// <param name="devicesAtLoc">the devices to check</param>
        /// <param name="person">the traveling person</param>
        /// <param name="deviceOwnerships">the device ownership object</param>
        /// <returns>devices that can be used in this step</returns>
        public List<CalcTransportationDevice> GetUsableDevices(List<CalcTransportationDevice> devicesAtLoc, CalcPersonDto person, DeviceOwnershipMapping<string, CalcTransportationDevice> deviceOwnerships)
        {
            return [.. devicesAtLoc.Where(x => x.Category == TransportationDeviceCategory && deviceOwnerships.CanUse(person.Name, x))];
        }
    }
}
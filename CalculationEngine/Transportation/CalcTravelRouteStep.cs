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
        [ItemNotNull] [NotNull] private readonly List<CalcTransportationDevice> _vehiclePool;
        private readonly CalcRepo _calcRepo;

        public CalcTravelRouteStep([NotNull] string pName,
                                   [NotNull] CalcTransportationDeviceCategory transportationDeviceCategory, int stepNumber, double distanceInM,
                                   StrGuid guid, [NotNull] [ItemNotNull] List<CalcTransportationDevice> vehiclePool,
                                   CalcRepo calcRepo) : base(pName, guid)
        {
            TransportationDeviceCategory = transportationDeviceCategory;
            StepNumber = stepNumber;
            _distanceOfStepInM = distanceInM;
            _vehiclePool = vehiclePool;
            _calcRepo = calcRepo;
        }

        [NotNull]
        public CalcTransportationDeviceCategory TransportationDeviceCategory { get;  }
        public int StepNumber { get; }

        public double DistanceOfStepInM => _distanceOfStepInM;

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
                    srcdevices = new List<CalcTransportationDevice> { ownedDevice };
                } else
                {
                    // if no matching device is owned, try the other unowned devices at the src site
                    srcdevices = devicesAtSrcLoc.Where(x => x.Category == TransportationDeviceCategory && deviceOwnerships.CanUse(person.Name, x)).ToList();
                }
                bool addedVehiclePoolAlready = false;
                if (srcdevices.Count == 0)
                {
                    srcdevices.AddRange(vehiclepool.Where(x=> x.Category == TransportationDeviceCategory));
                    addedVehiclePoolAlready = true;
                }
                while (srcdevices.Count > 0) {
                    //pick a random one and try it out
                    CalcTransportationDevice td = srcdevices[_calcRepo.Rnd.Next(srcdevices.Count)];
                    durationInTimesteps = td.CalculateDurationOfTimestepsForDistance(_distanceOfStepInM);
                    if (td.IsBusy(timestepOfThisStep, durationInTimesteps)) {
                        srcdevices.Remove(td);
                    }
                    else {
                        /*if (Config.IsInUnitTesting) {
                            Logger.Debug("Activating " + td.Name + " for " + durationInTimesteps);
                        }*/
                        pickedDevice = td;
                        pickeddurationInTimesteps = durationInTimesteps;
                        return true;
                    }
                    if (srcdevices.Count == 0 && !addedVehiclePoolAlready) {
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
            durationInTimesteps = pickedDevice.CalculateDurationOfTimestepsForDistance(_distanceOfStepInM);
            pickeddurationInTimesteps = durationInTimesteps;
            return true;
            /*if (Config.IsInUnitTesting)
            {
                Logger.Debug("Activating " + pickedDevice.Name + " for " + durationInTimesteps);
            }*/
        }
    }
}
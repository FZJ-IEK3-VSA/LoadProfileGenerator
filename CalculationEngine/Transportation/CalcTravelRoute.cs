using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace CalculationEngine.Transportation
{
    public class CalcTravelRoute : CalcBase {
        [NotNull]
        private readonly HouseholdKey _householdkey;

        private readonly CalcRepo _calcRepo;

        [ItemNotNull]
        [NotNull]
        private readonly List<CalcTransportationDevice> _locationUnlimitedDevices;
        [ItemNotNull]
        [NotNull]
        private readonly List<CalcTransportationDevice> _vehiclePool;
        [NotNull]
        private PreviouslyPickedDevices _mypicks = new PreviouslyPickedDevices("", new TimeStep(-1,0,false));

        public CalcTravelRoute([NotNull] string pName, [NotNull] CalcSite siteA, [NotNull] CalcSite siteB,
            [NotNull][ItemNotNull] List<CalcTransportationDevice> vehiclePool,
            [NotNull][ItemNotNull] List<CalcTransportationDevice> locationUnlimitedDevices,
            [NotNull] HouseholdKey householdkey, StrGuid guid,
                               CalcRepo calcRepo) : base(pName, guid)
        {
            _householdkey = householdkey;
            _calcRepo = calcRepo;
            SiteA = siteA;
            SiteB = siteB;
            siteA.AddRoute(this);
            _vehiclePool = vehiclePool;
            _locationUnlimitedDevices = locationUnlimitedDevices;
        }

        //TODO: Time limit

        [NotNull]
        private CalcSite SiteA { get; }
        [NotNull]
        public CalcSite SiteB { get; }
        [NotNull]
        [ItemNotNull]
        private List<CalcTravelRouteStep> Steps { get; } = new List<CalcTravelRouteStep>();

        public class CalcTravelDeviceUseEvent {
            public CalcTravelDeviceUseEvent([NotNull] CalcTransportationDevice device, int durationInSteps, double totalDistance)
            {
                Device = device;
                DurationInSteps = durationInSteps;
                TotalDistance = totalDistance;
            }

            [NotNull]
            public CalcTransportationDevice Device { get; }
            public int DurationInSteps { get; }
            public double TotalDistance { get;  }

            [NotNull]
            public override string ToString()
            {
                return Device.Name + " (" + DurationInSteps + " steps)";
            }
        }
        public int Activate([NotNull] TimeStep currentTimeStep, [NotNull] string calcPersonName,
                            [NotNull][ItemNotNull] out List<CalcTravelDeviceUseEvent> usedDeviceEvents)
        {
            if (_mypicks.Timestep != currentTimeStep || _mypicks.CalcPersonName != calcPersonName) {
                throw new LPGException("Device was not previously picked?");
            }

            if (currentTimeStep.InternalStep == 14095) {
                Logger.Info("timestep 14095");
            }

            //_mypicks = new PreviouslyPickedDevices(calcPersonName, currentTimeStep);
            int totalDuration = 0;
            //int slidingTimeStep = currentTimeStep;
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(
                currentTimeStep,
                _householdkey, "\tActivating " + Name));
            usedDeviceEvents = new List<CalcTravelDeviceUseEvent>();

            foreach (CalcTravelRouteStep step in Steps) {
                int pickedDuration = _mypicks.PickedDurations[step];
                totalDuration += pickedDuration;
            }

            TimeStep transportationEventEndTimestep = currentTimeStep.AddSteps( totalDuration);
            TimeStep timeStepOfThisStep = currentTimeStep;

            foreach (CalcTravelRouteStep step in Steps) {
                var device = _mypicks.PickedDevices[step];
                int pickedDuration = _mypicks.PickedDurations[step];
                usedDeviceEvents.Add(new CalcTravelDeviceUseEvent(device, pickedDuration,step.DistanceOfStepInM));
                //step.CalculateDurationInTimestepsAndPickDevice(slidingTimeStep, out CalcTransportationDevice pickedDevice,
                //out int durationForPickedDeviceInTimesteps, SiteA, _vehiclePool, _locationUnlimitedDevices, rnd);
                _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(
                    currentTimeStep,
                    _householdkey,
                    "\tActiviating step " + step.Name + " Device " + device.Name + " Distance: "
                    + step.DistanceOfStepInM + " Step Duration: " + pickedDuration));
                //slidingTimeStep += pickedDuration;
                //_mypicks.PickedDevices.Add(step, pickedDevice);
                step.ActivateStep(timeStepOfThisStep, device, pickedDuration,
                    SiteA, SiteB, Name,calcPersonName,currentTimeStep,
                    transportationEventEndTimestep);
                timeStepOfThisStep = timeStepOfThisStep.AddSteps( pickedDuration);
            }
            _mypicks.PreviouslyCalculatedTimeSteps = totalDuration;
            return totalDuration;
        }

        public void AddTravelRouteStep([NotNull] string stepName, [NotNull] CalcTransportationDeviceCategory deviceCategory,
            int stepNumber, double distanceInM, StrGuid guid)
        {
            CalcTravelRouteStep trs = new CalcTravelRouteStep(
                stepName, deviceCategory, stepNumber,
                distanceInM, guid,_vehiclePool, _calcRepo);
            Steps.Add(trs);
        }

        [CanBeNull]
        public int? GetDuration([NotNull] TimeStep currentTimeStep, [NotNull] string calcPersonName,
                                [ItemNotNull] [NotNull] List<CalcTransportationDevice> allTransportationDevices)
        {
            if (_mypicks.Timestep == currentTimeStep && _mypicks.CalcPersonName == calcPersonName) {
                return _mypicks.PreviouslyCalculatedTimeSteps;
            }

            var picks = new PreviouslyPickedDevices(calcPersonName, currentTimeStep);
            int totalDuration = 0;
            TimeStep slidingTimeStep = currentTimeStep;
            var deviceAtSrc = allTransportationDevices.Where(x => x.Currentsite == SiteA).ToList();
            foreach (CalcTravelRouteStep step in Steps) {
                bool success = step.CalculateDurationInTimestepsAndPickDevice(slidingTimeStep,
                    out CalcTransportationDevice pickedDevice,
                    out int? durationForPickedDeviceInTimesteps,
                    _vehiclePool,
                    _locationUnlimitedDevices, deviceAtSrc);
                if (!success) {
                    //this travel route step not now available, thus the entire route is invalid.
                    return null;
                }

                if(pickedDevice?.Category != step.TransportationDeviceCategory) {
                    throw new LPGException("Invalid device was picked.");
                }

                if(durationForPickedDeviceInTimesteps == null) {
                    throw new LPGException("Failed Travel duration calculation!");
                }

                slidingTimeStep = slidingTimeStep.AddSteps( (int)durationForPickedDeviceInTimesteps);
                totalDuration += (int)durationForPickedDeviceInTimesteps;
                picks.PickedDevices.Add(step, pickedDevice);
                picks.PickedDurations.Add(step,(int) durationForPickedDeviceInTimesteps);
            }

            picks.PreviouslyCalculatedTimeSteps = totalDuration;
            _mypicks = picks;
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(
                currentTimeStep,
                _householdkey,
                "\t\t\tCalculated a duration for the route of " + totalDuration));
            return totalDuration;
        }

        public bool IsAvailableRouteFor([NotNull] CalcSite srcSite, [NotNull] CalcSite dstSite, [ItemNotNull] [NotNull] List<CalcTransportationDevice> devicesAtSrcLoc)
        {
            if (SiteA == srcSite && dstSite == SiteB) {
                List<CalcTransportationDeviceCategory> neededCategories =
                    CollectNeededCalcTransportationDeviceCategory();
                if (neededCategories.Count == 0) {
                    return true;
                }

                bool areCategoriesAvailable = srcSite.AreCategoriesAvailable(neededCategories, _vehiclePool, devicesAtSrcLoc);
                return areCategoriesAvailable;
            }

            return false;
        }

        [NotNull]
        [ItemNotNull]
        private List<CalcTransportationDeviceCategory> CollectNeededCalcTransportationDeviceCategory()
        {
            List<CalcTransportationDeviceCategory> dev = new List<CalcTransportationDeviceCategory>();
            foreach (CalcTravelRouteStep step in Steps) {
                if (step.TransportationDeviceCategory.IsLimitedToSingleLocation) {
                    dev.Add(step.TransportationDeviceCategory);
                }
            }

            return dev;
        }

        private class PreviouslyPickedDevices {
            public PreviouslyPickedDevices([NotNull] string calcPersonName, [NotNull] TimeStep timestep)
            {
                CalcPersonName = calcPersonName;
                Timestep = timestep;
            }

            [NotNull]
            public string CalcPersonName { get; }

            [NotNull]
            public Dictionary<CalcTravelRouteStep, CalcTransportationDevice> PickedDevices { get; } =
                new Dictionary<CalcTravelRouteStep, CalcTransportationDevice>();

            [NotNull]
            public Dictionary<CalcTravelRouteStep, int> PickedDurations { get; } =
                new Dictionary<CalcTravelRouteStep, int>();

            public int PreviouslyCalculatedTimeSteps { get; set; } = -1;
            [NotNull]
            public TimeStep Timestep { get; }
        }
    }
}
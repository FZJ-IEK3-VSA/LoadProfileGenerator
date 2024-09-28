using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
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

        public CalcTravelRoute([NotNull] string pName, int minimumAge, int maximumAge, Common.Enums.PermittedGender gender, string affordanceTaggingSetName, string affordanceTagName,
            int? personID, double weight, [NotNull] CalcSite siteA, [NotNull] CalcSite siteB,
            [NotNull][ItemNotNull] List<CalcTransportationDevice> vehiclePool,
            [NotNull][ItemNotNull] List<CalcTransportationDevice> locationUnlimitedDevices,
            [NotNull] HouseholdKey householdkey, StrGuid guid,
                               CalcRepo calcRepo) : base(pName, guid)
        {
            MinimumAge = minimumAge;
            MaximumAge = maximumAge;
            Gender = gender;
            AffordanceTaggingSetName = affordanceTaggingSetName;
            AffordanceTagName = affordanceTagName;
            PersonID = personID;
            Weight = weight;
            _householdkey = householdkey;
            _calcRepo = calcRepo;
            SiteA = siteA;
            SiteB = siteB;
            siteA.AddRoute(this);
            _vehiclePool = vehiclePool;
            _locationUnlimitedDevices = locationUnlimitedDevices;
        }

        public int MinimumAge { get; }
        public int MaximumAge { get; }
        public Common.Enums.PermittedGender Gender { get; }
        public string AffordanceTaggingSetName { get; }
        public string AffordanceTagName { get; }
        public int? PersonID { get; }
        public double Weight { get; }
        [NotNull]
        private CalcSite SiteA { get; }
        [NotNull]
        public CalcSite SiteB { get; }
        [NotNull]
        [ItemNotNull]
        private List<CalcTravelRouteStep> Steps { get; } = new List<CalcTravelRouteStep>();

        public class CalcTravelDeviceUseEvent([NotNull] CalcTransportationDevice device, int durationInSteps, double totalDistance)
        {
            [NotNull]
            public CalcTransportationDevice Device { get; } = device;
            public int DurationInSteps { get; } = durationInSteps;
            public double TotalDistance { get; } = totalDistance;

            [NotNull]
            public override string ToString()
            {
                return Device.Name + " (" + DurationInSteps + " steps)";
            }
        }

        public int Activate([NotNull] TimeStep currentTimeStep, [NotNull] string calcPersonName,
                            [NotNull][ItemNotNull] out List<CalcTravelDeviceUseEvent> usedDeviceEvents,
                            [NotNull] DeviceOwnershipMapping<string, CalcTransportationDevice> deviceOwnerships)
        {
            if (_mypicks.Timestep != currentTimeStep || _mypicks.CalcPersonName != calcPersonName) {
                throw new LPGException("Device was not previously picked?");
            }

            // log the activation of this route
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(
                currentTimeStep, _householdkey, "\tActivating " + Name));
            usedDeviceEvents = new List<CalcTravelDeviceUseEvent>();
            
            // calculate the total duration of the route
            int totalDuration = 0;
            foreach (CalcTravelRouteStep step in Steps) {
                int pickedDuration = _mypicks.PickedDurations[step];
                totalDuration += pickedDuration;
            }

            // activate each step separately
            TimeStep transportationEventEndTimestep = currentTimeStep.AddSteps(totalDuration);
            TimeStep timeStepOfThisStep = currentTimeStep;
            foreach (CalcTravelRouteStep step in Steps) {
                // obtain ownership for devices such as cars
                var device = _mypicks.PickedDevices[step];
                if (device.Category.IsLimitedToSingleLocation)
                {
                    deviceOwnerships.TrySetOwnership(calcPersonName, device);
                }
                // log the transportation status and activate the step
                int pickedDuration = _mypicks.PickedDurations[step];
                usedDeviceEvents.Add(new CalcTravelDeviceUseEvent(device, pickedDuration, step.DistanceOfStepInM));
                string status = "\tActiviating step " + step.Name + " Device " + device.Name + " Distance: "
                    + step.DistanceOfStepInM + " Step Duration: " + pickedDuration;
                _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus( currentTimeStep, _householdkey, status));
                step.ActivateStep(timeStepOfThisStep, device, pickedDuration,
                    SiteA, SiteB, Name, calcPersonName, currentTimeStep,
                    transportationEventEndTimestep);
                timeStepOfThisStep = timeStepOfThisStep.AddSteps( pickedDuration);
            }
            // cache the total route duration
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

        /// <summary>
        /// Checks if a route can be activated in the specified timestep, by the specified person. If activation
        /// is possible, the duration of the route is returned. Any device picks and step durations are cached.
        /// </summary>
        /// <param name="currentTimeStep">the time step for checking route activation</param>
        /// <param name="person">the person that wants to travel</param>
        /// <param name="allTransportationDevices">list of all transport devices</param>
        /// <param name="deviceOwnerships">device ownership mapping</param>
        /// <returns>the total travel duration, or null if activation is not possible</returns>
        /// <exception cref="LPGException"></exception>
        [CanBeNull]
        public int? GetDuration([NotNull] TimeStep currentTimeStep, [NotNull] CalcPersonDto person,
                                [ItemNotNull] [NotNull] List<CalcTransportationDevice> allTransportationDevices,
                                [NotNull] DeviceOwnershipMapping<string, CalcTransportationDevice> deviceOwnerships)
        {
            // check if the duration is already cached
            if (_mypicks.Timestep == currentTimeStep && _mypicks.CalcPersonName == person.Name) {
                return _mypicks.PreviouslyCalculatedTimeSteps;
            }

            // check each step separately and get its duration
            var picks = new PreviouslyPickedDevices(person.Name, currentTimeStep);
            int totalDuration = 0;
            TimeStep slidingTimeStep = currentTimeStep;
            var deviceAtSrc = allTransportationDevices.Where(x => x.Currentsite == SiteA).ToList();
            foreach (CalcTravelRouteStep step in Steps) {
                bool success = step.CalculateDurationInTimestepsAndPickDevice(slidingTimeStep,
                    out CalcTransportationDevice? pickedDevice,
                    out int? durationForPickedDeviceInTimesteps,
                    _vehiclePool, _locationUnlimitedDevices,
                    deviceAtSrc, person, deviceOwnerships);
                if (!success) {
                    //this travel route step not now available, thus the entire route is invalid.
                    return null;
                }

                // double check whether the step is valid
                if(pickedDevice?.Category != step.TransportationDeviceCategory) {
                    throw new LPGException("Invalid device was picked.");
                }
                if(durationForPickedDeviceInTimesteps == null) {
                    throw new LPGException("Failed Travel duration calculation!");
                }

                int routeStepDuration = durationForPickedDeviceInTimesteps.Value;
                slidingTimeStep = slidingTimeStep.AddSteps(routeStepDuration);
                totalDuration += routeStepDuration;
                picks.PickedDevices.Add(step, pickedDevice);
                picks.PickedDurations.Add(step, routeStepDuration);
            }

            // cache the total route duration
            picks.PreviouslyCalculatedTimeSteps = totalDuration;
            _mypicks = picks;
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(currentTimeStep,
                _householdkey, "\t\t\tCalculated a duration for the route of " + totalDuration));
            return totalDuration;
        }

        public bool IsAvailableRouteFor([NotNull] CalcSite srcSite, [NotNull] CalcSite dstSite, [ItemNotNull] [NotNull] List<CalcTransportationDevice> devicesAtSrcLoc,
            [NotNull] CalcPersonDto person, [NotNull] DeviceOwnershipMapping<string, CalcTransportationDevice> deviceOwnerships)
        {
            if (SiteA == srcSite && dstSite == SiteB) {
                var neededCategories = CollectNeededCalcTransportationDeviceCategory();
                if (neededCategories.Count == 0) {
                    return true;
                }

                // if the person currently owns a device then this device must be used
                var ownedDevice = deviceOwnerships.GetDevice(person.Name);
                if (ownedDevice != null)
                {
                    bool canUseOwnedDevice = neededCategories.Any(category => category == ownedDevice.Category);
                    if (!canUseOwnedDevice)
                    {
                        return false;
                    }
                }
                bool areCategoriesAvailable = srcSite.AreCategoriesAvailable(neededCategories, _vehiclePool, devicesAtSrcLoc, person, deviceOwnerships);
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

        private class PreviouslyPickedDevices([NotNull] string calcPersonName, [NotNull] TimeStep timestep)
        {
            [NotNull]
            public string CalcPersonName { get; } = calcPersonName;

            [NotNull]
            public Dictionary<CalcTravelRouteStep, CalcTransportationDevice> PickedDevices { get; } = [];

            [NotNull]
            public Dictionary<CalcTravelRouteStep, int> PickedDurations { get; } = [];

            public int PreviouslyCalculatedTimeSteps { get; set; } = -1;
            [NotNull]
            public TimeStep Timestep { get; } = timestep;
        }
    }
}
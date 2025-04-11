using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

namespace CalculationEngine.Transportation
{
    public class CalcTravelRoute : CalcBase
    {
        [NotNull]
        private readonly HouseholdKey _householdkey;

        private readonly CalcRepo _calcRepo;

        private readonly List<CalcTransportationDevice> _locationUnlimitedDevices;

        private readonly List<CalcTransportationDevice> _vehiclePool;

        private readonly DeviceOwnershipMapping<string, CalcTransportationDevice> _deviceOwnerships;

        private PreviouslyPickedDevices _mypicks = new PreviouslyPickedDevices("", new TimeStep(-1, 0, false));

        public CalcTravelRoute(string pName, int minimumAge, int maximumAge, Common.Enums.PermittedGender gender,
            string affordanceTaggingSetName, string affordanceTagName, int? personID, double weight, CalcSite siteA,
            CalcSite siteB, List<CalcTransportationDevice> vehiclePool,
            List<CalcTransportationDevice> locationUnlimitedDevices,
            DeviceOwnershipMapping<string, CalcTransportationDevice> deviceOwnerships, HouseholdKey householdkey,
            StrGuid guid, CalcRepo calcRepo) : base(pName, guid)
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
            _deviceOwnerships = deviceOwnerships;
        }

        public int MinimumAge { get; }
        public int MaximumAge { get; }
        public PermittedGender Gender { get; }
        public string AffordanceTaggingSetName { get; }
        public string AffordanceTagName { get; }
        public int? PersonID { get; }
        public double Weight { get; }
        public CalcSite SiteA { get; }
        public CalcSite SiteB { get; }
        private List<CalcTravelRouteStep> Steps { get; } = [];

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
                            [NotNull][ItemNotNull] out List<CalcTravelDeviceUseEvent> usedDeviceEvents)
        {
            if (_mypicks.Timestep != currentTimeStep || _mypicks.CalcPersonName != calcPersonName)
            {
                throw new LPGException("Device was not previously picked?");
            }

            // log the activation of this route
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(
                currentTimeStep, _householdkey, "\tActivating " + Name));
            usedDeviceEvents = new List<CalcTravelDeviceUseEvent>();

            // calculate the total duration of the route
            int totalDuration = 0;
            foreach (CalcTravelRouteStep step in Steps)
            {
                int pickedDuration = _mypicks.PickedDurations[step];
                totalDuration += pickedDuration;
            }

            // activate each step separately
            TimeStep transportationEventEndTimestep = currentTimeStep.AddSteps(totalDuration);
            TimeStep timeStepOfThisStep = currentTimeStep;
            foreach (CalcTravelRouteStep step in Steps)
            {
                // obtain ownership for devices such as cars
                var device = _mypicks.PickedDevices[step];
                if (device.Category.IsLimitedToSingleLocation)
                {
                    _deviceOwnerships.TrySetOwnership(calcPersonName, device);
                }
                // log the transportation status and activate the step
                int pickedDuration = _mypicks.PickedDurations[step];
                usedDeviceEvents.Add(new CalcTravelDeviceUseEvent(device, pickedDuration, step.DistanceOfStepInM));
                string status = "\tActiviating step " + step.Name + " Device " + device.Name + " Distance: "
                    + step.DistanceOfStepInM + " Step Duration: " + pickedDuration;
                _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(currentTimeStep, _householdkey, status));
                step.ActivateStep(timeStepOfThisStep, device, pickedDuration,
                    SiteA, SiteB, Name, calcPersonName, currentTimeStep,
                    transportationEventEndTimestep);
                timeStepOfThisStep = timeStepOfThisStep.AddSteps(pickedDuration);
            }
            // cache the total route duration
            _mypicks.PreviouslyCalculatedTimeSteps = totalDuration;
            return totalDuration;
        }

        public void AddTravelRouteStep([NotNull] string stepName, [NotNull] CalcTransportationDeviceCategory deviceCategory,
            int stepNumber, double distanceInM, StrGuid guid, double durationInS = -1)
        {
            CalcTravelRouteStep trs = new CalcTravelRouteStep(
                stepName, deviceCategory, stepNumber,
                distanceInM, guid, _vehiclePool, _calcRepo, durationInS);
            Steps.Add(trs);
        }

        /// <summary>
        /// Checks if a route can be activated in the specified timestep, by the specified person. If activation
        /// is possible, the duration of the route is returned. Any device picks and step durations are cached.
        /// </summary>
        /// <param name="currentTimeStep">the time step for checking route activation</param>
        /// <param name="person">the person that wants to travel</param>
        /// <param name="allTransportationDevices">list of all transport devices</param>
        /// <returns>the total travel duration, or null if activation is not possible</returns>
        /// <exception cref="LPGException"></exception>
        [CanBeNull]
        public int? GetDuration([NotNull] TimeStep currentTimeStep, [NotNull] CalcPersonDto person,
                                [ItemNotNull][NotNull] List<CalcTransportationDevice> allTransportationDevices)
        {
            // check if the duration is already cached
            if (_mypicks.Timestep == currentTimeStep && _mypicks.CalcPersonName == person.Name)
            {
                return _mypicks.PreviouslyCalculatedTimeSteps;
            }

            // check each step separately and get its duration
            var picks = new PreviouslyPickedDevices(person.Name, currentTimeStep);
            int totalDuration = 0;
            TimeStep slidingTimeStep = currentTimeStep;
            var deviceAtSrc = allTransportationDevices.Where(x => x.Currentsite == SiteA).ToList();
            foreach (CalcTravelRouteStep step in Steps)
            {
                bool success = step.CalculateDurationInTimestepsAndPickDevice(slidingTimeStep,
                    out CalcTransportationDevice? pickedDevice,
                    out int? durationForPickedDeviceInTimesteps,
                    _vehiclePool, _locationUnlimitedDevices,
                    deviceAtSrc, person, _deviceOwnerships);
                if (!success)
                {
                    //this travel route step not now available, thus the entire route is invalid.
                    return null;
                }

                // double check whether the step is valid
                if (pickedDevice?.Category != step.TransportationDeviceCategory)
                {
                    throw new LPGException("Invalid device was picked.");
                }
                if (durationForPickedDeviceInTimesteps == null)
                {
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

        public bool IsAvailableRouteFor([NotNull] CalcSite srcSite, [NotNull] ICalcSite dstSite, [ItemNotNull][NotNull] List<CalcTransportationDevice> devicesAtSrcLoc,
            [NotNull] CalcPersonDto person)
        {
            if (srcSite != SiteA || dstSite != SiteB)
                return false;
            if (!IsAllowedForPerson(person))
                return false;

            var neededCategories = CollectNeededCalcTransportationDeviceCategories();

            // if the person currently owns a device then this device must be used
            var ownedDevice = _deviceOwnerships.GetDevice(person.Name);
            if (ownedDevice != null)
            {
                bool canUseOwnedDevice = neededCategories.Any(category => category == ownedDevice.Category);
                if (!canUseOwnedDevice)
                {
                    // the person still owns a device that cannot be left at the current site, so this route is not available
                    return false;
                }
            }
            bool areCategoriesAvailable = AreCategoriesAvailable(neededCategories, devicesAtSrcLoc, person);
            return areCategoriesAvailable;
        }

        /// <summary>
        /// Checks if one device of each of the required categories for a route is available.
        /// </summary>
        /// <param name="neededDeviceCategories">the required device categories</param>
        /// <param name="devicesAtLoc">the devices available at a location</param>
        /// <param name="person">the person who wants to travel</param>
        /// <returns>true if a device for every category is available; otherwhise, false</returns>
        public bool AreCategoriesAvailable(List<CalcTransportationDeviceCategory> neededDeviceCategories, List<CalcTransportationDevice> devicesAtLoc, CalcPersonDto person)
        {
            //TODO: check for fuel on each transportation device
            foreach (var category in neededDeviceCategories)
            {
                // check if one of the available devices fits and can be used
                if (devicesAtLoc.Any(device => device.Category == category && _deviceOwnerships.CanUse(person.Name, device)))
                    continue;

                // if no device is found, check the vehicle pool
                if (!_vehiclePool.Any(d => d.Category == category))
                    return false;
            }
            // a device was found for each required category
            return true;
        }

        /// <summary>
        /// Returns those devices from the list than can be used by the specified person to travel on this route.
        /// </summary>
        /// <param name="devicesAtLoc">the transportation devices to consider</param>
        /// <param name="person">the traveling person</param>
        /// <returns>the devices the person can use</returns>
        public List<CalcTransportationDevice> GetUsableDevices(List<CalcTransportationDevice> devicesAtLoc, CalcPersonDto person)
        {
            return [.. Steps.SelectMany(step => step.GetUsableDevices(devicesAtLoc, person, _deviceOwnerships)).ToHashSet()];
        }

        /// <summary>
        /// Checks if this route can be used by the specified person. Checks all applicable restrictions, including
        /// age, gender, and whether the route is for a specific person only.
        /// </summary>
        /// <param name="person">the person to check</param>
        /// <returns>true if the person can use the route; otherwhise, false</returns>
        private bool IsAllowedForPerson(CalcPersonDto person)
        {
            return (PersonID == null || PersonID == person.ID)
                && (Gender == PermittedGender.All || person.Gender == PermittedGender.All || Gender == person.Gender)
                && (MinimumAge < 0 || MinimumAge <= person.Age)
                && (MaximumAge < 0 || MaximumAge >= person.Age);
        }

        /// <summary>
        /// Collects all device categories that are limited to a single location, like cars
        /// </summary>
        /// <returns>movable device categories required for this route</returns>
        [NotNull]
        [ItemNotNull]
        private List<CalcTransportationDeviceCategory> CollectNeededCalcTransportationDeviceCategories()
        {
            return [.. Steps.Select(step => step.TransportationDeviceCategory).Where(devCat => devCat.IsLimitedToSingleLocation)];
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
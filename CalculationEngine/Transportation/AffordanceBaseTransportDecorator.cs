using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition.SrgsGrammar;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.Activities;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging.Loggers;

namespace CalculationEngine.Transportation
{
    public class AffordanceBaseTransportDecorator : CalcBase, ICalcAffordanceBase
    {
        public readonly ICalcAffordanceBase SourceAffordance;
        protected readonly TransportationHandler _transportationHandler;
        protected readonly HouseholdKey _householdkey;

        protected readonly CalcRepo _calcRepo;

        /// <summary>
        /// Creates the correct transport decorator to use, depending on whether dynamic city simulation is enabled or not
        /// </summary>
        /// <param name="sourceAffordance">the affordance to decorate</param>
        /// <param name="transportationHandler">the transportation handler object</param>
        /// <param name="householdkey">the household key</param>
        /// <param name="guid">guid of the decorated affordance</param>
        /// <param name="calcRepo">the calc repo</param>
        /// <returns>the newly created transport decorator</returns>
        public static AffordanceBaseTransportDecorator CreateTransportDecorator(ICalcAffordanceBase sourceAffordance, TransportationHandler transportationHandler,
            HouseholdKey householdkey, StrGuid guid, CalcRepo calcRepo)
        {
            if (calcRepo.CalcParameters.CitySimulationEnabled)
            {
                return new AffordanceBaseTransportDecoratorDynamic(sourceAffordance, transportationHandler, householdkey, guid, calcRepo);
            }
            else
            {
                return new AffordanceBaseTransportDecorator(sourceAffordance, transportationHandler, householdkey, guid, calcRepo);
            }
        }

        protected AffordanceBaseTransportDecorator(ICalcAffordanceBase sourceAffordance, TransportationHandler transportationHandler,
            HouseholdKey householdkey, StrGuid guid, CalcRepo calcRepo) : base(sourceAffordance.Name, guid)
        {
            _householdkey = householdkey;
            _calcRepo = calcRepo;
            var status = new TransportationStatus(new TimeStep(0, 0, false), householdkey, "Initializing affordance base transport decorator for " + sourceAffordance.Name);
            _calcRepo.OnlineLoggingData.AddTransportationStatus(status);
            _transportationHandler = transportationHandler;
            SourceAffordance = sourceAffordance;
        }

        public string PrettyNameForDumping => Name + " (including transportation)";

        public CalcSite Site => SourceAffordance.Site ?? throw new LPGException("Incorrectly configured transport decorator: missing site");

        public virtual IEnumerable<IActivity> PlanActivation(TimeStep startTime, CalcPersonDto activator, ICalcSite? personSourceSite)
        {
            if (personSourceSite is null)
                throw new LPGException("When transport is enabled, the site must never be null.");
            // check if the person is already at the correct site
            if (personSourceSite == SourceAffordance.Site)
            {
                // no transport is necessary - simply pass on to the source affordance
                return SourceAffordance.PlanActivation(startTime, activator, personSourceSite);
            }

            // get the travel route determined and stored in the last IsBusy call
            if (!SelectedRoutes.TryGetValue(activator.Name, out var routeEntry) || routeEntry.PreviouslySelectedRoute is null)
                throw new LPGException("trying to activate without first checking if the affordance is busy is a bug. Please report.");
            CalcTravelRoute route = routeEntry.PreviouslySelectedRoute;

            // determine the arrival time at the target location
            int? travelDurationIfFound = route.GetDuration(startTime, activator, _transportationHandler.AllMoveableDevices);
            int travelDuration = travelDurationIfFound ?? throw new LPGException("Bug: couldn't calculate travel duration for route.");
            TimeStep affordanceStartTime = startTime.AddSteps(travelDuration);
            // TODO: the dynamic transport decorator needs to guess the route duration here instead, perhaps based on a moving average?

            // create the source affordance activity objects
            var sourceActivities = SourceAffordance.PlanActivation(affordanceStartTime, activator, personSourceSite);

            // collect all available alternative transportation devices the person could use for traveling
            var routes = _transportationHandler.CollectPossibleRoutes(personSourceSite.SiteCategory, Site.SiteCategory, activator, SourceAffordance);
            var devicesAtSrc = _transportationHandler.GetDevicesAtSite(personSourceSite.SiteCategory);
            var usableDevices = routes.SelectMany(route => route.GetUsableDevices(devicesAtSrc, activator)).Where(d => d.Category.IsLimitedToSingleLocation).Select(d => d.Name).Distinct().ToList();
            var deviceChoice = new TransportationDeviceChoice(_householdkey, startTime, activator.Name, usableDevices, personSourceSite.Name, Site.SiteCategory.Name, "");

            // create the travel activity
            var travelActivity = CreateActivity(activator, route, travelDuration, sourceActivities.First(), deviceChoice);

            // return the activity objects
            List<IActivity> activities = [travelActivity];
            activities.AddRange(sourceActivities);
            return activities;
        }

        protected virtual IActivity CreateActivity(CalcPersonDto activator, CalcTravelRoute route, int travelDuration, IActivity firstSourceActivity, TransportationDeviceChoice deviceChoice)
        {
            var name = "Travel Profile for Route " + route.Name + " to affordance " + SourceAffordance.Name;
            var stepValues = CalcProfile.MakeListwithValue1AndCustomDuration(travelDuration);
            string dataSource = firstSourceActivity.DataSource ?? SourceAffordance.Name;
            var travelProfile = new CalcProfile(name, StrGuid.New(), stepValues, ProfileType.Absolute, dataSource);
            return new StaticTravelActivity(activator.Name, travelProfile, this, new(route, deviceChoice));
        }

        public virtual void StartActivation(TimeStep startTime, string activatorName)
        { }

        public virtual void FinishActivation(TimeStep endTime, string activatorName)
        { }

        public virtual void Activate(TimeStep startTime, string activatorName, ICalcSite? personSourceSite, out IActivity personTimeProfile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Logs activation of a traveling activity.
        /// </summary>
        /// <param name="startTime">start timestep of the travel</param>
        /// <param name="personSourceSite">source site of the traveler</param>
        /// <param name="travelDuration">expected travel duration in timesteps</param>
        public void LogTransportationStatus(TimeStep startTime, ICalcSite personSourceSite, int travelDuration, TransportationDeviceChoice deviceChoice)
        {
            string transportation = travelDuration == 0 ? "no transportation" : $"a transportation duration of {travelDuration}";
            string status = $"\tActivating {Name} at {startTime} with {transportation}, moving from {personSourceSite} to {Site.Name}";
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(startTime, _householdkey, status));

            // set the correct owned devices
            deviceChoice.OwnedDevice = _transportationHandler.DeviceOwnerships.GetDevice(deviceChoice.PersonName)?.Name ?? "";
            _calcRepo.OnlineLoggingData.AddTransportationDeviceChoice(deviceChoice);
        }

        /// <summary>
        /// Log a transportation event for an activation of this travel affordance.
        /// </summary>
        /// <param activationName="usedDeviceEvents">list of travel device use events</param>
        /// <param activationName="activatorName">person activating the affordance</param>
        /// <param activationName="startTime">start time step of the travel affordance</param>
        /// <param activationName="sourceSite">the site the activating person was at before traveling</param>
        /// <param activationName="route">the selected route</param>
        /// <param activationName="duration">total duration of the travel</param>
        /// <param activationName="sourceAffordanceDuration">duration of the source affordance</param>
        public void LogTransportationEvent(List<CalcTravelRoute.CalcTravelDeviceUseEvent> usedDeviceEvents,
            string activatorName, TimeStep startTime, ICalcSite? sourceSite, CalcTravelRoute route, int duration,
            int sourceAffordanceDuration)
        {
            string usedDeviceNames = string.Join(", ", usedDeviceEvents.Select(x => x.Device.Name + "(" + x.DurationInSteps + ")"));
            _calcRepo.OnlineLoggingData.AddTransportationEvent(_householdkey, activatorName, startTime, sourceSite?.Name ?? "",
                Site.Name, route.Name, usedDeviceNames, duration, sourceAffordanceDuration, SourceAffordance.Name, usedDeviceEvents);
        }

        protected Dictionary<string, LastTimeEntry> SelectedRoutes { get; } = [];

        public string AffCategory => SourceAffordance.AffCategory;

        public ColorRGB AffordanceColor => SourceAffordance.AffordanceColor;

        public ActionAfterInterruption AfterInterruption => SourceAffordance.AfterInterruption;

        public CalcAffordanceType CalcAffordanceType => SourceAffordance.CalcAffordanceType;

        public IEnumerable<ICalcAffordanceBase> CollectSubAffordances(TimeStep time, bool onlyInterrupting, ICalcSite? srcSite)
            => SourceAffordance.CollectSubAffordances(time, onlyInterrupting, srcSite);

        public List<DeviceEnergyProfileTuple> Energyprofiles => SourceAffordance.Energyprofiles;

        /// <summary>
        /// Class for storing a selected route. A selected route is only valid for the specified
        /// </summary>
        /// <param name="validStartTimeForRoute">the timestep for which the route was selected</param>
        /// <param name="route">the selected route, or null if no route was found</param>
        protected class LastTimeEntry(TimeStep validStartTimeForRoute, CalcTravelRoute? route = null)
        {
            /// <summary>
            /// The timestep for which the route was selected and stored, and for which it is valid.
            /// </summary>
            public TimeStep ValidStartTimeForRoute { get; } = validStartTimeForRoute;

            /// <summary>
            /// The selected route. Can be null, which means that no route was found and the affordance
            /// is not available in this timestep.
            /// </summary>
            public CalcTravelRoute? PreviouslySelectedRoute { get; } = route;

            /// <summary>
            /// Checks whether this entry is applicable for the specified conditions. A time
            /// entry is only valid for one specific timestep.
            /// </summary>
            /// <param name="time">the current timestep</param>
            /// <returns>whether the time entry can be used</returns>
            internal bool IsApplicable(TimeStep time)
            {
                return ValidStartTimeForRoute == time;
            }
        }

        public BusynessType IsBusy(TimeStep time, ICalcSite? srcSite, CalcPersonDto calcPerson,
            bool clearDictionaries = true)
        {
            if (srcSite is null)
                throw new LPGException("When transport is enabled, the site must never be null.");

            // check if the person is already at the correct site
            if (srcSite == SourceAffordance.Site)
            {
                // no transport is necessary - simply check the source affordance for immediate activation
                return SourceAffordance.IsBusy(time, srcSite, calcPerson, clearDictionaries);
            }

            // if the last IsBusy call was not for the same timestep, reset the saved route
            if (SelectedRoutes.TryGetValue(calcPerson.Name, out var routeEntry) && !routeEntry.IsApplicable(time))
            {
                SelectedRoutes.Remove(calcPerson.Name);
                routeEntry = null;
            }

            // determine the route to the target location
            CalcTravelRoute? route;
            if (routeEntry is not null)
            {
                route = routeEntry.PreviouslySelectedRoute;
            }
            else
            {
                // select an appropriate travel route for the given situation and store it
                route = _transportationHandler.GetTravelRouteFromSrcLoc(srcSite.SiteCategory, Site.SiteCategory, time, calcPerson, SourceAffordance, _calcRepo);
                SelectedRoutes.Add(calcPerson.Name, new(time, route));
            }

            if (route == null)
            {
                return BusynessType.NoRoute;
            }

            // determine the arrival time at the target location
            int? travelDurationN = route.GetDuration(time, calcPerson, _transportationHandler.AllMoveableDevices);
            if (travelDurationN == null)
            {
                throw new LPGException("Bug: couldn't calculate travel duration for route.");
            }

            TimeStep dstStartTime = time.AddSteps(travelDurationN.Value);
            if (dstStartTime.InternalStep > _calcRepo.CalcParameters.InternalTimesteps)
            {
                // if the end of the travel is after the simulation, everything is ok.
                return BusynessType.NotBusy;
            }
            var result = SourceAffordance.IsBusy(dstStartTime, srcSite, calcPerson, clearDictionaries);
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(time,
                _householdkey, $"\t\t{time} @{srcSite} by {calcPerson.Name}. Checking {Name} for busyness: {result} @time {dstStartTime} with the " +
                $"route {route.Name} and a travel duration of {travelDurationN}"));
            return result;
        }

        public CalcSubAffordance GetAsSubAffordance() => SourceAffordance.GetAsSubAffordance();

        public bool IsInterruptable => SourceAffordance.IsInterruptable;

        public bool IsInterrupting => SourceAffordance.IsInterrupting;

        public BodilyActivityLevel BodilyActivityLevel => SourceAffordance.BodilyActivityLevel;

        public int MaximumAge => SourceAffordance.MaximumAge;

        public int MiniumAge => SourceAffordance.MiniumAge;

        public bool NeedsLight => SourceAffordance.NeedsLight;

        public CalcLocation ParentLocation => SourceAffordance.ParentLocation;

        public PermittedGender PermittedGender => SourceAffordance.PermittedGender;

        public bool RandomEffect => SourceAffordance.RandomEffect;

        public bool RequireAllAffordances => SourceAffordance.RequireAllAffordances;

        public List<CalcDesire> Satisfactionvalues => SourceAffordance.Satisfactionvalues;

        public string SourceTrait => SourceAffordance.SourceTrait;

        public List<ICalcAffordanceBase> SubAffordances => SourceAffordance.SubAffordances;

        public string? TimeLimitName => SourceAffordance.TimeLimitName;
        public bool AreThereDuplicateEnergyProfiles() => SourceAffordance.AreThereDuplicateEnergyProfiles();

        public string? AreDeviceProfilesEmpty() => SourceAffordance.AreDeviceProfilesEmpty();

        public double Weight => SourceAffordance.Weight;
    }
}

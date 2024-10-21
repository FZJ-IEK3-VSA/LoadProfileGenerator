using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging.Loggers;

namespace CalculationEngine.Transportation
{
    public class AffordanceBaseTransportDecorator : CalcBase, ICalcAffordanceBase {
        public readonly ICalcAffordanceBase SourceAffordance;
        protected readonly TransportationHandler _transportationHandler;
        protected readonly HouseholdKey _householdkey;

        protected readonly CalcRepo _calcRepo;

        /// <summary>
        /// General flag to decide whether dynamic simulation of travel times and remote affordances
        /// is done or not. If not, static route calculation and only fixed-duration affordances are used.
        /// </summary>
        public static readonly bool DynamicCitySimulation = true;

        /// <summary>
        /// Creates the correct transport decorator to use, depending on whether dynamic city simulation is enabled or not
        /// </summary>
        /// <param name="sourceAffordance">the affordance to decorate</param>
        /// <param name="transportationHandler">the transportation handler object</param>
        /// <param name="name">the name of the affordance</param>
        /// <param name="householdkey">the household key</param>
        /// <param name="guid">guid of the decorated affordance</param>
        /// <param name="calcRepo">the calc repo</param>
        /// <returns>the newly created transport decorator</returns>
        public static AffordanceBaseTransportDecorator CreateTransportDecorator(ICalcAffordanceBase sourceAffordance, TransportationHandler transportationHandler,
            string name, HouseholdKey householdkey, StrGuid guid, CalcRepo calcRepo)
        {
            if (DynamicCitySimulation)
            {
                return new AffordanceBaseTransportDecoratorDynamic(sourceAffordance, transportationHandler, name, householdkey, guid, calcRepo);
            } else
            {
                return new AffordanceBaseTransportDecorator(sourceAffordance, transportationHandler, name, householdkey, guid, calcRepo);
            }
        }

        protected AffordanceBaseTransportDecorator(ICalcAffordanceBase sourceAffordance, TransportationHandler transportationHandler,
            string name, HouseholdKey householdkey, StrGuid guid, CalcRepo calcRepo) : base(name, guid)
        {
            _householdkey= householdkey;
            _calcRepo = calcRepo;
            _calcRepo.OnlineLoggingData.AddTransportationStatus( new TransportationStatus(new TimeStep(0,0,false), householdkey, "Initializing affordance base transport decorator for " + name));
            _transportationHandler = transportationHandler;
            SourceAffordance = sourceAffordance;
        }

        public string PrettyNameForDumping => Name + " (including transportation)";

        public ICalcSite Site => SourceAffordance.Site ?? throw new LPGException("Incorrectly configured transport decorator: missing site");

        public virtual void Activate(TimeStep startTime, string activatorName, CalcLocation personSourceLocation,
            out IAffordanceActivation activationInfo)
        {
            if (_myLastTimeEntry.TimeOfLastEvalulation != startTime) {
                throw new LPGException("trying to activate without first checking if the affordance is busy is a bug. Please report.");
            }

            // TODO: this condition is not sufficient with the current CalcSites and Locations in the LPG (multiple locations all in the same "Event Location" site)
            if (personSourceLocation.CalcSite == SourceAffordance.Site)
            {
                // no transport is necessary - simply activate the source affordance
                SourceAffordance.Activate(startTime, activatorName, personSourceLocation, out activationInfo);
                return;
            }


            // get the route which was already determined in IsBusy and activate it
            CalcTravelRoute route = _myLastTimeEntry.PreviouslySelectedRoutes[personSourceLocation];
            int routeduration = route.Activate(startTime, activatorName, out var usedDeviceEvents, _transportationHandler.DeviceOwnerships);
            // TODO: probably with full transport simulation, the route will not be activated here, but step by step in CalcPerson

            // log transportation info
            string status;
            if (routeduration == 0) {
                status = $"\tActivating {Name} at {startTime} with no transportation and moving from {personSourceLocation} to "
                    + $"{SourceAffordance.ParentLocation.Name} for affordance {SourceAffordance.Name}";
            }
            else {
                status = $"\tActivating {Name} at {startTime} with a transportation duration of {routeduration} for moving from "
                    + $"{personSourceLocation} to {SourceAffordance.ParentLocation.Name}";
            }
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(startTime, _householdkey, status));


            // no dynamic travel is happening, so the affordance can now be activated in advance
            IAffordanceActivation? sourceActivation = null;
            TimeStep affordanceStartTime = startTime.AddSteps(routeduration);
            if (affordanceStartTime.InternalStep < _calcRepo.CalcParameters.InternalTimesteps)
            {
                // only activate the source affordance if the activation is still in the simulation time frame
                SourceAffordance.Activate(affordanceStartTime, activatorName, personSourceLocation, out sourceActivation);
            }

            // create the travel profile
            var name = "Travel Profile for Route " + route.Name + " to affordance " + SourceAffordance.Name;
            var stepValues = CalcProfile.MakeListwithValue1AndCustomDuration(routeduration);
            string dataSource = sourceActivation?.DataSource ?? SourceAffordance.Name;
            var newPersonProfile = new CalcProfile(name, StrGuid.New(), stepValues, ProfileType.Absolute, dataSource);

            int sourceAffDuration = 0;
            if (sourceActivation is CalcProfile sourcePersonProfile)
            {
                // if the source affordance was activated and provided a profile, append it to the travel profile
                newPersonProfile.AppendProfile(sourcePersonProfile);
                sourceAffDuration = sourcePersonProfile.StepValues.Count;
            }
            activationInfo = newPersonProfile;

            // log the transportation event
            LogTransportationEvent(usedDeviceEvents, activatorName, startTime, personSourceLocation.CalcSite, route, routeduration, sourceAffDuration);
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
            string activatorName, TimeStep startTime, CalcSite? sourceSite, CalcTravelRoute route, int duration,
            int sourceAffordanceDuration)
        {
            string usedDeviceNames = string.Join(", ", usedDeviceEvents.Select(x => x.Device.Name + "(" + x.DurationInSteps + ")"));
            _calcRepo.OnlineLoggingData.AddTransportationEvent(_householdkey, activatorName, startTime, sourceSite?.Name ?? "",
                Site.Name, route.Name, usedDeviceNames, duration, sourceAffordanceDuration, SourceAffordance.Name, usedDeviceEvents);
        }

        public string AffCategory => SourceAffordance.AffCategory;

        public ColorRGB AffordanceColor => SourceAffordance.AffordanceColor;

        public ActionAfterInterruption AfterInterruption => SourceAffordance.AfterInterruption;

        public CalcAffordanceType CalcAffordanceType => SourceAffordance.CalcAffordanceType;

        public IEnumerable<ICalcAffordanceBase> CollectSubAffordances(TimeStep time, bool onlyInterrupting,
                                                             CalcLocation srcLocation) =>
            SourceAffordance.CollectSubAffordances(time, onlyInterrupting, srcLocation);

        public List<DeviceEnergyProfileTuple> Energyprofiles => SourceAffordance.Energyprofiles;

        protected class LastTimeEntry(string personName, TimeStep timeOfLastEvalulation)
        {
            public string PersonName { get; } = personName;
            public TimeStep TimeOfLastEvalulation { get; } = timeOfLastEvalulation;
            public Dictionary<CalcLocation, CalcTravelRoute> PreviouslySelectedRoutes { get; } = [];
        }

        /// <summary>
        /// Whenever a route is generated to check if affordance activation is possible in IsBusy, this
        /// field saves the route. This is necessary to use the same route in case the affordance is
        /// actually activated in the same timestep.
        /// </summary>
        protected LastTimeEntry _myLastTimeEntry = new("",new TimeStep(-1,0,false));

        public BusynessType IsBusy(TimeStep time, CalcLocation srcLocation, CalcPersonDto calcPerson,
            bool clearDictionaries = true)
        {
            // if the last IsBusy call was not for the same person and time, reset the saved routes
            if (_myLastTimeEntry.TimeOfLastEvalulation != time || _myLastTimeEntry.PersonName != calcPerson.Name) {
                _myLastTimeEntry = new LastTimeEntry(calcPerson.Name, time);
            }

            // determine the route to the target location
            CalcTravelRoute? route;
            if (_myLastTimeEntry.PreviouslySelectedRoutes.ContainsKey(srcLocation)) {
                route = _myLastTimeEntry.PreviouslySelectedRoutes[srcLocation];
            }
            else {
                route = _transportationHandler.GetTravelRouteFromSrcLoc(srcLocation, Site, time, calcPerson, SourceAffordance, _calcRepo);
                if (route != null) {
                    _myLastTimeEntry.PreviouslySelectedRoutes.Add(srcLocation, route);

                    if (_myLastTimeEntry.PreviouslySelectedRoutes.Count >= 2)
                        Debugger.Break(); // TODO remove
                }
            }

            if (route == null) {
                return BusynessType.NoRoute;
            }

            // determine the arrival time at the target location
            int? travelDurationN = route.GetDuration(time, calcPerson, _transportationHandler.AllMoveableDevices, _transportationHandler.DeviceOwnerships);
            if (travelDurationN == null) {
                throw new LPGException("Bug: couldn't calculate travel duration for route.");
            }

            TimeStep dstStartTime = time.AddSteps(travelDurationN.Value);
            if (dstStartTime.InternalStep > _calcRepo.CalcParameters.InternalTimesteps) {
                // if the end of the travel is after the simulation, everything is ok.
                return BusynessType.NotBusy;
            }
            var result = SourceAffordance.IsBusy(dstStartTime, srcLocation, calcPerson,
                clearDictionaries);
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(
                time,
                _householdkey, "\t\t" + time  + " @" + srcLocation + " by " + calcPerson.Name
                                             + "Checking " + Name + " for busyness: " + result + " @time " + dstStartTime
                                             + " with the route " + route.Name + " and a travel duration of " + travelDurationN));
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

        public int Weight => SourceAffordance.Weight;
    }
}

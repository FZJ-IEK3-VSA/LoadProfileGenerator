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
        public readonly ICalcAffordanceBase _sourceAffordance;
        private readonly TransportationHandler _transportationHandler;
        private readonly HouseholdKey _householdkey;

        private readonly CalcRepo _calcRepo;

        /// <summary>
        /// General flag to decide whether dynamic simulation of travel times is done or
        /// not. If not, static route calculation is used.
        /// </summary>
        public static readonly bool DynamicTransportSimulation = true;

        public AffordanceBaseTransportDecorator(ICalcAffordanceBase sourceAffordance, CalcSite site,
            TransportationHandler transportationHandler, string name, HouseholdKey householdkey, StrGuid guid,
            CalcRepo calcRepo)
            : base(name, guid)
        {
            if (!site.Locations.Contains(sourceAffordance.ParentLocation)) {
                throw new LPGException("Wrong site. Bug. Please report.");
            }
            _householdkey= householdkey;
            _calcRepo = calcRepo;
            _calcRepo.OnlineLoggingData.AddTransportationStatus( new TransportationStatus(new TimeStep(0,0,false), householdkey, "Initializing affordance base transport decorator for " + name));
            Site = site;
            _transportationHandler = transportationHandler;
            _sourceAffordance = sourceAffordance;
        }

        /// <summary>
        /// Creates a copy of the specified transport decorator, but replaces the source affordance with a new remote affordance.
        /// </summary>
        /// <param name="original">the original affordance transport decorator</param>
        /// <param name="remoteAffordance">the remote affordance that will be used as source affordance</param>
        public AffordanceBaseTransportDecorator(AffordanceBaseTransportDecorator original, CalcAffordanceRemote remoteAffordance) : base(remoteAffordance.Name, StrGuid.New())
        {
            _householdkey = original._householdkey;
            _calcRepo = original._calcRepo;
            Site = original.Site;
            _transportationHandler = original._transportationHandler;
            _sourceAffordance = remoteAffordance;

            var message = "Copying affordance base transport decorator for remote affordance " + remoteAffordance;
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(new TimeStep(0, 0, false), _householdkey, message));
        }

        public string PrettyNameForDumping => Name + " (including transportation)";

        public CalcSite Site { get; }

        public void Activate(TimeStep startTime, string activatorName, CalcLocation personSourceLocation,
            out IAffordanceActivation activationInfo)
        {
            if (_myLastTimeEntry.TimeOfLastEvalulation != startTime) {
                throw new LPGException("trying to activate without first checking if the affordance is busy is a bug. Please report.");
            }

            // get the route which was already determined in IsBusy and activate it
            CalcTravelRoute route = _myLastTimeEntry.PreviouslySelectedRoutes[personSourceLocation];
            int routeduration = route.Activate(startTime, activatorName, out var usedDeviceEvents, _transportationHandler.DeviceOwnerships);
            // TODO: probably with full transport simulation, the route will not be activated here, but step by step in CalcPerson

            // log transportation info
            string status;
            if (routeduration == 0) {
                status = $"\tActivating {Name} at {startTime} with no transportation and moving from {personSourceLocation} to "
                    + $"{_sourceAffordance.ParentLocation.Name} for affordance {_sourceAffordance.Name}";
            }
            else {
                status = $"\tActivating {Name} at {startTime} with a transportation duration of {routeduration} for moving from "
                    + $"{personSourceLocation} to {_sourceAffordance.ParentLocation.Name}";
            }
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(startTime, _householdkey, status));


            // check if a travel of dynamic duration will start
            var profileGuid = System.Guid.NewGuid().ToStrGuid();
            IAffordanceActivation? sourceActivation = null;
            if (DynamicTransportSimulation && personSourceLocation.CalcSite != _sourceAffordance.Site)
            {
                // person has to travel to the target site with an unknown duration - cannot activate the source affordance yet
                var activationName = "Dynamic Travel Profile for Route " + route.Name + " to affordance " + _sourceAffordance.Name;
                // determine the travel target: the POI of the affordance if it is remote, else null (for home)
                var destination = _sourceAffordance is CalcAffordanceRemote remoteAff ? remoteAff.PointOfInterest : null;
                activationInfo = new RemoteAffordanceActivation(activationName, _sourceAffordance.Name, startTime, destination, route, personSourceLocation.CalcSite, this);
                return;
            }

            // no dynamic travel is happening, so the affordance can now be activated in advance
            TimeStep affordanceStartTime = startTime.AddSteps(routeduration);
            if (affordanceStartTime.InternalStep < _calcRepo.CalcParameters.InternalTimesteps)
            {
                // only activate the source affordance if the activation is still in the simulation time frame
                _sourceAffordance.Activate(affordanceStartTime, activatorName, personSourceLocation, out sourceActivation);
                if (!sourceActivation.IsDetermined)
                {
                    if (DynamicTransportSimulation)
                    {
                        // person must already be at the target site
                        // affordance duration is unknown - do I still need to wrap into travel profile?
                        throw new NotImplementedException("TODO: remote activity without travel not implemented yet.");
                    }
                    else
                    {
                        throw new LPGException("Remote affordances may only occur when dynamic transport is enabled.");
                    }
                }
            }

            int sourceAffDuration = -1;
            // create the travel profile
            var name = "Travel Profile for Route " + route.Name + " to affordance " + _sourceAffordance.Name;
            var stepValues = CalcProfile.MakeListwithValue1AndCustomDuration(routeduration);
            string dataSource = sourceActivation?.DataSource ?? _sourceAffordance.Name;
            var newPersonProfile = new CalcProfile(name, profileGuid, stepValues, ProfileType.Absolute, dataSource);

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
                Site.Name, route.Name, usedDeviceNames, duration, sourceAffordanceDuration, _sourceAffordance.Name, usedDeviceEvents);
        }

        public string AffCategory => _sourceAffordance.AffCategory;

        public ColorRGB AffordanceColor => _sourceAffordance.AffordanceColor;

        public ActionAfterInterruption AfterInterruption => _sourceAffordance.AfterInterruption;

        public CalcAffordanceType CalcAffordanceType => _sourceAffordance.CalcAffordanceType;

        public List<CalcSubAffordance> CollectSubAffordances(TimeStep time, bool onlyInterrupting,
                                                             CalcLocation srcLocation) =>
            _sourceAffordance.CollectSubAffordances(time, onlyInterrupting, srcLocation);

        public List<DeviceEnergyProfileTuple> Energyprofiles => _sourceAffordance.Energyprofiles;

        private class LastTimeEntry(string personName, TimeStep timeOfLastEvalulation)
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
        private LastTimeEntry _myLastTimeEntry = new("",new TimeStep(-1,0,false));

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
                route = _transportationHandler.GetTravelRouteFromSrcLoc(srcLocation, Site, time, calcPerson, _sourceAffordance, _calcRepo);
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
            var result = _sourceAffordance.IsBusy(dstStartTime, srcLocation, calcPerson,
                clearDictionaries);
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(
                time,
                _householdkey, "\t\t" + time  + " @" + srcLocation + " by " + calcPerson.Name
                                             + "Checking " + Name + " for busyness: " + result + " @time " + dstStartTime
                                             + " with the route " + route.Name + " and a travel duration of " + travelDurationN));
            return result;
        }

        public bool IsInterruptable => _sourceAffordance.IsInterruptable;

        public bool IsInterrupting => _sourceAffordance.IsInterrupting;

        public BodilyActivityLevel BodilyActivityLevel => _sourceAffordance.BodilyActivityLevel;

        public int MaximumAge => _sourceAffordance.MaximumAge;

        public int MiniumAge => _sourceAffordance.MiniumAge;

        public bool NeedsLight => _sourceAffordance.NeedsLight;

        public CalcLocation ParentLocation => _sourceAffordance.ParentLocation;

        public PermittedGender PermittedGender => _sourceAffordance.PermittedGender;

        public bool RandomEffect => _sourceAffordance.RandomEffect;

        public bool RequireAllAffordances => _sourceAffordance.RequireAllAffordances;

        public List<CalcDesire> Satisfactionvalues => _sourceAffordance.Satisfactionvalues;

        public string SourceTrait => _sourceAffordance.SourceTrait;

        public List<CalcSubAffordance> SubAffordances => _sourceAffordance.SubAffordances;

        public string? TimeLimitName => _sourceAffordance.TimeLimitName;
        public bool AreThereDuplicateEnergyProfiles() => _sourceAffordance.AreThereDuplicateEnergyProfiles();

        public string? AreDeviceProfilesEmpty() => _sourceAffordance.AreDeviceProfilesEmpty();

        public int Weight => _sourceAffordance.Weight;
    }
}

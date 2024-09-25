using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging.Loggers;

namespace CalculationEngine.Transportation {
    public class AffordanceBaseTransportDecorator : CalcBase, ICalcAffordanceBase {
        [JetBrains.Annotations.NotNull]
        private readonly ICalcAffordanceBase _sourceAffordance;
        private readonly TransportationHandler _transportationHandler;
        [JetBrains.Annotations.NotNull]
        private readonly HouseholdKey _householdkey;

        private readonly CalcRepo _calcRepo;

        //TODO: fix the requirealldesires flag in the constructor
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        //[CanBeNull]
        //private CalcAffordanceBase _MainAffordance;
        public AffordanceBaseTransportDecorator([JetBrains.Annotations.NotNull] ICalcAffordanceBase sourceAffordance,
            [JetBrains.Annotations.NotNull] CalcSite site, [JetBrains.Annotations.NotNull] TransportationHandler transportationHandler,
            [JetBrains.Annotations.NotNull] string name,   [JetBrains.Annotations.NotNull] HouseholdKey householdkey,
                                                StrGuid guid, CalcRepo calcRepo)
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

        public string PrettyNameForDumping => Name + " (including transportation)";

        [JetBrains.Annotations.NotNull]
        public CalcSite Site { get; }

        public void Activate(TimeStep startTime, string activatorName, CalcLocation personSourceLocation,
            out ICalcProfile personTimeProfile)
        {
            if (_myLastTimeEntry.TimeOfLastEvalulation != startTime) {
                throw new LPGException("trying to activate without first checking if the affordance is busy is a bug. Please report.");
            }

            // get the route which was already determined in IsBusy and activate it
            CalcTravelRoute route = _myLastTimeEntry.PreviouslySelectedRoutes[personSourceLocation];
            int routeduration = route.Activate(startTime, activatorName, out var usedDeviceEvents, _transportationHandler.DeviceOwnerships);

            // log transportation info
            if (routeduration == 0) {
                _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(startTime, _householdkey,
                    "\tActivating " + Name + " at " + startTime + " with no transportation and moving from " + personSourceLocation + " to " + _sourceAffordance.ParentLocation.Name + " for affordance " + _sourceAffordance.Name));
            }
            else {
                _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(startTime, _householdkey,
                    "\tActivating " + Name + " at " + startTime + " with a transportation duration of " + routeduration + " for moving from " + personSourceLocation + " to " + _sourceAffordance.ParentLocation.Name));
            }

            // activate the source affordance and create the person profile
            TimeStep affordanceStartTime = startTime.AddSteps(routeduration);
            if (affordanceStartTime.InternalStep < _calcRepo.CalcParameters.InternalTimesteps) {
                _sourceAffordance.Activate(affordanceStartTime, activatorName, personSourceLocation, out var sourcePersonProfile);
                // create the travel profile and append the actual affordance profile
                CalcProfile newPersonProfile = new CalcProfile(
                    "Travel Profile for Route " + route.Name + " to affordance " + _sourceAffordance.Name,
                    System.Guid.NewGuid().ToStrGuid(),
                    CalcProfile.MakeListwithValue1AndCustomDuration(routeduration), ProfileType.Absolute,
                    sourcePersonProfile.DataSource);
                newPersonProfile.AppendProfile(sourcePersonProfile);
                personTimeProfile = newPersonProfile;
                // log the transportation event
                string usedDeviceNames = String.Join(", ", usedDeviceEvents.Select(x => x.Device.Name + "(" + x.DurationInSteps + ")"));
                _calcRepo.OnlineLoggingData.AddTransportationEvent(_householdkey, activatorName, startTime, personSourceLocation.CalcSite?.Name ?? "",
                    Site.Name, route.Name, usedDeviceNames, routeduration, sourcePersonProfile.StepValues.Count, _sourceAffordance.Name, usedDeviceEvents);
            }
            else {
                // this is if the simulation ends during a transport
                personTimeProfile = new CalcProfile(
                    "Travel Profile for Route " + route.Name + " to affordance " + _sourceAffordance.Name,
                    System.Guid.NewGuid().ToStrGuid(),
                    CalcProfile.MakeListwithValue1AndCustomDuration(routeduration),  ProfileType.Absolute,
                    _sourceAffordance.Name);
                // log the transportation event
                string usedDeviceNames = String.Join(", ", usedDeviceEvents.Select(x => x.Device.Name + "(" + x.DurationInSteps + ")"));
                _calcRepo.OnlineLoggingData.AddTransportationEvent(_householdkey, activatorName, startTime, personSourceLocation.CalcSite?.Name ?? "",
                    Site.Name, route.Name, usedDeviceNames, routeduration, personTimeProfile.StepValues.Count, _sourceAffordance.Name,usedDeviceEvents);
            }
        }

        public string AffCategory => _sourceAffordance.AffCategory;

        public ColorRGB AffordanceColor => _sourceAffordance.AffordanceColor;

        public ActionAfterInterruption AfterInterruption => _sourceAffordance.AfterInterruption;

        public CalcAffordanceType CalcAffordanceType => _sourceAffordance.CalcAffordanceType;

        public List<CalcSubAffordance> CollectSubAffordances(TimeStep time, bool onlyInterrupting,
                                                             CalcLocation srcLocation) =>
            _sourceAffordance.CollectSubAffordances(time, onlyInterrupting, srcLocation);

        public List<DeviceEnergyProfileTuple> Energyprofiles => _sourceAffordance.Energyprofiles;

        private class LastTimeEntry {
            public LastTimeEntry([JetBrains.Annotations.NotNull] string personName, [JetBrains.Annotations.NotNull] TimeStep timeOfLastEvalulation)
            {
                PersonName = personName;
                TimeOfLastEvalulation = timeOfLastEvalulation;
            }

            [JetBrains.Annotations.NotNull]
            public string PersonName { get; }
            [JetBrains.Annotations.NotNull]
            public TimeStep TimeOfLastEvalulation { get; }
            [JetBrains.Annotations.NotNull]
            public Dictionary<CalcLocation, CalcTravelRoute> PreviouslySelectedRoutes { get; } = [];
        }

        /// <summary>
        /// Whenever a route is generated to check if affordance activation is possible in IsBusy, this
        /// field saves the route. This is necessary to use the same route in case the affordance is
        /// actually activated in the same timestep.
        /// </summary>
        [JetBrains.Annotations.NotNull]
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
            // ReSharper disable once PossibleInvalidOperationException
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

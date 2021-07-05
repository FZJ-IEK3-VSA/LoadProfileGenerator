using System;
using System.Collections.Generic;
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
        private readonly TransportationHandler? _transportationHandler;
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

        //BitArray ICalcAffordanceBase.IsBusyArray { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Activate(TimeStep startTime, string activatorName, CalcLocation personSourceLocation,
            out ICalcProfile personTimeProfile)
        {
            if (_myLastTimeEntry.TimeOfLastEvalulation != startTime) {
                throw new LPGException("trying to activate without first checking if the affordance is busy is a bug. Please report.");
            }

            CalcTravelRoute route = _myLastTimeEntry.PreviouslySelectedRoutes[personSourceLocation];
            int routeduration = route.Activate(startTime,activatorName, out var usedDevices);

            if (routeduration == 0) {
                _calcRepo.OnlineLoggingData.AddTransportationStatus(
                    new TransportationStatus(startTime, _householdkey,
                    "\tActivating " + Name + " at " + startTime + " with no transportation and moving from " + personSourceLocation + " to " + _sourceAffordance.ParentLocation.Name + " for affordance " + _sourceAffordance.Name));
            }
            else {
                _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(
                    startTime,
                    _householdkey,
                    "\tActivating " + Name + " at " + startTime + " with a transportation duration of " + routeduration + " for moving from " + personSourceLocation + " to " + _sourceAffordance.ParentLocation.Name));
            }

            TimeStep affordanceStartTime = startTime.AddSteps(routeduration);
            if (affordanceStartTime.InternalStep < _calcRepo.CalcParameters.InternalTimesteps) {
                _sourceAffordance.Activate(affordanceStartTime, activatorName,  personSourceLocation,
                     out var sourcePersonProfile);
            CalcProfile newPersonProfile = new CalcProfile(
                    "Travel Profile for Route " + route.Name + " to affordance " + _sourceAffordance.Name,
                    System.Guid.NewGuid().ToStrGuid(),
                    CalcProfile.MakeListwithValue1AndCustomDuration(routeduration),  ProfileType.Absolute,
                    sourcePersonProfile.DataSource);
                newPersonProfile.AppendProfile(sourcePersonProfile);
                personTimeProfile = newPersonProfile;
                string usedDeviceNames = String.Join(", ", usedDevices.Select(x => x.Device.Name + "(" + x.DurationInSteps + ")"));
                _calcRepo.OnlineLoggingData.AddTransportationEvent(_householdkey, activatorName, startTime, personSourceLocation.CalcSite?.Name??"",
                    Site.Name,
                    route.Name, usedDeviceNames, routeduration, sourcePersonProfile.StepValues.Count,
                    _sourceAffordance.Name, usedDevices);
            }
            else {
                //this is if the simulation ends during a transport
                CalcProfile newPersonProfile = new CalcProfile(
                    "Travel Profile for Route " + route.Name + " to affordance " + _sourceAffordance.Name,
                    System.Guid.NewGuid().ToStrGuid(),
                    CalcProfile.MakeListwithValue1AndCustomDuration(routeduration),  ProfileType.Absolute,
                    _sourceAffordance.Name);
                personTimeProfile = newPersonProfile;
                string usedDeviceNames = String.Join(", ", usedDevices.Select(x => x.Device.Name + "(" + x.DurationInSteps + ")"));
                _calcRepo.OnlineLoggingData.AddTransportationEvent(_householdkey, activatorName, startTime,
                    personSourceLocation.CalcSite?.Name ?? "",
                    Site.Name,
                    route.Name, usedDeviceNames, routeduration, newPersonProfile.StepValues.Count,
                    _sourceAffordance.Name,usedDevices);
            }
        }

        public string AffCategory => _sourceAffordance.AffCategory;

        public ColorRGB AffordanceColor => _sourceAffordance.AffordanceColor;

        public ActionAfterInterruption AfterInterruption => _sourceAffordance.AfterInterruption;

        public int CalcAffordanceSerial => _sourceAffordance.CalcAffordanceSerial;

        public CalcAffordanceType CalcAffordanceType => _sourceAffordance.CalcAffordanceType;

        //public ICalcProfile CollectPersonProfile() => _sourceAffordance.CollectPersonProfile();

        public List<CalcSubAffordance> CollectSubAffordances(TimeStep time, bool onlyInterrupting,
                                                             CalcLocation srcLocation) =>
            _sourceAffordance.CollectSubAffordances(time, onlyInterrupting, srcLocation);

        public List<CalcAffordance.DeviceEnergyProfileTuple> Energyprofiles => _sourceAffordance.Energyprofiles;

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
            public Dictionary<CalcLocation, CalcTravelRoute>
                PreviouslySelectedRoutes { get; } = new Dictionary<CalcLocation, CalcTravelRoute>();
        }

        [JetBrains.Annotations.NotNull]
        private LastTimeEntry _myLastTimeEntry = new LastTimeEntry("",new TimeStep(-1,0,false));

        public int DefaultPersonProfileLength => _sourceAffordance.DefaultPersonProfileLength;

        public BusynessType IsBusy(TimeStep time,
                           CalcLocation srcLocation, string calcPersonName,
            bool clearDictionaries = true)
        {
            if (_myLastTimeEntry.TimeOfLastEvalulation != time || _myLastTimeEntry.PersonName != calcPersonName) {
                _myLastTimeEntry = new LastTimeEntry(calcPersonName,time);
            }

            CalcTravelRoute? route;
            if (_myLastTimeEntry.PreviouslySelectedRoutes.ContainsKey(srcLocation)) {
                route = _myLastTimeEntry.PreviouslySelectedRoutes[srcLocation];
            }
            else {
                if (_transportationHandler == null) {
                    throw new LPGException("was null");
                }
                route = _transportationHandler.GetTravelRouteFromSrcLoc(srcLocation, Site,time,calcPersonName, _sourceAffordance, _calcRepo);
                if (route != null) {
                    _myLastTimeEntry.PreviouslySelectedRoutes.Add(srcLocation, route);
                }
            }

            if (route == null) {
                return BusynessType.NoRoute;
            }

            if (_transportationHandler == null) {
                throw new LPGException("was null");
            }
            // ReSharper disable once PossibleInvalidOperationException
            int? travelDurationN = route.GetDuration(time, calcPersonName,
                _transportationHandler.AllMoveableDevices);
            if (travelDurationN == null) {
                throw new LPGException("Bug: couldn't calculate travel duration for route.");
            }

            TimeStep dstStartTime = time.AddSteps(travelDurationN.Value);
            if (dstStartTime.InternalStep > _calcRepo.CalcParameters.InternalTimesteps) {
                //if the end of the activity is after the simulation, everything is ok.
                return BusynessType.NotBusy;
            }
            var result = _sourceAffordance.IsBusy(dstStartTime, srcLocation, calcPersonName,
                clearDictionaries);
            _calcRepo.OnlineLoggingData.AddTransportationStatus(new TransportationStatus(
                time,
                _householdkey, "\t\t" + time  + " @" + srcLocation + " by " + calcPersonName
                                             + "Checking " + Name + " for busyness: " + result + " @time " + dstStartTime
                                             + " with the route " + route.Name + " and a travel duration of " + travelDurationN));
            return result;
        }

        //public BitArray IsBusyArray => _sourceAffordance.IsBusyArray;

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
        public bool AreThereDuplicateEnergyProfiles()
        {
            return _sourceAffordance.AreThereDuplicateEnergyProfiles();
        }

        public string? AreDeviceProfilesEmpty()
        {
            return _sourceAffordance.AreDeviceProfilesEmpty();
        }

        public int Weight => _sourceAffordance.Weight;
    }
}
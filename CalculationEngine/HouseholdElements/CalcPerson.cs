//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.Activities;
using CalculationEngine.CitySimulation;
using CalculationEngine.Helper;
using CalculationEngine.OnlineLogging;
using CalculationEngine.Transportation;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;

#endregion

namespace CalculationEngine.HouseholdElements
{
    public class CalcPerson : CalcBase
    {
        /// <summary>
        /// Name of the CalcSite "Home". This is relevant for the city simulation
        /// and determination of remote affordances.
        /// </summary>
        private const string NameOfHomeCalcSite = "Home";

        private readonly PotentialAffs _normalPotentialAffs = new PotentialAffs();

        private readonly CalcPersonDesires _normalDesires;

        /// <summary>
        /// Stores the last few activated affordances so repetitons can be avoided
        /// </summary>
        private readonly List<ICalcAffordanceBase> _previousAffordances = [];
        /// <summary>
        /// Stores the last few activated affordances including their respective end timesteps.
        /// This is needed to resume them in case they are interrupted.
        /// This list only stores affordances for which the duration is known in advance.
        /// </summary>
        private readonly List<Tuple<ICalcAffordanceBase, TimeStep>> _previousAffordancesWithEndTime = [];

        private readonly PotentialAffs _sicknessPotentialAffs = new PotentialAffs();

        private bool _alreadyloggedvacation;

        /// <summary>
        /// The currently active affordance. This can be a transport decorator.
        /// </summary>
        private ICalcAffordanceBase CurrentAffordance => activityQueue.CurrentActivity.Affordance;

        /// <summary>
        /// The site where the person currently is. May be one of the predefined CalcSites, or, with
        /// dynamic city simulation, a CitySite corresponding to a specific point of interest.
        /// Will always be null if transport is disabled, and must never be null if transport is enabled.
        /// </summary>
        private ICalcSite? _currentSite;

        /// <summary>
        /// The location of the currently active affordance. During transport, this is already the location
        /// of the target affordance.
        /// </summary>
        private CalcLocation _currentLocation;

        /// <summary>
        /// Is true if an affordance that interrupted another is currently active.
        /// Prevents interrupting an already interrupting affordance.
        /// </summary>
        private bool _isCurrentlyPriorityAffordanceRunning;

        private bool _isCurrentlySick;

        private readonly CalcPersonDto _calcPerson;

        public HouseholdKey HouseholdKey => _calcPerson.HouseholdKey;

        private readonly CalcRepo _calcRepo;

        /// <summary>
        /// Stores the next planned activities or activity steps.
        /// </summary>
        private ActivityQueue activityQueue = new();

        public CalcPerson(CalcPersonDto calcPerson,
                           CalcLocation startingLocation,
                           BitArray isSick,
                           BitArray isOnVacation, CalcRepo calcRepo)
            : base(calcPerson.Name, calcPerson.Guid)
        {
            _calcPerson = calcPerson;
            _calcRepo = calcRepo;
            _normalDesires = new CalcPersonDesires(_calcRepo);
            PersonDesires = _normalDesires;
            SicknessDesires = new CalcPersonDesires(_calcRepo);
            IsSick = isSick;
            IsOnVacation = isOnVacation;
            _currentLocation = startingLocation;
            // remark: _currentSite cannot be initialized here, because startingLocation.Site is not assigned yet
            _vacationAffordanceGuid = System.Guid.NewGuid().ToStrGuid();
        }

        //guid for all vacations of this person
        private readonly StrGuid _vacationAffordanceGuid;
        // use one vacation location guid for all persons
        private static readonly StrGuid _vacationLocationGuid = System.Guid.NewGuid().ToStrGuid();

        public int DesireCount => PersonDesires.Desires.Count;

        public BitArray IsOnVacation { get; }

        private BitArray IsSick { get; }

        public CalcPersonDesires PersonDesires { get; private set; }

        public CalcPersonDesires SicknessDesires { get; }

        public int ID => _calcPerson.ID;

        public string PrettyName => _calcPerson.Name + "(" + _calcPerson.Age + "/" + _calcPerson.Gender + ")";

        public RemoteActivityInfo GetRemoteActivityInfo()
        {
            if (_currentSite is null)
                throw new LPGException("When transport is enabled, currentSite must never be null.");
            if (activityQueue.CurrentActivity is not DynamicActivity dynamicActivity)
                throw new LPGException("Tried to access remote affordance info although no remote affordance is active.");
            return new RemoteActivityInfo(new(Name, HouseholdKey), dynamicActivity, _currentSite.PointOfInterest);
        }

        public PersonInformation MakePersonInformation() => new(Name, Guid, _calcPerson.TraitTag);

        /// <summary>
        /// Determines if the person is busy with an affordance in the sepcified timestep.
        /// Will always return true while the person is carrying out a remote affordance with unspecified duration.
        /// </summary>
        /// <param name="timeStep">the internal timestep index to check</param>
        /// <returns>whether the person is busy in the timestep</returns>
        private bool IsBusy(int timeStep)
        {
            // TODO: is this method still needed?
            return !activityQueue.IsEmpty;
        }

        /// <summary>
        /// Determines if the person is busy with an affordance in the sepcified timestep.
        /// Will always return true while the person is carrying out a remote affordance with unspecified duration.
        /// </summary>
        /// <param name="timeStep">the timestep to check</param>
        /// <returns>whether the person is busy in the timestep</returns>
        private bool IsBusy(TimeStep timeStep)
        {
            return IsBusy(timeStep.InternalStep);
        }

        public bool NewIsBasicallyValidAffordance(ICalcAffordanceBase aff, bool sickness, bool logDetails)
        {
            // exclude affordances with the wrong age
            if (_calcPerson.Age > aff.MaximumAge || _calcPerson.Age < aff.MiniumAge)
            {
                if (logDetails)
                {
                    Logger.Debug("Sickness: " + sickness + " Age doesn't fit");
                }

                return false;
            }

            // exclude affordances with the wrong gender
            if (aff.PermittedGender != PermittedGender.All && aff.PermittedGender != _calcPerson.Gender)
            {
                if (logDetails)
                {
                    Logger.Debug("Sickness: " + sickness + " Gender doesn't fit");
                }

                return false;
            }

            // exclude affordances that don't satisfy at least one desire of the person
            var desires = sickness ? SicknessDesires : _normalDesires;
            var satisfactionCount = 0;
            foreach (var satisfactionvalue in aff.Satisfactionvalues)
            {
                if (desires.Desires.ContainsKey(satisfactionvalue.DesireID))
                {
                    satisfactionCount++;
                }
            }

            if (!aff.RequireAllAffordances && satisfactionCount > 0)
            {
                if (logDetails)
                {
                    Logger.Debug("Sickness: " + sickness + " At least one desire satisfied");
                }

                return true;
            }

            if (aff.RequireAllAffordances && aff.Satisfactionvalues.Count == satisfactionCount)
            {
                if (logDetails)
                {
                    Logger.Debug("Sickness: " + sickness + " All required desires satisfied");
                }

                return true;
            }

            if (logDetails)
            {
                Logger.Debug("Satisfaction count doesn't fit: Desires satisfied: " + satisfactionCount +
                             " Requires all: " + aff.RequireAllAffordances + " Number of desires on aff: " +
                             aff.Satisfactionvalues.Count);
            }

            return false;
        }

        /// <summary>
        /// Simulates one timestep for the person.
        /// </summary>
        /// <param name="time">the timestep to simulate</param>
        /// <param name="locs">list of available locations</param>
        /// <param name="isDaylight">daylight information object</param>
        /// <param name="householdKey">household key</param>
        /// <param name="persons">all persons of the household</param>
        /// <param name="remoteActivityResult">contains the results if a remote activity was just finished</param>
        /// <returns>whether a new remote activity was started</returns>
        public bool NextStep(TimeStep time, List<CalcLocation> locs, DayLightStatus isDaylight,
                              HouseholdKey householdKey,
                              List<CalcPerson> persons,
                             RemoteActivityFinished? remoteActivityResult = null)
        {
            // initialize affordance lists
            if (time.InternalStep == 0)
            {
                Init(locs);
                // select initial activities
                PlanAndStartNewActivity(time, isDaylight, persons);
            }
            Debug.Assert(!activityQueue.IsEmpty, "Activity queue was empty in the beginning of a step. This should never happen.");

            if (_previousAffordances.Count > _calcRepo.CalcParameters.AffordanceRepetitionCount)
            {
                _previousAffordances.RemoveAt(0);
            }

            // log critical desire threshold violations
            if (_calcRepo.CalcParameters.IsSet(CalcOption.CriticalViolations))
            {
                PersonDesires.CheckForCriticalThreshold(this, time, _calcRepo.FileFactoryAndTracker, householdKey);
            }

            if (IsOnVacation[time.InternalStep])
            {
                BeOnVacation(time);
                return false;
            }

            UpdateHealthState(time);

            PersonDesires.ApplyDecay(time);
            WriteDesiresToLogfileIfNeeded(time, householdKey);

            // check if the current activity is finished
            if (activityQueue.CurrentActivity.IsFinished(time, remoteActivityResult))
            {
                FinishActivity(time, activityQueue.CurrentActivity, remoteActivityResult);

                // start the next activity if there is one planned
                if (!activityQueue.IsEmpty)
                {
                    StartActivity(time, isDaylight, activityQueue.CurrentActivity);
                    _isCurrentlyPriorityAffordanceRunning = false;
                    return !activityQueue.CurrentActivity.IsDetermined;
                }
            }

            // TODO: fortfahren nach Interrupt implementieren
            //ReturnToPreviousActivityIfPreviouslyInterrupted(time);

            // if the person is already busy with an activity, only check for a possible interruption
            if (!activityQueue.IsEmpty)
            {
                return InterruptIfNeeded(time, isDaylight, false);
            }

            return PlanAndStartNewActivity(time, isDaylight, persons);
        }

        private bool PlanAndStartNewActivity(TimeStep time, DayLightStatus isDaylight, List<CalcPerson> persons)
        {
            // find a new affordance and plan its activation
            var bestaff = FindBestAffordance(time, persons);
            // get activity objects and enqueue them
            var activities = PlanAffordanceActivation(time, isDaylight, bestaff);
            activityQueue.AddActivities(activities);

            // start the first of the new activities
            StartActivity(time, isDaylight, activityQueue.CurrentActivity);
            _isCurrentlyPriorityAffordanceRunning = false;

            // return whether a new remote activity was started
            return !activityQueue.CurrentActivity.IsDetermined;
        }

        /// <summary>
        /// Changes this person to be healthy or sick if planned for this timestep.
        /// If so, switches to the correct set of desires to use.
        /// </summary>
        /// <param name="time">the current timestep</param>
        private void UpdateHealthState(TimeStep time)
        {
            _alreadyloggedvacation = false;
            if (!_isCurrentlySick && IsSick[time.InternalStep])
            {
                // person gets sick
                BecomeSick(time);
            }

            if (_isCurrentlySick && !IsSick[time.InternalStep])
            {
                // person becomes healthy
                BecomeHealthy(time);
            }
        }

        private void BecomeHealthy(TimeStep time)
        {
            PersonDesires = _normalDesires;
            PersonDesires.CopyOtherDesires(SicknessDesires);
            _isCurrentlySick = false;
            LogThought(time, "I've just become healthy.");
        }

        private void BecomeSick(TimeStep time)
        {
            PersonDesires = SicknessDesires;
            PersonDesires.CopyOtherDesires(_normalDesires);
            _isCurrentlySick = true;
            LogThought(time, "I've just become sick.");
        }

        private void BeOnVacation(TimeStep time)
        {
            LogThought(time, "I'm on vacation.");

            // only log vacation if not done already and if the current time step does not belong to the setup time frame
            if (!_alreadyloggedvacation && time.DisplayThisStep)
            {
                _calcRepo.OnlineLoggingData.AddActionEntry(time, _calcPerson.Guid, _calcPerson.Name,
                    _isCurrentlySick, "taking a vacation", _vacationAffordanceGuid, _calcPerson.HouseholdKey,
                    "Vacation", BodilyActivityLevel.Outside);
                _calcRepo.OnlineLoggingData.AddLocationEntry(new LocationEntry(_calcPerson.HouseholdKey,
                    _calcPerson.Name, _calcPerson.Guid, time, "Vacation", _vacationLocationGuid));
                _alreadyloggedvacation = true;
            }
        }

        /// <summary>
        /// Checks if the current activity can be interrupted. If there is a good alternative affordance, the current activity
        /// is interrupted, the alternative is activated, and the behavior for after finishing the alternative is determined.
        /// </summary>
        /// <param name="time">current timestep</param>
        /// <param name="isDaylight">daylight information object</param>
        /// <param name="ignorePreviousAffordances">whether the constraint not to activate one of the last few affordances can be ignored</param>
        /// <returns>whether a remote activity was started</returns>
        /// <exception cref="LPGException"></exception>
        private bool InterruptIfNeeded(TimeStep time, DayLightStatus isDaylight,
                                       bool ignorePreviousAffordances)
        {
            // track whether a remote activity was started
            bool newActivityStarted = false;

            // check if the affordance may be interrupted and did not already interrupt another affordance itself
            if (CurrentAffordance?.IsInterruptable == true && !_isCurrentlyPriorityAffordanceRunning)
            {
                if (activityQueue.CurrentActivity.IsTravel)
                    throw new LPGException($"Travel affordance {CurrentAffordance} is marked as interruptable, this is not allowed.");
                if (!activityQueue.CurrentActivity.IsDetermined)
                    throw new LPGException($"Dynamic affordance {CurrentAffordance} is marked as interruptable, this is not allowed.");

                // select correct set of affordances
                var aff = IsSick[time.InternalStep] ? _sicknessPotentialAffs : _normalPotentialAffs;

                // find all affordances that can interrupt the current affordance
                var availableInterruptingAffordances =
                    NewGetAllViableAffordancesAndSubs(time, null, true, aff, ignorePreviousAffordances);
                if (availableInterruptingAffordances.Count != 0)
                {
                    // the current affordance will now be interrupted; choose which affordance is started instead
                    newActivityStarted = true;
                    var bestAffordance = GetBestAffordanceFromList(time, availableInterruptingAffordances);

                    // get the activation object for the interruption
                    var interruptActivities = PlanAffordanceActivation(time, isDaylight, bestAffordance);

                    switch (bestAffordance.AfterInterruption)
                    {
                        case ActionAfterInterruption.LookForNew:
                            // finish the current activity
                            FinishActivity(time, activityQueue.CurrentActivity, null);
                            break;
                        case ActionAfterInterruption.GoBackToOld:
                            // TODO: do anything here?
                            break;
                    }

                    // add the activity to the beginning of the queue so it is immediately carried out
                    activityQueue.AddFirst(interruptActivities);
                    StartActivity(time, isDaylight, activityQueue.CurrentActivity);
                    _isCurrentlyPriorityAffordanceRunning = true;

                    // log the interruption
                    LogThought(time, "Interrupting the previous affordance for " + bestAffordance.Name);
                }
            }

            // log that the person is busy, including their current health state
            string healthState = _isCurrentlySick ? "sick" : "healthy";
            LogThought(time, "I'm busy and " + healthState);
            return newActivityStarted && !activityQueue.CurrentActivity.IsDetermined;
        }

        /// <summary>
        /// Check if the last affordance had interrupted another one, and if so whether the interrupted affordance
        /// should now be resumed. If so, creates a new action entry for the resumed affordance. 
        /// </summary>
        /// <param name="time">current timestep</param>
        /// <exception cref="LPGException"></exception>
        private void ReturnToPreviousActivityIfPreviouslyInterrupted(TimeStep time)
        {
            // TODO: move this logging of resuming an affordance after interrupt to somewhere else

            // check if an affordance was interrupted previously, and if the interrupted affordance shall now be resumed
            // log that the interrupted affordance is now continued
            var thought = "Back to " + _previousAffordancesWithEndTime[_previousAffordancesWithEndTime.Count - 2];
            LogThought(time, thought);

            // -2 to get the affordance before the interrupting one
            ICalcAffordanceBase prevAff = _previousAffordancesWithEndTime[_previousAffordancesWithEndTime.Count - 2].Item1;
            _calcRepo.OnlineLoggingData.AddActionEntry(time, Guid, Name, _isCurrentlySick, prevAff.Name, prevAff.Guid,
                _calcPerson.HouseholdKey, prevAff.AffCategory, prevAff.BodilyActivityLevel);
        }

        private void WriteDesiresToLogfileIfNeeded(TimeStep time, HouseholdKey householdKey)
        {
            if (_calcRepo.CalcParameters.IsSet(CalcOption.DesiresLogfile))
            {
                if (!_isCurrentlySick)
                {
                    _calcRepo.Logfile.DesiresLogfile.WriteEntry(
                        new DesireEntry(this, time, PersonDesires, _calcRepo.Logfile.DesiresLogfile, _calcRepo.CalcParameters), householdKey);
                }
                else
                {
                    _calcRepo.Logfile.DesiresLogfile.WriteEntry(
                        new DesireEntry(this, time, SicknessDesires, _calcRepo.Logfile.DesiresLogfile, _calcRepo.CalcParameters), householdKey);
                }
            }
        }

        /// <summary>
        /// Randomly select one affordance from the list, based on the affordance weights.
        /// </summary>
        /// <param name="bestaffordances">list of affordances to choose from</param>
        /// <param name="time">current timestep</param>
        /// <param name="person">the person that will activate the selected affordance</param>
        /// <param name="householdKey">the household key</param>
        /// <returns>the randomly selected affordance</returns>
        /// <exception cref="LPGException">if no affordance could be selected</exception>

        public ICalcAffordanceBase PickRandomAffordanceFromEquallyAttractiveOnes(
             List<ICalcAffordanceBase> bestaffordances,
             TimeStep time, CalcPerson person, HouseholdKey householdKey)
        {
            // randomly select one of the affordances, based on their weights
            var weightsum = bestaffordances.Sum(x => x.Weight);
            var pick = _calcRepo.Rnd.Next(weightsum);
            ICalcAffordanceBase? selectedAff = null;
            var idx = 0;
            var cumulativesum = 0;

            while (idx < bestaffordances.Count)
            {
                var start = cumulativesum;
                var currentAff = bestaffordances[idx];
                var end = cumulativesum + currentAff.Weight;
                if (pick >= start && pick <= end)
                {
                    selectedAff = currentAff;
                    break;
                }

                cumulativesum += currentAff.Weight;
                idx++;
            }

            // log all affordance names with their respective weight
            if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
            {
                var bestaffnames = string.Empty;
                foreach (var calcAffordance in bestaffordances)
                {
                    bestaffnames = bestaffnames + calcAffordance.Name + "(" + calcAffordance.Weight + "), ";
                }

                var thought = "Found " + bestaffordances.Count + " affordances with identical attractiveness:" + bestaffnames;
                LogThought(time, thought);
            }

            if (selectedAff == null)
            {
                throw new LPGException("Could not select an affordance. Please fix.");
            }

            return selectedAff;
        }

        public override string ToString() => "Person:" + Name;

        /// <summary>
        /// Activate the specified affordance for this person.
        /// </summary>
        /// <param name="timestep">timestep for activating the affordance</param>
        /// <param name="isDaylight">daylight information object</param>
        /// <param name="bestaff">the affordance to activate</param>
        /// <returns>whether the activated affordance is a remote activity</returns>
        /// <exception cref="LPGException"></exception>
        private IEnumerable<IActivity> PlanAffordanceActivation(TimeStep timestep, DayLightStatus isDaylight, ICalcAffordanceBase bestaff)
        {
            if (_calcRepo.CalcParameters.TransportationEnabled && bestaff is not AffordanceBaseTransportDecorator)
            {
                throw new LPGException(
                    "Trying to activate a non-transport affordance in a household that has transportation enabled. This is a bug and should never happen. The affordance was: " +
                    bestaff.Name + ". Affordance Type: " + bestaff.GetType().FullName);
            }

            // log which affordance was selected, if thoughts logs are enabled
            LogThought(timestep, "Affordance selected: " + bestaff);

            // activate the affordance and switch to its location
            var activations = bestaff.PlanActivation(timestep, _calcPerson, _currentSite);

            // add to list of previous affordances to avoid repetitions
            _previousAffordances.Add(bestaff);
            if (bestaff is CalcSubAffordance subaff)
            {
                _previousAffordances.Add(subaff.ParentAffordance);
            }

            return activations;
        }

        public void StartActivity(TimeStep timestep, DayLightStatus dayLightStatus, IActivity activity)
        {
            // double-check if the affordance can be activated now
            var affordance = activity.Affordance;
            // TODO: put the IsBusy call in activity.Activate() ?
            if (affordance.IsBusy(timestep, _currentSite, _calcPerson) != BusynessType.NotBusy)
            {
                // the affordance is not available, cancel it
                string thought = "Planned affordance " + affordance.Name + " is not available anymore.";
                LogThought(timestep, thought);

                // remove the activity from the queue
                activityQueue.RemoveCurrentActivity();

                if (!activityQueue.IsEmpty)
                {
                    // TODO: what to do in this case? clear the queue, or resume with the next planned activity earlier?
                    //       if the unavailable afffordance was a travel, the following activities cannot be activated
                    throw new NotImplementedException("A planned activity was not available anymore while other follow-up activities are still in the queue.");
                }

                // no more planned activities - determine new activities to carry out
                PlanAndStartNewActivity(timestep, dayLightStatus, []); // TODO: remove third parameter
                return;
            }

            // create an action entry for this activation
            _calcRepo.OnlineLoggingData.AddActionEntry(timestep, Guid,
                Name, _isCurrentlySick, affordance.Name,
                affordance.Guid, _calcPerson.HouseholdKey,
                affordance.AffCategory, affordance.BodilyActivityLevel);

            LogThought(timestep, activity.GetStartThought());

            // adapt the desire values of the person
            if (!activity.IsTravel)
            {
                PersonDesires.ApplyAffordanceEffect(affordance.Satisfactionvalues, affordance.RandomEffect, affordance.Name);
            }

            // activate the activity
            activity.Start(timestep, dayLightStatus, _currentSite);

            // log wether light was switched on
            string message = activity.LightingSwitchedOn ? "Turning on the light for " : "No light needed for ";
            LogThought(timestep, message + activity.Affordance.ParentLocation.Name);
        }

        public void FinishActivity(TimeStep timestep, IActivity activity, RemoteActivityFinished? remoteActivityResult)
        {
            UpdateLocation(timestep, activity, remoteActivityResult);

            int duration = activity.Finish(timestep, remoteActivityResult);

            // log information about the full activity, including travel and source affordance
            string thought = "Finished executing " + activityQueue.CurrentActivity.Name + ", duration " + duration;
            LogThought(timestep, thought);

            // remove the activity from the queue
            activityQueue.RemoveCurrentActivity();
        }

        private void UpdateLocation(TimeStep timestep, IActivity activity, RemoteActivityFinished? remoteActivityResult)
        {
            // TODO: does this method work for non-city and non-transport simulations?
            var affordance = activity.Affordance;

            // update the location fields
            _currentLocation = affordance.ParentLocation;
            _currentSite = affordance.Site;

            // log the location where the affordance is taking place
            _calcRepo.OnlineLoggingData.AddLocationEntry(
                new LocationEntry(_calcPerson.HouseholdKey,
                    _calcPerson.Name,
                    _calcPerson.Guid,
                    timestep,
                    affordance.ParentLocation.Name,
                    affordance.ParentLocation.Guid)); // TODO: add POI-ID to log?
        }

        /// <summary>
        /// Logs a thought of the person if thought logging is enabled.
        /// </summary>
        /// <param name="timestep">the current timestep</param>
        /// <param name="thought">the thought message</param>
        public void LogThought(TimeStep timestep, string thought)
        {
            if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
            {
                _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(new ThoughtEntry(this, timestep, thought), _calcPerson.HouseholdKey);
            }
        }

        public void LogPersonStatus(TimeStep timestep)
        {
            var ps = new PersonStatus(_calcPerson.HouseholdKey, _calcPerson.Name,
                _calcPerson.Guid, _currentLocation.Name, _currentLocation.Guid, _currentSite?.Name,
                _currentLocation.CalcSite?.Guid, CurrentAffordance?.Name, CurrentAffordance?.Guid, timestep);
            _calcRepo.OnlineLoggingData.AddPersonStatus(ps);
        }


        private ICalcAffordanceBase FindBestAffordance(TimeStep time, List<CalcPerson> persons)
        {
            // determine affordance list to use
            var allAffs = IsSick[time.InternalStep] ? _sicknessPotentialAffs : _normalPotentialAffs;

            // if affordance statuses should be logged, initialize the object for that
            AffordanceStatusClass? status = null;
            if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
            {
                status = new AffordanceStatusClass();
            }

            // collect available affordances
            var allAffordances = NewGetAllViableAffordancesAndSubs(time, status, false, allAffs, false);
            if (allAffordances.Count == 0 && (time.ExternalStep < 0 || _calcRepo.CalcParameters.IgnorePreviousActivitesWhenNeeded))
            {
                // try again, now ignoring previous activities
                allAffordances = NewGetAllViableAffordancesAndSubs(time, status, false, allAffs, true);
            }

            // no affordances, so search again to collect the respective reasons for an error message
            if (allAffordances.Count == 0)
            {
                if (_calcRepo.CalcParameters.EnableIdlemode)
                {
                    // idle mode is enabled - select idle affordance
                    var idleaff = _currentLocation.IdleAffs[this];
                    idleaff.IsBusy(time, _currentSite, _calcPerson);
                    return idleaff;
                }

                // create an error message containing reasons for affordances being unavailable
                var status_err = new AffordanceStatusClass();
                NewGetAllViableAffordancesAndSubs(time, status_err, false, allAffs, false);
                var s = MakeDetailledAffordanceStatusMessage(time, persons, status_err, 0);
                throw new DataIntegrityException(s);
            }

            if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
            {
                // log reasons for affordances being available or unavailable
                var thought = MakeDetailledAffordanceStatusMessage(time, persons, status!, allAffordances.Count);
                LogThought(time, thought);
            }

            // select the best affordance
            allAffordances.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
            return GetBestAffordanceFromList(time, allAffordances);
        }

        /// <summary>
        /// Creates a detailed message naming the reason why affordances were unavailable.
        /// This can be used to check why certain affordances were selected, or why no affordance
        /// was available at all.
        /// </summary>
        /// <param name="time">current TimeStep</param>
        /// <param name="persons">list of all persons</param>
        /// <param name="status">the status object storing reasons for unavailable affordances</param>
        /// <param name="availableAffordances">number of available affordances</param>
        /// <returns>the created affordance status message</returns>
        private string MakeDetailledAffordanceStatusMessage(TimeStep time, List<CalcPerson> persons, AffordanceStatusClass status, int availableAffordances)
        {
            var ts = new TimeSpan(0, 0, 0,
                (int)_calcRepo.CalcParameters.InternalStepsize.TotalSeconds * time.InternalStep);
            var dt = _calcRepo.CalcParameters.InternalStartTime.Add(ts);
            var s = new StringBuilder();
            s.Append($"At Timestep {time.ExternalStep} ({dt.ToLongDateString()} {dt.ToShortTimeString()}) {availableAffordances} affordances " +
                     $"were available for {Name} in the household {_calcPerson.HouseholdName}.{Environment.NewLine}");
            if (availableAffordances == 0)
            {
                s.Append("Since the people in this simulation can't do nothing, calculation can not continue." + Environment.NewLine);
            }
            s.Append(Name + " was ");
            if (IsSick[time.InternalStep])
            {
                s.Append(" sick at the time." + Environment.NewLine);
            }
            else
            {
                s.Append(" not sick at the time." + Environment.NewLine);
            }

            s.Append($"{_calcPerson.Name} was at {_currentLocation.Name}.{Environment.NewLine}");
            s.Append($"The setting for the number of required unique affordances in a row was set to {_calcRepo.CalcParameters.AffordanceRepetitionCount}.{Environment.NewLine}");
            if (status.Reasons.Count > 0)
            {
                s.Append(" The status of each affordance is as follows:" + Environment.NewLine);
                foreach (var reason in status.Reasons)
                {
                    s.Append(Environment.NewLine + reason.Affordance.Name + ":" + reason.Reason);
                }
            }
            else
            {
                s.Append(" Not a single viable affordance was found.");
            }

            s.Append(Environment.NewLine + Environment.NewLine + "The last activity of each Person was:");
            foreach (var calcPerson in persons)
            {
                var name = "(none)";
                if (!calcPerson.activityQueue.IsEmpty)
                {
                    name = calcPerson.CurrentAffordance.Name;
                }

                s.Append(Environment.NewLine + calcPerson.Name + ": " + name);
            }
            return s.ToString();
        }

        /// <summary>
        /// Out of the available affordances, find the one which will result in the lowest desire difference after activation.
        /// </summary>
        /// <param name="time">the current timestep</param>
        /// <param name="allAvailableAffordances">list of available affordances</param>
        /// <returns>the affordance resulting in the best desire values</returns>
        /// <exception cref="LPGException">if the ThoughtsLogFile would have been required but was null</exception>
        private ICalcAffordanceBase GetBestAffordanceFromList(TimeStep time, List<ICalcAffordanceBase> allAvailableAffordances)
        {
            var bestdiff = decimal.MaxValue;
            var bestaff = allAvailableAffordances[0];
            var bestaffordances = new List<ICalcAffordanceBase>();
            foreach (var affordance in allAvailableAffordances)
            {
                var desireDiff = PersonDesires.CalcEffect(affordance.Satisfactionvalues, out var thoughtstring, affordance.Name);

                // log the desire difference that will occur if this affordance is activated
                var thought = "Desirediff for " + affordance.Name + " is :" + desireDiff.ToString("#,##0.0", Config.CultureInfo) + " In detail: " + thoughtstring;
                LogThought(time, thought);

                if (desireDiff < bestdiff)
                {
                    bestdiff = desireDiff;
                    bestaff = affordance;
                    bestaffordances.Clear();
                }
                if (desireDiff == bestdiff)
                {
                    bestaffordances.Add(affordance);
                }
            }

            if (bestaffordances.Count > 1)
            {
                bestaff = PickRandomAffordanceFromEquallyAttractiveOnes(bestaffordances, time,
                    this, _calcPerson.HouseholdKey);
            }

            return bestaff;
        }

        /// <summary>
        /// If the affordance takes place at another site, replaces it with a new remote affordance.
        /// Otherwise just returns the affordance unchanged. In any case, the returned affordance can
        /// be added to this person's list of affordances.
        /// </summary>
        /// <param name="affordance">the affordance to check and possibly replace</param>
        /// <returns>the affordance to add to the person's list</returns>
        private ICalcAffordanceBase ReplaceWithRemoteAffordanceIfNecessary(ICalcAffordanceBase affordance)
        {
            // TODO: put this option somewhere more suitable
            if (!AffordanceBaseTransportDecorator.DynamicCitySimulation)
            {
                // not dynamic city simulation
                return affordance;
            }
            if (affordance is not AffordanceBaseTransportDecoratorDynamic transportAffordance)
            {
                // to replace an affordance with a remote affordance it needs a dynamic transport decorator
                throw new LPGException("Tried to replace an affordance without a dynamic transport decorator.");
            }

            var site = transportAffordance.Site;
            if (site?.Name == NameOfHomeCalcSite)
            {
                // affordance takes place at home - no remote affordance
                return affordance;
            }

            // TODO: get the POI preferences of this person
            var poi = new PointOfInterestId(0, 0);
            var citySite = new CitySite(poi, transportAffordance.Site.SiteCategory);

            // turn the affordance into a remote affordance for this person
            var remoteAff = CalcAffordanceRemote.CreateFromNormalAffordance(transportAffordance.SourceAffordance, citySite);
            // create a new transport decorator to avoid conflicts as persons can have different remote affordances
            return new AffordanceBaseTransportDecoratorDynamic(transportAffordance, remoteAff);
        }

        /// <summary>
        /// Initializes everything at the start of the first timestep.
        /// </summary>
        /// <param name="locs">the list of locations</param>
        private void Init(List<CalcLocation> locs)
        {
            // get the initial site from the location
            _currentSite = _currentLocation.CalcSite;
            if (AffordanceBaseTransportDecorator.DynamicCitySimulation && _currentSite?.Name != NameOfHomeCalcSite)
            {
                // TODO: correctly initialize currentPOI
                throw new NotImplementedException("Starting at a site other than 'Home' is not yet implemented for the city simulation.");
            }
            InitAffordanceLists(locs, _sicknessPotentialAffs, true);
            InitAffordanceLists(locs, _normalPotentialAffs, false);
        }

        /// <summary>
        /// Initializes the lists of available affordances. Is called in the beginning of the simulation, once for the states healthy and sick each.
        /// </summary>
        /// <param name="locs">available locations offering affordances</param>
        /// <param name="pa">affordance list to initialize</param>
        /// <param name="sickness">True if affordances shall be initialized for state sick</param>
        private void InitAffordanceLists(List<CalcLocation> locs, PotentialAffs pa, bool sickness)
        {
            pa.PotentialAffordances.Clear();
            pa.PotentialInterruptingAffordances.Clear();
            pa.PotentialAffordancesWithSubAffordances.Clear();
            pa.PotentialAffordancesWithInterruptingSubAffordances.Clear();
            // collect affordances from all locations
            foreach (var loc in locs)
            {
                foreach (var availableAffordance in loc.Affordances)
                {
                    var affordance = ReplaceWithRemoteAffordanceIfNecessary(availableAffordance);

                    if (NewIsBasicallyValidAffordance(affordance, sickness, false))
                    {
                        pa.PotentialAffordances.Add(affordance);
                        if (affordance.IsInterrupting)
                        {
                            pa.PotentialInterruptingAffordances.Add(affordance);
                        }
                    }

                    // also collect all suitable subaffordances
                    foreach (var subAffordance in affordance.SubAffordances)
                    {
                        if (NewIsBasicallyValidAffordance(subAffordance, sickness, false))
                        {
                            if (!pa.PotentialAffordancesWithSubAffordances.Contains(affordance))
                            {
                                pa.PotentialAffordancesWithSubAffordances.Add(affordance);
                            }

                            if (subAffordance.IsInterrupting)
                            {
                                if (!pa.PotentialAffordancesWithInterruptingSubAffordances.Contains(affordance))
                                {
                                    pa.PotentialAffordancesWithInterruptingSubAffordances.Add(affordance);
                                }
                            }
                        }
                    }
                }
            }

            pa.PotentialAffordances.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
            pa.PotentialInterruptingAffordances.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
            pa.PotentialAffordancesWithSubAffordances.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
            pa.PotentialAffordancesWithInterruptingSubAffordances.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
        }

        private List<ICalcAffordanceBase> NewGetAllViableAffordancesAndSubs(TimeStep timeStep,
            AffordanceStatusClass? errors, bool getOnlyInterrupting, PotentialAffs potentialAffs,
            bool ignorePreviousAffordances)
        {
            var getOnlyRelevantDesires = getOnlyInterrupting; // just for clarity
            // normal affs
            var resultingAff = new List<ICalcAffordanceBase>();
            List<ICalcAffordanceBase> srcList;
            if (getOnlyInterrupting)
            {
                srcList = potentialAffs.PotentialInterruptingAffordances;
            }
            else
            {
                srcList = potentialAffs.PotentialAffordances;
            }

            foreach (var calcAffordanceBase in srcList)
            {
                if (NewIsAvailableAffordance(timeStep, calcAffordanceBase, errors, getOnlyRelevantDesires,
                    ignorePreviousAffordances))
                {
                    resultingAff.Add(calcAffordanceBase);
                }
            }

            // subaffs
            List<ICalcAffordanceBase> subSrcList;
            if (getOnlyInterrupting)
            {
                subSrcList = potentialAffs.PotentialAffordancesWithInterruptingSubAffordances;
            }
            else
            {
                subSrcList = potentialAffs.PotentialAffordancesWithSubAffordances;
            }

            foreach (var affordance in subSrcList)
            {
                var spezsubaffs = affordance.CollectSubAffordances(timeStep, getOnlyInterrupting, _currentSite);
                foreach (var spezsubaff in spezsubaffs)
                {
                    if (NewIsAvailableAffordance(timeStep, spezsubaff, errors,
                        getOnlyRelevantDesires, ignorePreviousAffordances))
                    {
                        resultingAff.Add(spezsubaff);
                    }
                }
            }

            if (getOnlyInterrupting)
            {
                foreach (var affordance in resultingAff)
                {
                    if (!PersonDesires.HasAtLeastOneDesireBelowThreshold(affordance))
                    {
                        throw new LPGException("something went wrong while getting an interrupting affordance!");
                    }
                }
            }

            return resultingAff;
        }

        private bool NewIsAvailableAffordance(TimeStep timeStep, ICalcAffordanceBase aff, AffordanceStatusClass? errors,
            bool checkForRelevance, bool ignorePreviousAffordances)
        {
            Debug.Assert(_calcRepo.CalcParameters.TransportationEnabled == (aff is AffordanceBaseTransportDecorator), "Affordance does not match transport setting");

            if (!ignorePreviousAffordances && _previousAffordances.Contains(aff))
            {
                errors?.Reasons.Add(new AffordanceStatusTuple(aff, "Just did this."));
                return false;
            }

            var busynessResult = aff.IsBusy(timeStep, _currentSite, _calcPerson);
            if (busynessResult != BusynessType.NotBusy)
            {
                errors?.Reasons.Add(new AffordanceStatusTuple(aff, "Affordance is busy:" + busynessResult.ToString()));
                return false;
            }

            if (checkForRelevance && !PersonDesires.HasAtLeastOneDesireBelowThreshold(aff))
            {
                errors?.Reasons.Add(new AffordanceStatusTuple(aff,
                        "Person has no desires below the threshold for this affordance, so it is not relevant right now."));
                return false;
            }

            return true;
        }

        private class AffordanceStatusClass
        {
            public List<AffordanceStatusTuple> Reasons { get; } = [];
        }

        private class AffordanceStatusTuple(ICalcAffordanceBase affordance, string reason)
        {
            public ICalcAffordanceBase Affordance { get; } = affordance;
            public string Reason { get; } = reason;
        }

        private class PotentialAffs
        {
            public List<ICalcAffordanceBase> PotentialAffordances { get; } = [];

            public List<ICalcAffordanceBase> PotentialAffordancesWithInterruptingSubAffordances { get; } = [];

            public List<ICalcAffordanceBase> PotentialAffordancesWithSubAffordances { get; } = [];

            public List<ICalcAffordanceBase> PotentialInterruptingAffordances { get; } = [];
        }

    }

    //public class HumanHeatGainManager {
    //    private readonly HumanHeatGainSpecification _hhgs;
    //    private readonly Dictionary<string, CalcDevice> _devices = new Dictionary<string, CalcDevice>();

    //    public HumanHeatGainManager(CalcPerson person,  List<CalcLocation> allLocations,  CalcRepo calcRepo)
    //    {
    //        _hhgs = calcRepo.HumanHeatGainSpecification;
    //        var sampleLoc = allLocations[0];
    //        foreach (BodilyActivityLevel activityLevel in Enum.GetValues(typeof(BodilyActivityLevel)))
    //        {
    //            //power load type split by location and activity level
    //            //register all possible combinations for the power
    //            foreach (var location in allLocations) {
    //            //register all possible combinations for the power
    //                var devicename = "Inner Heat Gain - " + person.Name + " - " + _hhgs.PowerLoadtype.Name + " - " + location.Name;
    //                CalcDeviceLoad cdl = new CalcDeviceLoad(_hhgs.PowerLoadtype.Name, 1, _hhgs.PowerLoadtype, 0, 0);
    //                var cdlList = new List<CalcDeviceLoad>();
    //                cdlList.Add(cdl);
    //                CalcDeviceDto cdd = new CalcDeviceDto(devicename, Guid.NewGuid().ToStrGuid(), person.HouseholdKey,
    //                    OefcDeviceType.HumanInnerGains, "Human Inner Gains", "", Guid.NewGuid().ToStrGuid(),
    //                    location.Guid,
    //                    location.Name);
    //                CalcDevice cd = new CalcDevice(cdlList, location, cdd, calcRepo);
    //                var key = MakePowerKey(person,  location, activityLevel);
    //                _devices.Add(key,cd);
    //            }
    //            //powercounts: one activity level per person for a single location, count as 0/1
    //            //register all possible combinations for the power
    //            var countDevicename = "Inner Heat Gain - " + person.Name + " - " + _hhgs.CountLoadtype.Name;
    //            CalcDeviceLoad countCdl = new CalcDeviceLoad(_hhgs.CountLoadtype.Name, 1, _hhgs.CountLoadtype, 0, 0);
    //            var countCdlList = new List<CalcDeviceLoad>();
    //            countCdlList.Add(countCdl);
    //            CalcDeviceDto countCdd = new CalcDeviceDto(countDevicename, Guid.NewGuid().ToStrGuid(), person.HouseholdKey,
    //                OefcDeviceType.HumanInnerGains, "Human Inner Gains", "", Guid.NewGuid().ToStrGuid(),
    //                sampleLoc.Guid,sampleLoc.Name);
    //            CalcDevice countCd = new CalcDevice(countCdlList, sampleLoc, countCdd, calcRepo);
    //            var ckey = MakeCountKey(person, activityLevel);
    //            _devices.Add(ckey, countCd);
    //        }
    //    }

    //    public double GetPowerForActivityLevel(BodilyActivityLevel bal)
    //    {
    //        switch (bal) {
    //            case BodilyActivityLevel.Unknown:
    //                return 0;
    //            case BodilyActivityLevel.Outside:
    //                return 0;
    //            case BodilyActivityLevel.Low:
    //                return 100;
    //            case BodilyActivityLevel.High:
    //                return 150;
    //            default:
    //                throw new ArgumentOutOfRangeException(nameof(bal), bal, null);
    //        }
    //    }

    //    public void Activate( CalcPerson person, BodilyActivityLevel level,   CalcLocation loc,  TimeStep timeidx,  ICalcProfile personProfile,
    //                          string affordanceName)
    //    {
    //        List<double> powerProfile = new List<double>();
    //        List<double> countProfile = new List<double>();
    //        //rectangle power profile
    //        double power = GetPowerForActivityLevel(level);
    //        foreach (var value in personProfile.StepValues)
    //        {
    //            if (value > 0)
    //            {
    //                powerProfile.Add(power);
    //                countProfile.Add(1);
    //            }
    //            else
    //            {
    //                powerProfile.Add(0);
    //                countProfile.Add(0);
    //            }
    //        }

    //        {
    //            var key = MakePowerKey(person, loc, level);
    //            var dev = _devices[key];

    //            CalcProfile cp = new CalcProfile("PersonProfile", personProfile.Guid, powerProfile,
    //                ProfileType.Absolute, "Inner Gains");
    //            dev.SetTimeprofile(cp, timeidx, _hhgs.PowerLoadtype, "", affordanceName, 1, true);
    //        }
    //        var countKey = MakeCountKey(person,  level);
    //        var dev2 = _devices[countKey];
    //        CalcProfile countCp = new CalcProfile("PersonProfile", personProfile.Guid, countProfile,
    //            ProfileType.Absolute, "Inner Gains Count");
    //        dev2.SetTimeprofile(countCp, timeidx, _hhgs.CountLoadtype, "", affordanceName, 1, true);

    //    }

    //    
    //    public string MakePowerKey( CalcPerson person,   CalcLocation location, BodilyActivityLevel level)
    //    {
    //        return person.HouseholdKey.Key + "#" + _hhgs.PowerLoadtype.Name + "#" + person.Name + "#" + location.Name + "#" +
    //               level.ToString();
    //    }

    //    
    //    public string MakeCountKey( CalcPerson person, BodilyActivityLevel level)
    //    {
    //        return person.HouseholdKey.Key + "#" + person.Name + "#" + level.ToString();
    //    }
    //}
}

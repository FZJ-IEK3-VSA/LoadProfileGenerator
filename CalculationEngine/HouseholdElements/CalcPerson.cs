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

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private readonly BitArray _isBusy;
        private bool _isBusyForUnknownDuration;

        [JetBrains.Annotations.NotNull]
        private readonly PotentialAffs _normalPotentialAffs = new PotentialAffs();
        [JetBrains.Annotations.NotNull]
        private readonly CalcPersonDesires _normalDesires;

        /// <summary>
        /// Stores the last few activated affordances so repetitons can be avoided
        /// </summary>
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private readonly List<ICalcAffordanceBase> _previousAffordances;
        /// <summary>
        /// Stores the last few activated affordances including their respective end timesteps.
        /// This is needed to resume them in case they are interrupted.
        /// This list only stores affordances for which the duration is known in advance.
        /// </summary>
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private readonly List<Tuple<ICalcAffordanceBase, TimeStep>> _previousAffordancesWithEndTime = [];

        [JetBrains.Annotations.NotNull]
        private readonly PotentialAffs _sicknessPotentialAffs = new PotentialAffs();
        private bool _alreadyloggedvacation;

        /// <summary>
        /// The currently active affordance. This can be a transport decorator.
        /// </summary>
        private ICalcAffordanceBase? _currentAffordance;

        /// <summary>
        /// The ID of the point of interest where the person is at the moment, or null
        /// if the person is at home.
        /// </summary>
        private PointOfInterestId? currentPOI;

        /// <summary>
        /// Stores relevant information whenever this person is busy with a remote activity.
        /// This field also used to find the correct remote target for the activity.
        /// </summary>
        private RemoteAffordanceActivation? CurrentActivationInfo;

        /// <summary>
        /// Is true if an affordance that interrupted another is currently active.
        /// Prevents interrupting an already interrupting affordance.
        /// </summary>
        private bool _isCurrentlyPriorityAffordanceRunning;
        private bool _isCurrentlySick;
        [JetBrains.Annotations.NotNull]
        private readonly CalcPersonDto _calcPerson;

        [JetBrains.Annotations.NotNull]
        public HouseholdKey HouseholdKey => _calcPerson.HouseholdKey;

        private readonly CalcRepo _calcRepo;

        public CalcPerson([JetBrains.Annotations.NotNull] CalcPersonDto calcPerson,
                          [JetBrains.Annotations.NotNull] CalcLocation startingLocation,
                          [JetBrains.Annotations.NotNull][ItemNotNull] BitArray isSick,
                          [JetBrains.Annotations.NotNull][ItemNotNull] BitArray isOnVacation, CalcRepo calcRepo)
            : base(calcPerson.Name, calcPerson.Guid)
        {
            _calcPerson = calcPerson;
            _calcRepo = calcRepo;
            _isBusy = new BitArray(_calcRepo.CalcParameters.InternalTimesteps);
            _normalDesires = new CalcPersonDesires(_calcRepo);
            PersonDesires = _normalDesires;
            SicknessDesires = new CalcPersonDesires(_calcRepo);
            _previousAffordances = new List<ICalcAffordanceBase>();
            IsSick = isSick;
            IsOnVacation = isOnVacation;
            CurrentLocation = startingLocation;
            _vacationAffordanceGuid = System.Guid.NewGuid().ToStrGuid();
        }

        /// <summary>
        /// Indicates that a remote affordance or travel activity has just been finished.
        /// </summary>
        public RemoteActivityFinished? remoteActivityResult;

        //guid for all vacations of this person
        private readonly StrGuid _vacationAffordanceGuid;
        // use one vacation location guid for all persons
        private static readonly StrGuid _vacationLocationGuid = System.Guid.NewGuid().ToStrGuid();

        [JetBrains.Annotations.NotNull]
        private CalcLocation CurrentLocation { get; set; }

        public int DesireCount => PersonDesires.Desires.Count;

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public BitArray IsOnVacation { get; }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private BitArray IsSick { get; }

        [JetBrains.Annotations.NotNull]
        public CalcPersonDesires PersonDesires { get; private set; }

        [JetBrains.Annotations.NotNull]
        public CalcPersonDesires SicknessDesires { get; }

        private TimeStep? TimeToResetActionEntryAfterInterruption { get; set; }

        public int ID => _calcPerson.ID;

        [JetBrains.Annotations.NotNull]
        public string PrettyName => _calcPerson.Name + "(" + _calcPerson.Age + "/" + _calcPerson.Gender + ")";

        public RemoteActivityInfo GetRemoteActivityInfo()
        {
            if (CurrentActivationInfo is null)
                throw new LPGException("Tried to access remote affordance info although no remote affordance is active.");
            return new RemoteActivityInfo(new(Name, HouseholdKey), CurrentActivationInfo, currentPOI);
        }

        [JetBrains.Annotations.NotNull]
        public PersonInformation MakePersonInformation() => new(Name, Guid, _calcPerson.TraitTag);

        /// <summary>
        /// Determines if the person is busy with an affordance in the sepcified timestep.
        /// Will always return true while the person is carrying out a remote affordance with unspecified duration.
        /// </summary>
        /// <param name="timeStep">the internal timestep index to check</param>
        /// <returns>whether the person is busy in the timestep</returns>
        private bool IsBusy(int timeStep)
        {
            return _isBusyForUnknownDuration || _isBusy[timeStep];
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

        public bool NewIsBasicallyValidAffordance([JetBrains.Annotations.NotNull] ICalcAffordanceBase aff, bool sickness, bool logDetails)
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
            CalcPersonDesires desires;
            if (sickness)
            {
                desires = SicknessDesires;
            }
            else
            {
                desires = _normalDesires;
            }

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
        /// <param name="simulationSeed">the seed used in the current simulation</param>
        /// <returns>whether a new remote activity was started</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public bool NextStep([JetBrains.Annotations.NotNull] TimeStep time, [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcLocation> locs, [JetBrains.Annotations.NotNull] DayLightStatus isDaylight,
                             [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
                             [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcPerson> persons,
                             int simulationSeed)
        {
            // initialize affordance lists
            if (time.InternalStep == 0)
            {
                Init(locs);
            }

            if (_previousAffordances.Count > _calcRepo.CalcParameters.AffordanceRepetitionCount)
            {
                _previousAffordances.RemoveAt(0);
            }

            // log critical desire threshold violations
            if (_calcRepo.CalcParameters.IsSet(CalcOption.CriticalViolations))
            {
                PersonDesires.CheckForCriticalThreshold(this, time, _calcRepo.FileFactoryAndTracker, householdKey);
            }

            PersonDesires.ApplyDecay(time);
            WriteDesiresToLogfileIfNeeded(time, householdKey);

            // check if an ongoing remote affordance was finished
            if (remoteActivityResult is not null)
            {
                bool remoteActivityStarted = UpdateRemoteActivity(time, isDaylight);
                // check if a new affordance was started
                if (IsBusy(time))
                {
                    // return whether the new affordance is a remote activity or not
                    return remoteActivityStarted;
                }
            }

            ReturnToPreviousActivityIfPreviouslyInterrupted(time);

            // if the person is already busy with an activity, only check for a possible interruption
            if (IsBusy(time))
            {
                return InterruptIfNeeded(time, isDaylight, false);
            }

            if (IsOnVacation[time.InternalStep])
            {
                BeOnVacation(time);
                return false;
            }

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

            //activate new affordance
            var bestaff = FindBestAffordance(time, persons, simulationSeed);
            bool isAffordanceRemote = ActivateAffordance(time, isDaylight, bestaff);
            _isCurrentlyPriorityAffordanceRunning = false;
            return isAffordanceRemote;
        }

        /// <summary>
        /// Finishes the current remote activity. Logs information and resets respective flags.
        /// If the activity was a transport activity, also activates the corresponding source activity at the
        /// target location, no matter if that is a remote affordance or not.
        /// </summary>
        /// <param name="time">current timestep</param>
        /// <param name="isDaylight">daylight status objects</param>
        /// <returns>whether a new remote activity was started</returns>
        /// <exception cref="LPGException">if the remote activity was not correctly initialized</exception>
        private bool UpdateRemoteActivity(TimeStep time, DayLightStatus isDaylight)
        {
            if (CurrentActivationInfo == null)
                throw new LPGException("Activation info for remote affordance " + _currentAffordance?.Name + " is missing.");

            bool remoteActivityStarted = false;

            // update the location
            currentPOI = remoteActivityResult!.NewLocation;
            // reset the result object to correctly detect future remote updates
            remoteActivityResult = null;

            // calculate the duration of the remote activity
            int duration = time.InternalStep - CurrentActivationInfo.Start.InternalStep;

            // check if the remote activity was traveling, in which case the source affordance needs to be activated
            if (_currentAffordance is AffordanceBaseTransportDecorator transportAffordance)
            {
                // finished traveling - log the transportation event
                var sourceAffordanceDuration = -1; // dummy value - is currently not used in transportation logging
                transportAffordance.LogTransportationEvent(CurrentActivationInfo.TravelDeviceUseEvents, Name, CurrentActivationInfo.Start, CurrentActivationInfo.SourceSite,
                    CurrentActivationInfo.Route!, duration, sourceAffordanceDuration);

                // select the actual affordance and check if it is available
                _currentAffordance = transportAffordance._sourceAffordance;
                if (_currentAffordance.IsBusy(time, CurrentLocation, _calcPerson) != BusynessType.NotBusy)
                {
                    // the affordance is not available, abort it
                    if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
                    {
                        string thought = "Planned affordance " + _currentAffordance.Name + " is not available after dynamic traveling.";
                        _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(new ThoughtEntry(this, time, thought), _calcPerson.HouseholdKey);
                    }
                    return false;
                }

                // activate the actual affordance
                _currentAffordance.Activate(time, Name, CurrentLocation, out var sourceActivation);
                if (sourceActivation is CalcProfile personProfile)
                {
                    // the actual activity is not remote and has a predetermined duration
                    SetBusyAndActivateLighting(time, personProfile, _currentAffordance.ParentLocation, isDaylight, _currentAffordance.NeedsLight);
                }
                else
                {
                    // the source affordance is a remote activity as well
                    CurrentActivationInfo = (RemoteAffordanceActivation)sourceActivation;
                    remoteActivityStarted = true;
                }
            }
            else
            {
                // finished a remote activity
                var affordance = (CalcAffordanceRemote)_currentAffordance!;
                _isBusyForUnknownDuration = false;
                affordance.Finish(time, Name);

                // log information about the full activity, including travel and source affordance
                if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
                {
                    string thought = "Finished executing " + CurrentActivationInfo.Name + ", duration " + duration;
                    _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(new ThoughtEntry(this, time, thought), _calcPerson.HouseholdKey);
                }
            }
            return remoteActivityStarted;
        }

        private void BecomeHealthy([JetBrains.Annotations.NotNull] TimeStep time)
        {
            PersonDesires = _normalDesires;
            PersonDesires.CopyOtherDesires(SicknessDesires);
            _isCurrentlySick = false;
            if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
            {
                _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(new ThoughtEntry(this, time, "I've just become healthy."),
                    _calcPerson.HouseholdKey);
            }
        }

        private void BecomeSick([JetBrains.Annotations.NotNull] TimeStep time)
        {
            PersonDesires = SicknessDesires;
            PersonDesires.CopyOtherDesires(_normalDesires);
            _isCurrentlySick = true;
            if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
            {
                _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(new ThoughtEntry(this, time, "I've just become sick."),
                    _calcPerson.HouseholdKey);
            }
        }

        private void BeOnVacation([JetBrains.Annotations.NotNull] TimeStep time)
        {
            if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
            {
                _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(new ThoughtEntry(this, time, "I'm on vacation."), _calcPerson.HouseholdKey);
            }

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
        private bool InterruptIfNeeded([JetBrains.Annotations.NotNull] TimeStep time, [JetBrains.Annotations.NotNull] DayLightStatus isDaylight,
                                       bool ignorePreviousAffordances)
        {
            // track whether a remote activity was started
            bool remoteActivityStarted = false;

            // check if the affordance may be interrupted and did not already interrupt another affordance itself
            if (_currentAffordance?.IsInterruptable == true && !_isCurrentlyPriorityAffordanceRunning)
            {
                PotentialAffs aff;
                if (IsSick[time.InternalStep])
                {
                    aff = _sicknessPotentialAffs;
                }
                else
                {
                    aff = _normalPotentialAffs;
                }

                var availableInterruptingAffordances =
                    NewGetAllViableAffordancesAndSubs(time, null, true, aff, ignorePreviousAffordances);
                if (availableInterruptingAffordances.Count != 0)
                {
                    // the current affordance will now be interrupted; choose which affordance is started instead
                    var bestAffordance = GetBestAffordanceFromList(time, availableInterruptingAffordances);

                    if (bestAffordance.AfterInterruption == ActionAfterInterruption.LookForNew)
                    {
                        if (_isBusyForUnknownDuration)
                        {
                            throw new LPGException("Remote affordances cannot be interrupted.");
                        }
                        // reset the IsBusy array of the person in case the interrupted and stopped affordance would have lasted
                        // longer than the new, interrupting affordance
                        var t = time;
                        while (t.InternalStep < _calcRepo.CalcParameters.InternalTimesteps && IsBusy(t))
                        {
                            _isBusy[t.InternalStep] = false;
                            t = t.AddSteps(1);
                        }
                    }

                    remoteActivityStarted = ActivateAffordance(time, isDaylight, bestAffordance);

                    if (bestAffordance.AfterInterruption == ActionAfterInterruption.GoBackToOld)
                    {
                        if (_previousAffordancesWithEndTime.Count < 2)
                        {
                            throw new LPGException("The interrupted activity was not properly saved.");
                        }
                        // check if the interrupted affordance lasts beyond the end of the interrupting one
                        var endtime = _previousAffordancesWithEndTime[_previousAffordancesWithEndTime.Count - 1].Item2;
                        var endtimePrev = _previousAffordancesWithEndTime[_previousAffordancesWithEndTime.Count - 2].Item2;
                        if (endtimePrev > endtime)
                        {
                            // save the timestep when to resume the interrupted affordance
                            TimeToResetActionEntryAfterInterruption = endtime;
                        }
                    }

                    // log the interruption
                    if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
                    {
                        _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(
                            new ThoughtEntry(this, time,
                                "Interrupting the previous affordance for " + bestAffordance.Name),
                            _calcPerson.HouseholdKey);
                    }

                    _isCurrentlyPriorityAffordanceRunning = true;
                }
            }

            // log that the person is busy, including their current health state
            if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
            {
                string healthState = _isCurrentlySick ? "sick" : "healthy";
                _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(new ThoughtEntry(this, time, "I'm busy and " + healthState),
                        _calcPerson.HouseholdKey);
            }
            return remoteActivityStarted;
        }

        /// <summary>
        /// Check if the last affordance had interrupted another one, and if so whether the interrupted affordance
        /// should now be resumed. If so, creates a new action entry for the resumed affordance. 
        /// </summary>
        /// <param name="time">current timestep</param>
        /// <exception cref="LPGException"></exception>
        private void ReturnToPreviousActivityIfPreviouslyInterrupted([JetBrains.Annotations.NotNull] TimeStep time)
        {
            // check if an affordance was interrupted previously, and if the interrupted affordance shall now be resumed
            if (time == TimeToResetActionEntryAfterInterruption)
            {
                // log that the interrupted affordance is now continued
                if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
                {
                    var thought = "Back to " + _previousAffordancesWithEndTime[_previousAffordancesWithEndTime.Count - 2];
                    _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(new ThoughtEntry(this, time, thought), _calcPerson.HouseholdKey);
                }

                // -2 to get the affordance before the interrupting one
                ICalcAffordanceBase prevAff =
                    _previousAffordancesWithEndTime[_previousAffordancesWithEndTime.Count - 2].Item1;
                _calcRepo.OnlineLoggingData.AddActionEntry(time, Guid, Name, _isCurrentlySick, prevAff.Name, prevAff.Guid,
                    _calcPerson.HouseholdKey, prevAff.AffCategory, prevAff.BodilyActivityLevel);
                TimeToResetActionEntryAfterInterruption = null;
            }
        }

        private void WriteDesiresToLogfileIfNeeded([JetBrains.Annotations.NotNull] TimeStep time, [JetBrains.Annotations.NotNull] HouseholdKey householdKey)
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
        [JetBrains.Annotations.NotNull]
        public ICalcAffordanceBase PickRandomAffordanceFromEquallyAttractiveOnes(
            [JetBrains.Annotations.NotNull][ItemNotNull] List<ICalcAffordanceBase> bestaffordances,
            [JetBrains.Annotations.NotNull] TimeStep time, [JetBrains.Annotations.NotNull] CalcPerson person, [JetBrains.Annotations.NotNull] HouseholdKey householdKey)
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
                _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(new ThoughtEntry(person, time, thought), householdKey);
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
        /// <param name="currentTimeStep">timestep for activating the affordance</param>
        /// <param name="isDaylight">daylight information object</param>
        /// <param name="bestaff">the affordance to activate</param>
        /// <returns>whether the activated affordance is a remote activity</returns>
        /// <exception cref="LPGException"></exception>
        private bool ActivateAffordance([JetBrains.Annotations.NotNull] TimeStep currentTimeStep, [JetBrains.Annotations.NotNull] DayLightStatus isDaylight,
                                         [JetBrains.Annotations.NotNull] ICalcAffordanceBase bestaff)
        {
            if (_calcRepo.CalcParameters.TransportationEnabled && bestaff is not AffordanceBaseTransportDecorator)
            {
                throw new LPGException(
                    "Trying to activate a non-transport affordance in a household that has transportation enabled. This is a bug and should never happen. The affordance was: " +
                    bestaff.Name + ". Affordance Type: " + bestaff.GetType().FullName);
            }

            // log the location where the affordance is taking place
            _calcRepo.OnlineLoggingData.AddLocationEntry(
                new LocationEntry(_calcPerson.HouseholdKey,
                    _calcPerson.Name,
                    _calcPerson.Guid,
                     currentTimeStep,
                    bestaff.ParentLocation.Name,
                    bestaff.ParentLocation.Guid));

            // log which affordance was selected, if thoughts logs are enabled
            if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
            {
                _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(new ThoughtEntry(this, currentTimeStep, "Action selected:" + bestaff),
                    _calcPerson.HouseholdKey);
            }
            // create an action entry for this activation and adapt the desire values
            _calcRepo.OnlineLoggingData.AddActionEntry(currentTimeStep, Guid,
                Name, _isCurrentlySick, bestaff.Name,
                bestaff.Guid, _calcPerson.HouseholdKey,
                bestaff.AffCategory, bestaff.BodilyActivityLevel);
            PersonDesires.ApplyAffordanceEffect(bestaff.Satisfactionvalues, bestaff.RandomEffect, bestaff.Name);

            // activate the affordance and switch to its location
            bestaff.Activate(currentTimeStep, Name, CurrentLocation, out var activationInfo);
            CurrentLocation = bestaff.ParentLocation;

            // set this flag if the person is starting an affordance or travel with yet undetermined duration
            _isBusyForUnknownDuration = !activationInfo.IsDetermined;

            // mark the person as busy
            if (activationInfo.IsDetermined && activationInfo is ICalcProfile personProfile)
            {
                // the affordance is a normal activity with known duration
                SetBusyAndActivateLighting(currentTimeStep, personProfile, bestaff.ParentLocation, isDaylight, bestaff.NeedsLight);

                // save the end timestep of this affordance activation; this is needed if it gets interrupted and then resumed
                int duration = personProfile.StepValues.Count;
                _previousAffordancesWithEndTime.Add(new Tuple<ICalcAffordanceBase, TimeStep>(bestaff, currentTimeStep.AddSteps(duration)));
                while (_previousAffordancesWithEndTime.Count > 5)
                {
                    _previousAffordancesWithEndTime.RemoveAt(0);
                }

                // log affordance activation
                if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
                {
                    string thought = "Starting to execute " + personProfile.Name + ", basis duration " + personProfile.StepValues.Count + " time factor "
                        + personProfile.TimeFactor + ", total duration " + personProfile.StepValues.Count;
                    _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(new ThoughtEntry(this, currentTimeStep, thought), _calcPerson.HouseholdKey);
                }
            }
            else
            {
                // the affordance is a remote activity; store relevant information for logging it later
                CurrentActivationInfo = (RemoteAffordanceActivation)activationInfo;
            }
            // remark: no lighting simulation for remote affordances

            // add to list of previous affordances to avoid repetitions
            _previousAffordances.Add(bestaff);
            if (bestaff is CalcSubAffordance subaff)
            {
                _previousAffordances.Add(subaff.ParentAffordance);
            }

            _currentAffordance = bestaff;
            return !activationInfo.IsDetermined;
        }

        public void LogPersonStatus([JetBrains.Annotations.NotNull] TimeStep timestep)
        {
            var ps = new PersonStatus(_calcPerson.HouseholdKey, _calcPerson.Name,
                _calcPerson.Guid, CurrentLocation.Name, CurrentLocation.Guid, CurrentLocation.CalcSite?.Name,
                CurrentLocation.CalcSite?.Guid, _currentAffordance?.Name, _currentAffordance?.Guid, timestep);
            _calcRepo.OnlineLoggingData.AddPersonStatus(ps);
        }

        [JetBrains.Annotations.NotNull]
        private ICalcAffordanceBase FindBestAffordance([JetBrains.Annotations.NotNull] TimeStep time,
                                                       [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcPerson> persons, int simulationSeed)
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
                    var idleaff = CurrentLocation.IdleAffs[this];
                    idleaff.IsBusy(time, CurrentLocation, _calcPerson);
                    return idleaff;
                }

                // create an error message containing reasons for affordances being unavailable
                var status_err = new AffordanceStatusClass();
                NewGetAllViableAffordancesAndSubs(time, status_err, false, allAffs, false);
                var s = MakeDetailledAffordanceStatusMessage(time, persons, simulationSeed, status_err, 0);
                throw new DataIntegrityException(s);
            }

            if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
            {
                // log reasons for affordances being available or unavailable
                var thought = MakeDetailledAffordanceStatusMessage(time, persons, simulationSeed, status!, allAffordances.Count);
                var thoughtEntry = new ThoughtEntry(this, time, thought);
                _calcRepo.Logfile.ThoughtsLogFile1!.WriteEntry(thoughtEntry, _calcPerson.HouseholdKey);
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
        /// <param name="simulationSeed">the simulation seed</param>
        /// <param name="status">the status object storing reasons for unavailable affordances</param>
        /// <param name="availableAffordances">number of available affordances</param>
        /// <returns>the created affordance status message</returns>
        private string MakeDetailledAffordanceStatusMessage(TimeStep time, List<CalcPerson> persons, int simulationSeed, AffordanceStatusClass status, int availableAffordances)
        {
            var ts = new TimeSpan(0, 0, 0,
                (int)_calcRepo.CalcParameters.InternalStepsize.TotalSeconds * time.InternalStep);
            var dt = _calcRepo.CalcParameters.InternalStartTime.Add(ts);
            var s = new StringBuilder();
            s.Append("At Timestep " + time.ExternalStep + " (" + dt.ToLongDateString() + " " + dt.ToShortTimeString() + ")" +
                    availableAffordances + " affordances were available for " + Name +
                    " in the household " + _calcPerson.HouseholdName + "." + Environment.NewLine);
            if (availableAffordances == 0)
            {
                s.Append("Since the people in this simulation can't do nothing, calculation can not continue. ");
            }
            s.Append("The simulation seed was " + simulationSeed + ". " + Environment.NewLine + Name + " was ");
            if (IsSick[time.InternalStep])
            {
                s.Append(" sick at the time." + Environment.NewLine);
            }
            else
            {
                s.Append(" not sick at the time." + Environment.NewLine);
            }

            s.Append(_calcPerson.Name + " was at " + CurrentLocation.Name + "." + Environment.NewLine);
            s.Append("The setting for the number of required unique affordances in a row was set to " + _calcRepo.CalcParameters.AffordanceRepetitionCount + "." + Environment.NewLine);
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
                if (calcPerson._currentAffordance != null)
                {
                    name = calcPerson._currentAffordance.Name;
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
        [JetBrains.Annotations.NotNull]
        private ICalcAffordanceBase GetBestAffordanceFromList([JetBrains.Annotations.NotNull] TimeStep time,
                                                              [JetBrains.Annotations.NotNull][ItemNotNull] List<ICalcAffordanceBase> allAvailableAffordances)
        {
            var bestdiff = decimal.MaxValue;
            var bestaff = allAvailableAffordances[0];
            var bestaffordances = new List<ICalcAffordanceBase>();
            foreach (var affordance in allAvailableAffordances)
            {
                var desireDiff = PersonDesires.CalcEffect(affordance.Satisfactionvalues, out var thoughtstring, affordance.Name);

                // log the desire difference that will occur if this affordance is activated
                if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
                {
                    var thought = "Desirediff for " + affordance.Name + " is :" + desireDiff.ToString("#,##0.0", Config.CultureInfo) + " In detail: " + thoughtstring;
                    _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(new ThoughtEntry(this, time, thought), _calcPerson.HouseholdKey);
                }

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
            if (affordance is not AffordanceBaseTransportDecorator transportAffordance)
            {
                // without transport remote affordances are not possible
                return affordance;
            }

            var site = transportAffordance.ParentLocation.CalcSite;
            if (site?.Name == NameOfHomeCalcSite)
            {
                // affordance takes place at home - no remote affordance
                return affordance;
            }

            // TODO: get the POI preferences of this person
            var poi = new PointOfInterestId(0, 0);

            // turn the affordance into a remote affordance for this person
            var remoteAff = CalcAffordanceRemote.CreateFromNormalAffordance(transportAffordance._sourceAffordance, poi);
            // create a new transport decorator to avoid conflicts as persons can have different remote affordances
            return new AffordanceBaseTransportDecorator(transportAffordance, remoteAff);
        }

        /// <summary>
        /// Initializes everything at the start of the first timestep.
        /// </summary>
        /// <param name="locs">the list of locations</param>
        private void Init(List<CalcLocation> locs)
        {
            if (CurrentLocation.CalcSite?.Name != NameOfHomeCalcSite && AffordanceBaseTransportDecorator.DynamicCitySimulation)
            {
                // TODO: correctly initialize currentPOI
                throw new NotImplementedException("Starting at another site than 'Home' is not yet implemented.");
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
        private void InitAffordanceLists([JetBrains.Annotations.NotNull][ItemNotNull] List<CalcLocation> locs, [JetBrains.Annotations.NotNull] PotentialAffs pa, bool sickness)
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

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<ICalcAffordanceBase> NewGetAllViableAffordancesAndSubs([JetBrains.Annotations.NotNull] TimeStep timeStep,
                                                                            AffordanceStatusClass? errors,
                                                                            bool getOnlyInterrupting,
                                                                            [JetBrains.Annotations.NotNull] PotentialAffs potentialAffs,
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
                    CurrentLocation.CalcSite, ignorePreviousAffordances))
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
                var spezsubaffs = affordance.CollectSubAffordances(timeStep, getOnlyInterrupting, CurrentLocation);
                foreach (var spezsubaff in spezsubaffs)
                {
                    if (NewIsAvailableAffordance(timeStep, spezsubaff, errors,
                        getOnlyRelevantDesires, CurrentLocation.CalcSite, ignorePreviousAffordances))
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

        private bool NewIsAvailableAffordance([JetBrains.Annotations.NotNull] TimeStep timeStep,
                                              [JetBrains.Annotations.NotNull] ICalcAffordanceBase aff,
                                              AffordanceStatusClass? errors, bool checkForRelevance,
                                              CalcSite? srcSite, bool ignorePreviousAffordances)
        {
            Debug.Assert(_calcRepo.CalcParameters.TransportationEnabled == (aff is AffordanceBaseTransportDecorator), "Affordance does not match transport setting");

            if (!ignorePreviousAffordances && _previousAffordances.Contains(aff))
            {
                if (errors != null)
                {
                    errors.Reasons.Add(new AffordanceStatusTuple(aff, "Just did this."));
                }

                return false;
            }

            var busynessResult = aff.IsBusy(timeStep, CurrentLocation, _calcPerson);
            if (busynessResult != BusynessType.NotBusy)
            {
                if (errors != null)
                {
                    errors.Reasons.Add(new AffordanceStatusTuple(aff, "Affordance is busy:" + busynessResult.ToString()));
                }

                return false;
            }

            if (checkForRelevance && !PersonDesires.HasAtLeastOneDesireBelowThreshold(aff))
            {
                if (errors != null)
                {
                    errors.Reasons.Add(new AffordanceStatusTuple(aff,
                        "Person has no desires below the threshold for this affordance, so it is not relevant right now."));
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Mark this person as busy for an activity, according to the specified profile. Also activates ligthing
        /// devices, if necessary.
        /// </summary>
        /// <param name="time">current timestep</param>
        /// <param name="personCalcProfile">the profile specifying when the person should be marked as busy</param>
        /// <param name="loc">location of the activity</param>
        /// <param name="isDaylight">daylight info object</param>
        /// <param name="needsLight">whether the activity requires light</param>
        /// <exception cref="LPGException"></exception>
        private void SetBusyAndActivateLighting([JetBrains.Annotations.NotNull] TimeStep time, [JetBrains.Annotations.NotNull] ICalcProfile personCalcProfile,
            [JetBrains.Annotations.NotNull] CalcLocation loc, [JetBrains.Annotations.NotNull] DayLightStatus isDaylight, bool needsLight)
        {
            // when this method is called, any remote activity must be over already
            _isBusyForUnknownDuration = false;

            // initialize light profile
            var isLightActivationneeded = false;
            var lightprofile = new List<double>(personCalcProfile.StepValues.Count);
            for (var i = 0; i < personCalcProfile.StepValues.Count; i++)
            {
                lightprofile.Add(0);
            }
            // mark person as busy and determine lighting profile for the activity
            for (var idx = 0; idx < personCalcProfile.StepValues.Count && idx + time.InternalStep < _calcRepo.CalcParameters.InternalTimesteps; idx++)
            {
                if (personCalcProfile.StepValues[idx] > 0)
                {
                    _isBusy[time.InternalStep + idx] = true;
                    if (!isDaylight.Status[time.InternalStep + idx] && needsLight)
                    {
                        lightprofile[idx] = 1;
                        isLightActivationneeded = true;
                    }
                }
            }

            // activate all light devices at the location
            if (isLightActivationneeded)
            {
                var cp = new CalcProfile(loc.Name + " - light", System.Guid.NewGuid().ToStrGuid(), lightprofile, ProfileType.Relative,
                    "Synthetic for Light Device");

                // this function is for a light device so that the light is turned on, even if someone else was already in the room
                if (loc.LightDevices.Count > 0 && loc.LightDevices[0].LoadCount > 0 &&
                    !loc.LightDevices[0].IsBusyDuringTimespan(time, 1, 1, loc.LightDevices[0].Loads[0].LoadType))
                {
                    for (var i = 0; i < loc.LightDevices.Count; i++)
                    {
                        loc.LightDevices[i].SetAllLoadTypesToTimeprofile(cp, time, "Light", Name, 1);
                    }
                }
            }

            // log the light
            if (_calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile))
            {
                string message = isLightActivationneeded ? "Turning on the light for " : "No light needed for ";
                _calcRepo.Logfile.ThoughtsLogFile1.WriteEntry(new ThoughtEntry(this, time, message + loc.Name), _calcPerson.HouseholdKey);
            }
        }

        private class AffordanceStatusClass
        {
            public AffordanceStatusClass() => Reasons = new List<AffordanceStatusTuple>();

            [JetBrains.Annotations.NotNull]
            public List<AffordanceStatusTuple> Reasons { get; }
        }

        private class AffordanceStatusTuple
        {
            public AffordanceStatusTuple(ICalcAffordanceBase affordance, string reason)
            {
                Affordance = affordance;
                Reason = reason;
            }

            public ICalcAffordanceBase Affordance { get; }
            public string Reason { get; }
        }
        private class PotentialAffs
        {
            [JetBrains.Annotations.NotNull]
            [ItemNotNull]
            public List<ICalcAffordanceBase> PotentialAffordances { get; } = new List<ICalcAffordanceBase>();

            [JetBrains.Annotations.NotNull]
            [ItemNotNull]
            public List<ICalcAffordanceBase> PotentialAffordancesWithInterruptingSubAffordances { get; } =
                new List<ICalcAffordanceBase>();

            [JetBrains.Annotations.NotNull]
            [ItemNotNull]
            public List<ICalcAffordanceBase> PotentialAffordancesWithSubAffordances { get; } =
                new List<ICalcAffordanceBase>();

            [JetBrains.Annotations.NotNull]
            [ItemNotNull]
            public List<ICalcAffordanceBase> PotentialInterruptingAffordances { get; } =
                new List<ICalcAffordanceBase>();
        }

    }

    //public class HumanHeatGainManager {
    //    private readonly HumanHeatGainSpecification _hhgs;
    //    private readonly Dictionary<string, CalcDevice> _devices = new Dictionary<string, CalcDevice>();

    //    public HumanHeatGainManager(CalcPerson person, [JetBrains.Annotations.NotNull] List<CalcLocation> allLocations, [JetBrains.Annotations.NotNull] CalcRepo calcRepo)
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

    //    public void Activate([JetBrains.Annotations.NotNull] CalcPerson person, BodilyActivityLevel level,  [JetBrains.Annotations.NotNull] CalcLocation loc, [JetBrains.Annotations.NotNull] TimeStep timeidx, [JetBrains.Annotations.NotNull] ICalcProfile personProfile,
    //                         [JetBrains.Annotations.NotNull] string affordanceName)
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

    //    [JetBrains.Annotations.NotNull]
    //    public string MakePowerKey([JetBrains.Annotations.NotNull] CalcPerson person,  [JetBrains.Annotations.NotNull] CalcLocation location, BodilyActivityLevel level)
    //    {
    //        return person.HouseholdKey.Key + "#" + _hhgs.PowerLoadtype.Name + "#" + person.Name + "#" + location.Name + "#" +
    //               level.ToString();
    //    }

    //    [JetBrains.Annotations.NotNull]
    //    public string MakeCountKey([JetBrains.Annotations.NotNull] CalcPerson person, BodilyActivityLevel level)
    //    {
    //        return person.HouseholdKey.Key + "#" + person.Name + "#" + level.ToString();
    //    }
    //}
}

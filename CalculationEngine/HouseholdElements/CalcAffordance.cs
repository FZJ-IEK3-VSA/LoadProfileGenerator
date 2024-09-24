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
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

#endregion

namespace CalculationEngine.HouseholdElements
{
    /// <summary>
    /// Default implementation of an affordance. Device activations and durations are determined randomly and stored
    /// per timestep for consistent results.
    /// </summary>
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public class CalcAffordance : CalcKnownDurationAffordance
    {
        [JetBrains.Annotations.NotNull] private readonly CalcProfile _personProfile;

        [JetBrains.Annotations.NotNull] private readonly Dictionary<int, double> _probabilitiesForTimes = [];

        [JetBrains.Annotations.NotNull] private readonly Dictionary<int, double> _timeFactorsForTimes = [];

        private readonly double _timeStandardDeviation;

        /// <summary>
        /// Stores all current activations of this affordance. Maps name of the activating person
        /// to start time and end time of the person time (the time the person is busy with the affordance).
        /// </summary>
        private Dictionary<string, Tuple<TimeStep, TimeStep>> _currentActivations = [];

        public CalcAffordance([JetBrains.Annotations.NotNull] string pName, [JetBrains.Annotations.NotNull] CalcProfile personProfile, [JetBrains.Annotations.NotNull] CalcLocation loc, bool randomEffect,
            [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcDesire> satisfactionvalues, int miniumAge, int maximumAge, PermittedGender permittedGender, bool needsLight, double timeStandardDeviation,
            ColorRGB affordanceColor, [JetBrains.Annotations.NotNull] string pAffCategory, bool isInterruptable, bool isInterrupting, [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcAffordanceVariableOp> variableOps,
            [JetBrains.Annotations.NotNull][ItemNotNull] List<VariableRequirement> variableRequirements, ActionAfterInterruption actionAfterInterruption, [JetBrains.Annotations.NotNull] string timeLimitName, int weight,
            bool requireAllDesires, [JetBrains.Annotations.NotNull] string srcTrait, StrGuid guid, [JetBrains.Annotations.NotNull] CalcVariableRepository variableRepository,
            [JetBrains.Annotations.NotNull][ItemNotNull] List<DeviceEnergyProfileTuple> energyprofiles, [ItemNotNull][JetBrains.Annotations.NotNull] BitArray isBusy, BodilyActivityLevel bodilyActivityLevel,
            [JetBrains.Annotations.NotNull] CalcRepo calcRepo, HouseholdKey householdKey)
            : base(pName, loc, satisfactionvalues, miniumAge, maximumAge, permittedGender, needsLight, randomEffect, pAffCategory, isInterruptable, isInterrupting, actionAfterInterruption, weight, requireAllDesires,
                  CalcAffordanceType.Affordance, guid, isBusy, bodilyActivityLevel, calcRepo, householdKey, energyprofiles, affordanceColor, srcTrait, timeLimitName, variableRepository, variableOps, variableRequirements)
        {
            if (personProfile == null)
            {
#pragma warning disable IDE0016 // Use 'throw' expression
                throw new DataIntegrityException("The affordance " + Name + " has no person profile!");
#pragma warning restore IDE0016 // Use 'throw' expression
            }
            _timeStandardDeviation = timeStandardDeviation;
            _personProfile = personProfile;
        }

        /// <summary>
        /// Creates all device profiles for one activation of the affordance
        /// </summary>
        /// <param name="startTime">start time step for the affordance activation</param>
        /// <param name="activatorName">person who activates the affordance</param>
        /// <returns>time step in which the last device profile ends</returns>
        private TimeStep CreateDeviceProfilesForActivation(TimeStep startTime, string activatorName)
        {
            TimeStep timeLastDeviceEnds = startTime.GetAbsoluteStep(0);
            //flexibility
            var allDevices = Energyprofiles.Select(x => x.CalcDevice).Distinct().ToList();
            foreach (var device in allDevices)
            {
                device.ActivationCount++;
            }

            // create time profiles and shiftable activations for each device
            foreach (var device in allDevices)
            {
                bool flexibleDevice = CalcRepo.CalcParameters.FlexibilityEnabled && device.FlexibilityMode == FlexibilityType.ProfileShiftable;
                TimeShiftableDeviceActivation tsactivation = new(device.DeviceDto, startTime, HouseholdKey);
                // get all energy profiles that belong to this device
                var profs = Energyprofiles.Where(x => x.CalcDevice == device).ToList();
                foreach (var dpt in profs)
                {
                    if (dpt.Probability > _probabilitiesForTimes[startTime.InternalStep])
                    {
                        CalcProfile adjustedProfile = dpt.TimeProfile.CompressExpandDoubleArray(_timeFactorsForTimes[startTime.InternalStep]);
                        var endtime = dpt.CalcDevice.SetTimeprofile(adjustedProfile, startTime.AddSteps(dpt.TimeOffsetInSteps), dpt.LoadType, Name,
                            activatorName, dpt.Multiplier, false, out var finalValues);
                        if (endtime > timeLastDeviceEnds)
                        {
                            timeLastDeviceEnds = endtime;
                        }

                        if (flexibleDevice)
                        {
                            var tsdp = new TimeShiftableDeviceProfile(
                                dpt.LoadType.ConvertToDto(), dpt.TimeOffsetInSteps, finalValues.Values);
                            tsactivation.Profiles.Add(tsdp);
                        }
                    }
                }

                if (flexibleDevice)
                {
                    tsactivation.TotalDuration = (timeLastDeviceEnds.InternalStep - startTime.InternalStep);
                    CalcRepo.OnlineLoggingData.AddTimeShiftableEntry(tsactivation);
                }
            }
            return timeLastDeviceEnds;
        }

        public override void Activate(TimeStep startTime, string activatorName, CalcLocation personSourceLocation, out ICalcProfile personTimeProfile)
        {
            TimeStep timeLastDeviceEnds = CreateDeviceProfilesForActivation(startTime, activatorName);

            // determine the time the activating person is busy with the affordance
            int personsteps = CalcProfile.GetNewLengthAfterCompressExpand(_personProfile.StepValues.Count, _timeFactorsForTimes[startTime.InternalStep]);
            TimeStep personEndTime = startTime.AddSteps(personsteps);

            // save start and end time of the person's activity
            _currentActivations[activatorName] = new(startTime, personEndTime);

            ExecuteVariableOperations(startTime, timeLastDeviceEnds, personEndTime);

            // adapt the default person profile according to the time factor for this time step
            var tf = _timeFactorsForTimes[startTime.InternalStep];
            _probabilitiesForTimes.Clear();
            _timeFactorsForTimes.Clear();
            personTimeProfile = _personProfile.CompressExpandDoubleArray(tf);
        }

        /// <summary>
        /// Collect all subaffordances of this affordance that are currently available. This depends
        /// on whether this affordance is currently active, whether the activation is far enough behind 
        /// </summary>
        /// <param name="time">current timestep</param>
        /// <param name="onlyInterrupting">whether only interrupting subaffordances should be collected</param>
        /// <param name="srcLocation">the current location of the person</param>
        /// <returns>a list of available subaffordances</returns>
        public override List<CalcSubAffordance> CollectSubAffordances(TimeStep time, bool onlyInterrupting, CalcLocation srcLocation)
        {
            if (SubAffordances.Count == 0)
            {
                return [];
            }

            // remove activations that are already over
            var outDatedActivations = _currentActivations.Where(kvp => kvp.Value.Item2 < time);
            foreach (var kvPair in outDatedActivations)
            {
                _currentActivations.Remove(kvPair.Key);
            }
            if (_currentActivations.Count == 0)
            {
                // the affordance is currently not active
                return [];
            }

            // collect all available subaffordances
            var availableSubAffs = new List<CalcSubAffordance>();
            foreach (var subAffordance in SubAffordances)
            {
                if (!onlyInterrupting || subAffordance.IsInterrupting)
                {
                    if (IsSubaffordanceAvailable(time, srcLocation, subAffordance))
                    {
                        availableSubAffs.Add(subAffordance);
                    }
                }
            }
            return availableSubAffs;
        }

        /// <summary>
        /// Checks if there is a current activation of the affordance that offers the specified subaffordance.
        /// </summary>
        /// <param name="time">the timestep for which to check if the subaffordance is available</param>
        /// <param name="srcLocation">the location of the affordance</param>
        /// <param name="subAffordance">the affordance to check</param>
        /// <returns>whether the subaffordance is currently available</returns>
        private bool IsSubaffordanceAvailable(TimeStep time, CalcLocation srcLocation, CalcSubAffordance subAffordance)
        {
            // check all current activations if one of them offers the subaffordance now
            foreach (var kvPair in _currentActivations)
            {
                // start and end time for this activation
                int personStartTime = kvPair.Value.Item1.InternalStep;
                int personEndTime = kvPair.Value.Item2.InternalStep;

                // the subaffordance can only be activated after the delay time, but before the buffer time is over
                var isDelayTimePassed = personStartTime + subAffordance.Delaytimesteps < time.InternalStep;
                var isBufferTimePassed = personStartTime + subAffordance.Delaytimesteps + SubAffordanceStartFrame <= time.InternalStep;
                // check if the subaffordance could be activated right now
                var person = new CalcPersonDto("name", null, -1, PermittedGender.All, null, null, null, -1, null, null);
                var isSubAffordanceBusy = subAffordance.IsBusy(time, srcLocation, person);
                if (isDelayTimePassed && !isBufferTimePassed && isSubAffordanceBusy == BusynessType.NotBusy)
                {
                    var remainingActiveTime = personEndTime - time.InternalStep;
                    subAffordance.SetDurations(remainingActiveTime);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if time factors have already been set for this time step, and if not sets them.
        /// </summary>
        /// <param name="time">the time step to check</param>
        private void DetermineTimeFactors(TimeStep time)
        {
            if (!_timeFactorsForTimes.ContainsKey(time.InternalStep))
            {
                _timeFactorsForTimes[time.InternalStep] = CalcRepo.NormalRandom.NextDouble(1, _timeStandardDeviation);
                if (_timeFactorsForTimes[time.InternalStep] < 0)
                {
                    throw new DataIntegrityException("The duration standard deviation on " + Name + " is too large: a negative value of " +
                                                     _timeFactorsForTimes[time.InternalStep] + " came up. The standard deviation is " +
                                                     _timeStandardDeviation);
                }
            }
        }

        /// <summary>
        /// Checks if probabilities have already been set for this time step, and if not sets them.
        /// </summary>
        /// <param name="time">the time step to check</param>
        private void DetermineProbabilities(TimeStep time)
        {
            if (!_probabilitiesForTimes.ContainsKey(time.InternalStep))
            {
                _probabilitiesForTimes[time.InternalStep] = CalcRepo.Rnd.NextDouble();
            }
        }

        /// <summary>
        /// Checks if all devices are free for activating the affordance.
        /// </summary>
        /// <param name="time">the time step to check for activation</param>
        /// <returns>true if all devices are free, else false</returns>
        private bool AreDevicesOccupied(TimeStep time)
        {
            foreach (var dpt in Energyprofiles)
            {
                if (dpt.Probability > _probabilitiesForTimes[time.InternalStep])
                {
                    if (dpt.CalcDevice.IsBusyDuringTimespan(time.AddSteps(dpt.TimeOffsetInSteps), dpt.TimeProfile.StepValues.Count,
                        _timeFactorsForTimes[time.InternalStep], dpt.LoadType))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override BusynessType IsBusy(TimeStep time, CalcLocation srcLocation, CalcPersonDto calcPerson, bool clearDictionaries = true)
        {
            DetermineTimeFactors(time);
            DetermineProbabilities(time);

            if (AreDevicesOccupied(time))
            {
                return BusynessType.Occupied;
            }

            return base.IsBusy(time, srcLocation, calcPerson, clearDictionaries);
        }

        public void AddDeviceTuple([JetBrains.Annotations.NotNull] CalcDevice dev, [JetBrains.Annotations.NotNull] CalcProfile newprof,
                                   [JetBrains.Annotations.NotNull] CalcLoadType lt, decimal timeoffset, TimeSpan internalstepsize, double multiplier,
                                   double probability)
        {
            //TODO: remove this, it is only used in unit testing
            var calctup = new DeviceEnergyProfileTuple(dev, newprof, lt, timeoffset, internalstepsize, multiplier, probability);
            Energyprofiles.Add(calctup);
        }
    }
}

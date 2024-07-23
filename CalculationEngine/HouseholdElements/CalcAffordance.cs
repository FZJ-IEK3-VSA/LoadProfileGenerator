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
    public partial class CalcAffordance : CalcKnownDurationAffordance
    {
        [JetBrains.Annotations.NotNull] private readonly CalcProfile _personProfile;

        [JetBrains.Annotations.NotNull] private readonly Dictionary<int, double> _probabilitiesForTimes = new();

        [JetBrains.Annotations.NotNull] private readonly Dictionary<int, double> _timeFactorsForTimes = new();

        private readonly double _timeStandardDeviation;

        // store start and end time step of the person time of the latest activation
        private TimeStep? _personStartTimeStep;
        private TimeStep? _personEndTimeStep;

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

        public static bool DoubleCheckBusyArray { get; set; }

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

        private void MarkAffordanceAsBusy(TimeStep startTime)
        {
            // determine the time the activating person is busy with the affordance
            var personsteps = CalcProfile.GetNewLengthAfterCompressExpand(_personProfile.StepValues.Count,
                _timeFactorsForTimes[startTime.InternalStep]);
            _personStartTimeStep = startTime;
            _personEndTimeStep = startTime.AddSteps(personsteps);

            if (DoubleCheckBusyArray)
            {
                for (var i = 0; i < personsteps && i + startTime.InternalStep < CalcRepo.CalcParameters.InternalTimesteps; i++)
                {
                    if (IsBusyArray[i + startTime.InternalStep])
                    {
                        throw new LPGException("Affordance was already busy");
                    }
                }
            }

            MarkAffordanceAsBusy(startTime, personsteps);
        }

        public override void Activate(TimeStep startTime, string activatorName, CalcLocation personSourceLocation, out ICalcProfile personTimeProfile)
        {
            TimeStep timeLastDeviceEnds = CreateDeviceProfilesForActivation(startTime, activatorName);

            MarkAffordanceAsBusy(startTime);

            ExecuteVariableOperations(startTime, timeLastDeviceEnds, _personEndTimeStep!);

            // adapt the default person profile according to the time factor for this time step
            var tf = _timeFactorsForTimes[startTime.InternalStep];
            _probabilitiesForTimes.Clear();
            _timeFactorsForTimes.Clear();
            personTimeProfile = _personProfile.CompressExpandDoubleArray(tf);
        }

        public override List<CalcSubAffordance> CollectSubAffordances(TimeStep time, bool onlyInterrupting, CalcLocation srcLocation)
        {
            if (SubAffordances.Count == 0)
            {
                return new List<CalcSubAffordance>();
            }

            if (RemainingActiveTime(time) < 1)
            {
                return new List<CalcSubAffordance>();
            }

            // es gibt subaffs und diese aff ist aktiv
            var result = new List<CalcSubAffordance>();
            foreach (var calcSubAffordance in SubAffordances)
            {
                if (onlyInterrupting && calcSubAffordance.IsInterrupting || !onlyInterrupting)
                {
                    var delaytimesteps = calcSubAffordance.Delaytimesteps;
                    var hasbeenactivefor = HasBeenActiveFor(time);
                    var person = new CalcPersonDto("name", null, -1, PermittedGender.All, null, null, null, -1, null, null);
                    var issubaffbusy = calcSubAffordance.IsBusy(time, srcLocation, person);
                    if (delaytimesteps < hasbeenactivefor && issubaffbusy == BusynessType.NotBusy)
                    {
                        calcSubAffordance.SetDurations(RemainingActiveTime(time));
                        result.Add(calcSubAffordance);
                    }
                }
            }

            return result;
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

        public override string ToString() => "Affordance:" + Name;

        private int HasBeenActiveFor([JetBrains.Annotations.NotNull] TimeStep currentTime)
        {
            if (currentTime < _personStartTimeStep)
            {
                return -1;
            }

            if (currentTime > _personEndTimeStep)
            {
                return -1;
            }

            if (_personStartTimeStep == null)
            {
                throw new LPGException("Start time step was null");
            }

            var hasbeenactive = currentTime.InternalStep - _personStartTimeStep.InternalStep;
            return hasbeenactive;
        }

        private int RemainingActiveTime([JetBrains.Annotations.NotNull] TimeStep currentTime)
        {
            if (currentTime == null)
            {
                throw new ArgumentNullException(nameof(currentTime));
            }

            if (currentTime < _personStartTimeStep)
            {
                return -1;
            }

            if (currentTime > _personEndTimeStep)
            {
                return -1;
            }

            if (_personEndTimeStep == null)
            {
                return -1;
            }

            var remainingTime = _personEndTimeStep.InternalStep - currentTime.InternalStep;
            return remainingTime;
        }
    }
}

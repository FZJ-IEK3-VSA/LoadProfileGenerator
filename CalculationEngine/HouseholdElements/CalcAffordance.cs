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
using CalculationEngine.Helper;
using Common;
using Common.Enums;
using Common.JSON;
using JetBrains.Annotations;

#endregion

namespace CalculationEngine.HouseholdElements {
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public class CalcAffordance : CalcAffordanceBase {
        [NotNull] private readonly CalcParameters _calcParameters;

        [NotNull] private readonly CalcProfile _personProfile;

        [NotNull] private readonly Dictionary<int, double> _probabilitiesForTimes = new Dictionary<int, double>();

        [NotNull] private readonly Dictionary<int, double> _timeFactorsForTimes = new Dictionary<int, double>();

        private readonly double _timeStandardDeviation;

        [ItemNotNull] [NotNull] private readonly List<CalcAffordanceVariableOp> _variableOps;

        [NotNull] private readonly CalcVariableRepository _variableRepository;

        [ItemNotNull] [NotNull] private readonly List<VariableRequirement> _variableRequirements;

        [CanBeNull] private TimeStep _endTimeStep;
        [CanBeNull] private TimeStep _startTimeStep;

        public CalcAffordance([NotNull] string pName,
                              [NotNull] CalcProfile personProfile,
                              [NotNull] CalcLocation loc,
                              bool randomEffect,
                              [NotNull] [ItemNotNull] List<CalcDesire> satisfactionvalues,
                              int miniumAge,
                              int maximumAge,
                              PermittedGender permittedGender,
                              bool needsLight,
                              double timeStandardDeviation,
                              ColorRGB affordanceColor,
                              [NotNull] string pAffCategory,
                              bool isInterruptable,
                              bool isInterrupting,
                              [NotNull] [ItemNotNull] List<CalcAffordanceVariableOp> variableOps,
                              [NotNull] [ItemNotNull] List<VariableRequirement> variableRequirements,
                              ActionAfterInterruption actionAfterInterruption,
                              [NotNull] string timeLimitName,
                              int weight,
                              bool requireAllDesires,
                              [NotNull] string srcTrait,
                              [NotNull] CalcParameters calcParameters,
                              [NotNull] string guid,
                              [NotNull] CalcVariableRepository variableRepository,
                              [NotNull] [ItemNotNull] List<DeviceEnergyProfileTuple> energyprofiles,
                              [ItemNotNull] [NotNull] BitArray isBusy) : base(pName,
            loc,
            satisfactionvalues,
            miniumAge,
            maximumAge,
            permittedGender,
            needsLight,
            randomEffect,
            pAffCategory,
            isInterruptable,
            isInterrupting,
            actionAfterInterruption,
            weight,
            requireAllDesires,
            CalcAffordanceType.Affordance,
            calcParameters,
            guid,
            isBusy)
        {
            _variableOps = variableOps;
            _variableRequirements = variableRequirements;
            _calcParameters = calcParameters;
            _variableRepository = variableRepository;
            Energyprofiles = energyprofiles;
            SourceTrait = srcTrait;
            if (personProfile == null) {
#pragma warning disable IDE0016 // Use 'throw' expression
                throw new DataIntegrityException("The affordance " + Name + " has no person profile!");
#pragma warning restore IDE0016 // Use 'throw' expression
            }

            _timeStandardDeviation = timeStandardDeviation;
            SubAffordances = new List<CalcSubAffordance>();
            _personProfile = personProfile;
            AffordanceColor = affordanceColor;
            TimeLimitName = timeLimitName;
        }

        public override ColorRGB AffordanceColor { get; }

        public override int DefaultPersonProfileLength => _personProfile.StepValues.Count;
        public static bool DoubleCheckBusyArray { get; set; }

        [NotNull]
        [ItemNotNull]
        public override List<DeviceEnergyProfileTuple> Energyprofiles { get; }

        [NotNull]
        public override string SourceTrait { get; }

        // public int PersonProfileDuration => _personProfile.StepValues.Count;
        [NotNull]
        [ItemNotNull]
        public override List<CalcSubAffordance> SubAffordances { get; }

        [NotNull]
        public override string TimeLimitName { get; }

        public override void Activate([NotNull] TimeStep startTime,
                                      [NotNull] string activatorName,
                                      [NotNull] CalcLocation personSourceLocation,
                                      [NotNull] out ICalcProfile personTimeProfile)
        {
            TimeStep timeLastDeviceEnds = startTime.GetAbsoluteStep(0);

            foreach (var dpt in Energyprofiles) {
                if (dpt.Probability > _probabilitiesForTimes[startTime.InternalStep]) {
                    //_calcDevice.SetTimeprofile(tbp, startidx + TimeOffsetInSteps, loadType, timeFactor, affordancename,activatorName, _multiplier);
                    CalcProfile adjustedProfile = dpt.TimeProfile.CompressExpandDoubleArray(_timeFactorsForTimes[startTime.InternalStep]);
                    var endtime = dpt.CalcDevice.SetTimeprofile(adjustedProfile,
                        startTime.AddSteps(dpt.TimeOffsetInSteps),
                        dpt.LoadType,
                        Name,
                        activatorName,
                        dpt.Multiplier,
                        false);
                    if (endtime > timeLastDeviceEnds) {
                        timeLastDeviceEnds = endtime;
                    }
                }
            }

            var personsteps = CalcProfile.GetNewLengthAfterCompressExpand(_personProfile.StepValues.Count,
                _timeFactorsForTimes[startTime.InternalStep]);
            _startTimeStep = startTime;
            _endTimeStep = startTime.AddSteps(personsteps);
            if (DoubleCheckBusyArray) {
                for (var i = 0; i < personsteps && i + startTime.InternalStep < _calcParameters.InternalTimesteps; i++) {
                    if (IsBusyArray[i + startTime.InternalStep]) {
                        throw new LPGException("Affordance was already busy");
                    }
                }
            }

            for (var i = 0; i < personsteps && i + startTime.InternalStep < _calcParameters.InternalTimesteps; i++) {
                IsBusyArray[i + startTime.InternalStep] = true;
            }

            if (_variableOps.Count > 0) {
                foreach (var op in _variableOps) {
                    // figure out end time
                    TimeStep time;
                    switch (op.ExecutionTime) {
                        case VariableExecutionTime.Beginning:
                            time = startTime;
                            break;
                        case VariableExecutionTime.EndOfPerson:
                            time = _endTimeStep;
                            break;
                        case VariableExecutionTime.EndofDevices:
                            time = timeLastDeviceEnds;
                            break;
                        default:
                            throw new LPGException("Forgotten Variable Execution Time");
                    }

                    _variableRepository.AddExecutionEntry(op.Name, op.Value, op.CalcLocation, op.VariableAction, time, op.VariableGuid);
                    _variableRepository.Execute(startTime);
                }
            }

            double tf = _timeFactorsForTimes[startTime.InternalStep];
            _probabilitiesForTimes.Clear();
            _timeFactorsForTimes.Clear();
            personTimeProfile = _personProfile.CompressExpandDoubleArray(tf);
            //return tf ;
        }

        public void AddDeviceTuple([NotNull] CalcDevice dev,
                                   [NotNull] CalcProfile newprof,
                                   [NotNull] CalcLoadType lt,
                                   decimal timeoffset,
                                   TimeSpan internalstepsize,
                                   double multiplier,
                                   double probability)
        {
            //TODO: remove this, it is only used in unit testing
            var calctup = new DeviceEnergyProfileTuple(dev, newprof, lt, timeoffset, internalstepsize, multiplier, probability);
            Energyprofiles.Add(calctup);
        }

        [CanBeNull]
        public override string AreDeviceProfilesEmpty()
        {
            var areDeviceProfilesEmpty = Energyprofiles
                .Where(deviceEnergyProfileTuple => deviceEnergyProfileTuple.TimeProfile.TimeSpanDataPoints.Count < 2)
                .Select(deviceEnergyProfileTuple => deviceEnergyProfileTuple.TimeProfile.Name).FirstOrDefault();

            return areDeviceProfilesEmpty;
        }

        public override bool AreThereDuplicateEnergyProfiles()
        {
            foreach (var tuple in Energyprofiles) {
                foreach (var subtuple in Energyprofiles) {
                    if (tuple != subtuple && tuple.CalcDevice == subtuple.CalcDevice && tuple.LoadType == subtuple.LoadType &&
                        subtuple.TimeOffset == tuple.TimeOffset) {
                        return true;
                    }
                }
            }

            return false;
        }

        //public override ICalcProfile CollectPersonProfile() => _personProfile;

        [NotNull]
        [ItemNotNull]
        public override List<CalcSubAffordance> CollectSubAffordances([NotNull] TimeStep time,
                                                                      [NotNull] NormalRandom nr,
                                                                      bool onlyInterrupting,
                                                                      [NotNull] Random r,
                                                                      [NotNull] CalcLocation srcLocation)
        {
            if (SubAffordances.Count == 0) {
                return new List<CalcSubAffordance>();
            }

            if (RemainingActiveTime(time) < 1) {
                return new List<CalcSubAffordance>();
            }

            // es gibt subaffs und diese aff ist aktiv
            var result = new List<CalcSubAffordance>();
            foreach (var calcSubAffordance in SubAffordances) {
                if (onlyInterrupting && calcSubAffordance.IsInterrupting || !onlyInterrupting) {
                    var delaytimesteps = calcSubAffordance.Delaytimesteps;
                    var hasbeenactivefor = HasBeenActiveFor(time);
                    var issubaffbusy = calcSubAffordance.IsBusy(time, nr, r, srcLocation, "name");
                    if (delaytimesteps < hasbeenactivefor && !issubaffbusy) {
                        calcSubAffordance.SetDurations(RemainingActiveTime(time));
                        result.Add(calcSubAffordance);
                    }
                }
            }

            return result;
        }

        public override bool IsBusy([NotNull] TimeStep time,
                                    [NotNull] NormalRandom nr,
                                    [NotNull] Random r,
                                    [NotNull] CalcLocation srcLocation,
                                    [NotNull] string calcPersonName,
                                    bool clearDictionaries = true)
        {
            if (!_timeFactorsForTimes.ContainsKey(time.InternalStep)) {
                if (clearDictionaries) {
                    //        _timeFactorsForTimes.Clear();
                }

                _timeFactorsForTimes[time.InternalStep] = nr.NextDouble(1, _timeStandardDeviation);
                if (_timeFactorsForTimes[time.InternalStep] < 0) {
                    throw new DataIntegrityException("The duration standard deviation on " + Name + " is too large: a negative value of " +
                                                     _timeFactorsForTimes[time.InternalStep] + " came up. The standard deviation is " +
                                                     _timeStandardDeviation);
                }
            }

            if (!_probabilitiesForTimes.ContainsKey(time.InternalStep)) {
                if (clearDictionaries) {
                    //      _probabilitiesForTimes.Clear();
                }

                _probabilitiesForTimes[time.InternalStep] = r.NextDouble();
            }

            if (_variableRequirements.Count > 0) {
                foreach (var requirement in _variableRequirements) {
                    if (!requirement.IsMet()) {
                        return true; // return is busy right now and not available.
                    }
                }
            }

            if (time.InternalStep >= IsBusyArray.Length) {
                return false;
            }

            if (IsBusyArray[time.InternalStep]) {
                return true;
            }

            foreach (var dpt in Energyprofiles) {
                if (dpt.Probability > _probabilitiesForTimes[time.InternalStep]) {
                    if (dpt.CalcDevice.IsBusyDuringTimespan(time.AddSteps(dpt.TimeOffsetInSteps),
                        dpt.TimeProfile.StepValues.Count,
                        _timeFactorsForTimes[time.InternalStep],
                        dpt.LoadType)) {
                        return true;
                    }
                }
            }

            return false;
        }

        [NotNull]
        public override string ToString() => "Affordance:" + Name;

        private int HasBeenActiveFor([NotNull] TimeStep currentTime)
        {
            if (currentTime < _startTimeStep) {
                return -1;
            }

            if (currentTime > _endTimeStep) {
                return -1;
            }

            if (_startTimeStep == null) {
                throw new LPGException("Start time step was null");
            }

            var hasbeenactive = currentTime.InternalStep - _startTimeStep.InternalStep;
            return hasbeenactive;
        }

        private int RemainingActiveTime([NotNull] TimeStep currentTime)
        {
            if (currentTime == null) {
                throw new ArgumentNullException(nameof(currentTime));
            }

            if (currentTime < _startTimeStep) {
                return -1;
            }

            if (currentTime > _endTimeStep) {
                return -1;
            }

            if (_endTimeStep == null) {
                return -1;
            }

            var remainingTime = _endTimeStep.InternalStep - currentTime.InternalStep;
            return remainingTime;
        }

        #region Nested type: DeviceEnergyProfileTuple

        public class DeviceEnergyProfileTuple {
            [NotNull] private readonly CalcDevice _calcDevice;

            private readonly double _multiplier;

            public DeviceEnergyProfileTuple([NotNull] CalcDevice pdev,
                                            [NotNull] CalcProfile ep,
                                            [NotNull] CalcLoadType pLoadType,
                                            decimal timeOffset,
                                            TimeSpan stepsize,
                                            double multiplier,
                                            double probability)
            {
                _calcDevice = pdev;
                TimeProfile = ep;
                LoadType = pLoadType;
                TimeOffset = timeOffset;
                _multiplier = multiplier;
                var minutesperstep = (decimal)stepsize.TotalMinutes;
                TimeOffsetInSteps = (int)(timeOffset / minutesperstep);
                Probability = probability;
            }

            [NotNull]
            public CalcDevice CalcDevice => _calcDevice;

            [NotNull]
            public CalcLoadType LoadType { get; }

            public double Multiplier => _multiplier;

            public double Probability { get; }

            public decimal TimeOffset { get; }

            [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "InSteps")]
            public int TimeOffsetInSteps { get; }

            [NotNull]
            public CalcProfile TimeProfile { get; }

            [NotNull]
            public override string ToString() => "Device:" + _calcDevice.Name + ", Profile " + TimeProfile.Name + ", Offset " + TimeOffset;
        }

        #endregion
    }
}
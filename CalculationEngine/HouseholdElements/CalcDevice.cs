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
using System.Text;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.OnlineDeviceLogging;
using Common;
using Common.CalcDto;
using JetBrains.Annotations;

#endregion

namespace CalculationEngine.HouseholdElements {
    /*public struct BusyEntry {
        public override int GetHashCode()
        {
            unchecked {
                return (_end * 397) ^ _start;
            }
        }

        private readonly int _end;
        private readonly int _start;

        public BusyEntry(int start, int end) : this()
        {
            _start = start;
            _end = end;
        }

        //public int End => _end;

        //public int Start => _start;

        public override string ToString() => "Start:" + _start + " End:" + _end;

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public bool Equals(BusyEntry other) => _end == other._end && _start == other._start;

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            return obj is BusyEntry && Equals((BusyEntry) obj);
        }

        public static bool operator ==(BusyEntry point1, BusyEntry point2) => point1.Equals(point2);

        public static bool operator !=(BusyEntry point1, BusyEntry point2) => !point1.Equals(point2);
    }*/

    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public class CalcDevice : CalcBase {
        [NotNull]
        private readonly CalcLocation _calcLocation;
        [NotNull]
        private readonly Dictionary<CalcLoadType, BitArray> _isBusyForLoadType;
        [NotNull]
        private readonly Dictionary<CalcLoadType, CalcDeviceLoad> _deviceLoadsBy = new Dictionary<CalcLoadType, CalcDeviceLoad>();

        public List<CalcDeviceLoad> Loads { get; } = new List<CalcDeviceLoad>();
        private readonly CalcDeviceDto _calcDeviceDto;
        private readonly CalcRepo _calcRepo;

        public CalcDevice( [NotNull][ItemNotNull] List<CalcDeviceLoad> powerUsage,
                            [NotNull] CalcLocation calcLocation,
                           [NotNull] CalcDeviceDto calcDeviceDto,
                           [NotNull] CalcRepo calcRepo) : base(calcDeviceDto.Name,  calcDeviceDto.Guid)
        {
            _calcDeviceDto = calcDeviceDto;
            _calcRepo = calcRepo;
            if (_calcDeviceDto.LocationGuid != calcLocation.Guid) {
                throw new LPGException("Inconsistent locations. This is a bug.");
            }
            _calcLocation = calcLocation;
            foreach (var load in powerUsage) {
                _deviceLoadsBy.Add(load.LoadType,load);
                Loads.Add(load);
            }
            /*foreach (var load in powerUsage) {
                if (Math.Abs(load.Value) < 0.0000001 && load.Name.ToLower(CultureInfo.InvariantCulture) != "none") {
                    throw new LPGException("Trying to run the device " + calcDeviceDto.Name + " with a power load factor for " + load.LoadType.Name + " of 0. This is not going to work.");
                }
            }*/
            if (calcRepo.CalcParameters.InternalTimesteps == 0) {
                throw new LPGException("Can't run with 0 timesteps");
            }
            _isBusyForLoadType = new Dictionary<CalcLoadType, BitArray>();
            foreach (var calcDeviceLoad in powerUsage) {
                _isBusyForLoadType.Add(calcDeviceLoad.LoadType,
                    new BitArray(calcRepo.CalcParameters.InternalTimesteps));
                _isBusyForLoadType[calcDeviceLoad.LoadType].SetAll(false);
                //var key = new OefcKey(calcDeviceDto.HouseholdKey, calcDeviceDto.DeviceType,calcDeviceDto.Guid, calcDeviceDto.LocationGuid,calcDeviceLoad.LoadType.Guid, calcDeviceDto.DeviceCategoryName);
                var key = calcRepo.Odap.RegisterDevice( calcDeviceLoad.LoadType.ConvertToDto(), calcDeviceDto);
                    _keyByLoad.Add(calcDeviceLoad.LoadType, key);
            }
        }
        private readonly Dictionary<CalcLoadType, OefcKey> _keyByLoad = new Dictionary<CalcLoadType, OefcKey>();
        [NotNull]
        public CalcLocation CalcLocation => _calcLocation;

        [NotNull]
        public Dictionary<CalcLoadType, BitArray> IsBusyForLoadType => _isBusyForLoadType;

        public StrGuid DeviceCategoryGuid => _calcDeviceDto.DeviceCategoryGuid;
        [NotNull]
        [ItemNotNull]
        public List<CalcAutoDev> MatchingAutoDevs { get; } = new List<CalcAutoDev>();

    //    [NotNull]
  //      [ItemNotNull]
// public List<CalcDeviceLoad> PowerUsage => _powerUsage;

        public int LoadCount => _deviceLoadsBy.Count;

        [NotNull]
        public string PrettyName {
            get {
                var builder = new StringBuilder();
                builder.Append(Name).Append(" (");
                foreach (var load in _deviceLoadsBy.Values) {
                    builder.Append(load.Name).Append(", ");
                }
                var s = builder.ToString();
                if (_deviceLoadsBy.Count > 0) {
                    s = s.Substring(0, s.Length - 2);
                }
                s += ")";
                return s;
            }
        }

        public CalcRepo CalcRepo => _calcRepo;

        //for the device factories
        public void ApplyBitArry([NotNull][ItemNotNull] BitArray br, [NotNull] CalcLoadType lt)
        {
            if (br.Length != _isBusyForLoadType[lt].Length) {
                throw new LPGException("Bitarry Length was not equal to BusyArrayLength. Please Report.");
            }
            for (var i = 0; i < br.Length; i++) {
                _isBusyForLoadType[lt][i] = br[i];
            }
        }

        public bool GetIsBusyForTesting([NotNull] TimeStep i, [NotNull] CalcLoadType lt) => _isBusyForLoadType[lt][i.InternalStep];

        public bool IsBusyDuringTimespan([NotNull] TimeStep startidx, int duration, double timefactor, [NotNull] CalcLoadType lt)
        {
            var dstduration = CalcProfile.GetNewLengthAfterCompressExpand(duration, timefactor);
            var lastbit = startidx.InternalStep + dstduration;

            for (var i = startidx.InternalStep; i < lastbit && i < _isBusyForLoadType[lt].Length; i++) {
                if (_isBusyForLoadType[lt][i]) {
                    return true;
                }
            }
            return false;
        }
        /*
        public static List<BusyEntry> MakeBusyEntries(BitArray isBusy)
        {
            var busy = false;
            var start = 0;
            var entries = new List<BusyEntry>();
            for (var i = 0; i < isBusy.Length; i++) {
                if (isBusy[i] && !busy) {
                    start = i;
                    busy = true;
                }
                if (busy && !isBusy[i]) {
                    busy = false;
                    var be = new BusyEntry(start, i - 1);
                    entries.Add(be);
                }
            }
            if (busy) {
                var be = new BusyEntry(start, isBusy.Length);
                entries.Add(be);
            }
            return entries;
        }*/

        public void SetAllLoadTypesToTimeprofile([NotNull] CalcProfile tbp, [NotNull] TimeStep startidx,
            [NotNull] string affordanceName, [NotNull] string activatorName, double multiplier)
        {
            foreach (var calcDeviceLoad in _deviceLoadsBy.Values) {
                SetTimeprofile(tbp, startidx, calcDeviceLoad.LoadType,  affordanceName, activatorName,
                    multiplier, false);
            }
        }

/*        private static int BusyEntryComparer(BusyEntry e1, BusyEntry e2)
        {
            int result = e1.Start.CompareTo(e2.Start);
            if (result != 0)
                return result;
            return e1.End.CompareTo(e2.End);
        }*/

        private void SetBusy([NotNull] TimeStep startIndex, int duration, [NotNull] CalcLoadType loadType, bool activateDespiteBeingBusy)
        {
            /*if (UseRanges)
            {
                BusyEntry be = new BusyEntry(startidx, startidx + duration - 1);
                _busyEntries.Add(be);
                _busyEntries.Sort(BusyEntryComparer);
            }*/
            // if (Name == "Cell Phone Samsung Charging") {Logger.Info("Setting Busy at " + startIndex + " for a duration of " + duration + " until " +(startIndex + duration));}
            var endidx = Math.Min(startIndex.InternalStep + duration, _isBusyForLoadType[loadType].Length);
            for (var i = startIndex.InternalStep; i < endidx; i++) {
                // if (!UseRanges)
                //   _isBusy.Set(startidx + i, true);
                if (!activateDespiteBeingBusy && _isBusyForLoadType[loadType][i]) {
                    throw new LPGException("The device " + Name +
                                           " was somehow activated, even though it was already active." +
                                           " This points towards something being quite wrong. Timestep: " +
                                           i);
                }
                _isBusyForLoadType[loadType].Set(i, true);
            }
        }

        //function only used for testing
        public void SetIsBusyForTesting([NotNull] TimeStep i, bool value, [NotNull] CalcLoadType lt)
        {
            _isBusyForLoadType[lt][i.InternalStep] = value;
            foreach (var pair in _isBusyForLoadType) {
                pair.Value[i.InternalStep] = value;
            }
        }

        [NotNull]
        public TimeStep SetTimeprofile([NotNull] TimeStep startidx, [NotNull] CalcLoadType loadType,
            [NotNull] string affordanceName, [NotNull] string activatingPersonName,
                                        bool activateDespiteBeingBusy, [NotNull] StepValues stepValues)
        {
            if (stepValues.Values.Count == 0)
            {
                throw new LPGException("Trying to set empty device profile. This is a bug. Please report.");
            }

            CalcDeviceLoad cdl = _deviceLoadsBy[loadType];
            if (cdl == null)
            {
                throw new LPGException("It was tried to activate the loadtype " + loadType.Name +
                                       " even though that one is not set for the device " + Name);
            }

            /*if (Math.Abs(cdl.Value) < 0.00000001 ) {
                throw new LPGException("Trying to calculate with a power consumption factor of 0. This is wrong.");
            }*/
            if (CalcRepo.Odap == null)
            {
                throw new LPGException("ODAP was null. Please report");
            }
            var totalDuration = stepValues.Values.Count; //.GetNewLengthAfterCompressExpand(timefactor);
                                                              //calcProfile.CompressExpandDoubleArray(timefactor,allProfiles),
            var key = _keyByLoad[cdl.LoadType];
            CalcRepo.Odap.AddNewStateMachine(
                startidx,
                cdl.LoadType.ConvertToDto(),
                affordanceName,
                activatingPersonName,
                key,
                _calcDeviceDto, stepValues);

            if (MatchingAutoDevs.Count > 0)
            {
                foreach (CalcAutoDev matchingAutoDev in MatchingAutoDevs)
                {
                    if (matchingAutoDev._keyByLoad.ContainsKey(cdl.LoadType))
                    {
                        var zerokey = matchingAutoDev._keyByLoad[cdl.LoadType];
                        CalcRepo.Odap.AddZeroEntryForAutoDev(zerokey,
                            startidx,
                            totalDuration);
                    }
                }
            }
            SetBusy(startidx, totalDuration, loadType, activateDespiteBeingBusy);
            return startidx.AddSteps(totalDuration);
        }

        private class RandomValueProfileEntry {
            public RandomValueProfileEntry(TimeStep timeStep, double powerStandardDeviation, RandomValueProfile randomValueProfile)
            {
                TimeStep = timeStep;
                PowerStandardDeviation = powerStandardDeviation;
                RandomValueProfile = randomValueProfile;
            }

            public TimeStep TimeStep { get; }
            public double PowerStandardDeviation { get; }
            public RandomValueProfile RandomValueProfile { get; }

        }

        private RandomValueProfile GetRandomValueProfile(int count, CalcDeviceLoad cdl, TimeStep timeStep)
        {
            var entry = _randomValues.FirstOrDefault(x =>
                x.TimeStep == timeStep && Math.Abs(x.PowerStandardDeviation - cdl.PowerStandardDeviation) < 0.000000001 && x.RandomValueProfile.Values.Count == count);
            if (entry != null) {
                return entry.RandomValueProfile;
            }
            var newrvp = RandomValueProfile.MakeStepValues(count, CalcRepo.NormalRandom,
                cdl.PowerStandardDeviation);
            while (_randomValues.Count > 10) {
                _randomValues.RemoveAt(0);
            }
            var rvpe = new RandomValueProfileEntry(timeStep,cdl.PowerStandardDeviation,newrvp);
            _randomValues.Add(rvpe);
            return newrvp;
        }
        private readonly List<RandomValueProfileEntry> _randomValues = new List<RandomValueProfileEntry>();
        //private Dictionary<string, RandomValueProfile> _randomProfileCache;
        [NotNull]
        public TimeStep SetTimeprofile([NotNull] CalcProfile calcProfile, [NotNull] TimeStep startTimeStep, [NotNull] CalcLoadType loadType,
            [NotNull] string affordanceName, [NotNull] string activatingPersonName, double multiplier, bool activateDespiteBeingBusy)
        {
            if (calcProfile.StepValues.Count == 0) {
                throw new LPGException("Trying to set empty device profile. This is a bug. Please report.");
            }

            CalcDeviceLoad cdl = _deviceLoadsBy[loadType];
            if (cdl == null) {
                throw new LPGException("It was tried to activate the loadtype " + loadType.Name +
                                       " even though that one is not set for the device " + Name);
            }

            /*if (Math.Abs(cdl.Value) < 0.00000001 ) {
                throw new LPGException("Trying to calculate with a power consumption factor of 0. This is wrong.");
            }*/
            if (CalcRepo.Odap == null) {
                throw new LPGException("ODAP was null. Please report");
            }
            var totalDuration = calcProfile.StepValues.Count; //.GetNewLengthAfterCompressExpand(timefactor);
            //calcProfile.CompressExpandDoubleArray(timefactor,allProfiles),
                var key = _keyByLoad[cdl.LoadType];

                RandomValueProfile rvp = GetRandomValueProfile(calcProfile.StepValues.Count,cdl,startTimeStep);
                StepValues sv = StepValues.MakeStepValues(calcProfile,
                    multiplier, rvp, cdl );
                CalcRepo.Odap.AddNewStateMachine(
                    startTimeStep,
                    cdl.LoadType.ConvertToDto(),
                    affordanceName,
                    activatingPersonName,
                    key,
                    _calcDeviceDto, sv);

            if (MatchingAutoDevs.Count > 0) {
                foreach (CalcAutoDev matchingAutoDev in MatchingAutoDevs) {
                    if (matchingAutoDev._keyByLoad.ContainsKey(cdl.LoadType)) {
                        var zerokey = matchingAutoDev._keyByLoad[cdl.LoadType];
                        CalcRepo.Odap.AddZeroEntryForAutoDev(zerokey,
                            startTimeStep,
                            totalDuration);
                    }
                }
            }
            SetBusy(startTimeStep, totalDuration, loadType, activateDespiteBeingBusy);
            return  startTimeStep.AddSteps(totalDuration);
        }

        public override string ToString() => "Device:" + Name;
    }
}
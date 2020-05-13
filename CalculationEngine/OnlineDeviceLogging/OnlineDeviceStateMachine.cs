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

using System;
using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.OnlineDeviceLogging {
    public class StepValues {
        private StepValues([NotNull] List<double> values)
        {
            Values = values;
        }
        [NotNull]
        public  static StepValues MakeStepValues([NotNull] CalcProfile srcProfile,
                                                 [NotNull] NormalRandom nr,
                                                 [NotNull] CalcDeviceLoad cdl, double multiplier)
        {

            var powerUsage = cdl.Value * multiplier;
            if (srcProfile.ProfileType == ProfileType.Absolute)
            {
                powerUsage = 1 * multiplier;
            }
            var values = new List<double>(srcProfile.StepValues);
            for (var i = 0; i < values.Count; i++)
            {
                double spread = 1;
                if (Math.Abs(cdl.PowerStandardDeviation) > 0.00000001)
                {
                    spread = nr.NextDouble(1, cdl.PowerStandardDeviation);
                }
                values[i] = values[i] * powerUsage * spread;
            }
            return new StepValues(values);
        }
        [NotNull]
        public List<double> Values { get; }
    }
    public class OnlineDeviceStateMachine {
        [NotNull]
        private readonly string _deviceName;
        [NotNull]
        private readonly CalcParameters _calcParameters;
        [NotNull] private readonly TimeStep _startTimeStep;
        //private readonly OefcKey _zek;

        public OnlineDeviceStateMachine( [NotNull] TimeStep startTimeStep,
              [NotNull] CalcLoadTypeDto loadType,
            [NotNull] string deviceName, OefcKey deviceKey, [NotNull] string affordanceName, [NotNull] CalcParameters calcParameters,
                                         [NotNull] StepValues stepValues) {
            //_zek = new ZeroEntryKey(deviceKey.HouseholdKey, deviceKey.ThisDeviceType,deviceKey.DeviceGuid,deviceKey.LocationGuid);
            _deviceName = deviceName;
            _calcParameters = calcParameters;
            DeviceKey = deviceKey;
            LoadType = loadType;
            _startTimeStep = startTimeStep;
            // time profile einladen, zeitlich variieren, normalverteilt variieren und dann als stepvalues speichern
          ////    throw new LPGException("power usage factor was 0. this is a bug. Device " + deviceName + ", Loadtype " + loadType);
            //}
            StepValues = stepValues;
            AffordanceName = affordanceName;
            HouseholdKey = deviceKey.HouseholdKey;
        }

        [NotNull]
        public string AffordanceName { get; }

        public OefcKey DeviceKey { get; }

        [NotNull]
        public HouseholdKey HouseholdKey { get; }

        [NotNull]
        public CalcLoadTypeDto LoadType { get; }

        [NotNull]
        public StepValues StepValues { get; }

        public double CalculateOfficialEnergyUse() {
            var settlingsteps = _calcParameters.DummyCalcSteps;
            // take everything
            if (_startTimeStep.InternalStep >= settlingsteps &&
                _startTimeStep.InternalStep + StepValues.Values.Count < _calcParameters.InternalTimesteps) {
                return StepValues.Values.Sum();
            }
            var firststep = 0;
            if (_startTimeStep.InternalStep < _calcParameters.DummyCalcSteps) {
                firststep = _calcParameters.DummyCalcSteps - _startTimeStep.InternalStep;
            }
            if (firststep < 0) {
                throw new LPGException("below 0");
            }
            var laststep = StepValues.Values.Count;
            if (_startTimeStep.InternalStep + StepValues.Values.Count > _calcParameters.InternalTimesteps) {
                var diff = _startTimeStep.InternalStep + StepValues.Values.Count - _calcParameters.InternalTimesteps;
                laststep -= diff;
            }
            if (laststep > StepValues.Values.Count) {
                throw new LPGException("above length");
            }
            double sum1 = 0;
            for (var i = firststep; i < laststep; i++) {
                sum1 += StepValues.Values[i];
            }
            return sum1;
        }

        public double GetEnergyValueForTimeStep([NotNull] TimeStep timestep, [NotNull] CalcLoadTypeDto lt, [NotNull][ItemNotNull] List<SetToZeroEntry> zeroEntries) {
            if (lt == null) {
                throw new LPGException("Loadtype should never be null");
            }
            if (lt != LoadType) {
                return 0;
            }
            foreach (var setToZeroEntry in zeroEntries) {
                if (setToZeroEntry.Key == DeviceKey) {
                    if (timestep >= setToZeroEntry.StartTime && timestep < setToZeroEntry.EndTime) {
                        return 0;
                    }
                }
            }
            var time = timestep - _startTimeStep;
            if (time.InternalStep < 0) {
                return 0;
            }
            if (time.InternalStep >= StepValues.Values.Count) {
                return 0;
            }
            return StepValues.Values[time.InternalStep];
        }

        public bool IsExpired([NotNull] TimeStep timestep) {
            if (timestep >= _startTimeStep.AddSteps( StepValues.Values.Count)) {
                return true;
            }
            return false;
        }

        [NotNull]
        public override string ToString() => _deviceName;
    }
}
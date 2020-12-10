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
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.OnlineDeviceLogging {

    public class RandomValueProfile
    {
        private RandomValueProfile([NotNull] List<double> values)
        {
            Values = values;
        }

        [NotNull]
        public static RandomValueProfile MakeStepValues(int stepCount, [NotNull] NormalRandom nr, double powerStandardDeviation)
        {
            if (stepCount == 0) {
                throw new LPGException("stepcount was 0");
            }
            var values = new List<double>(new double[stepCount]);
            if (Math.Abs(powerStandardDeviation) > 0.00000001) {
                for (var i = 0; i < stepCount; i++) {
                    values[i] = nr.NextDouble(1, powerStandardDeviation);
                }
            }
            else {
                for (var i = 0; i < stepCount; i++) {
                    values[i] = 1;
                }
            }
            return new RandomValueProfile(values);
        }

        [NotNull]
        public List<double> Values { get; }
    }
    public class StepValues {
        private StepValues([NotNull] List<double> values, string name, string dataSource)
        {
            Values = values;
            Name = name;
            DataSource = dataSource;
        }
        [NotNull]
        public  static StepValues MakeStepValues([NotNull] CalcProfile srcProfile,
                                                  double multiplier, RandomValueProfile rvp, [NotNull] CalcDeviceLoad cdl)
        {

            var powerUsage = cdl.Value * multiplier;
            if (srcProfile.ProfileType == ProfileType.Absolute)
            {
                powerUsage = 1 * multiplier;
            }
            var values = new List<double>(srcProfile.StepValues);
            for (var i = 0; i < values.Count; i++)
            {
                values[i] = values[i] * powerUsage * rvp.Values[i];
            }
            return new StepValues(values, srcProfile.Name, srcProfile.DataSource);
        }
        [NotNull]
        public List<double> Values { get; }

        public string Name { get; }
        public string DataSource { get; }
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
                                         [NotNull] StepValues stepValues, int columnNumber) {
            //_zek = new ZeroEntryKey(deviceKey.HouseholdKey, deviceKey.ThisDeviceType,deviceKey.DeviceGuid,deviceKey.LocationGuid);
            _deviceName = deviceName;
            _calcParameters = calcParameters;
            OefcKey = deviceKey;
            LoadType = loadType;
            if (loadType == null) {
                throw new LPGException("loadtype for an osm was null");
            }
            _startTimeStep = startTimeStep;
            // time profile einladen, zeitlich variieren, normalverteilt variieren und dann als stepvalues speichern
          ////    throw new LPGException("power usage factor was 0. this is a bug. Device " + deviceName + ", Loadtype " + loadType);
            //}
            StepValues = stepValues;
            ColumnNumber = columnNumber;
            AffordanceName = affordanceName;
            HouseholdKey = deviceKey.HouseholdKey;
        }

        [NotNull]
        public string AffordanceName { get; }

        public OefcKey OefcKey { get; }

        [NotNull]
        public HouseholdKey HouseholdKey { get; }

        [NotNull]
        public CalcLoadTypeDto LoadType { get; }

        [NotNull]
        public StepValues StepValues { get; }

        public int ColumnNumber { get; }

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

        public double GetEnergyValueForTimeStep([NotNull] TimeStep timestep, [NotNull][ItemNotNull] List<SetToZeroEntry> zeroEntries) {
            foreach (var setToZeroEntry in zeroEntries) {
                if (setToZeroEntry.Key == OefcKey) {
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
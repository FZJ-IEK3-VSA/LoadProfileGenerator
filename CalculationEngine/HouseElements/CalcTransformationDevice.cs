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
using Automation;
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.HouseElements {
    public class CalcTransformationDevice : CalcBase {
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private readonly List<CalcTransformationCondition> _conditions;
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private readonly List<DataPoint> _datapoints = new List<DataPoint>();
        private readonly Dictionary<CalcLoadType,  OefcKey> _devProcessorKeys = new Dictionary<CalcLoadType, OefcKey>();
        private readonly double _maxPower;
        private readonly CalcDeviceDto _deviceDto;
        private readonly double _maxValue;
        private readonly double _minPower;
        private readonly double _minValue;
        [JetBrains.Annotations.NotNull]
        private readonly IOnlineDeviceActivationProcessor _odap;
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private readonly List<OutputLoadType> _outputLoadTypes;
        [JetBrains.Annotations.NotNull] private readonly CalcLoadType _inputLoadType;

        public CalcTransformationDevice([JetBrains.Annotations.NotNull] IOnlineDeviceActivationProcessor odap, double minValue,
            double maxValue, double minimumOutputPower, double maximumInputPower, [JetBrains.Annotations.NotNull] CalcDeviceDto deviceDto,
            [JetBrains.Annotations.NotNull] CalcLoadType inputLoadType)
            : base(deviceDto.Name,  deviceDto.Guid) {
            _odap = odap;
            _minValue = minValue;
            _maxValue = maxValue;
            _outputLoadTypes = new List<OutputLoadType>();
            _conditions = new List<CalcTransformationCondition>();
            _minPower = minimumOutputPower;
            _maxPower = maximumInputPower;
            _inputLoadType = inputLoadType;
            _deviceDto = deviceDto;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<OutputLoadType> OutputLoadTypes => _outputLoadTypes;

        public void AddCondition([JetBrains.Annotations.NotNull] string name,
             [JetBrains.Annotations.NotNull] CalcVariable variable, double minValue, double maxValue, StrGuid guid) {
            var cond =
                new CalcTransformationCondition(name, variable, minValue, maxValue, guid);
            _conditions.Add(cond);
        }

        public void AddDatapoint(double myref, double val) {
            var dp = new DataPoint(myref, val);
            _datapoints.Add(dp);
        }

        public void AddOutputLoadType([JetBrains.Annotations.NotNull] CalcLoadType loadType, double factor, TransformationOutputFactorType factorType) {
            var olt = new OutputLoadType(loadType, factor, factorType);
            _devProcessorKeys.Add(loadType, _odap.RegisterDevice( loadType.ConvertToDto(), _deviceDto));
            _outputLoadTypes.Add(olt);
        }

        private double GetValue(double input, double factor, TransformationOutputFactorType factorType, double demandValue) {
            switch (factorType) {
                case TransformationOutputFactorType.FixedFactor: return factor * demandValue ;
                case TransformationOutputFactorType.Interpolated:
                    if (input <= _datapoints[0].Ref) {
                        return _datapoints[0].Val;
                    }
                    var max = _datapoints.Count - 1;
                    if (input >= _datapoints[max].Ref) {
                        return _datapoints[max].Val;
                    }
                    var i = 0;
                    while (i < _datapoints.Count && _datapoints[i].Ref < input) {
                        i++;
                    }
                    var y1 = _datapoints[i - 1].Val;
                    var x1 = _datapoints[i - 1].Ref;
                    var y2 = _datapoints[i].Val;
                    var x2 = _datapoints[i].Ref;
                    var newfactor = (y2 - y1) / (x2 - x1) * (input - x1) + y1;
                    return newfactor * demandValue;
                case TransformationOutputFactorType.FixedValue:
                    return factor;
                default: throw new LPGException("Forgotten factortype");
            }
        }

        public bool ProcessOneTimestep([JetBrains.Annotations.NotNull][ItemNotNull] List<OnlineEnergyFileRow> fileRows, [ItemNotNull] List<string>? log) {
            var madeChanges = false;
            var conditionsValid = true;
            foreach (var condition in _conditions) {
                if (!condition.GetResult()) {
                    conditionsValid = false;
                }
            }

            var inputRow = fileRows.FirstOrDefault(x => x.LoadType == _inputLoadType.ConvertToDto());
            var outputloadtypes = _outputLoadTypes.Select(x => x.LoadType.ConvertToDto()).ToList();
            double inputSum=0;
            if (inputRow != null) {
                if (outputloadtypes.Contains(_inputLoadType.ConvertToDto())) {
                    // input and output to the same load type
                    var inputColumn = _odap.Oefc.GetColumnNumber(_inputLoadType.ConvertToDto(), _devProcessorKeys[_inputLoadType]);
                    inputSum = inputRow.SumFresh(inputColumn);
                }
                else {
                    inputSum = inputRow.SumFresh();
                }
            }

            if (Name == "Continuous Flow Gas Heater for Space Heating") {
                Logger.Info("hi");
            }
            // for all the output loadtypes
            foreach (var outloadType in _outputLoadTypes)
            {
                // look for the destination row
                var dstrow = fileRows.First(ofr => ofr.LoadType == outloadType.LoadType.ConvertToDto());
                var oefckey = _devProcessorKeys[outloadType.LoadType];
                var column = _odap.Oefc.GetColumnNumber(outloadType.LoadType.ConvertToDto(), oefckey);

                if ((inputSum >= _minValue && inputSum <= _maxValue && conditionsValid))
                {
                    var desiredpower = inputSum;
                    if (desiredpower < _minPower)
                    {
                        desiredpower = _minPower;
                    }
                    if (desiredpower > _maxPower)
                    {
                        desiredpower = _maxPower;
                    }
                    var newValue = GetValue(inputSum, outloadType.ValueScalingFactor, outloadType.FactorType, desiredpower);
                    if (Math.Abs(dstrow.EnergyEntries[column] - newValue) > Constants.Ebsilon)
                    {
                        dstrow.EnergyEntries[column] = newValue;
                        log?.Add(Name + " set " + outloadType.LoadType.Name + " to " + newValue);
                        madeChanges = true;
                    }
                }
                else
                {
                    if (Math.Abs(dstrow.EnergyEntries[column]) > Constants.Ebsilon)
                    {
                        madeChanges = true;
                        log?.Add(Name + " set " + outloadType.LoadType.Name + " to 0");
                        dstrow.EnergyEntries[column] = 0;
                    }
                }
            }
            return madeChanges;
        }

        private class DataPoint {
            public DataPoint(double myref, double val) {
                Ref = myref;
                Val = val;
            }

            public double Ref { get; }

            public double Val { get; }
        }
    }

    public class OutputLoadType {
        public OutputLoadType([JetBrains.Annotations.NotNull] CalcLoadType loadType, double valueScalingFactor, TransformationOutputFactorType factorType) {
            LoadType = loadType;
            ValueScalingFactor = valueScalingFactor;
            FactorType = factorType;
        }

        public double ValueScalingFactor { get; }

        public TransformationOutputFactorType FactorType { get; }

        //        public string FactorTypeStr => FactorType.ToString();

        [JetBrains.Annotations.NotNull]
        public CalcLoadType LoadType { get; }
    }
}
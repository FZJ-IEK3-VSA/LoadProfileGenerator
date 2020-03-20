//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
// All advertising materials mentioning features or use of this software must display the following acknowledgement:
// “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
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
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;

#endregion

namespace CalculationEngine.HouseholdElements {
    public class SharedDesireValue {
        public SharedDesireValue(decimal currentValue, [CanBeNull] TimeStep lastDecay)
        {
            CurrentValue = currentValue;
            LastDecay = lastDecay;
        }

        public decimal CurrentValue { get; set; }
        [CanBeNull]
        public TimeStep LastDecay { get; set; }
    }

    public class CalcDesire {
        private readonly decimal _decayRate;
        private readonly bool _isSharedValue;
        [NotNull]
        private readonly string _name;
        [CanBeNull] private readonly SharedDesireValue _sharedDesireValue;

        private readonly int _timestepsPerHour;
        [NotNull]
        private readonly string _desireCategory;
        [NotNull]
        private readonly string _sourceTrait;
        private decimal _value;

        public CalcDesire([NotNull] string name, int desireID, decimal threshold, decimal decayTime, decimal value,
            decimal weight,
            int timestepsPerHour, decimal criticalThreshold, [CanBeNull] SharedDesireValue sharedDesireValue,
            [NotNull] string sourceTrait, [NotNull] string desireCategory)
        {
            //todo: check the whole shared desire thing and write some unit tests for it. It is initzialized in the factory to null?
            _timestepsPerHour = timestepsPerHour;
            _name = name;
            DesireID = desireID;
            Threshold = threshold;
            if (decayTime > 0) {
                _decayRate = GetRate(decayTime);
            }
            _sourceTrait = sourceTrait;
            _desireCategory = desireCategory;
            Value = value;
            TempValue = 0;
            Weight = weight;
            DecayTime = decayTime;

            CriticalThreshold = criticalThreshold;
            _sharedDesireValue = sharedDesireValue;
            if (sharedDesireValue != null) {
                _isSharedValue = true;
            }
        }

        public decimal CriticalThreshold { get; }

        // public decimal DecayRate => _decayRate;

        public decimal DecayTime { get; }

        [NotNull]
        public string DesireCategory => _desireCategory;

        public int DesireID { get; }

        [CanBeNull]
        private TimeStep LastDecay {
            get => _sharedDesireValue?.LastDecay;
            set {
                if (_sharedDesireValue == null) {
                    return;
                }
                if (value != null) {
                    _sharedDesireValue.LastDecay = value;
                }
            }
        }

        [NotNull]
        public string Name => _name;

        [NotNull]
        public string SourceTrait => _sourceTrait;

        public decimal TempValue { get; set; }

        public decimal Threshold { get; }

        public decimal Value {
            get {
                if (_sharedDesireValue == null) {
                    return _value;
                }
                return _sharedDesireValue.CurrentValue;
            }
            set {
                if (_sharedDesireValue == null) {
                    _value = value;
                    return;
                }
                _sharedDesireValue.CurrentValue = value;
            }
        }

        public decimal Weight { get; }

        public void ApplyDecay([NotNull] TimeStep timestep)
        {
            if (!_isSharedValue) {
                _value *= _decayRate;
            }
            else if (LastDecay != timestep) // shared desires only need to be decayed once.
            {
                if (_sharedDesireValue == null) {
                    throw new LPGException("Shared Desire Value was null");
                }
                _sharedDesireValue.CurrentValue *= _decayRate;
                LastDecay = timestep;
            }

            if (_value > 1) {
                throw new DataIntegrityException("Desire value above 1. This should never happen.");
            }
        }

        private decimal GetRate(decimal decayTime)
        {
            var logVal = (decimal) Math.Log(0.5);
            var decayTimesteps = decayTime * _timestepsPerHour;
            var exponent = (double) (logVal / decayTimesteps);
            var factor = (decimal) Math.Exp(exponent);
            return factor;
        }

        [NotNull]
        public override string ToString() => "desire:" + _name;
    }
}
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

using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using Common;
using JetBrains.Annotations;

namespace CalculationEngine.HouseElements {
    using OnlineDeviceLogging;

    public class CalcEnergyStorageSignal : CalcBase {
        private readonly double _triggerOffPercent;
        private readonly double _triggerOnPercent;
        private readonly double _value;
        private readonly CalcVariable _calcVariable;
        [CanBeNull] private TimeStep _currtimestep;
        private bool _isTurnedOn;
        private bool _lastTurnedOn;
        private double _tankCurValue;
        private double _tankLastLastValue;
        private double _tankLastValue;

        public CalcEnergyStorageSignal([NotNull] string pName, double triggerOff, double triggerOn, double value,
            CalcVariable calcVariable, [NotNull] string guid) : base(pName,guid) {
            _triggerOffPercent = triggerOff / 100;
            _triggerOnPercent = triggerOn / 100;
            _value = value;
            _calcVariable = calcVariable;
        }

        public OefcKey ProcessorKey { get; set; }

        public CalcVariable CalcVariable => _calcVariable;

        public double GetValue([NotNull] TimeStep timestep, double tankCapacity, double currentFill) {
            var currentFillPercent = currentFill / tankCapacity;
            if (currentFillPercent < 0) {
                throw new LPGException("Energy Storage less than 0% full. This is a bug.");
            }
            if (currentFillPercent > 1) {
                throw new LPGException("Energy Storage more than 100% full. This is a bug.");
            }
            var isFirsttimeStep = false;
            if (_currtimestep == null) {
                _tankLastLastValue = currentFillPercent;
                _tankLastValue = currentFillPercent;
                _tankCurValue = currentFillPercent;
                isFirsttimeStep = true;
            }
            if (timestep != _currtimestep) {
                _currtimestep = timestep;
                _tankLastLastValue = _tankLastValue;
                _tankLastValue = _tankCurValue;
                _tankCurValue = currentFillPercent;
                _lastTurnedOn = _isTurnedOn;
            }
            if (_lastTurnedOn) {
                if (_tankLastLastValue >= _triggerOffPercent && _tankLastValue <= _triggerOffPercent ||
                    _tankLastLastValue <= _triggerOffPercent && _tankLastValue >= _triggerOffPercent) {
                    _isTurnedOn = false;
                }
            }
            else if (_tankLastLastValue >= _triggerOnPercent && _tankLastValue <= _triggerOnPercent ||
                     _tankLastLastValue <= _triggerOnPercent && _tankLastValue >= _triggerOnPercent ||
                     isFirsttimeStep && _tankLastValue <= _triggerOnPercent) {
                _isTurnedOn = true;
            }
            if (_isTurnedOn) {
                return _value;
            }
            return 0;
        }

        [NotNull]
        public override string ToString() {
            var s = "Signal for " + _calcVariable.Name + " for levels between " + _triggerOnPercent * 100 + "% and " +
                    _triggerOffPercent * 100 + "%";
            return s;
        }
    }
}
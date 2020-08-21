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

using System;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.Transportation;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements {
    public class CalcLocation : CalcBase {
        [ItemNotNull]
        [NotNull]
        private readonly List<ICalcAffordanceBase> _pureAffordances = new List<ICalcAffordanceBase>();
        [ItemNotNull]
        [NotNull]
        private readonly List<ICalcAffordanceBase> _siteAffordances = new List<ICalcAffordanceBase>();
        private bool _isTransportationEnabled;
        //public object Variables;

        public CalcSite? CalcSite { get; set; }
        public CalcLocation([NotNull] string pName, StrGuid guid) : base(pName, guid) => Devices = new List<CalcDevice>();

        [NotNull]
        [ItemNotNull]
        public List<ICalcAffordanceBase> Affordances {
            get {
                if (_isTransportationEnabled) {
                    return _siteAffordances;
                }

                return _pureAffordances;
            }
        }

        [NotNull]
        [ItemNotNull]
        public List<ICalcAffordanceBase> PureAffordances => _pureAffordances;

        [NotNull]
        [ItemNotNull]
        public List<CalcDevice> Devices { get; }

        [NotNull]
        [ItemNotNull]
        public List<CalcDevice> LightDevices { get; } = new List<CalcDevice>();

        public Dictionary<CalcPerson, ICalcAffordanceBase> IdleAffs {
            get;
        } = new Dictionary<CalcPerson, ICalcAffordanceBase>();

        public void AddAffordance([NotNull] CalcAffordance aff)
        {
            if(_isTransportationEnabled) {
                throw new LPGException("Error: tried to add an normal affordance after transportation was enabled.");
            }

            _pureAffordances.Add(aff);
        }

        public void AddTransportationAffordance([NotNull] AffordanceBaseTransportDecorator transportationAffordance)
        {
            _siteAffordances.Add(transportationAffordance);
            _isTransportationEnabled = true;
        }

        public void AddLightDevice([NotNull] CalcDevice device)
        {
            LightDevices.Add(device);
        }
        /*
        public List<CalcAffordance> GetAvailableActions(int time, NormalRandom nr, bool getEverything, Random r,
            bool onlyInterruppting, CalcLocation srcLocation)
        {
            var available = new List<CalcAffordance>(); // Affordances.Count

            if (getEverything) {
                available.AddRange(_affordances);
                return available;
            }
            foreach (var affordance in _affordances) {
                if (onlyInterruppting && !affordance.IsInterrupting) {
                    continue;
                }

            if (onlyInterruppting && !affordance.IsInterrupting) {
                    continue;
                }

            if (onlyInterruppting && !affordance.IsInterrupting) {
                    continue;
                }

                if (!affordance.IsBusy(time, nr, r, srcLocation)) {
                    available.Add(affordance);
                }
            }

            return available;
        }
        */
        public void SortAffordances()
        {
            Affordances.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
        }

        public override string ToString() => Name;
    }
}
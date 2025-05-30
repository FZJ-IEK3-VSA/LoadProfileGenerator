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

using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using CalculationEngine.Transportation;

namespace CalculationEngine.HouseholdElements
{
    public class CalcLocation(string pName, StrGuid guid) : CalcBase(pName, guid)
    {
        private readonly List<ICalcAffordanceBase> _pureAffordances = [];

        private readonly List<ICalcAffordanceBase> _siteAffordances = [];

        private bool _isTransportationEnabled;

        public CalcSite? CalcSite { get; set; }

        public IReadOnlyList<ICalcAffordanceBase> Affordances
        {
            get
            {
                if (_isTransportationEnabled)
                {
                    return _siteAffordances;
                }

                return _pureAffordances;
            }
        }

        public IReadOnlyList<ICalcAffordanceBase> PureAffordances => _pureAffordances;

        public List<CalcDevice> Devices { get; } = [];

        public List<CalcDevice> LightDevices { get; } = [];

        public Dictionary<CalcPerson, ICalcAffordanceBase> IdleAffs { get; } = [];

        public void AddAffordance(CalcAffordance aff)
        {
            if (_isTransportationEnabled)
            {
                throw new LPGException("Error: tried to add a normal affordance after transportation was enabled.");
            }

            _pureAffordances.Add(aff);
        }

        public void AddTransportationAffordance(AffordanceBaseTransportDecorator transportationAffordance)
        {
            _siteAffordances.Add(transportationAffordance);
            _isTransportationEnabled = true;
        }

        public void AddLightDevice(CalcDevice device)
        {
            LightDevices.Add(device);
        }

        public override string ToString() => Name;
    }
}
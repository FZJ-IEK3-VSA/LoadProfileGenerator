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

using System.Collections.Generic;
using Automation.ResultFiles;
using CalculationEngine.Helper;
using CalculationEngine.OnlineDeviceLogging;
using Common;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements {
    public class CalcAutoDev : CalcDevice {
        [NotNull]
        private readonly CalcLocation _calcLocation;
        [NotNull]
        private readonly CalcProfile _calcProfile;
        private readonly double _multiplier;
        //private readonly double _setVariableValue;
        private readonly double _timeStandardDeviation;
        [ItemNotNull]
        [NotNull]
        private readonly List<VariableRequirement> _requirements = new List<VariableRequirement>();
        [NotNull] private TimeStep _earliestNextStart;

        public CalcAutoDev([NotNull] string pName, [NotNull] CalcProfile pCalcProfile, [NotNull] CalcLoadType loadType,
            [NotNull][ItemNotNull] List<CalcDeviceLoad> loads, double timeStandardDeviation, [NotNull] string deviceCategoryGuid,
            [NotNull] IOnlineDeviceActivationProcessor odap, [NotNull] HouseholdKey householdKey, double multiplier,
            [NotNull] CalcLocation calclocation,
            [NotNull] string deviceCategory, [NotNull] CalcParameters calcParameters, [NotNull] string guid, [NotNull][ItemNotNull] List<VariableRequirement> requirements)
            : base(
                pName,  loads, deviceCategoryGuid, odap, calclocation, householdKey,
                OefcDeviceType.AutonomousDevice, deviceCategory, " (autonomous)",calcParameters, guid)
        {
            _earliestNextStart = new TimeStep(-1,0,true);
            _calcProfile = pCalcProfile;
            LoadType = loadType;
            _timeStandardDeviation = timeStandardDeviation;
            _multiplier = multiplier;
            _calcLocation = calclocation;
            if (requirements.Count > 0) // initialize the whole variable thing
            {
                _requirements.AddRange(requirements);
            }
        }

        [NotNull]
        public CalcProfile CalcProfile => _calcProfile;

        [NotNull]
        public CalcLoadType LoadType { get; }

        [NotNull]
        public string Location => _calcLocation.Name;

        public void Activate([NotNull] TimeStep time, [NotNull] NormalRandom nr)
        {
            if (time < _earliestNextStart)
            {
                return;
            }
            var shouldExecute = AreVariableRequirementsMet();
            //double check because first check needs to be if a variable is even set.
            if (shouldExecute)
            {
                var timefactor = nr.NextDouble(1, _timeStandardDeviation);
                while (timefactor < 0 || timefactor > 5)
                {
                    timefactor = nr.NextDouble(1, _timeStandardDeviation);
                }

                CalcProfile adjustedProfile = _calcProfile.CompressExpandDoubleArray(timefactor);
                _earliestNextStart = SetTimeprofile(adjustedProfile, time, LoadType,
                    "(autonomous)", "(autonomous)", _multiplier, true);
            }
        }

        private bool AreVariableRequirementsMet()
        {
            if (_requirements.Count > 0)
            {
                foreach (VariableRequirement requirement in _requirements)
                {
                    if (!requirement.IsMet()) {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
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
using Common;
using Common.CalcDto;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements {
    public class CalcAutoDevProfile {
        public CalcAutoDevProfile(CalcProfile profile, CalcLoadType loadType, double multiplier)
        {
            Profile = profile;
            LoadType = loadType;
            Multiplier = multiplier;
        }

        public CalcProfile Profile { get; }
        public CalcLoadType LoadType { get; }
        public double Multiplier { get; }
    }

    public class CalcAutoDev : CalcDevice {
        [NotNull]
        private readonly CalcLocation _calcLocation;
        //private readonly double _setVariableValue;
        [NotNull] private readonly List<CalcAutoDevProfile> _calcProfiles;
        private readonly double _timeStandardDeviation;
        [ItemNotNull]
        [NotNull]
        private readonly List<VariableRequirement> _requirements = new List<VariableRequirement>();
        [NotNull] private TimeStep _earliestNextStart;

        public CalcAutoDev( [NotNull]List<CalcAutoDevProfile> pCalcProfiles,
            [NotNull][ItemNotNull] List<CalcDeviceLoad> loads, double timeStandardDeviation,
            [NotNull] CalcLocation calclocation,
                           [NotNull][ItemNotNull] List<VariableRequirement> requirements, [NotNull] CalcDeviceDto autoDevDto, [NotNull] CalcRepo calcRepo)
            : base(
                  loads, calclocation,    autoDevDto, calcRepo)
        {
            _earliestNextStart = new TimeStep(-1,0,true);
            _calcProfiles = pCalcProfiles;
            _timeStandardDeviation = timeStandardDeviation;
            _calcLocation = calclocation;
            if (requirements.Count > 0) // initialize the whole variable thing
            {
                _requirements.AddRange(requirements);
            }
        }

        [NotNull]
        public string Location => _calcLocation.Name;

        public void Activate([NotNull] TimeStep time)
        {
            if (time < _earliestNextStart)
            {
                return;
            }
            var shouldExecute = AreVariableRequirementsMet();
            //double check because first check needs to be if a variable is even set.
            if (shouldExecute)
            {
                var timefactor = CalcRepo.NormalRandom.NextDouble(1, _timeStandardDeviation);
                while (timefactor < 0 || timefactor > 5)
                {
                    timefactor = CalcRepo.NormalRandom.NextDouble(1, _timeStandardDeviation);
                }

                foreach (var cprof in _calcProfiles) {
                    CalcProfile adjustedProfile = cprof.Profile.CompressExpandDoubleArray(timefactor);
                    _earliestNextStart = SetTimeprofile(adjustedProfile, time, cprof.LoadType,
                        "(autonomous)", "(autonomous)", cprof.Multiplier, true, out var _);
                }
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

        public bool IsAutodevBusyDuringTimespan(TimeStep timestep, int duration, int timefactor)
        {

            foreach (var autoDevLoad in _calcProfiles) {
                if (IsBusyDuringTimespan(timestep, duration, timefactor, autoDevLoad.LoadType)) {
                    return true;
                }
            }
            return false;
        }
    }
}
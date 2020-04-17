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
using Automation.ResultFiles;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationEngine.HouseElements {
    public class CalcSpaceHeating : CalcDevice {
        [NotNull] private readonly Dictionary<Tuple<int, int, int>, CalcDegreeDay> _calcDegreeDays;

        [NotNull] private readonly CalcParameters _calcParameters;

        public CalcSpaceHeating(
                                [NotNull] [ItemNotNull] List<CalcDeviceLoad> powerUsage,
                                [NotNull] IOnlineDeviceActivationProcessor odap,
                                [NotNull] Dictionary<Tuple<int, int, int>, CalcDegreeDay> calcDegreeDays,
                                [NotNull] CalcLocation cloc,
                                [NotNull] CalcParameters calcParameters,
                                 [NotNull] CalcDeviceDto deviceDto) : base(
            powerUsage,
            odap,
            cloc,
            calcParameters,
            deviceDto)
        {
            if (powerUsage.Count != 1) {
                throw new LPGException("there should be exactly one loadtype for space heating, not more or less.");
            }

            _calcDegreeDays = calcDegreeDays;
            _calcParameters = calcParameters;
        }

        [NotNull]
        public Dictionary<Tuple<int, int, int>, CalcDegreeDay> CalcDegreeDays => _calcDegreeDays;

        public void Activate([NotNull] TimeStep time, DateTime dateTime)
        {
            var oneDay = new TimeSpan(24, 0, 0);
            var numberOfValuesInOneDay = (int)(oneDay.TotalMilliseconds / _calcParameters.InternalStepsize.TotalMilliseconds);
            var heatingProfile = new List<double>(new double[numberOfValuesInOneDay]);
            var cdd = _calcDegreeDays[new Tuple<int, int, int>(dateTime.Year, dateTime.Month, dateTime.Day)];
            var timeUnitValue = cdd.HeatingAmount / numberOfValuesInOneDay / PowerUsage[0].LoadType.ConversionFactor;
            for (var i = 0; i < heatingProfile.Count; i++) {
                heatingProfile[i] = timeUnitValue;
            }

            var cp = new CalcProfile("Heating profile - " + time,
                System.Guid.NewGuid().ToString(),
                heatingProfile,
                ProfileType.Relative,
                "Calculated from Heating Degree Days");
            SetAllLoadTypesToTimeprofile(cp, time, "Space Heating", "Space Heater", 1);
        }
    }

    public class CalcDegreeDay {
        public int Day { get; set; }
        public double HeatingAmount { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }
    }
}
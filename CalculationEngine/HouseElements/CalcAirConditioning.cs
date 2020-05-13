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
using Common;
using Common.CalcDto;
using JetBrains.Annotations;

namespace CalculationEngine.HouseElements {
    public class CalcAirConditioning : CalcDevice {
        [NotNull] private readonly Dictionary<Tuple<int, int, int, int>, CalcDegreeHour> _calcDegreeHours;

        public CalcAirConditioning(
                                   [NotNull] [ItemNotNull] List<CalcDeviceLoad> powerUsage,
                                   [NotNull] Dictionary<Tuple<int, int, int, int>, CalcDegreeHour> calcDegreeHours,
                                   [NotNull] CalcLocation cloc,
                                   [NotNull] CalcDeviceDto calcDeviceDto, [NotNull] CalcRepo calcRepo)
            : base(
                 powerUsage,  cloc,
                  calcDeviceDto, calcRepo)
        {
            if (powerUsage.Count != 1) {
                throw new LPGException("there should be exactly one loadtype for air conditioning, not more or less.");
            }

            _calcDegreeHours = calcDegreeHours;
        }

        public void Activate([NotNull] TimeStep time, DateTime dateTime)
        {
            var oneHour = new TimeSpan(1, 0, 0);
            var numberOfValuesInOneHour =
                (int)(oneHour.TotalMilliseconds / CalcRepo.CalcParameters.InternalStepsize.TotalMilliseconds);
            var coolingProfile = new List<double>(new double[numberOfValuesInOneHour]);
            var cdd =
                _calcDegreeHours[
                    new Tuple<int, int, int, int>(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour)];
            var timeunitvalue = cdd.CoolingAmount / numberOfValuesInOneHour;
            for (var i = 0; i < coolingProfile.Count; i++) {
                coolingProfile[i] = timeunitvalue;
            }

            var cp = new CalcProfile("Cooling profile - " + time, System.Guid.NewGuid().ToStrGuid(), coolingProfile,
                ProfileType.Relative, "Calculated from Cooling Hours");
            SetAllLoadTypesToTimeprofile(cp, time, "Air Conditioning", "Air Conditioner", 1);
        }
    }

    public class CalcDegreeHour {
        public CalcDegreeHour(int year, int month, int day, int hour, double coolingAmount)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            CoolingAmount = coolingAmount;
        }

        public double CoolingAmount { get; }

        public int Day { get; }
        public int Hour { get; }
        public int Month { get; }
        public int Year { get; }

        [NotNull]
        public override string ToString()
        {
            var dt = new DateTime(Year, Month, Day, Hour, 0, 0);
            var s = dt.ToShortDateString() + " " + dt.ToShortTimeString() + " " + CoolingAmount;
            return s;
        }
    }
}
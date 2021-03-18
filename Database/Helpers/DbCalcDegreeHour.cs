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
using Common;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Helpers {
    public class DbCalcDegreeHour {
        private readonly double _averageTemperature;
        private readonly DateTime _date;

        private DbCalcDegreeHour(DateTime date, double averageTemperature)
        {
            _date = date;
            _averageTemperature = averageTemperature;
        }

        public double AverageTemperature => _averageTemperature;

        public double CoolingAmount { get; private set; }

        public DateTime Date => _date;

        public double Percentage { get; private set; }

        private int Year => _date.Year;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public static List<DbCalcDegreeHour> GetCalcDegreeHours([JetBrains.Annotations.NotNull] TemperatureProfile temperatureProfile, DateTime startTime,
            DateTime endTime, double coolingTemp, double yearlycoolingAmount, bool adjustYearlyEnergy,
            double referenceCoolingHours)
        {
            var years = new List<int>();
            var startyear = startTime.Year;
            years.Add(startyear);
            while (startyear < endTime.Year) {
                startyear++;
                years.Add(startyear);
            }
            var firstoffirstyear = new DateTime(startTime.Year, 1, 1);

            var lastoflastyear = new DateTime(endTime.Year, 12, 31);
            Logger.Info(firstoffirstyear + " " + lastoflastyear);
            if (temperatureProfile == null) {
                throw new DataIntegrityException("There was no temperature profile set!");
            }
            var temperatureValues = temperatureProfile.GetTemperatureArray(firstoffirstyear,
                lastoflastyear.AddDays(1), new TimeSpan(1, 0, 0));
            var curdate = firstoffirstyear;
            // degree days anlegen
            var degreeHoursByHour = new Dictionary<DateTime, DbCalcDegreeHour>();
            var i = 0;
            while (curdate <= lastoflastyear) {
                var dd = new DbCalcDegreeHour(curdate, temperatureValues[i]);
                i++;
                degreeHoursByHour.Add(curdate, dd);
                curdate = curdate.AddHours(1);
            }
            // calculate average temperature
            foreach (var myyear in years) {
                // calculate degree day sum
                double degreeHourSum = 0;
                foreach (var hour in degreeHoursByHour.Values) {
                    if (hour.Year == myyear) {
                        degreeHourSum += hour.GetDegreeDay(coolingTemp);
                    }
                }
                var correctYearlyCoolingAmount = yearlycoolingAmount;
                if (adjustYearlyEnergy) {
                    correctYearlyCoolingAmount = yearlycoolingAmount * degreeHourSum / referenceCoolingHours;
                }
                // spread the sum over the year
                foreach (var hour in degreeHoursByHour.Values) {
                    if (hour.Year == myyear) {
                        hour.Percentage = hour.GetDegreeDay(coolingTemp) / degreeHourSum;
                        hour.CoolingAmount = hour.Percentage * correctYearlyCoolingAmount;
                    }
                }
            }
            var degreeHours = new List<DbCalcDegreeHour>();
            foreach (var degreeHour in degreeHoursByHour.Values) {
                if (degreeHour.Date >= startTime && degreeHour.Date <= endTime) {
                    degreeHours.Add(degreeHour);
                }
            }
            return degreeHours;
        }

        public double GetDegreeDay(double coolingTemperature)
        {
            if (_averageTemperature < coolingTemperature) {
                return 0;
            }
            var degreeHourvalue = _averageTemperature - coolingTemperature;
            return degreeHourvalue;
        }

        [JetBrains.Annotations.NotNull]
        public override string ToString() => _date.ToShortDateString() + " " + _date.ToShortTimeString() + ": " +
                                             _averageTemperature;
    }
}
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
using Common;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Helpers {
    public static class MakeDegreeDaysClass {
        [ItemNotNull]
        [NotNull]
        public static List<DegreeDay> MakeDegreeDays([NotNull] TemperatureProfile temperatureProfile, DateTime startTime,
            DateTime endTime, double heatingTemp, double roomTemperature, double yearlyHeatingAmount, bool adjustValue,
            double referenceValue) {
            var years = new List<int>();
            var startyear = startTime.Year;
            years.Add(startyear);
            while (startyear < endTime.Year) {
                startyear++;
                years.Add(startyear);
            }
            var firstoffirstyear = new DateTime(startTime.Year, 1, 1);

            var lastoflastyear = new DateTime(endTime.Year, 12, 31);
            if (temperatureProfile == null) {
                throw new DataIntegrityException("There was no temperature profile set!");
            }
            var temperatureValues = temperatureProfile.GetTemperatureArray(firstoffirstyear,
                lastoflastyear.AddDays(1), new TimeSpan(1, 0, 0));
            var curdate = firstoffirstyear;
            // degree days anlegen
            var degreedaysByDay = new Dictionary<DateTime, DegreeDay>();
            while (curdate <= lastoflastyear) {
                var dd = new DegreeDay(curdate);

                degreedaysByDay.Add(curdate, dd);
                curdate = curdate.AddDays(1);
            }
            // calculate average temperature
            curdate = firstoffirstyear;
            for (var i = 0; i < temperatureValues.Length; i++) {
                var currentDay = new DateTime(curdate.Year, curdate.Month, curdate.Day);

                var dd = degreedaysByDay[currentDay];
                if(dd == null) {
                    throw new LPGException("dd was null");
                }
                dd.TempSum += temperatureValues[i];
                dd.Tempcounter += 1;
                curdate = curdate.AddHours(1);
            }
            foreach (var day in degreedaysByDay.Values) {
                if (day.Tempcounter == 0) {
                    throw new LPGException("No temperature values for this day:" + day.Date);
                }
                day.AverageTemperature = day.TempSum / day.Tempcounter;
            }
            foreach (var myyear in years) {
                // calculate degree day sum
                double degreedaysum = 0;
                foreach (var day in degreedaysByDay.Values) {
                    if (day.Year == myyear) {
                        degreedaysum += day.GetDegreeDay(heatingTemp, roomTemperature);
                    }
                }
                var yearlyAmountToUse = yearlyHeatingAmount;
                if (adjustValue) {
                    yearlyAmountToUse = yearlyHeatingAmount * degreedaysum / referenceValue;
                }
                // spread the sum over the year
                foreach (var day in degreedaysByDay.Values) {
                    if (day.Year == myyear) {
                        day.Percentage = day.GetDegreeDay(heatingTemp, roomTemperature) / degreedaysum;
                        day.HeatingAmount = day.Percentage * yearlyAmountToUse;
                    }
                }
            }
            var degreeDays = new List<DegreeDay>();
            foreach (var degreeday in degreedaysByDay.Values) {
                if (degreeday.Date >= startTime && degreeday.Date <= endTime) {
                    degreeDays.Add(degreeday);
                }
            }
            return degreeDays;
        }
    }
}
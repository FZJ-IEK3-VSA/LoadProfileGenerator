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

#region

using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

#endregion

namespace Database.Helpers {
    public class SunriseTimes {
        [NotNull] private readonly GeographicLocation _geoloc;
        [NotNull] private readonly Dictionary<DateTime, DateTime> _sunriseTimes;
        [NotNull] private readonly Dictionary<DateTime, DateTime> _sunsetTimes;

        public SunriseTimes([NotNull] GeographicLocation geoloc) {
            _geoloc = geoloc;
            _sunriseTimes = new Dictionary<DateTime, DateTime>();
            _sunsetTimes = new Dictionary<DateTime, DateTime>();
        }

        [ItemNotNull]
        [NotNull]
        public BitArray MakeArray(int timesteps, DateTime startTime, DateTime endTime, TimeSpan stepSize) {
            Logger.Info("Starting to calculate daylight hours...");
            MakeLists(startTime, endTime);

            var isDaylight = new BitArray(timesteps);
            var starttime = startTime;
            var endtime = endTime;
            var stepsize = stepSize;
            var step = 0;
            while (starttime < endtime) {
                var day2Check = new DateTime(starttime.Year, starttime.Month, starttime.Day);
                var sunrise = _sunriseTimes[day2Check];
                var sunset = _sunsetTimes[day2Check];
                if (starttime < sunrise || starttime > sunset) {
                    isDaylight[step] = false;
                }
                else {
                    isDaylight[step] = true;
                }
                step++;
                starttime += stepsize;
            }
            Logger.Info("Finished calculating daylight hours...");
            return isDaylight;
        }

        private void MakeLists(DateTime startTime, DateTime endTime) {
            //if (_geoloc != null) {
                var lat = new CalcSunriseTimes.LatitudeCoords(_geoloc.LatHour, _geoloc.LatMinute, _geoloc.LatSecond,
                    _geoloc.LatDirectionEnum);
                var lon = new CalcSunriseTimes.LongitudeCoords(_geoloc.LongHour, _geoloc.LongMinute, _geoloc.LongSecond,
                    _geoloc.LongDirectionEnum);
            //}
            var sunrisetime = DateTime.Now;
            var sunsettime = DateTime.Now;
            var issunrise = false;
            var issunset = false;
            var day = startTime;
            var enddate = endTime;
            day = new DateTime(day.Year, day.Month, day.Day);
            while (day <= enddate) {
                CalcSunriseTimes.Instance.CalculateSunriseSetTimes(lat, lon, day, ref sunrisetime, ref sunsettime,
                    ref issunrise, ref issunset);
                _sunriseTimes.Add(day, sunrisetime);
                _sunsetTimes.Add(day, sunsettime);
                day = day.AddDays(1);
            }
        }
    }
}
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
using System.Diagnostics.CodeAnalysis;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;

#endregion

namespace Database.Helpers {
//////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  C# Singleton class and thread-safe class for calculating Sunrise and Sunset times.
//
// The algorithm was adapted from the JavaScript sample provided here:
//      http://home.att.net/~srschmitt/script_sun_rise_set.html
//
//  NOTICE: this code is provided "as-is", without any warrenty, obligations or liability for it.
//          You may use this code freely for any use.
//
//  Zacky Pickholz (zacky.pickholz@gmail.com)
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

    public sealed class CalcSunriseTimes {
        private const double Mk1 = 15 * PiBy180 * 1.0027379;
        private const double PiBy180 = Math.PI / 180;
        [NotNull] private readonly double[] _mDecensionArr = {0.0, 0.0, 0.0};
        [NotNull] private readonly object _mLock = new object();
        [NotNull] private readonly double[] _mRightAscentionArr = {0.0, 0.0, 0.0};
        [NotNull] private readonly int[] _mRiseTimeArr = {0, 0};
        [NotNull] private readonly int[] _mSetTimeArr = {0, 0};
        [NotNull] private readonly double[] _mSunPositionInSkyArr = {0.0, 0.0};
        [NotNull] private readonly double[] _mVHzArr = {0.0, 0.0, 0.0};

        /// <summary>
        ///     Calculate sunrise and sunset times. Returns false if time zone and longitude are incompatible.
        /// </summary>
        /// <param name="lat"> Latitude coordinates. </param>
        /// <param name="lon"> Longitude coordinates. </param>
        /// <param name="date"> Date for which to calculate. </param>
        /// <param name="riseTime"> Sunrise time (output) </param>
        /// <param name="setTime"> Sunset time (output) </param>
        /// <param name="isSunrise"> Whether or not the sun rises at that day </param>
        /// <param name="isSunset"> Whether or not the sun sets at that day </param>
        public void CalculateSunriseSetTimes([NotNull] LatitudeCoords lat, [NotNull] LongitudeCoords lon, DateTime date,
            ref DateTime riseTime, ref DateTime setTime, ref bool isSunrise, ref bool isSunset)
        {
            CalculateSunriseSetTimes(lat.ToDouble(), lon.ToDouble(), date, ref riseTime, ref setTime, ref isSunrise,
                ref isSunset);
        }

        /// <summary>
        ///     Calculate sunrise and sunset times. Returns false if time zone and longitude are incompatible.
        /// </summary>
        /// <param name="lat"> Latitude in decimal notation. </param>
        /// <param name="myLon"> Longitude in decimal notation. </param>
        /// <param name="date"> Date for which to calculate. </param>
        /// <param name="riseTime"> Sunrise time (output) </param>
        /// <param name="setTime"> Sunset time (output) </param>
        /// <param name="isSunrise"> Whether or not the sun rises at that day </param>
        /// <param name="isSunset"> Whether or not the sun sets at that day </param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void CalculateSunriseSetTimes(double lat, double myLon, DateTime date, ref DateTime riseTime,
            ref DateTime setTime, ref bool isSunrise, ref bool isSunset)
        {
            var lon = myLon;
            lock (_mLock) // lock for thread safety
            {
                double zone = -(int) Math.Round(TimeZoneInfo.Local.GetUtcOffset(date).TotalSeconds / 3600);
                var jd = GetJulianDay(date) - 2451545; // Julian day relative to Jan 1.5, 2000

                if (Sign(zone) == Sign(lon) && Math.Abs(zone) > 0.0000001) {
                    Logger.Error("WARNING: time zone and longitude are incompatible!");
                    throw new LPGException("The time zone set in Windows is not compatible with the time zone of the geographic location." +
                                           "Yes, this is stupid, but it's a bug in the library used to calculate the sunrise times.");
                }

                lon /= 360;
                var tz = zone / 24;
                var ct = jd / 36525 + 1; // centuries since 1900.0
                var t0 = LocalSiderealTimeForTimeZone(lon, jd, tz); // local sidereal time

                // get sun position at start of day
                jd += tz;
                CalculateSunPosition(jd, ct);
                var ra0 = _mSunPositionInSkyArr[0];
                var dec0 = _mSunPositionInSkyArr[1];

                // get sun position at end of day
                jd += 1;
                CalculateSunPosition(jd, ct);
                var ra1 = _mSunPositionInSkyArr[0];
                var dec1 = _mSunPositionInSkyArr[1];

                // make continuous
                if (ra1 < ra0) {
                    ra1 += 2 * Math.PI;
                }

                // initialize
                _mIsSunrise = false;
                _mIsSunset = false;

                _mRightAscentionArr[0] = ra0;
                _mDecensionArr[0] = dec0;

                // check each hour of this day
                for (var k = 0; k < 24; k++) {
                    _mRightAscentionArr[2] = ra0 + (k + 1) * (ra1 - ra0) / 24;
                    _mDecensionArr[2] = dec0 + (k + 1) * (dec1 - dec0) / 24;
                    _mVHzArr[2] = TestHour(k, t0, lat);

                    // advance to next hour
                    _mRightAscentionArr[0] = _mRightAscentionArr[2];
                    _mDecensionArr[0] = _mDecensionArr[2];
                    _mVHzArr[0] = _mVHzArr[2];
                }
                try {
                    riseTime = new DateTime(date.Year, date.Month, date.Day, _mRiseTimeArr[0], _mRiseTimeArr[1], 0);
                    setTime = new DateTime(date.Year, date.Month, date.Day, _mSetTimeArr[0], _mSetTimeArr[1], 0);
                }
                catch (Exception e) {
                    Logger.Error(
                        "No daylight calculation for this geographic location was possible. Maybe the coordinates are wrong? The calculated sunrise time was " +
                        _mRiseTimeArr[0] + ":" + _mRiseTimeArr[1] + ", the exact error message was " + e.Message);
                    Logger.Exception(e);
                }
                isSunset = true;
                isSunrise = true;

                // neither sunrise nor sunset
                if (!_mIsSunrise && !_mIsSunset) {
                    if (_mVHzArr[2] < 0) {
                        isSunrise = false; // Sun down all day
                    }
                    else {
                        isSunset = false; // Sun up all day
                    }
                }
                // sunrise or sunset
                else {
                    if (!_mIsSunrise) // No sunrise this date
                    {
                        isSunrise = false;
                    }
                    else if (!_mIsSunset) // No sunset this date
                    {
                        isSunset = false;
                    }
                }
            }
        }

        #region Nested type: Coords

        public abstract class Coords {
            private int _degrees;
            private int _minutes;
            private int _seconds;

            protected void SetDegrees(int value)
            {
                _degrees = value;
            }

            protected void SetMinutes(int value)
            {
                _minutes = value;
            }

            protected void SetSeconds(int value)
            {
                _seconds = value;
            }

            protected abstract int Sign();

            public double ToDouble() => Sign() * (_degrees + (double) _minutes / 60 + (double) _seconds / 3600);
        }

        #endregion

        #region Nested type: LatitudeCoords

        public class LatitudeCoords : Coords {
            #region Direction enum

            public enum Direction {
                North,
                South
            }

            #endregion

            private readonly Direction _direction;

            public LatitudeCoords(int degrees, int minutes, int seconds, Direction direction)
            {
                SetDegrees(degrees);
                SetMinutes(minutes);
                SetSeconds(seconds);
                _direction = direction;
            }

            protected override int Sign() => _direction == Direction.North ? 1 : -1;
        }

        #endregion

        #region Nested type: LongitudeCoords

        public class LongitudeCoords : Coords {
            #region Direction enum

            public enum Direction {
                East,
                West
            }

            #endregion

            private readonly Direction _direction;

            public LongitudeCoords(int degrees, int minutes, int seconds, Direction direction)
            {
                SetDegrees(degrees);
                SetMinutes(minutes);
                SetSeconds(seconds);
                _direction = direction;
            }

            protected override int Sign() => _direction == Direction.East ? 1 : -1;
        }

        #endregion

#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables
        private bool _mIsSunrise;
        private bool _mIsSunset;
#pragma warning restore S1450 // Private fields only used as local variables in methods should become local variables

        #region Singleton

        private CalcSunriseTimes()
        {
        }

        [NotNull]
        public static CalcSunriseTimes Instance { get; } = new CalcSunriseTimes();

        #endregion

        #region Private Methods

        // sun's position using fundamental arguments
        // (Van Flandern & Pulkkinen, 1979)
        private void CalculateSunPosition(double jd, double ct)
        {
            var lo = 0.779072 + 0.00273790931 * jd;
            lo -= Math.Floor(lo);
            lo = lo * 2 * Math.PI;

            var g = 0.993126 + 0.0027377785 * jd;
            g -= Math.Floor(g);
            g = g * 2 * Math.PI;

            var v = 0.39785 * Math.Sin(lo);
            v -= 0.01 * Math.Sin(lo - g);
            v += 0.00333 * Math.Sin(lo + g);
            v -= 0.00021 * ct * Math.Sin(lo);

            var u = 1 - 0.03349 * Math.Cos(g);
            u -= 0.00014 * Math.Cos(2 * lo);
            u += 0.00008 * Math.Cos(lo);

            var w = -0.0001 - 0.04129 * Math.Sin(2 * lo);
            w += 0.03211 * Math.Sin(g);
            w += 0.00104 * Math.Sin(2 * lo - g);
            w -= 0.00035 * Math.Sin(2 * lo + g);
            w -= 0.00008 * ct * Math.Sin(g);

            // compute sun's right ascension
            var s = w / Math.Sqrt(u - v * v);
            _mSunPositionInSkyArr[0] = lo + Math.Atan(s / Math.Sqrt(1 - s * s));

            // ...and declination
            s = v / Math.Sqrt(u);
            _mSunPositionInSkyArr[1] = Math.Atan(s / Math.Sqrt(1 - s * s));
        }

        private static double GetJulianDay(DateTime date)
        {
            var month = date.Month;
            var day = date.Day;
            var year = date.Year;

            var gregorian = year >= 1583;

            if (month == 1 || month == 2) {
                year -= 1;
                month += 12;
            }

            var a = Math.Floor((double) year / 100);
            double b;

            if (gregorian) {
                b = 2 - a + Math.Floor(a / 4);
            }
            else {
                b = 0.0;
            }

            var jd = Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + b - 1524.5;

            return jd;
        }

        private static double LocalSiderealTimeForTimeZone(double lon, double jd, double z)
        {
            var s = 24110.5 + 8640184.812999999 * jd / 36525 + 86636.6 * z + 86400 * lon;
            s /= 86400;
            s -= Math.Floor(s);
            return s * 360 * PiBy180;
        }

        private static int Sign(double value)
        {
            if (value > 0.0) {
                return 1;
            }
            if (value < 0.0) {
                return -1;
            }
            return 0;
        }

        // test an hour for an event
        private double TestHour(int k, double t0, double lat)
        {
            var ha = new double[3];

            ha[0] = t0 - _mRightAscentionArr[0] + k * Mk1;
            ha[2] = t0 - _mRightAscentionArr[2] + k * Mk1 + Mk1;

            ha[1] = (ha[2] + ha[0]) / 2; // hour angle at half hour
            _mDecensionArr[1] = (_mDecensionArr[2] + _mDecensionArr[0]) / 2; // declination at half hour

            var s = Math.Sin(lat * PiBy180);
            var c = Math.Cos(lat * PiBy180);
            var z = Math.Cos(90.833 * PiBy180);

            if (k <= 0) {
                _mVHzArr[0] = s * Math.Sin(_mDecensionArr[0]) + c * Math.Cos(_mDecensionArr[0]) * Math.Cos(ha[0]) - z;
            }

            _mVHzArr[2] = s * Math.Sin(_mDecensionArr[2]) + c * Math.Cos(_mDecensionArr[2]) * Math.Cos(ha[2]) - z;

            if (Sign(_mVHzArr[0]) == Sign(_mVHzArr[2])) {
                return _mVHzArr[2]; // no event this hour
            }

            _mVHzArr[1] = s * Math.Sin(_mDecensionArr[1]) + c * Math.Cos(_mDecensionArr[1]) * Math.Cos(ha[1]) - z;

            var a = 2 * _mVHzArr[0] - 4 * _mVHzArr[1] + 2 * _mVHzArr[2];
            var b = -3 * _mVHzArr[0] + 4 * _mVHzArr[1] - _mVHzArr[2];
            var d = b * b - 4 * a * _mVHzArr[0];

            if (d < 0) {
                return _mVHzArr[2]; // no event this hour
            }

            d = Math.Sqrt(d);
            var e = (-b + d) / (2 * a);

            if (e > 1 || e < 0) {
                e = (-b - d) / (2 * a);
            }

            var time = k + e + 1 / 120D;

            var hr = (int) Math.Floor(time);
            var min = (int) Math.Floor((time - hr) * 60);

            if (_mVHzArr[0] < 0 && _mVHzArr[2] > 0) {
                _mRiseTimeArr[0] = hr;
                _mRiseTimeArr[1] = min;
                _mIsSunrise = true;
            }

            if (_mVHzArr[0] > 0 && _mVHzArr[2] < 0) {
                _mSetTimeArr[0] = hr;
                _mSetTimeArr[1] = min;
                _mIsSunset = true;
            }

            return _mVHzArr[2];
        }

        #endregion  // Private Methods
    }
}
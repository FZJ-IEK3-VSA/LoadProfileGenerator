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
using Common.CalcDto;
using Common.JSON;
using JetBrains.Annotations;

namespace CalculationController.DtoFactories {
    public class VacationDtoFactory {
        [JetBrains.Annotations.NotNull]
        private readonly CalcParameters _calcParameters;
        [JetBrains.Annotations.NotNull]
        private readonly Random _rnd;

        public VacationDtoFactory([JetBrains.Annotations.NotNull] CalcParameters calcParameters, [JetBrains.Annotations.NotNull] Random rnd)
        {
            _calcParameters = calcParameters;
            _rnd = rnd;
        }

        [JetBrains.Annotations.NotNull]
        private readonly Dictionary<Tuple<DateTime, DateTime>, Tuple<DateTime, DateTime>> _startEndTimes =
            new Dictionary<Tuple<DateTime, DateTime>, Tuple<DateTime, DateTime>>();

        private static DateTime MapDateTime(int year, DateTime src) {
            var foundDate = false;
            var dt = src;
            var result = DateTime.MinValue;
            while (!foundDate) {
                try {
                    if (DateTime.DaysInMonth(year, dt.Month) >= dt.Day) {
                        result = new DateTime(year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                        foundDate = true;
                    }
                    else {
                        dt = dt.AddDays(-1);
                    }
                }
                // ReSharper disable once UncatchableException
                catch (ArgumentOutOfRangeException) {
                    Logger.Warning(
                        "An invalid date had been found while trying to map the dates to the current year:" + year +
                        " - " + dt.Month + " - " + dt.Day);
                    dt = dt.AddDays(-1);
                }
            }
            return result;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<DateSpan> GetDatesForOneVacation([JetBrains.Annotations.NotNull] VacationTimeframe hhvac) {
            List<DateSpan> dateSpans = new List<DateSpan>();
            if (!hhvac.MapToOtherYears) {
                dateSpans.Add(AdjustVacationDatesForMoreRealism(hhvac.StartDate, hhvac.EndDate));
                return dateSpans;
            }
            for (var year = _calcParameters.InternalStartTime.Year;
                year <= _calcParameters.InternalEndTime.Year;year++) {
                var startTime = MapDateTime(year, hhvac.StartDate);
                var endTime = MapDateTime(year, hhvac.EndDate);
                 dateSpans.Add(AdjustVacationDatesForMoreRealism(startTime, endTime));
            }
            return dateSpans;
        }

        /*
        // urlaub setzen
        private void SetOneVacation(CalcPerson cp, DateTime startdateVac, DateTime enddateVac, Random r) {
            var startendTuple = new Tuple<DateTime, DateTime>(startdateVac, enddateVac);
            var startdate = startdateVac;
            var enddate = enddateVac;
            cp.Vacations.Add(startendTuple);
            if (startdate.Hour == 0 && startdate.Minute == 0) {
                // random choice
                if (_startEndTimes.ContainsKey(startendTuple)) {
                    var startend = _startEndTimes[startendTuple];
                    startdate = startend.Item1;
                    enddate = startend.Item2;
                }
                else {
                    var starthour = r.Next(24);
                    var endhour = r.Next(24);
                    startdate = startdate.AddHours(starthour);
                    enddate = enddate.AddHours(endhour);
                    _startEndTimes.Add(startendTuple, new Tuple<DateTime, DateTime>(startdate, enddate));
                }
            }

            var startidx = InternalDateTimeForSteps.Count;
            for (var i = 0; i < InternalDateTimeForSteps.Count; i++) {
                if (InternalDateTimeForSteps[i] > startdate) {
                    startidx = i;
                    break;
                }
            }

            var endidx = 0;
            for (var i = 0; i < InternalDateTimeForSteps.Count; i++) {
                endidx = i;
                if (InternalDateTimeForSteps[i] > enddate) {
                    break;
                }
            }

            for (var i = startidx; i < endidx; i++) {
                cp.IsOnVacation[i] = true;
            }
        }*/

        // urlaub setzen
        [JetBrains.Annotations.NotNull]
        private DateSpan AdjustVacationDatesForMoreRealism( DateTime startdateVac, DateTime enddateVac)
        {
            var startendTuple = new Tuple<DateTime, DateTime>(startdateVac, enddateVac);
            var startdate = startdateVac;
            var enddate = enddateVac;
            //cp.Vacations.Add(startendTuple);
            if (startdate.Hour == 0 && startdate.Minute == 0)
            {
                // random choice
                if (_startEndTimes.ContainsKey(startendTuple))
                {
                    var startend = _startEndTimes[startendTuple];
                    startdate = startend.Item1;
                    enddate = startend.Item2;
                }
                else
                {
                    var starthour = _rnd.Next(24);
                    var endhour = _rnd.Next(24);
                    startdate = startdate.AddHours(starthour);
                    enddate = enddate.AddHours(endhour);
                    _startEndTimes.Add(startendTuple, new Tuple<DateTime, DateTime>(startdate, enddate));
                }
            }
            DateSpan ds = new DateSpan(startdate,enddate);
            return ds;
        }

        /*
        var startidx = InternalDateTimeForSteps.Count;
            for (var i = 0; i < InternalDateTimeForSteps.Count; i++)
            {
                if (InternalDateTimeForSteps[i] > startdate)
                {
                    startidx = i;
                    break;
                }
            }

            var endidx = 0;
            for (var i = 0; i < InternalDateTimeForSteps.Count; i++)
            {
                endidx = i;
                if (InternalDateTimeForSteps[i] > enddate)
                {
                    break;
                }
            }

            for (var i = startidx; i < endidx; i++)
            {
                cp.IsOnVacation[i] = true;
            }
         */

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<DateSpan> GetVacationSpans([JetBrains.Annotations.NotNull][ItemNotNull] List<VacationTimeframe> hhvac) {
            List<DateSpan> dateSpans = new List<DateSpan>();
            foreach (var timeframe in hhvac) {
                dateSpans.AddRange( GetDatesForOneVacation(timeframe));
            }
            return dateSpans;
        }
    }
}
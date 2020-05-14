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
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Enums;
using Common.Tests;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Database.Tests.Tables {
    [TestFixture]
    public class TimeLimitEntryTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void GetOneYearArrayTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(TimeLimit.TableName);
                db.ClearTable(TimeLimitEntry.TableName);
                var temperaturProfiles = db.LoadTemperatureProfiles();
                var timeLimits = new ObservableCollection<TimeLimit>();
                var dateBasedProfiles = db.LoadDateBasedProfiles();
                TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
                var geographicLocations = db.LoadGeographicLocations(out _,
                    timeLimits);
                Assert.AreEqual(0, timeLimits.Count);
                var dt = new TimeLimit("hey", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                dt.SaveToDB();

                var dtbe = dt.AddTimeLimitEntry(null, dateBasedProfiles);
                dtbe.AnyAll = AnyAllTimeLimitCondition.Any;
                var dtbe2 = dt.AddTimeLimitEntry(dtbe, dateBasedProfiles);
                dtbe2.RepeaterType = PermissionMode.EveryXDay;
                dtbe2.DailyDayCount = 1;
                dtbe2.StartTimeTimeSpan = new TimeSpan(6, 0, 0);
                dtbe2.EndTimeTimeSpan = new TimeSpan(12, 0, 0);
                var dtbe3 = dt.AddTimeLimitEntry(dtbe, dateBasedProfiles);
                dtbe3.DailyDayCount = 1;
                dtbe3.StartTimeTimeSpan = new TimeSpan(13, 0, 0);
                dtbe3.EndTimeTimeSpan = new TimeSpan(14, 0, 0);

                dtbe.Subentries.Add(dtbe2);
                dtbe.Subentries.Add(dtbe2);
                var r = new Random();
                var vac = new Vacation("vac", null, db.ConnectionString, 1, 99, CreationType.ManuallyCreated, Guid.NewGuid().ToStrGuid());
                vac.SaveToDB();
                vac.AddVacationTime(new DateTime(2014, 3, 1), new DateTime(2014, 5, 1), VacationType.GoAway);
                var timeframes = vac.VacationTimeframes();
                var br = dtbe.GetOneYearArray(new TimeSpan(1, 0, 0), new DateTime(2014, 1, 1), new DateTime(2014, 2, 1),
                    temperaturProfiles[0], geographicLocations[0], r, timeframes, "test", out _, 0, 0, 0, 0);
                for (var i = 0; i < 24; i++)
                {
                    Logger.Info(i + ":" + br[i]);
                }
                db.Cleanup();
            }
        }

        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void TimeLimitEntryTests1()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(TimeLimit.TableName);
                db.ClearTable(TimeLimitEntry.TableName);
                var temperaturProfiles = db.LoadTemperatureProfiles();
                var timeLimits = new ObservableCollection<TimeLimit>();
                var dateBasedProfiles = db.LoadDateBasedProfiles();
                TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
                var geographicLocations = db.LoadGeographicLocations(out var holidays,
                    timeLimits);
                Assert.AreEqual(0, timeLimits.Count);
                var startTime = new DateTime(1900, 1, 1, 6, 0, 0);
                var endTime = new DateTime(1900, 1, 1, 10, 0, 0);
                var dt = new TimeLimitEntry(null, -1, startTime, endTime, PermissionMode.EveryWorkday, 1, 1, true,
                    true, true, true, true, true, true, 1, 1, 1, 1, 1, -100, 100, true, false, null,
                    AnyAllTimeLimitCondition.Any, startTime, endTime, null, 0, 0, false, false, false, false, 5, false,
                    dateBasedProfiles, db.ConnectionString, 0,
                    0, 0, Guid.NewGuid().ToStrGuid());
                var stepsize = new TimeSpan(1, 0, 0);
                var arraystart = new DateTime(2013, 12, 28, 5, 0, 0);
                var arrayend = new DateTime(2014, 1, 3, 9, 0, 0);
                geographicLocations[0].AddHoliday(holidays[0]);
                geographicLocations[0].AddHoliday(holidays[1]);
                Assert.Greater(geographicLocations[0].Holidays.Count, 1);
                Logger.Info("Holidays:" + geographicLocations[0].Holidays.Count);
                var r = new Random();
                var timeframes = new List<VacationTimeframe>();
                var br = dt.GetOneYearArray(stepsize, arraystart, arrayend, temperaturProfiles[0],
                    geographicLocations[0], r, timeframes, "test", out _, 0, 0, 0, 0);
                var curr = arraystart;
                for (var i = 0; i < br.Length; i++)
                {
                    Logger.Info(curr.ToLongDateString() + " " + curr.ToShortTimeString() + " :" + br[i]);
                    curr = curr.Add(stepsize);
                }

                db.Cleanup();
            }
        }

        public TimeLimitEntryTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
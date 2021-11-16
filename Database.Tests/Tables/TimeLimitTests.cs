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
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Helpers;
using Database.Tables.BasicElements;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


#endregion

namespace Database.Tests.Tables {

    public class TimeLimitTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TimeLimitArrayTestDateProfileTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(TimeLimit.TableName);
                db.ClearTable(TimeLimitEntry.TableName);
                // test if night + day is always
                var dateBasedProfile = new DateBasedProfile("blub", "bla", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                dateBasedProfile.SaveToDB();
                dateBasedProfile.AddNewDatePoint(new DateTime(2014, 1, 1), 100);
                dateBasedProfile.AddNewDatePoint(new DateTime(2014, 1, 3), 0);
                dateBasedProfile.SaveToDB();
                var start = new DateTime(1900, 1, 1, 0, 0, 0);
                var end = new DateTime(1900, 1, 1, 1, 1, 1);
                var dt = new TimeLimit("blub", db.ConnectionString, Guid.NewGuid().ToStrGuid(), 1);
                var temp = new TemperatureProfile("blub", 1, "bla", string.Empty, Guid.NewGuid().ToStrGuid());
                var enddate = new DateTime(1900, 1, 2, 0, 0, 0);
                temp.AddTemperature(start, 0, 1, true, false);
                temp.AddTemperature(enddate, 10, 1, true, false);
                var dateBasedProfiles = new ObservableCollection<DateBasedProfile>();
                dt.AddTimeLimitEntry(null, start, end, PermissionMode.ControlledByDateProfile, 1, 1, true, true, true, true,
                    true, true, true, 1, 1, 1, 1, 1, 1, 1, true, false, AnyAllTimeLimitCondition.Any, start, end,
                    dateBasedProfile.ID, 150, 50, false, false, true, false, 5, false, dateBasedProfiles, 0, 0, 0);
                dateBasedProfiles.Add(dateBasedProfile);
                var geoloc = new GeographicLocation(db.ConnectionString, null, null, Guid.NewGuid().ToStrGuid());
                var r = new Random();
                var vacationTimeFrames = new List<VacationTimeframe>();
                var br = dt.TimeLimitEntries[0].GetOneYearHourArray(temp, geoloc, r, vacationTimeFrames, "test",
                    out _);

                for (var i = 0; i < br.Count; i++)
                {
                    if (i < 48)
                    {
                        (br[i]).Should().Be(true);
                    }
                    else
                    {
                        (br[i]).Should().Be(false);
                    }
                }
                db.Cleanup();
            }
        }

        /// <summary>
        /// Tests if the or-combination of a timelimit that needs light and one that needs darkness is always true
        /// </summary>
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TimeLimitArrayTestlightControlledTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(TimeLimit.TableName);
                db.ClearTable(TimeLimitEntry.TableName);
                // test if night + day is always
                var temp = new TemperatureProfile("blub", 1, "bla", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                var start = new DateTime(1900, 1, 1, 0, 0, 0);
                var end = new DateTime(1900, 1, 1, 1, 1, 1);
                temp.AddTemperature(start, 10, 1, true, false);
                temp.AddTemperature(end, 0, 1, true, false);
                var dt = new TimeLimit("blub", db.ConnectionString, Guid.NewGuid().ToStrGuid(), 1);
                var dateBasedProfiles = new ObservableCollection<DateBasedProfile>();
                var dtbe = dt.AddTimeLimitEntry(null, start, end, PermissionMode.LightControlled, 1, 1, true,
                    true, true, true, true, true, true, 1, 1, 1, 1, 1, 1, 1, true, false, AnyAllTimeLimitCondition.Any,
                    start, end, -1, 0, 0, false, false, true, false, 5, false, dateBasedProfiles, 0, 0, 0);
                dt.AddTimeLimitEntry(dtbe, start, end, PermissionMode.LightControlled, 1, 1, true, true, true, true, true,
                    true, true, 1, 1, 1, 1, 1, 1, 1, true, false, AnyAllTimeLimitCondition.Any, start, end, -1, 0, 0, false,
                    false, false, false, 5, false, dateBasedProfiles, 0, 0, 0);
                dt.AddTimeLimitEntry(dtbe, start, end, PermissionMode.LightControlled, 1, 1, true, true, true, true, true,
                    true, true, 1, 1, 1, 1, 1, 1, 1, false, true, AnyAllTimeLimitCondition.Any, start, end, -1, 0, 0, false,
                    false, false, false, 5, false, dateBasedProfiles, 0, 0, 0);
                // create an arbitrary radiation profile; the values should not matter as the two opposite timelimits should cancel themselves out anyways
                var radiationProfile = new DateBasedProfile("TestRadiationProfile", "Random solar radiation profile for testing", db.ConnectionString, Guid.NewGuid().ToStrGuid(), -1);
                radiationProfile.AddNewDatePoint(start, 0, false);
                radiationProfile.AddNewDatePoint(start + (end - start) / 2, 100, false);
                radiationProfile.AddNewDatePoint(end, 0, false);
                // create a new location with default radiation threshold of 50
                var geoloc = new GeographicLocation(db.ConnectionString, null, radiationProfile, Guid.NewGuid().ToStrGuid());
                var r = new Random();
                var vacations = new List<VacationTimeframe>();
                var br = dt.TimeLimitEntries[0].GetOneYearHourArray(temp, geoloc, r, vacations, "test", out _);
                var br2 = dt.TimeLimitEntries[1].GetOneYearHourArray(temp, geoloc, r, vacations, "test", out _);
                var brtotal = new BitArray(br);
                brtotal = brtotal.Or(br2);
                foreach (bool b in brtotal)
                {
                    (b).Should().Be(true);
                }
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TimeLimitArrayTestTemperatureControlledTest() {
            // test temperatures
            var temp = new TemperatureProfile("blub", 1, "bla", string.Empty, Guid.NewGuid().ToStrGuid());
            var start = new DateTime(1900, 1, 1, 0, 0, 0);
            var end = new DateTime(1900, 1, 1, 1, 1, 1);
            var enddate = new DateTime(1900, 1, 2, 0, 0, 0);
            temp.AddTemperature(start, 0, 1, true, false);
            temp.AddTemperature(enddate, 10, 1, true, false);
            var dt = new TimeLimit("blub", string.Empty, Guid.NewGuid().ToStrGuid(),1);
            var dateBasedProfiles = new ObservableCollection<DateBasedProfile>();
            dt.AddTimeLimitEntry(null, start, end, PermissionMode.Temperature, 1, 1, true, true, true, true, true, true,
                true, 1, 1, 1, 1, 1, -10, 5, true, false, AnyAllTimeLimitCondition.Any, start, end, -1, 0, 0, false,
                false, false, false, 5, false, dateBasedProfiles, 0, 0, 0, false);
            var geoloc = new GeographicLocation(string.Empty, null, null, Guid.NewGuid().ToStrGuid());
            var r = new Random();
            var hhVacations = new List<VacationTimeframe>();
            var br = dt.TimeLimitEntries[0].GetOneYearHourArray(temp, geoloc, r, hhVacations, "test",
                out _);
            for (var i = 0; i < 24; i++) {
                Logger.Info("hour: " + i + ":" + br[i]);
                (br[i]).Should().Be(true);
            }
            for (var i = 24; i < br.Length; i++) {
                if (i % 24 == 0) {
                    Logger.Info("hour: " + i + ":" + br[i]);
                }
                (br[i]).Should().Be(false);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TimeLimitBoolEntryLoadCreationAndSaveTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(TimeLimit.TableName);
                db.ClearTable(TimeLimitEntry.TableName);
                var timeLimits = new ObservableCollection<TimeLimit>();
                var dateBasedProfiles = db.LoadDateBasedProfiles();
                TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
                (timeLimits.Count).Should().Be(0);
                var dt = new TimeLimit("hey", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                dt.SaveToDB();
                TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
                (timeLimits.Count).Should().Be(1);
                var dtl = timeLimits[0];
                (dtl.Name).Should().Be("hey");
                dt.DeleteFromDB();
                timeLimits.Clear();
                TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
                (timeLimits.Count).Should().Be(0);
                var dt2 = new TimeLimit("hey2", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                dt2.SaveToDB();
                var dtbe = dt2.AddTimeLimitEntry(null, dateBasedProfiles);
                (dt2.RootEntry).Should().Be(dtbe);
                var dtbe2 = dt2.AddTimeLimitEntry(dtbe, dateBasedProfiles);
                (dt2.RootEntry?.Subentries.Count).Should().Be(1);
                dt2.SaveToDB();
                (dt2.TimeLimitEntries.Count).Should().Be(2);
                timeLimits.Clear();
                TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
                (timeLimits[0].TimeLimitEntries.Count).Should().Be(2);
                (timeLimits[0].RootEntry?.ToString()).Should().Be(dtbe.ToString());
                (timeLimits[0].RootEntry?.Subentries.Count).Should().Be(1);
                (timeLimits[0].RootEntry?.Subentries[0].ToString()).Should().Be(dtbe2.ToString());
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TimeLimitLoadCreationAndSaveTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(TimeLimit.TableName);
                db.ClearTable(TimeLimitEntry.TableName);
                var timeLimits = new ObservableCollection<TimeLimit>();
                var dateBasedProfiles = db.LoadDateBasedProfiles();
                TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
                (timeLimits.Count).Should().Be(0);
                var dt = new TimeLimit("hey", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                dt.SaveToDB();
                TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
                (timeLimits.Count).Should().Be(1);
                var dtl = timeLimits[0];
                (dtl.Name).Should().Be("hey");
                (dtl.TimeLimitEntries.Count).Should().Be(0);
                dt.DeleteFromDB();
                timeLimits.Clear();
                TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
                (timeLimits.Count).Should().Be(0);
                var dt2 = new TimeLimit("hey2", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                dt2.SaveToDB();
                var start = new DateTime(2013, 1, 1);
                var end = new DateTime(2013, 2, 1);
                dt2.AddTimeLimitEntry(null, start, end, PermissionMode.LightControlled, 1, 1, true, true, true, true, true,
                    true, true, 1, 1, 1, 1, 1, 1, 2, true, true, AnyAllTimeLimitCondition.Any, start, end, -1, 0, 0, false,
                    false, false, false, 5, false, dateBasedProfiles, 0, 0, 0);
                dt2.SaveToDB();
                (dt2.TimeLimitEntries.Count).Should().Be(1);
                timeLimits.Clear();
                TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
                (timeLimits[0].TimeLimitEntries.Count).Should().Be(1);
                db.Cleanup();
            }
        }

        public TimeLimitTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
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
using NUnit.Framework;

#endregion

namespace Database.Tests.Tables {
    [TestFixture]
    public class TimeLimitTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TimeLimitArrayTestDateProfileTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            db.ClearTable(TimeLimit.TableName);
            db.ClearTable(TimeLimitEntry.TableName);
            // test if night + day is always
            var dateBasedProfile = new DateBasedProfile("blub", "bla", db.ConnectionString, Guid.NewGuid().ToString());
            dateBasedProfile.SaveToDB();
            dateBasedProfile.AddNewDatePoint(new DateTime(2014, 1, 1), 100);
            dateBasedProfile.AddNewDatePoint(new DateTime(2014, 1, 3), 0);
            dateBasedProfile.SaveToDB();
            var start = new DateTime(1900, 1, 1, 0, 0, 0);
            var end = new DateTime(1900, 1, 1, 1, 1, 1);
            var dt = new TimeLimit("blub", db.ConnectionString, Guid.NewGuid().ToString(), 1);
            var temp = new TemperatureProfile("blub", 1, "bla", string.Empty, Guid.NewGuid().ToString());
            var enddate = new DateTime(1900, 1, 2, 0, 0, 0);
            temp.AddTemperature(start, 0, 1, true, false);
            temp.AddTemperature(enddate, 10, 1, true, false);
            var dateBasedProfiles = new ObservableCollection<DateBasedProfile>();
            dt.AddTimeLimitEntry(null, start, end, PermissionMode.ControlledByDateProfile, 1, 1, true, true, true, true,
                true, true, true, 1, 1, 1, 1, 1, 1, 1, true, false, AnyAllTimeLimitCondition.Any, start, end,
                dateBasedProfile.ID, 150, 50, false, false, true, false, 5, false, dateBasedProfiles, 0, 0, 0);
            dateBasedProfiles.Add(dateBasedProfile);
            var geoloc = new GeographicLocation(db.ConnectionString, null, Guid.NewGuid().ToString());
            var r = new Random();
            var vacationTimeFrames = new List<VacationTimeframe>();
            var br = dt.TimeLimitEntries[0].GetOneYearHourArray(temp, geoloc, r, vacationTimeFrames, "test",
                out _);

            for (var i = 0; i < br.Count; i++) {
                if (i < 48) {
                    Assert.AreEqual(true, br[i]);
                }
                else {
                    Assert.AreEqual(false, br[i]);
                }
            }
            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TimeLimitArrayTestlightControlledTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            db.ClearTable(TimeLimit.TableName);
            db.ClearTable(TimeLimitEntry.TableName);
            // test if night + day is always
            var temp = new TemperatureProfile("blub", 1, "bla", db.ConnectionString, Guid.NewGuid().ToString());
            var start = new DateTime(1900, 1, 1, 0, 0, 0);
            var end = new DateTime(1900, 1, 1, 1, 1, 1);
            temp.AddTemperature(start, 10, 1, true, false);
            temp.AddTemperature(end, 0, 1, true, false);
            var dt = new TimeLimit("blub", db.ConnectionString, Guid.NewGuid().ToString(),1);
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
            var geoloc = new GeographicLocation(db.ConnectionString, null, Guid.NewGuid().ToString());
            var r = new Random();
            var vacations = new List<VacationTimeframe>();
            var br = dt.TimeLimitEntries[0].GetOneYearHourArray(temp, geoloc, r, vacations, "test", out _);
            var br2 = dt.TimeLimitEntries[1].GetOneYearHourArray(temp, geoloc, r, vacations, "test", out _);
            var brtotal = new BitArray(br);
            brtotal = brtotal.Or(br2);
            foreach (bool b in brtotal) {
                Assert.AreEqual(true, b);
            }
            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TimeLimitArrayTestTemperatureControlledTest() {
            // test temperatures
            var temp = new TemperatureProfile("blub", 1, "bla", string.Empty, Guid.NewGuid().ToString());
            var start = new DateTime(1900, 1, 1, 0, 0, 0);
            var end = new DateTime(1900, 1, 1, 1, 1, 1);
            var enddate = new DateTime(1900, 1, 2, 0, 0, 0);
            temp.AddTemperature(start, 0, 1, true, false);
            temp.AddTemperature(enddate, 10, 1, true, false);
            var dt = new TimeLimit("blub", string.Empty, Guid.NewGuid().ToString(),1);
            var dateBasedProfiles = new ObservableCollection<DateBasedProfile>();
            dt.AddTimeLimitEntry(null, start, end, PermissionMode.Temperature, 1, 1, true, true, true, true, true, true,
                true, 1, 1, 1, 1, 1, -10, 5, true, false, AnyAllTimeLimitCondition.Any, start, end, -1, 0, 0, false,
                false, false, false, 5, false, dateBasedProfiles, 0, 0, 0, false);
            var geoloc = new GeographicLocation(string.Empty, null, Guid.NewGuid().ToString());
            var r = new Random();
            var hhVacations = new List<VacationTimeframe>();
            var br = dt.TimeLimitEntries[0].GetOneYearHourArray(temp, geoloc, r, hhVacations, "test",
                out _);
            for (var i = 0; i < 24; i++) {
                Logger.Info("hour: " + i + ":" + br[i]);
                Assert.AreEqual(true, br[i]);
            }
            for (var i = 24; i < br.Length; i++) {
                if (i % 24 == 0) {
                    Logger.Info("hour: " + i + ":" + br[i]);
                }
                Assert.AreEqual(false, br[i]);
            }
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TimeLimitBoolEntryLoadCreationAndSaveTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            db.ClearTable(TimeLimit.TableName);
            db.ClearTable(TimeLimitEntry.TableName);
            var timeLimits = new ObservableCollection<TimeLimit>();
            var dateBasedProfiles = db.LoadDateBasedProfiles();
            TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
            Assert.AreEqual(0, timeLimits.Count);
            var dt = new TimeLimit("hey", db.ConnectionString, Guid.NewGuid().ToString());
            dt.SaveToDB();
            TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
            Assert.AreEqual(1, timeLimits.Count);
            var dtl = timeLimits[0];
            Assert.AreEqual("hey", dtl.Name);
            dt.DeleteFromDB();
            timeLimits.Clear();
            TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
            Assert.AreEqual(0, timeLimits.Count);
            var dt2 = new TimeLimit("hey2", db.ConnectionString, Guid.NewGuid().ToString());
            dt2.SaveToDB();
            var dtbe = dt2.AddTimeLimitEntry(null, dateBasedProfiles);
            Assert.AreEqual(dtbe, dt2.RootEntry);
            var dtbe2 = dt2.AddTimeLimitEntry(dtbe, dateBasedProfiles);
            Assert.AreEqual(1, dt2.RootEntry?.Subentries.Count);
            dt2.SaveToDB();
            Assert.AreEqual(2,  dt2.TimeLimitEntries.Count);
            timeLimits.Clear();
            TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
            Assert.AreEqual(2, timeLimits[0].TimeLimitEntries.Count);
            Assert.AreEqual(dtbe.ToString(), timeLimits[0].RootEntry?.ToString());
            Assert.AreEqual(1, timeLimits[0].RootEntry?.Subentries.Count);
            Assert.AreEqual(dtbe2.ToString(), timeLimits[0].RootEntry?.Subentries[0].ToString());
            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TimeLimitLoadCreationAndSaveTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            db.ClearTable(TimeLimit.TableName);
            db.ClearTable(TimeLimitEntry.TableName);
            var timeLimits = new ObservableCollection<TimeLimit>();
            var dateBasedProfiles = db.LoadDateBasedProfiles();
            TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
            Assert.AreEqual(0, timeLimits.Count);
            var dt = new TimeLimit("hey", db.ConnectionString, Guid.NewGuid().ToString());
            dt.SaveToDB();
            TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
            Assert.AreEqual(1, timeLimits.Count);
            var dtl = timeLimits[0];
            Assert.AreEqual("hey", dtl.Name);
            Assert.AreEqual(0, dtl.TimeLimitEntries.Count);
            dt.DeleteFromDB();
            timeLimits.Clear();
            TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
            Assert.AreEqual(0, timeLimits.Count);
            var dt2 = new TimeLimit("hey2", db.ConnectionString, Guid.NewGuid().ToString());
            dt2.SaveToDB();
            var start = new DateTime(2013, 1, 1);
            var end = new DateTime(2013, 2, 1);
            dt2.AddTimeLimitEntry(null, start, end, PermissionMode.LightControlled, 1, 1, true, true, true, true, true,
                true, true, 1, 1, 1, 1, 1, 1, 2, true, true, AnyAllTimeLimitCondition.Any, start, end, -1, 0, 0, false,
                false, false, false, 5, false, dateBasedProfiles, 0, 0, 0);
            dt2.SaveToDB();
            Assert.AreEqual(1, dt2.TimeLimitEntries.Count);
            timeLimits.Clear();
            TimeLimit.LoadFromDatabase(timeLimits, dateBasedProfiles, db.ConnectionString, false);
            Assert.AreEqual(1, timeLimits[0].TimeLimitEntries.Count);
            db.Cleanup();
        }
    }
}
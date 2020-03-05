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
using System.Collections.ObjectModel;
using System.Windows.Media;
using Common;
using Common.Enums;
using Common.Tests;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using NUnit.Framework;

namespace Database.Tests.Tables {
    [TestFixture]
    public class AffordanceDeviceTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void LoadFromDatabaseTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            var affdev = new ObservableCollection<AffordanceDevice>();
            var deviceCategories = new ObservableCollection<DeviceCategory>();
            var realDevices = new ObservableCollection<RealDevice>();
            var timeBasedProfiles = new ObservableCollection<TimeBasedProfile>();
            var affordances = new ObservableCollection<Affordance>();
            var dateBasedProfiles = db.LoadDateBasedProfiles();
            var timeLimits = db.LoadTimeLimits(dateBasedProfiles);
            var loadtypes = db.LoadLoadTypes();
            var deviceActionGroups = db.LoadDeviceActionGroups();
            var deviceActions = new ObservableCollection<DeviceAction>();
            db.ClearTable(AffordanceDevice.TableName);
            AffordanceDevice.LoadFromDatabase(affdev, db.ConnectionString, deviceCategories, realDevices,
                timeBasedProfiles, affordances, loadtypes, deviceActions, deviceActionGroups, false);
            Assert.AreEqual(0, affdev.Count);
            var rd = new RealDevice("blub", 1, "1", null, "name", true, false, db.ConnectionString, Guid.NewGuid().ToString());
            rd.SaveToDB();
            realDevices.Add(rd);
            var tp = new TimeBasedProfile("blub", null, db.ConnectionString, TimeProfileType.Relative,
                "fake", Guid.NewGuid().ToString());
            tp.SaveToDB();
            timeBasedProfiles.Add(tp);
            var aff = new Affordance("blub", tp, null, true, PermittedGender.All, 0, Color.FromRgb(255, 0, 0),
                "bla", timeLimits[0], string.Empty, db.ConnectionString, false, false, 0, 99, false,
                ActionAfterInterruption.GoBackToOld,false, Guid.NewGuid().ToString());
            aff.SaveToDB();
            affordances.Add(aff);
            var newaffdev = new AffordanceDevice(rd, tp, null, 0, aff.ID, realDevices, deviceCategories,
                "blub", loadtypes[0], db.ConnectionString, 1, Guid.NewGuid().ToString());
            newaffdev.SaveToDB();
            AffordanceDevice.LoadFromDatabase(affdev, db.ConnectionString, deviceCategories, realDevices,
                timeBasedProfiles, affordances, loadtypes, deviceActions, deviceActionGroups, false);
            Assert.AreEqual(1, affdev.Count);
            Assert.AreEqual(loadtypes[0], affdev[0].LoadType);
            db.Cleanup();
        }
    }
}
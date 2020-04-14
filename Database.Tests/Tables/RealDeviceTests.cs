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
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Helpers;
using Database.Tables.BasicHouseholds;
using NUnit.Framework;

#endregion

namespace Database.Tests.Tables {
    [TestFixture]
    public class RealDeviceTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RealDeviceLoadCreationAndSaveTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());

            var loadTypes = db.LoadLoadTypes();
            var alldevices = new ObservableCollection<RealDevice>();
            var deviceCategories = db.LoadDeviceCategories(alldevices, out var dcnone,
                false);
            var profiles = db.LoadTimeBasedProfiles();

            RealDevice.LoadFromDatabase(alldevices, deviceCategories, dcnone, db.ConnectionString, loadTypes, profiles,
                false);

            db.ClearTable(RealDevice.TableName);
            db.ClearTable(RealDeviceLoadType.TableName);
            alldevices.Clear();
            RealDevice.LoadFromDatabase(alldevices, deviceCategories, dcnone, db.ConnectionString, loadTypes, profiles,
                false);
            Assert.AreEqual(0, alldevices.Count);
            var rd = new RealDevice("bla", 3, "p1", null, "name", true, false, db.ConnectionString, Guid.NewGuid().ToString());
            rd.SaveToDB();
            alldevices.Clear();
            RealDevice.LoadFromDatabase(alldevices, deviceCategories, dcnone, db.ConnectionString, loadTypes, profiles,
                false);
            Assert.AreEqual(1, alldevices.Count);
            var rd2 = new RealDevice("bla2", 3, "p1", null, "name", true, false, db.ConnectionString, Guid.NewGuid().ToString());
            rd2.SaveToDB();
            alldevices.Clear();
            RealDevice.LoadFromDatabase(alldevices, deviceCategories, dcnone, db.ConnectionString, loadTypes, profiles,
                false);
            Assert.AreEqual(2, alldevices.Count);
            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RealDeviceTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());

            db.ClearTable(RealDevice.TableName);
            db.ClearTable(RealDeviceLoadType.TableName);
            var rdcat = new CategoryDBBase<RealDevice>("blub");
            var rd = rdcat.CreateNewItem(db.ConnectionString);
            var profiles = db.LoadTimeBasedProfiles();
            var alldevices = db.LoadRealDevices(out var deviceCategories, out var dcNone,
                out var loadTypes, profiles);
            Assert.AreEqual(1, alldevices.Count);
            alldevices.Clear();
            rdcat.DeleteItem(rd);
            RealDevice.LoadFromDatabase(alldevices, deviceCategories, dcNone, db.ConnectionString, loadTypes, profiles,
                false);
            Assert.AreEqual(0, alldevices.Count);
            db.Cleanup();
        }
    }
}
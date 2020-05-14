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
using Automation;
using Common;
using Common.Tests;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Database.Tests.Tables.ModularHouseholds {
    [TestFixture]
    public class DeviceSelectionTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void DeviceSelectionTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(DeviceSelection.TableName);
                db.ClearTable(DeviceSelectionItem.TableName);
                db.ClearTable(DeviceSelectionDeviceAction.TableName);
                var timeBasedProfiles = db.LoadTimeBasedProfiles();
                var devices = db.LoadRealDevices(out var deviceCategories, out _, out var loadTypes,
                    timeBasedProfiles);

                var deviceSelections = new ObservableCollection<DeviceSelection>();
                var groups = db.LoadDeviceActionGroups();
                var deviceActions = db.LoadDeviceActions(timeBasedProfiles, devices,
                    loadTypes, groups);
                DeviceSelection.LoadFromDatabase(deviceSelections, db.ConnectionString, deviceCategories, devices,
                    deviceActions, groups, false);

                var ds = new DeviceSelection("blub", null, "bla", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                ds.SaveToDB();
                ds.AddItem(deviceCategories[0], devices[0]);

                ds.AddAction(groups[0], deviceActions[0]);
                ds.SaveToDB();
                Assert.AreEqual(1, ds.Items.Count);
                Assert.AreEqual(1, ds.Actions.Count);

                // loading test
                deviceSelections = new ObservableCollection<DeviceSelection>();
                DeviceSelection.LoadFromDatabase(deviceSelections, db.ConnectionString, deviceCategories, devices,
                    deviceActions, groups, false);

                ds = deviceSelections[0];
                Assert.AreEqual(1, ds.Actions.Count);
                Assert.AreEqual(1, ds.Items.Count);
                ds.DeleteItemFromDB(ds.Items[0]);
                ds.DeleteActionFromDB(ds.Actions[0]);
                Assert.AreEqual(0, ds.Items.Count);
                Assert.AreEqual(0, ds.Actions.Count);

                // deleting and loading
                deviceSelections = new ObservableCollection<DeviceSelection>();
                DeviceSelection.LoadFromDatabase(deviceSelections, db.ConnectionString, deviceCategories, devices,
                    deviceActions, groups, false);
                Assert.AreEqual(0, ds.Items.Count);
                ds = deviceSelections[0];
                ds.DeleteFromDB();

                // deleting
                deviceSelections = new ObservableCollection<DeviceSelection>();
                DeviceSelection.LoadFromDatabase(deviceSelections, db.ConnectionString, deviceCategories, devices,
                    deviceActions, groups, false);
                Assert.AreEqual(0, deviceSelections.Count);
                db.Cleanup();
            }
        }

        public DeviceSelectionTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
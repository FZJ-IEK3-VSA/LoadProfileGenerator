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
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System.Collections.ObjectModel;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Tests;
using Database.Tables.BasicElements;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.BasicElements {

    public class DeviceTaggingSetTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void DeviceTaggingSetTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(DeviceTaggingSet.TableName);
                db.ClearTable(DeviceTaggingEntry.TableName);
                db.ClearTable(DeviceTag.TableName);
                db.ClearTable(DeviceTaggingReference.TableName);
                db.ClearTable(DeviceTaggingSetLoadType.TableName);
                var profiles = db.LoadTimeBasedProfiles();
                var realDevices = db.LoadRealDevices(out _, out var loadTypes,
                    profiles);

                var ats = new ObservableCollection<DeviceTaggingSet>();
                DeviceTaggingSet.LoadFromDatabase(ats, db.ConnectionString, false, realDevices, loadTypes);
                var ats1 = new DeviceTaggingSet("test", "desc", db.ConnectionString, System.Guid.NewGuid().ToStrGuid());
                ats1.SaveToDB();
                var tag = ats1.AddNewTag("newtag");
                ats1.SaveToDB();

                if (tag == null)
                {
                    throw new LPGException("Tag was null");
                }
                ats1.AddTaggingEntry(tag, realDevices[0]);
                ats1.SaveToDB();
                ats1.AddReferenceEntry(tag, 1, 10, loadTypes[0]);
                ats.Clear();
                DeviceTaggingSet.LoadFromDatabase(ats, db.ConnectionString, false, realDevices, loadTypes);
                ats[0].References.Count.Should().Be(1);
                ats[0].References[0].PersonCount.Should().Be(1);
                ats[0].References[0].ReferenceValue.Should().Be(10);
                ats1 = ats[0];
                ats1.DeleteTag(ats1.Tags[0]);
                ats1.DeleteFromDB();
                ats.Clear();
                DeviceTaggingSet.LoadFromDatabase(ats, db.ConnectionString, false, realDevices, loadTypes);
                ats.Count.Should().Be(0);
                db.Cleanup();
            }
            CleanTestBase.RunAutomatically(true);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void DeviceTaggingSetTestNone()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                db.Cleanup();
                foreach (var tagset in sim.DeviceTaggingSets.Items)
                {
                    foreach (var deviceTaggingEntry in tagset.Entries)
                    {
                        if (deviceTaggingEntry.Tag == null)
                        {
                            throw new LPGException("Tag was null");
                        }
                        if (deviceTaggingEntry.Tag.Name == "none")
                        {
                            throw new LPGException("None-Tag found in " + tagset.Name + " on " +
                                                   deviceTaggingEntry.Device?.Name);
                        }
                    }
                }
            }
        }

        public DeviceTaggingSetTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
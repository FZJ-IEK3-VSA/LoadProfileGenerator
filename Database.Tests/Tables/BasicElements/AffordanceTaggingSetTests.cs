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
using Common.Enums;
using Common.Tests;
using Database.Tables.BasicElements;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.BasicElements {

    public class AffordanceTaggingSetTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void AffordanceTaggingSetTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(AffordanceTaggingSet.TableName);
                db.ClearTable(AffordanceTaggingEntry.TableName);
                db.ClearTable(AffordanceTag.TableName);
                db.ClearTable(AffordanceTagReference.TableName);
                db.ClearTable(AffordanceTaggingSetLoadType.TableName);
                var profiles = db.LoadTimeBasedProfiles();
                var realDevices = db.LoadRealDevices(out var deviceCategories, out var loadTypes,
                    profiles);
                var timeBasedProfiles = db.LoadTimeBasedProfiles();
                var desires = db.LoadDesires();
                var dateBasedProfiles = db.LoadDateBasedProfiles();

                var timeLimits = db.LoadTimeLimits(dateBasedProfiles);
                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions = db.LoadDeviceActions(timeBasedProfiles, realDevices,
                    loadTypes, deviceActionGroups);
                var locations = db.LoadLocations(realDevices, deviceCategories, loadTypes);
                var variables = db.LoadVariables();
                var affordances = db.LoadAffordances(timeBasedProfiles, out _,
                    deviceCategories, realDevices, desires, loadTypes, timeLimits, deviceActions, deviceActionGroups,
                    locations, variables);
                var ats = new ObservableCollection<AffordanceTaggingSet>();
                AffordanceTaggingSet.LoadFromDatabase(ats, db.ConnectionString, false, affordances, loadTypes);
                var ats1 = new AffordanceTaggingSet("test", "desc", db.ConnectionString, true, System.Guid.NewGuid().ToStrGuid());
                ats1.SaveToDB();
                var tag = ats1.AddNewTag("newtag");
                if (tag == null)
                {
                    throw new LPGException("Tag was null");
                }
                ats1.SaveToDB();
                ats1.AddTaggingEntry(tag, affordances[0]);
                ats1.AddTagReference(tag, PermittedGender.Male, 1, 99, 0.15);
                ats1.SaveToDB();
                ats.Clear();
                AffordanceTaggingSet.LoadFromDatabase(ats, db.ConnectionString, false, affordances, loadTypes);
                ats1 = ats[0];
                ats1.DeleteTag(ats1.Tags[0]);
                ats1.DeleteFromDB();
                ats.Clear();
                AffordanceTaggingSet.LoadFromDatabase(ats, db.ConnectionString, false, affordances, loadTypes);
                ats.Count.Should().Be(0);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void AffordanceTaggingSetTestNone()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                foreach (var affordanceTaggingSet in sim.AffordanceTaggingSets.MyItems)
                {
                    foreach (var affordanceTaggingEntry in affordanceTaggingSet.Entries)
                    {
                        if (affordanceTaggingEntry.Tag == null)
                        {
                            throw new LPGException("Tag was null");
                        }
                        if (affordanceTaggingEntry.Tag.Name == "none")
                        {
                            throw new LPGException("None-Tag found in " + affordanceTaggingSet.Name + " for affordance " +
                                                   affordanceTaggingEntry.Affordance?.Name);
                        }
                    }
                }
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RemoveAllOldEntriesTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                foreach (var affordanceTaggingSet in sim.AffordanceTaggingSets.MyItems)
                {
                    Logger.Info(affordanceTaggingSet.Name);
                    if (affordanceTaggingSet.Name.ToUpperInvariant().Contains("PLANNING"))
                    {
                        affordanceTaggingSet.RemoveAllOldEntries(sim.Affordances.It);
                    }
                }
                db.Cleanup();
            }
        }

        public AffordanceTaggingSetTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
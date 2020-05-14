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
using System.Diagnostics.CodeAnalysis;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Common.Tests;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using FluentAssertions;
using FluentAssertions.Equivalency;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Database.Tests.Tables.ModularHouseholds {
    [TestFixture]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class HouseholdTraitTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void TraitImportExportTest()
        {
            //  WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            using (var db1 = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim1 = new Simulator(db1.ConnectionString);
                using (var db2 = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim2 = new Simulator(db2.ConnectionString);
                    //check if the basic thing is identical
                    /*sim1.HouseholdTraits[0].Should().BeEquivalentTo(sim2.HouseholdTraits[0], o => o.Excluding(
                        x => IsInvalidMember(x)
                        ).IgnoringCyclicReferences());*/


                    //check import
                    List<HouseholdTrait.JsonDto> dto1 = new List<HouseholdTrait.JsonDto>();
                    foreach (var trait in sim1.HouseholdTraits.It)
                    {
                        dto1.Add(trait.GetJson());
                    }

                    sim2.HouseholdTraits.DeleteItem(sim2.HouseholdTraits[0]);
                    var result = HouseholdTrait.ImportObjectFromJson(sim2, dto1);
                    if (sim1.HouseholdTraits.It.Count != sim2.HouseholdTraits.It.Count)
                    {
                        throw new LPGException("count not equal");
                    }
                    sim2.HouseholdTraits.It.Sort();
                    sim1.HouseholdTraits[0].Should().BeEquivalentTo(result[0], o => o.IgnoringCyclicReferences()
                        .Using<IRelevantGuidProvider>(x => x.Subject.RelevantGuid.Should().BeEquivalentTo(x.Expectation.RelevantGuid)).WhenTypeIs<IRelevantGuidProvider>()
                        .Excluding(x => IsInvalidMember(x)
                        ));
                    result.Should().HaveCount(1);
                    sim1.HouseholdTraits[0].Name.Should().BeEquivalentTo(sim2.HouseholdTraits[0].Name);
                    sim1.HouseholdTraits[0].Should().BeEquivalentTo(sim2.HouseholdTraits[0], o => o.IgnoringCyclicReferences()
                        .Using<IRelevantGuidProvider>(x => x.Subject.RelevantGuid.Should().BeEquivalentTo(x.Expectation.RelevantGuid)).WhenTypeIs<IRelevantGuidProvider>()
                        .Excluding(x => IsInvalidMember(x)
                    ));
                }
            }

        }

        private static bool IsInvalidMember([JetBrains.Annotations.NotNull] IMemberInfo x)
        {
            if(x.SelectedMemberPath.EndsWith("ConnectionString")) {
                return true;
            }
            if (x.SelectedMemberPath.Contains(".ParentCategory.")) {
                return true;
            }
            if (x.SelectedMemberPath.Contains(".DeleteThis")) {
                return true;
            }
            if (x.SelectedMemberPath.Contains(".ParentAffordance"))
            {
                return true;
            }
            if (x.SelectedMemberPath.EndsWith("ID"))
            {
                return true;
            }
            if (x.SelectedMemberPath.EndsWith(".CarpetPlotBrush"))
            {
                return true;
            }

            return false;
        }

        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseholdTraitImportTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                for (int i = 0; i < sim.HouseholdTraits.It.Count && i < 3; i++)
                {
                    Logger.Info("Importing trait from #" + i);
                    var newTrait = sim.HouseholdTraits.CreateNewItem(db.ConnectionString);
                    newTrait.ImportHouseholdTrait(sim.HouseholdTraits[i]);
                    newTrait.DeleteFromDB();
                }
                db.Cleanup();
            }
        }

        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseholdTraitAffordanceTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(HouseholdTrait.TableName);
                db.ClearTable(HHTAutonomousDevice.TableName);
                db.ClearTable(HHTDesire.TableName);
                db.ClearTable(HHTLocation.TableName);
                db.ClearTable(HHTAffordance.TableName);
                db.ClearTable(HHTTrait.TableName);
                db.ClearTable(HHTTag.TableName);
                var timeBasedProfiles = db.LoadTimeBasedProfiles();
                var dateBasedProfiles = db.LoadDateBasedProfiles();
                var devices = db.LoadRealDevices(out ObservableCollection<DeviceCategory> deviceCategories, out _, out var loadTypes,
                    timeBasedProfiles);

                var timeLimits = db.LoadTimeLimits(dateBasedProfiles);
                var desires = db.LoadDesires();
                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions = db.LoadDeviceActions(timeBasedProfiles, devices,
                    loadTypes, deviceActionGroups);
                var locations = db.LoadLocations(devices, deviceCategories, loadTypes);
                var variables = db.LoadVariables();
                var affordances = db.LoadAffordances(timeBasedProfiles, out _,
                    deviceCategories, devices, desires, loadTypes, timeLimits, deviceActions, deviceActionGroups, locations,
                    variables);
                var hht = new HouseholdTrait("blub", null, "blub", db.ConnectionString, "none", 1, 100, 10, 1, 1,
                    TimeType.Day, 1, 1, TimeType.Day, 1, 0, EstimateType.Theoretical, "", Guid.NewGuid().ToStrGuid());

                hht.SaveToDB();

                var loc = new Location("loc1", null, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                locations.Add(loc);
                loc.SaveToDB();
                Logger.Info("adding affordance");
                hht.AddLocation(loc);

                hht.AddAffordanceToLocation(loc, affordances[0], timeLimits[0], 100, 0, 0, 0, 0);
                hht.SaveToDB();
                Logger.Info("reloading ");
                var hhts = new ObservableCollection<HouseholdTrait>();
                var tags = db.LoadTraitTags();

                HouseholdTrait.LoadFromDatabase(hhts, db.ConnectionString, locations, affordances, devices,
                    deviceCategories,
                    timeBasedProfiles, loadTypes, timeLimits, desires, deviceActions, deviceActionGroups, tags, false,
                    variables);
                Assert.AreEqual(1, hhts.Count);
                var hht3 = hhts[0];
                Assert.AreEqual(1, hht3.Locations[0].AffordanceLocations.Count);
                Logger.Info("deleting affordance");
                hht3.DeleteAffordanceFromDB(hht3.Locations[0].AffordanceLocations[0]);

                hhts.Clear();
                Logger.Info("Loading again...");
                HouseholdTrait.LoadFromDatabase(hhts, db.ConnectionString, locations, affordances, devices,
                    deviceCategories,
                    timeBasedProfiles, loadTypes, timeLimits, desires, deviceActions, deviceActionGroups, tags, false,
                    variables);
                var hht4 = hhts[0];
                Assert.AreEqual(0, hht4.Locations[0].AffordanceLocations.Count);

                db.Cleanup();
            }
        }

        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseholdTraitTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(HouseholdTrait.TableName);
                db.ClearTable(HHTAutonomousDevice.TableName);
                db.ClearTable(HHTDesire.TableName);
                db.ClearTable(HHTLocation.TableName);
                db.ClearTable(HHTAffordance.TableName);
                db.ClearTable(HHTTag.TableName);
                var timeBasedProfiles = db.LoadTimeBasedProfiles();
                var dateBasedProfiles = db.LoadDateBasedProfiles();
                var devices = db.LoadRealDevices(out ObservableCollection<DeviceCategory> deviceCategories, out _, out ObservableCollection<VLoadType> loadTypes,
                    timeBasedProfiles);

                var timeLimits = db.LoadTimeLimits(dateBasedProfiles);
                var desires = db.LoadDesires();
                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions = db.LoadDeviceActions(timeBasedProfiles, devices,
                    loadTypes, deviceActionGroups);
                var locations = db.LoadLocations(devices, deviceCategories, loadTypes);
                var variables = db.LoadVariables();
                var affordances = db.LoadAffordances(timeBasedProfiles, out _,
                    deviceCategories, devices, desires, loadTypes, timeLimits, deviceActions, deviceActionGroups, locations,
                    variables);
                var hht = new HouseholdTrait("blub", null, "blub", db.ConnectionString, "none", 1, 100, 10, 1, 1,
                    TimeType.Day, 1, 1, TimeType.Day, 1, 0, EstimateType.Theoretical, "", Guid.NewGuid().ToStrGuid());
                hht.SaveToDB();
                var hht2 = new HouseholdTrait("blub2", null, "blub2", db.ConnectionString, "none", 1, 100, 10, 1,
                    1, TimeType.Day, 1, 1, TimeType.Day, 1, 0, EstimateType.Theoretical, "", Guid.NewGuid().ToStrGuid());
                hht2.SaveToDB();
                var loc = new Location("loc1", -1, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                hht.AddAutomousDevice(devices[0], timeBasedProfiles[0], 0, loadTypes[0], timeLimits[0], loc, 0,
                    VariableCondition.Equal, variables[0]);

                var hhtl = hht.AddLocation(locations[0]);
                hht.SaveToDB();
                var tag = new TraitTag("tag", db.ConnectionString,
                    TraitLimitType.NoLimit, TraitPriority.Mandatory, Guid.NewGuid().ToStrGuid());
                tag.SaveToDB();
                hht.AddTag(tag);
                hht.AddAffordanceToLocation(hhtl, affordances[0], timeLimits[0], 100, 0, 0, 0, 0);
                hht.SaveToDB();
                hht.AddDesire(desires[0], 1, "Healthy", 1, 1, 0, 100, PermittedGender.All);
                hht.SaveToDB();
                hht.AddTrait(hht2);
                hht.SaveToDB();
                Assert.AreEqual(1, hht.SubTraits.Count);
                var hhts = new ObservableCollection<HouseholdTrait>();
                var tags = db.LoadTraitTags();
                HouseholdTrait.LoadFromDatabase(hhts, db.ConnectionString, locations, affordances, devices,
                    deviceCategories,
                    timeBasedProfiles, loadTypes, timeLimits, desires, deviceActions, deviceActionGroups, tags, false,
                    variables);
                Assert.AreEqual(2, hhts.Count);
                var hht3 = hhts[0];
                Assert.AreEqual(1, hht3.MinimumPersonsInCHH);
                Assert.AreEqual(100, hht3.MaximumPersonsInCHH);
                Assert.AreEqual(1, hht3.Desires.Count);
                Assert.AreEqual(1, hht3.SubTraits.Count);
                Assert.AreEqual("none", hht3.Classification);
                Assert.AreEqual(1, hht3.Tags.Count);
                Assert.AreEqual("tag", hht3.Tags[0].Tag.Name);
                foreach (var trait in hhts)
                {
                    trait.DeleteFromDB();
                }
                hhts.Clear();
                Logger.Info("Loading again...");
                HouseholdTrait.LoadFromDatabase(hhts, db.ConnectionString, locations, affordances, devices,
                    deviceCategories,
                    timeBasedProfiles, loadTypes, timeLimits, desires, deviceActions, deviceActionGroups, tags, false,
                    variables);
                Assert.AreEqual(0, hhts.Count);
                db.Cleanup();
            }
        }

        public HouseholdTraitTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
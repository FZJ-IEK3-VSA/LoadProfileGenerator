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
using Common.Enums;
using Common.Tests;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using FluentAssertions;
using Newtonsoft.Json;

using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.ModularHouseholds {

    public class ModularHouseholdTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void ModularHouseholdJsonTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim = new Simulator(db.ConnectionString);
                    //make the first
                    var tt = sim.ModularHouseholds[0];
                    var jsonHH1 = tt.GetJson();
                    var jsonHH1String = JsonConvert.SerializeObject(jsonHH1, Formatting.Indented);
                    //make a new one, import the first one and compare
                    sim.ModularHouseholds.DeleteItem(tt);
                    var newhh = sim.ModularHouseholds.CreateNewItem(sim.ConnectionString);
                    newhh.ImportFromJsonTemplate(jsonHH1, sim);
                    var newJson = newhh.GetJson();
                    var s2 = JsonConvert.SerializeObject(newJson, Formatting.Indented);
                    (s2).Should().Be(jsonHH1String);

                    //
                    //modify the trait entry and make sure it is different
                    int prevcount = tt.Traits.Count;
                    int idx = 0;
                    while (jsonHH1.Traits.Count == prevcount)
                    {
                        //count up idx because the trait might already exist
                        newhh.AddTrait(sim.HouseholdTraits[idx++], ModularHouseholdTrait.ModularHouseholdTraitAssignType.Age, null);
                        prevcount++;
                    }

                    (newhh.Traits.Count).Should().Be(jsonHH1.Traits.Count + 1);
                    var newJson2 = newhh.GetJson();
                    var newJson2String = JsonConvert.SerializeObject(newJson2, Formatting.Indented);
                    jsonHH1String.Should().NotBe(newJson2String);

                    //import again
                    newhh.ImportFromJsonTemplate(jsonHH1, sim);
                    var newJson3 = newhh.GetJson();
                    var newJson3String = JsonConvert.SerializeObject(newJson3, Formatting.Indented);
                    (newJson3String).Should().Be(jsonHH1String);


                    //modify the persons and make sure it is different
                    newhh.AddPerson(sim.Persons[5], sim.LivingPatternTags[0]);
                    (newhh.Persons.Count).Should().Be(jsonHH1.Persons.Count + 1);
                    var newJson5 = newhh.GetJson();
                    var newJson5String = JsonConvert.SerializeObject(newJson5, Formatting.Indented);
                    jsonHH1String.Should().NotBe(newJson5String);

                    //import again
                    newhh.ImportFromJsonTemplate(jsonHH1, sim);
                    var newJson6 = newhh.GetJson();
                    var s6 = JsonConvert.SerializeObject(newJson6, Formatting.Indented);
                    (s6).Should().Be(jsonHH1String);


                    db.Cleanup();
                }
                wd.CleanUp();
            }
        }


        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void ModularHouseholdOldImportTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (var db1 = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim1A = new Simulator(db1.ConnectionString);
                    //make the first
                    var tt = sim1A.ModularHouseholds[0];
                    var jsonHH1 = tt.GetJson();
                    //make a new one, import the first one and compare
                    sim1A.ModularHouseholds.DeleteItem(tt);
                    // ReSharper disable once RedundantAssignment
                    sim1A = null;

                    var sim1B = new Simulator(db1.ConnectionString);
                    using (var db2 = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                    {
                        //var sim2 = new Simulator(db1.ConnectionString);
                        DatabaseMerger.DatabaseMerger dbm = new DatabaseMerger.DatabaseMerger(sim1B);
                        dbm.RunFindItems(db2.FileName, null);
                        dbm.RunImport(null);
                    }
                    // ReSharper disable once RedundantAssignment
                    sim1B = null;

                    var sim1C = new Simulator(db1.ConnectionString);
                    var tt2 = sim1C.ModularHouseholds[0];
                    var jsonHH2 = tt2.GetJson();
                    jsonHH2.Should().BeEquivalentTo(jsonHH1, o => o
                        .Excluding(x => x.Path.EndsWith("Guid")
                                        || x.Path.EndsWith("ID")));
                    db1.Cleanup();
                }
                wd.CleanUp(0, false);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SwapPersonsTestWithOther()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                ModularHousehold mhh = sim.ModularHouseholds.Items[0];
                ModularHouseholdPerson mhhPerson = mhh.Persons[0];
                Person dstPerson = sim.Persons.Items[10];
                LivingPatternTag dstTag = sim.LivingPatternTags.Items[10];
                int traitsBefore = mhh.Traits.Count;
                mhh.SwapPersons(mhhPerson, dstPerson, dstTag);
                int traitsAfter = mhh.Traits.Count;
                (traitsAfter).Should().Be(traitsBefore);

                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SwapTagTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                ModularHousehold mhh = sim.ModularHouseholds.Items[0];
                ModularHouseholdPerson mhhPerson = mhh.Persons[0];
                Person dstPerson = mhhPerson.Person;
                var dstTag = sim.LivingPatternTags.Items[10];
                int traitsBefore = mhh.Traits.Count;
                mhh.SwapPersons(mhhPerson, dstPerson, dstTag);
                int traitsAfter = mhh.Traits.Count;
                (traitsAfter).Should().Be(traitsBefore);

                db.Cleanup();
            }
        }


        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void ModularHouseholdJsonImportExporTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                ModularHousehold mhh = sim.ModularHouseholds.Items[0];
                var modjson = mhh.GetJson();
                var newhh = sim.ModularHouseholds.CreateNewItem(sim.ConnectionString);
                newhh.ImportFromJsonTemplate(modjson, sim);
                newhh.Should().BeEquivalentTo(mhh, options => options
                    .Excluding(x => x.Path.EndsWith(".Guid",StringComparison.OrdinalIgnoreCase) ||
                                    x.Path.EndsWith(".ID", StringComparison.OrdinalIgnoreCase) ||
                                    x.Path.EndsWith(".IntID", StringComparison.OrdinalIgnoreCase) ||
                                    x.Path.EndsWith(".ModularHouseholdID", StringComparison.OrdinalIgnoreCase)
                                    || x.Path.EndsWith(".PrettyName", StringComparison.OrdinalIgnoreCase)
                                    || x.Path.EndsWith(".HeaderString", StringComparison.OrdinalIgnoreCase)).Excluding(x => x.ID).Excluding(x => x.IntID));

                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void ModularHouseholdTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(ModularHousehold.TableName);
                db.ClearTable(ModularHouseholdTrait.TableName);
                db.ClearTable(ModularHouseholdPerson.TableName);
                var persons = new ObservableCollection<Person>();
                var result = new ObservableCollection<ModularHousehold>();
                var householdTraits = new ObservableCollection<HouseholdTrait>();
                var hht = new HouseholdTrait("blub", null, "blub", db.ConnectionString, "none", 1, 100, 10, 1, 1,
                    TimeType.Day, 1, 1, TimeType.Day, 1, 0, EstimateType.Theoretical, "", Guid.NewGuid().ToStrGuid());
                hht.SaveToDB();
                householdTraits.Add(hht);
                var deviceSelections = new ObservableCollection<DeviceSelection>();
                var ds = new DeviceSelection("ds", null, "bla", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                ds.SaveToDB();
                deviceSelections.Add(ds);
                var vacations = db.LoadVacations();
                var hhTags = db.LoadHouseholdTags();
                var traitTags = db.LoadTraitTags();
                var lpTags = db.LoadLivingPatternTags();
                ModularHousehold.LoadFromDatabase(result, db.ConnectionString, householdTraits, deviceSelections, false,
                    persons, vacations, hhTags, traitTags, lpTags);
                (result.Count).Should().Be(0);
                var chh = new ModularHousehold("blub", null, "blub", db.ConnectionString, ds, "src", null, null,
                    EnergyIntensityType.Random, CreationType.ManuallyCreated, Guid.NewGuid().ToStrGuid());
                chh.SaveToDB();
                chh.AddTrait(hht, ModularHouseholdTrait.ModularHouseholdTraitAssignType.Age, null);
                chh.SaveToDB();
                result.Clear();
                ModularHousehold.LoadFromDatabase(result, db.ConnectionString, householdTraits, deviceSelections, false,
                    persons, vacations, hhTags, traitTags, lpTags);
                (result.Count).Should().Be(1);
                (result[0].Traits.Count).Should().Be(1);
                db.Cleanup();
            }
        }

        public ModularHouseholdTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
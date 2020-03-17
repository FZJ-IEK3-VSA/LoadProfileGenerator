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
using NUnit.Framework;

namespace Database.Tests.Tables.ModularHouseholds {
    [TestFixture]
    public class ModularHouseholdTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void ModularHouseholdJsonTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
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
            Assert.AreEqual(jsonHH1String, s2);

            //
            //modify the trait entry and make sure it is different
            int prevcount = tt.Traits.Count;
            int idx = 0;
            while (jsonHH1.Traits.Count == prevcount)
            {
                //count up idx because the trait might already exist
                newhh.AddTrait(sim.HouseholdTraits[idx++],ModularHouseholdTrait.ModularHouseholdTraitAssignType.Age,null);
                prevcount++;
            }

            Assert.AreEqual(jsonHH1.Traits.Count + 1, newhh.Traits.Count);
            var newJson2 = newhh.GetJson();
            var newJson2String = JsonConvert.SerializeObject(newJson2, Formatting.Indented);
            Assert.AreNotEqual(jsonHH1String, newJson2String);

            //import again
            newhh.ImportFromJsonTemplate(jsonHH1, sim);
            var newJson3 = newhh.GetJson();
            var newJson3String = JsonConvert.SerializeObject(newJson3, Formatting.Indented);
            Assert.AreEqual(jsonHH1String, newJson3String);


            //modify the persons and make sure it is different
            newhh.AddPerson(sim.Persons[5], sim.TraitTags[0]);
            Assert.AreEqual(jsonHH1.Persons.Count + 1, newhh.Persons.Count);
            var newJson5 = newhh.GetJson();
            var newJson5String = JsonConvert.SerializeObject(newJson5, Formatting.Indented);
            Assert.AreNotEqual(jsonHH1String, newJson5String);

            //import again
            newhh.ImportFromJsonTemplate(jsonHH1, sim);
            var newJson6 = newhh.GetJson();
            var s6 = JsonConvert.SerializeObject(newJson6, Formatting.Indented);
            Assert.AreEqual(jsonHH1String, s6);


            db.Cleanup();
            wd.CleanUp();
        }


        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void ModularHouseholdOldImportTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var db1 = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            var sim1A = new Simulator(db1.ConnectionString);
            //make the first
            var tt = sim1A.ModularHouseholds[0];
            var jsonHH1 = tt.GetJson();
            //make a new one, import the first one and compare
            sim1A.ModularHouseholds.DeleteItem(tt);
            sim1A = null;

            var sim1B = new Simulator(db1.ConnectionString);
            var db2 = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            //var sim2 = new Simulator(db1.ConnectionString);
            DatabaseMerger.DatabaseMerger dbm = new DatabaseMerger.DatabaseMerger(sim1B);
            dbm.RunFindItems(db2.FileName,null);
            dbm.RunImport(null);
            sim1B = null;

            var sim1C = new Simulator(db1.ConnectionString);
            var tt2 = sim1C.ModularHouseholds[0];
            var jsonHH2 = tt2.GetJson();
            jsonHH2.Should().BeEquivalentTo(jsonHH1, o => o
                .Excluding(x => x.SelectedMemberPath.EndsWith("Guid")
                                || x.SelectedMemberPath.EndsWith("ID")));
            db1.Cleanup();
            wd.CleanUp(0,false);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void SwapPersonsTestWithOther()
        {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            Simulator sim = new Simulator(db.ConnectionString);
            ModularHousehold mhh = sim.ModularHouseholds.It[0];
            ModularHouseholdPerson mhhPerson = mhh.Persons[0];
            Person dstPerson = sim.Persons.It[10];
            TraitTag dstTag = sim.TraitTags.It[10];
            int traitsBefore = mhh.Traits.Count;
            mhh.SwapPersons(mhhPerson, dstPerson, dstTag);
            int traitsAfter = mhh.Traits.Count;
            Assert.AreEqual(traitsBefore, traitsAfter);

            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void SwapTagTest()
        {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            Simulator sim = new Simulator(db.ConnectionString);
            ModularHousehold mhh = sim.ModularHouseholds.It[0];
            ModularHouseholdPerson mhhPerson = mhh.Persons[0];
            Person dstPerson = mhhPerson.Person;
            TraitTag dstTag = sim.TraitTags.It[10];
            int traitsBefore = mhh.Traits.Count;
            mhh.SwapPersons(mhhPerson, dstPerson, dstTag);
            int traitsAfter = mhh.Traits.Count;
            Assert.AreEqual(traitsBefore, traitsAfter);

            db.Cleanup();
        }


        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void ModularHouseholdJsonImportExporTest()
        {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            Simulator sim = new Simulator(db.ConnectionString);
            ModularHousehold mhh = sim.ModularHouseholds.It[0];
            var modjson = mhh.GetJson();
            var newhh = sim.ModularHouseholds.CreateNewItem(sim.ConnectionString);
            newhh.ImportFromJsonTemplate(modjson,sim);
            newhh.Should().BeEquivalentTo(mhh, options => options
                .Excluding(x => x.SelectedMemberPath.EndsWith(".Guid") ||
                                x.SelectedMemberPath.EndsWith(".ID") ||
                                x.SelectedMemberPath.EndsWith(".IntID") ||
                                x.SelectedMemberPath.EndsWith(".ModularHouseholdID")
                                ||x.SelectedMemberPath.EndsWith(".PrettyName")
                                || x.SelectedMemberPath.EndsWith(".HeaderString")).Excluding(x=> x.ID).Excluding(x=> x.IntID));

            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void ModularHouseholdTest()
        {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            db.ClearTable(ModularHousehold.TableName);
            db.ClearTable(ModularHouseholdTrait.TableName);
            db.ClearTable(ModularHouseholdPerson.TableName);
            var persons = new ObservableCollection<Person>();
            var result = new ObservableCollection<ModularHousehold>();
            var householdTraits = new ObservableCollection<HouseholdTrait>();
            var hht = new HouseholdTrait("blub", null, "blub", db.ConnectionString, "none", 1, 100, 10, 1, 1,
                TimeType.Day, 1, 1, TimeType.Day, 1, 0, EstimateType.Theoretical, "", Guid.NewGuid().ToString());
            hht.SaveToDB();
            householdTraits.Add(hht);
            var deviceSelections = new ObservableCollection<DeviceSelection>();
            var ds = new DeviceSelection("ds", null, "bla", db.ConnectionString, Guid.NewGuid().ToString());
            ds.SaveToDB();
            deviceSelections.Add(ds);
            var vacations = db.LoadVacations();
            var hhTags = db.LoadHouseholdTags();
            var traitTags = db.LoadTraitTags();

            ModularHousehold.LoadFromDatabase(result, db.ConnectionString, householdTraits, deviceSelections, false,
                persons, vacations, hhTags, traitTags);
            Assert.AreEqual(0, result.Count);
            var chh = new ModularHousehold("blub", null, "blub", db.ConnectionString, ds, "src", null, null,
                EnergyIntensityType.Random, CreationType.ManuallyCreated, Guid.NewGuid().ToString());
            chh.SaveToDB();
            chh.AddTrait(hht, ModularHouseholdTrait.ModularHouseholdTraitAssignType.Age, null);
            chh.SaveToDB();
            result.Clear();
            ModularHousehold.LoadFromDatabase(result, db.ConnectionString, householdTraits, deviceSelections, false,
                persons, vacations, hhTags, traitTags);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].Traits.Count);
            db.Cleanup();
        }
    }
}
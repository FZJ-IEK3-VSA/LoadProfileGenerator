using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database.Helpers;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.ModularHouseholds {

    public class HouseholdTemplateTests : UnitTestBaseClass
    {

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseholdTemplateJsonTestWithFluentAssertion()
        {
            //  WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            using (var db1 = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim1 = new Simulator(db1.ConnectionString);
                using (var db2 = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim2 = new Simulator(db2.ConnectionString);
                    //check if the basic thing is identical
                    sim1.HouseholdTemplates[0].Should().BeEquivalentTo(sim2.HouseholdTemplates[0], o => o.Excluding(
                        x => x.SelectedMemberPath.EndsWith("ConnectionString", StringComparison.OrdinalIgnoreCase)));


                    //check import
                    List<HouseholdTemplate.JsonDto> dto1 = new List<HouseholdTemplate.JsonDto>();
                    foreach (var template in sim1.HouseholdTemplates.Items)
                    {
                        dto1.Add(template.GetJson());
                    }

                    sim2.HouseholdTemplates.DeleteItem(sim2.HouseholdTemplates[0]);
                    HouseholdTemplate.ImportObjectFromJson(sim2, dto1);
                    sim2.HouseholdTemplates.Items.Sort();
                    sim1.HouseholdTemplates[0].Should().BeEquivalentTo(sim2.HouseholdTemplates[0], o => o
                        .Using<IRelevantGuidProvider>(x => x.Subject.RelevantGuid.Should().BeEquivalentTo(x.Expectation.RelevantGuid)).WhenTypeIs<IRelevantGuidProvider>()
                                .Excluding(x => x.SelectedMemberPath.EndsWith("ConnectionString", StringComparison.OrdinalIgnoreCase)
                        || x.SelectedMemberPath.EndsWith("ID", StringComparison.OrdinalIgnoreCase)));
                }
            }
            //              ||regex2.IsMatch(x.SelectedMemberPath))

        }
        /*
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseholdTemplateJsonTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            var sim = new Simulator(db.ConnectionString);
            //make the first
            var tt = sim.HouseholdTemplates[0];
            var j1= tt.GetJson();
            var s1 = JsonConvert.SerializeObject(j1,Formatting.Indented);
            //make a new one, import the first one and compare
            sim.HouseholdTemplates.DeleteItem(tt);
            var newhht = sim.HouseholdTemplates.CreateNewItem(sim.ConnectionString);
            newhht.ImportFromJsonTemplate(j1,sim);
            var newJson = newhht.GetJson();
            var s2 = JsonConvert.SerializeObject(newJson, Formatting.Indented);
           // Logger.Info(s1);
            //Logger.Info(s2);
            Assert.AreEqual(s1,s2);

            //
            //modify the trait entry and make sure it is different
            int prevcount = tt.Entries.Count;
            int idx = 0;
            while (newhht.Entries.Count == prevcount) {
                List<Person> persons = new List<Person> {sim.Persons[0]};
                newhht.AddEntry( sim.TraitTags[idx],1,100,persons);
                prevcount++;
            }

            (newhht.Entries.Count).Should().Be(j1.TraitEntries.Count + 1);
            var newJson2 = newhht.GetJson();
            var s3 = JsonConvert.SerializeObject(newJson2, Formatting.Indented);
            Assert.AreNotEqual(s1, s3);

            //import again
            newhht.ImportFromJsonTemplate(j1, sim);
            var newJson3 = newhht.GetJson();
            var s4 = JsonConvert.SerializeObject(newJson3, Formatting.Indented);
            (s4).Should().Be(s1);


            //modify the persons and make sure it is different
            newhht.AddPerson(sim.Persons[5],sim.TraitTags[0]);
            (newhht.Persons.Count).Should().Be(j1.TemplatePersons.Count + 1);
            var newJson5 = newhht.GetJson();
            var s5 = JsonConvert.SerializeObject(newJson5, Formatting.Indented);
            Assert.AreNotEqual(s1, s5);

            //import again
            newhht.ImportFromJsonTemplate(j1, sim);
            var newJson6 = newhht.GetJson();
            var s6 = JsonConvert.SerializeObject(newJson6, Formatting.Indented);
            (s6);


            db.Cleanup().Should().Be(s1);
            wd.CleanUp();
        }*/



        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseholdTemplateGenerationTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var gen = sim.HouseholdTemplates.CreateNewItem(db.ConnectionString);
                gen.NewHHName = "hh";
                var entry = gen.AddEntry(sim.TraitTags[0], 5, 10);
                LivingPatternTag tt = sim.LivingPatternTags[0];
                gen.AddPerson(sim.Persons[0], tt);
                entry.AddPerson(sim.Persons[0]);

                gen.SaveToDB();
                gen.GenerateHouseholds(sim, true, new List<STTraitLimit>(), new List<TraitTag>());
                foreach (var household in gen.GeneratedHouseholds)
                {
                    Logger.Info(household.Name);
                    foreach (var trait in household.Traits)
                    {
                        Logger.Info("\t" + trait.HouseholdTrait.Name);
                    }
                }
                db.Cleanup();
            }
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void TestForbiddenTags()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                SimIntegrityChecker.Run(sim, CheckingOptions.Default());
                var gen = sim.HouseholdTemplates.CreateNewItem(db.ConnectionString);
                gen.NewHHName = "hh";
                gen.AddVacation(sim.Vacations[0]);
                var entry = gen.AddEntry(sim.TraitTags[0], 5, 10);
                LivingPatternTag tt = sim.LivingPatternTags[0];
                gen.AddPerson(sim.Persons[0], tt);
                entry.AddPerson(sim.Persons[0]);

                gen.SaveToDB();
                var tags = sim.TraitTags.Items.ToList();
                var newhhs = gen.GenerateHouseholds(sim, true, new List<STTraitLimit>(),tags );
                foreach (var household in gen.GeneratedHouseholds)
                {
                    Logger.Info(household.Name);
                    foreach (var trait in household.Traits)
                    {
                        Logger.Info("\t" + trait.HouseholdTrait.Name);
                    }
                }

                foreach (var household in newhhs) {
                    household.Traits.Count.Should().Be(0);
                }
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseholdTemplateAddTests()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var gen = sim.HouseholdTemplates.CreateNewItem(db.ConnectionString);
                gen.NewHHName = "hh";

                gen.AddVacation(sim.Vacations[0]);
                gen.AddVacationFromJson(sim.Vacations[0].GetJsonReference(), sim);

                gen.SaveToDB();
                gen.GenerateHouseholds(sim, true, new List<STTraitLimit>(), new List<TraitTag>());
                foreach (var household in gen.GeneratedHouseholds)
                {
                    Logger.Info(household.Name);
                    foreach (var trait in household.Traits)
                    {
                        Logger.Info("\t" + trait.HouseholdTrait.Name);
                    }
                }
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseholdTemplateTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(HouseholdTemplate.TableName);
                db.ClearTable(HHTemplateEntry.TableName);
                db.ClearTable(HHTemplateVacation.TableName);
                db.ClearTable(HHTemplateEntryPerson.TableName);
                db.ClearTable(HHTemplateTag.TableName);
                var sim = new Simulator(db.ConnectionString);
                var hhtemplate = sim.HouseholdTemplates.CreateNewItem(db.ConnectionString);
                hhtemplate.SaveToDB();
                hhtemplate.AddEntry(sim.TraitTags[0], 1, 100);
                hhtemplate.AddPerson(sim.Persons[0], sim.LivingPatternTags[0]);
                sim.HouseholdTags.CreateNewItem(sim.ConnectionString);
                hhtemplate.AddTemplateTag(sim.HouseholdTags[0]);
                (sim.HouseholdTemplates.Items.Count).Should().Be(1);
                (hhtemplate.Entries.Count).Should().Be(1);
                var sim2 = new Simulator(db.ConnectionString);
                (sim2.HouseholdTemplates.Items.Count).Should().Be(1);
                (sim2.HouseholdTemplates[0].Entries.Count).Should().Be(1);
                (sim2.HouseholdTemplates[0].Persons.Count).Should().Be(1);
                (sim2.HouseholdTemplates[0].TemplateTags.Count).Should().Be(1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseholdTemplateTest2()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(HouseholdTemplate.TableName);
                db.ClearTable(HHTemplateEntry.TableName);
                db.ClearTable(HHTemplateEntryPerson.TableName);
                var cat = new CategoryDBBase<HouseholdTemplate>("Household Generator");

                var timeBasedProfiles = db.LoadTimeBasedProfiles();
                var desires = db.LoadDesires();
                var persons = db.LoadPersons();

                var realDevices =
                    db.LoadRealDevices(out var deviceCategories, out _, out var loadTypes, timeBasedProfiles);
                var dateBasedProfiles = db.LoadDateBasedProfiles();
                var timeLimits = db.LoadTimeLimits(dateBasedProfiles);
                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions =
                    db.LoadDeviceActions(timeBasedProfiles, realDevices, loadTypes, deviceActionGroups);
                var locations = db.LoadLocations(realDevices, deviceCategories, loadTypes);
                var variables = db.LoadVariables();
                var affordances = db.LoadAffordances(timeBasedProfiles, out _,
                    deviceCategories, realDevices, desires, loadTypes, timeLimits, deviceActions, deviceActionGroups,
                    locations, variables);

                var traittags = db.LoadTraitTags();
                var lptags = db.LoadLivingPatternTags();
                var traits = db.LoadHouseholdTraits(locations, affordances, realDevices,
                    deviceCategories, timeBasedProfiles, loadTypes, timeLimits, desires, deviceActions, deviceActionGroups,
                    traittags, variables, lptags);
                var vacations = db.LoadVacations();
                var templateTags = db.LoadHouseholdTags();
                var datebasedProfiles = db.LoadDateBasedProfiles();
                HouseholdTemplate.LoadFromDatabase(cat.Items, db.ConnectionString, traits, false, persons, traittags,
                    vacations,
                    templateTags, datebasedProfiles, lptags);
                (cat.Items.Count).Should().Be(0);

                var gen = cat.CreateNewItem(db.ConnectionString);
                var entry = gen.AddEntry(traittags[0], 0, 10);
                entry.AddPerson(persons[0]);
                cat.SaveToDB();
                var generators = new ObservableCollection<HouseholdTemplate>();
                HouseholdTemplate.LoadFromDatabase(generators, db.ConnectionString, traits, false, persons, traittags,
                    vacations, templateTags, datebasedProfiles, lptags);
                (generators.Count).Should().Be(1);
                (generators[0].Entries.Count).Should().Be(1);
                (generators[0].Entries[0].Persons.Count).Should().Be(1);

                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void ImportExistingModularTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var newHouseholdTemplate = sim.HouseholdTemplates.CreateNewItem(db.ConnectionString);
                newHouseholdTemplate.NewHHName = "hh";
                var existingHouseholdTemplate =
                    sim.ModularHouseholds.Items.FirstOrDefault(x => x.Name.StartsWith("CHS01", StringComparison.Ordinal));
                if (existingHouseholdTemplate == null)
                {
                    throw new LPGException("Could not find chs01");
                }
                newHouseholdTemplate.ImportExistingModularHouseholds(existingHouseholdTemplate);

                newHouseholdTemplate.SaveToDB();
                newHouseholdTemplate.GenerateHouseholds(sim, true, new List<STTraitLimit>(), new List<TraitTag>());
                var total = 0;
                foreach (var entry in newHouseholdTemplate.Entries)
                {
                    Logger.Info(entry.PrettyString);
                    total += entry.TraitCountMax;
                }

                total.Should().BeGreaterOrEqualTo(existingHouseholdTemplate.Traits.Count);
                db.Cleanup();
            }
        }

        public HouseholdTemplateTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
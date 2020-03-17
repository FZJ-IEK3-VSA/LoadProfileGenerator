using System.Collections.Generic;
using Automation;
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database;
using Database.Tables.ModularHouseholds;
using Database.Tests;
using NUnit.Framework;

namespace LoadProfileGenerator.Tests {
    [TestFixture]
    public class TemplatePersonTesterTests : UnitTestBaseClass
    {
#pragma warning disable S125 // Sections of code should not be "commented out"
/*
        [Test]
        public void FillPersonDescriptions()
        {
            DatabaseSetup db = new DatabaseSetup();
            db.SetupDatabase("FillPersonDescriptions");
            Simulator sim = new Simulator(db.ConnectionString);
            sim.MyGeneralConfig.PerformCleanUpChecks = "False";
            PersonDescriptionFixer pdf = new PersonDescriptionFixer();
            pdf.FillPersonDescriptions(sim);
            db.Cleanup();
        }
        */
        /*
        [Test]
        public void RunTemplatePersonCreator()
        {
            DatabaseSetup db = new DatabaseSetup();
            db.SetupDatabase("TemplatePersonTesterTests");
            Simulator sim = new Simulator(db.ConnectionString);
            sim.MyGeneralConfig.PerformCleanUpChecks = "False";
            int missingTraits = 0;
            foreach (ModularHousehold chh in sim.ModularHouseholds.It)
            {
                if (chh.Name.StartsWith("O", StringComparison.Ordinal))
                    continue;
                foreach (ModularHouseholdPerson chhPerson in chh.Persons)
                {
                    Person p = chhPerson.Person;
                    ProgressLogger.Get().ReportInformation("Processing " + p.PrettyName);
                    TemplatePerson tp = sim.TemplatePersons.CreateNewItem(sim.ConnectionString);
                    tp.Name = p.Description;
                    tp.Age = p.Age;
                    tp.Gender = p.Gender;
                    tp.BasePerson = p;
                    tp.BaseHousehold = chh;
                    tp.Description = "Automatically created template person";
                    // collect all the traits to look at
                    List<ModularHouseholdTrait> modularHouseholdTraits =
                        chh.Traits.Where(
                            x =>
                                x.AssignType == ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name &&
                                x.DstPerson == p).ToList();
                    List<HouseholdTrait> personTraits = new List<HouseholdTrait>();
                    foreach (ModularHouseholdTrait modularHouseholdTrait in modularHouseholdTraits)
                    {
                        personTraits.AddRange(modularHouseholdTrait.HouseholdTrait.CollectRecursiveSubtraits());
                    }

                    // collect the web traits
                    List<TraitTag> tags =
                        sim.TraitTags.It.Where(x => x.TraitPriority != TraitPriority.ForExperts).ToList();
                    List<HouseholdTrait> webTraits =
                        sim.HouseholdTraits.It.Where(x => x.Tags.Any(y => tags.Contains(y.Tag))).ToList();


                    // try adding them
                    foreach (HouseholdTrait householdTrait in personTraits)
                    {
                        bool foundTrait = false;
                        foreach (HouseholdTrait webTrait in webTraits)
                        {
                            var subtraits = webTrait.CollectRecursiveSubtraits();
                            if (subtraits.Contains(householdTrait))
                            {
                                tp.AddTrait(webTrait);
                                foundTrait = true;
                                break;
                            }
                        }
                        if (!foundTrait)
                        {
                            ProgressLogger.Get().ReportInformation("For the trait " + householdTrait.PrettyName + " there was no webtrait");
                            missingTraits++;
                            if (missingTraits > 30)
                                return;
                        }
                    }
                    tp.SaveToDB();
                }
            }
        }*/

        [Test]
        [Category(UnitTestCategories.BasicTest)]
#pragma warning restore S125 // Sections of code should not be "commented out"
        public void RunTemplatePersonCreation() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.LoadProfileGenerator);
            var sim = new Simulator(db.ConnectionString) {MyGeneralConfig = {PerformCleanUpChecks = "False"}};
            SimIntegrityChecker.Run(sim);
            TemplatePersonCreator.CreateTemplatePersons(sim);
            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TemplatePersonFullCalculationTest() {
            //TODO: fix this test
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.LoadProfileGenerator);
            var sim = new Simulator(db.ConnectionString) {MyGeneralConfig = {PerformCleanUpChecks = "False"}};
            SimIntegrityChecker.Run(sim);
            var newchh = TemplatePersonCreator.RunCalculationTests(sim);
            foreach (var household in newchh) {
                var traits2Delete = new List<ModularHouseholdTrait>();
                foreach (var modularHouseholdTrait in household.Traits) {
                    if (modularHouseholdTrait.HouseholdTrait.MinimumPersonsInCHH > 1) {
                        traits2Delete.Add(modularHouseholdTrait);
                    }
                }
                foreach (var modularHouseholdTrait in traits2Delete) {
                    household.DeleteTraitFromDB(modularHouseholdTrait);
                }
            }

            SimIntegrityChecker.Run(sim);
            sim.MyGeneralConfig.StartDateUIString = "1.1.2015";
            sim.MyGeneralConfig.EndDateUIString = "5.01.2015";
            sim.MyGeneralConfig.InternalTimeResolution = "00:01:00";
            sim.MyGeneralConfig.RandomSeed = 5;
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.Reasonable);
            //ChartLocalizer.ShouldTranslate = false;
            //ConfigSetter.SetGlobalTimeParameters(sim.MyGeneralConfig);
            Assert.AreNotEqual(null, sim);
            db.Cleanup();
            wd.CleanUp();
        }
    }
}
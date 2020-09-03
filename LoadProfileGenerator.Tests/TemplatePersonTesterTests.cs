using System.Collections.Generic;
using Automation;
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database;
using Database.Tables.ModularHouseholds;
using Database.Tests;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace LoadProfileGenerator.Tests {
    public class TemplatePersonTesterTests : UnitTestBaseClass
    {
#pragma warning disable S125 // Sections of code should not be "commented out"
/*
        [Fact]
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
        [Fact]
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

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
#pragma warning restore S125 // Sections of code should not be "commented out"
        public void RunTemplatePersonCreation()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString) { MyGeneralConfig = { PerformCleanUpChecks = "False" } };
                SimIntegrityChecker.Run(sim, CheckingOptions.Default());
                TemplatePersonCreator.CreateTemplatePersons(sim);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TemplatePersonFullCalculationTest()
        {
            //TODO: fix this test
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim = new Simulator(db.ConnectionString) { MyGeneralConfig = { PerformCleanUpChecks = "False" } };
                    SimIntegrityChecker.Run(sim, CheckingOptions.Default());
                    var newchh = TemplatePersonCreator.RunCalculationTests(sim);
                    foreach (var household in newchh)
                    {
                        var traits2Delete = new List<ModularHouseholdTrait>();
                        foreach (var modularHouseholdTrait in household.Traits)
                        {
                            if (modularHouseholdTrait.HouseholdTrait.MinimumPersonsInCHH > 1)
                            {
                                traits2Delete.Add(modularHouseholdTrait);
                            }
                        }
                        foreach (var modularHouseholdTrait in traits2Delete)
                        {
                            household.DeleteTraitFromDB(modularHouseholdTrait);
                        }
                    }

                    SimIntegrityChecker.Run(sim, CheckingOptions.Default());
                    sim.MyGeneralConfig.StartDateUIString = "1.1.2015";
                    sim.MyGeneralConfig.EndDateUIString = "5.01.2015";
                    sim.MyGeneralConfig.InternalTimeResolution = "00:01:00";
                    sim.MyGeneralConfig.RandomSeed = 5;
                    sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.Reasonable);
                    //ChartLocalizer.ShouldTranslate = false;
                    //ConfigSetter.SetGlobalTimeParameters(sim.MyGeneralConfig);
                    sim.Should().NotBeNull();
                    db.Cleanup();
                }
                wd.CleanUp();
            }
        }

        public TemplatePersonTesterTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
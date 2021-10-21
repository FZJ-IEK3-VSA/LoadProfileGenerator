using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Tests;
using Database;
using Database.Tests;
using SimulationEngineLib.HouseJobProcessor;
using Xunit;
using Xunit.Abstractions;

namespace SimulationEngine.Tests {
    public class MakeHomeOfficeHouseholds: UnitTestBaseClass {
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void RunHomeOfficeTest()
        {
            Logger.Threshold = Severity.Warning;
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentAssemblyVersion());
            Simulator sim = new Simulator(db.ConnectionString);
            int i = 0;
            var original = sim.HouseholdTemplates.Items.ToList();
            //foreach (var tag in sim.TraitTags.Items) {
            //    Logger.Warning("Tag: " + tag.Name);
            //}
            var homeofficetag = sim.LivingPatternTags.Items.Single(x=> x.Name.Contains("Living Pattern / Work From Home / Part Time"));
            foreach (var template in original) {
                bool hasworker = false;
                var hhjson = template.GetJson();
                foreach (var templatePerson in hhjson.TemplatePersons)
                {
                    if (templatePerson.LivingPatternTraitTagReference == null) {
                        throw new LPGException("was null");
                    }
                    Logger.Warning(templatePerson.Name + ": " + templatePerson.LivingPatternTraitTagReference.Name);
                    if (templatePerson.LivingPatternTraitTagReference == null)
                    {
                        throw new LPGException("was null");
                    }
                    if (templatePerson.LivingPatternTraitTagReference?.Name?.Contains("Living Pattern / Office Job")==true) {
                        hasworker = true;
                    }
                }
                hhjson.Name = "CHC" + hhjson.Name.Substring(3);
                Logger.Warning("HH Name: " + hhjson.Name + ": has office worker: " + hasworker);
                if (!hasworker) {
                    continue;
                }

                foreach (var templatePerson in hhjson.TemplatePersons) {
                    // ReSharper disable once PossibleNullReferenceException
                    if (templatePerson.LivingPatternTraitTagReference.Name?.Contains("Living Pattern / Office Job")==true) {
                        Logger.Warning("Exchanging: " + templatePerson.Name + ": " + templatePerson.LivingPatternTraitTagReference.Name + " to " +  homeofficetag.Name);
                        templatePerson.LivingPatternTraitTagReference = homeofficetag.GetJsonReference();
                    }
                }
                var newTemplkate = sim.HouseholdTemplates.CreateNewItem(sim.ConnectionString);
                newTemplkate.ImportFromJsonTemplate(hhjson, sim);
                i++;
                if (i > 2) {
                    break;
                }
                newTemplkate.Guid = StrGuid.New();
                newTemplkate.SaveToDB();
                RunSingleHouse(sim, (sim1) => {
                    var hj =
                        PrepareNewHouseForHouseholdTemplateTesting(sim1, newTemplkate.Name);
                    sim.MyGeneralConfig.PerformCleanUpChecksBool = false;
                    if (hj.CalcSpec?.CalcOptions == null) {
                        throw new LPGException("was null");
                    }
                    hj.CalcSpec.CalcOptions.Add(CalcOption.ActionCarpetPlot);
                    return hj;
                }, db, true);
            }

        }
        public static HouseCreationAndCalculationJob PrepareNewHouseForHouseholdTemplateTesting(Simulator sim, string hhtemplatename)
        {
            var hj = new HouseCreationAndCalculationJob
            {
                CalcSpec = JsonCalcSpecification.MakeDefaultsForTesting()
            };
            hj.CalcSpec.StartDate = new DateTime(2020, 1, 1);
            hj.CalcSpec.EndDate = new DateTime(2020, 7, 1);
            hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.NoFiles;
            hj.CalcSpec.DeleteSqlite = false;
            hj.CalcSpec.ExternalTimeResolution = "00:15:00";
            hj.CalcSpec.EnableTransportation = false;
            hj.House = new HouseData(StrGuid.FromString("houseguid"), "HT01", 1000, 100, "housename")
            {
                Households = new List<HouseholdData>()
            };
            var hhd = new HouseholdData("householdid",
                "householdname", sim.ChargingStationSets[0].GetJsonReference(),
                sim.TransportationDeviceSets[0].GetJsonReference(),
                sim.TravelRouteSets[0].GetJsonReference(), null,
                HouseholdDataSpecificationType.ByTemplateName)
            {
                HouseholdTemplateSpec = new HouseholdTemplateSpecification(hhtemplatename)
            };
            hj.House.Households.Add(hhd);
            return hj;
        }
        private static void RunSingleHouse(Simulator sim, Func<Simulator, HouseCreationAndCalculationJob> makeHj,
                                           DatabaseSetup db,
                                           bool skipcleaning = false)
        {
            Logger.Get().StartCollectingAllMessages();
            //Logger.Threshold = Severity.Debug;
            using var wd = new WorkingDir(Utili.GetCallingMethodAndClass());
            wd.SkipCleaning = skipcleaning;
            var targetdb = wd.Combine("profilegenerator.db3");
            File.Copy(db.FileName, targetdb, true);
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            var houseGenerator = new HouseGenerator();
            var hj = makeHj(sim);
            houseGenerator.ProcessSingleHouseJob(hj, sim);
            if (hj.CalcSpec?.OutputDirectory == null)
            {
                throw new LPGException("calcspec was null");
            }
        }
        public MakeHomeOfficeHouseholds([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
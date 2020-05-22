using System.Linq;
using Automation;
using Common;
using Common.Tests;
using Database;
using Database.Tables.ModularHouseholds;
using Database.Tests;
using FluentAssertions;
using JetBrains.Annotations;
using SimulationEngineLib.Other;
using Xunit;
using Xunit.Abstractions;


namespace SimulationEngine.Tests.Other
{
    public class HouseholdTemplateExportTester : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunHouseholdTemplateTests()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    JsonDatabaseExporter hte = new JsonDatabaseExporter(db.ConnectionString);
                    JsonDatabaseExportOptions hteo = new JsonDatabaseExportOptions();
                    string jsonPath = wd.Combine("hhexport.json");
                    hteo.Output = jsonPath;
                    hteo.ProcessingType = TypesToProcess.HouseholdTemplates;
                    hte.Export(hteo);
                    JsonDatabaseImportOptions htio = new JsonDatabaseImportOptions { Input = jsonPath, Type = TypesToProcess.HouseholdTemplates };
                    JsonDatabaseImporter hti = new JsonDatabaseImporter(db.ConnectionString);
                    hti.Import(htio);
                }
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunHouseholdTraits()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    JsonDatabaseExporter hte = new JsonDatabaseExporter(db.ConnectionString);
                    JsonDatabaseExportOptions hteo = new JsonDatabaseExportOptions();
                    string jsonPath = wd.Combine("hhtraitexport.json");
                    hteo.Output = jsonPath;
                    hteo.ProcessingType = TypesToProcess.HouseholdTraits;
                    hte.Export(hteo);
                    JsonDatabaseImportOptions htio = new JsonDatabaseImportOptions { Input = jsonPath, Type = TypesToProcess.HouseholdTraits };
                    JsonDatabaseImporter hti = new JsonDatabaseImporter(db.ConnectionString);
                    hti.Import(htio);
                }
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunHouseholdTraitsWithDeviceCategories()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    JsonDatabaseExporter hte = new JsonDatabaseExporter(db.ConnectionString);
                    JsonDatabaseExportOptions hteo = new JsonDatabaseExportOptions();
                    string jsonPath = wd.Combine("hhtraitexport.json");
                    hteo.Output = jsonPath;
                    hteo.ProcessingType = TypesToProcess.HouseholdTraitsWithDeviceCategories;
                    hte.Export(hteo);
                }
            }
        }
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest5)]
        public void RunModularHouseholdTests()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    JsonDatabaseExporter hte = new JsonDatabaseExporter(db.ConnectionString);
                    JsonDatabaseExportOptions hteo = new JsonDatabaseExportOptions();
                    string jsonPath = wd.Combine("hhexport.json");
                    hteo.Output = jsonPath;
                    hteo.ProcessingType = TypesToProcess.ModularHouseholds;
                    hte.Export(hteo);
                    Simulator sim1 = new Simulator(db.ConnectionString);
                    int hhcount = sim1.ModularHouseholds.It.Count;
                    foreach (ModularHousehold household in sim1.ModularHouseholds.It)
                    {
                        household.DeleteFromDB();
                    }

                    foreach (var house in sim1.Houses.It) {
                        var households = house.Households.ToList();
                        foreach (var household in households) {
                            house.DeleteHouseholdFromDB(household);
                        }
                    }
                    Logger.Info("################################################");
                    Logger.Info("Finished deleting");
                    Logger.Info("################################################");
                    JsonDatabaseImportOptions htio = new JsonDatabaseImportOptions { Input = jsonPath, Type = TypesToProcess.ModularHouseholds };
                    JsonDatabaseImporter hti = new JsonDatabaseImporter(db.ConnectionString);
                    hti.Import(htio);
                    Simulator sim2 = new Simulator(db.ConnectionString);
                    hhcount.Should().Be(sim2.ModularHouseholds.It.Count);
                }
            }
        }

        public HouseholdTemplateExportTester([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}

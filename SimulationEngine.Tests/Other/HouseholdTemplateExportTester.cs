using System;
using Common;
using Database;
using Database.Tables.ModularHouseholds;
using Database.Tests;
using NUnit.Framework;
using SimulationEngine.Other;

namespace SimulationEngine.Tests.Other
{
    [TestFixture]
    public class HouseholdTemplateExportTester
    {
        [Test]
        [Category("BasicTest")]
        public void RunHouseholdTemplateTests()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(),DatabaseSetup.TestPackage.SimulationEngine);
            JsonDatabaseExporter hte = new JsonDatabaseExporter(db.ConnectionString);
            JsonDatabaseExportOptions hteo = new JsonDatabaseExportOptions();
            string jsonPath = wd.Combine("hhexport.json");
            hteo.Output = jsonPath;
            hteo.ProcessingType = TypesToProcess.HouseholdTemplates;
            hte.Export(hteo);
            JsonDatabaseImportOptions htio = new JsonDatabaseImportOptions {Input = jsonPath, Type = TypesToProcess.HouseholdTemplates};
            JsonDatabaseImporter hti = new JsonDatabaseImporter(db.ConnectionString);
            hti.Import(htio);
        }

        [Test]
        [Category("BasicTest")]
        public void RunHouseholdTraits()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
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

        [Test]
        [Category("BasicTest")]
        public void RunHouseholdTraitsWithDeviceCategories()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
            JsonDatabaseExporter hte = new JsonDatabaseExporter(db.ConnectionString);
            JsonDatabaseExportOptions hteo = new JsonDatabaseExportOptions();
            string jsonPath = wd.Combine("hhtraitexport.json");
            hteo.Output = jsonPath;
            hteo.ProcessingType = TypesToProcess.HouseholdTraitsWithDeviceCategories;
            hte.Export(hteo);
        }
        [Test]
        [Category("LongTest5")]
        public void RunModularHouseholdTests()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
            JsonDatabaseExporter hte = new JsonDatabaseExporter(db.ConnectionString);
            JsonDatabaseExportOptions hteo = new JsonDatabaseExportOptions();
            string jsonPath = wd.Combine("hhexport.json");
            hteo.Output = jsonPath;
            hteo.ProcessingType = TypesToProcess.ModularHouseholds;
            hte.Export(hteo);
            Simulator sim1 = new Simulator(db.ConnectionString);
            int hhcount = sim1.ModularHouseholds.It.Count;
            foreach (ModularHousehold household in sim1.ModularHouseholds.It) {
                household.DeleteFromDB();
            }
            Console.WriteLine("################################################");
            Console.WriteLine("Finished deleting");
            Console.WriteLine("################################################");
            JsonDatabaseImportOptions htio = new JsonDatabaseImportOptions {Input = jsonPath, Type = TypesToProcess.ModularHouseholds};
            JsonDatabaseImporter hti = new JsonDatabaseImporter(db.ConnectionString);
            hti.Import(htio);
            Simulator sim2 = new Simulator(db.ConnectionString);
            Assert.AreEqual(hhcount,sim2.ModularHouseholds.It.Count);
        }
    }
}

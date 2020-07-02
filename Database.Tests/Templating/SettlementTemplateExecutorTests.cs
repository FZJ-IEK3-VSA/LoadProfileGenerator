using System.IO;
using System.Linq;
using Automation;
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database.Templating;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;

namespace Database.Tests.Templating {
    public class SettlementTemplateExecutorTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void GenerateSystematicSettlementTemplateTests()
        {
            using var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            var sw = new StreamWriter(@"C:\Work\LPGDev\Database.Tests\Templating\SystematicHouseholdTests.cs");
            sw.WriteLine("using Automation;");
            sw.WriteLine("using Common;");
            sw.WriteLine("using Common.Tests;");
            sw.WriteLine("using Xunit;");
            //sw.WriteLine("using Database;");
            sw.WriteLine("using Database.Templating;");
            sw.WriteLine("using CalculationController.Integrity;");
            //sw.WriteLine("using Database.Tests;");
            sw.WriteLine("using Xunit.Abstractions;");
            sw.WriteLine("using JetBrains.Annotations;");
            sw.WriteLine("#pragma warning disable 8602");
            sw.WriteLine("namespace Database.Tests.Templating {");
            sw.WriteLine("public class SystematicHouseholdTests :UnitTestBaseClass {");
            var settlements = sim.SettlementTemplates.Items.ToList();
            for (var i = 0; i < settlements.Count; i++) {
                WriteSettlementTemplateTestFunction(sw, i);
            }

            sw.WriteLine(
                "public SystematicHouseholdTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }");
            sw.WriteLine("}}");
            sw.Close();
        }

        private static void WriteSettlementTemplateTestFunction([NotNull] StreamWriter sw, int i)
        {
            sw.WriteLine("[Fact]");
            sw.WriteLine("[Trait(UnitTestCategories.Category, UnitTestCategories.SystematicSettlementTemplateTests)]");
            sw.WriteLine("public void GenerateSettlementPreviewTest"+ i+"()");
            sw.WriteLine("{");
            sw.WriteLine("    using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))");
            sw.WriteLine("    {");
            sw.WriteLine("        var sim = new Simulator(db.ConnectionString);");
            sw.WriteLine("        var template = sim.SettlementTemplates[" + i+"];");
            sw.WriteLine("        template.DesiredHHCount = 50;");
            sw.WriteLine("        var ste = new SettlementTemplateExecutor();");
            sw.WriteLine("        ste.GenerateSettlementPreview(sim, template);");
            sw.WriteLine("        ste.CreateSettlementFromPreview(sim, template);");
            sw.WriteLine("        SimIntegrityChecker.Run(sim);");
            sw.WriteLine("        db.Cleanup();");
            sw.WriteLine("    }");
            sw.WriteLine("}");
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest5)]
        public void GenerateSettlementPreviewTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var template = sim.SettlementTemplates.Items.First(x => x.Name.StartsWith("H0"));
                var ste = new SettlementTemplateExecutor();
                ste.GenerateSettlementPreview(sim, template);
                ste.CreateSettlementFromPreview(sim, template);
                SimIntegrityChecker.Run(sim);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void InitializeHouseSizesTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var template = sim.SettlementTemplates.CreateNewItem(sim.ConnectionString);
                //SettlementTemplateExecutor ste = new SettlementTemplateExecutor();
                template.AddHouseSize(5, 13, 0.16);
                template.AddHouseSize(2, 2, 0.21);
                template.AddHouseSize(1, 1, 0.63);
                template.DesiredHHCount = 1000;
                SettlementTemplateExecutor.InitializeHouseSizes(template);
                foreach (var templateHouseSize in template.HouseSizes)
                {
                    Logger.Info(templateHouseSize.MinimumHouseSize + " - " + templateHouseSize.MaximumHouseSize + ": " +
                                templateHouseSize.HouseCount);
                }
                db.Cleanup();
            }
        }

        public SettlementTemplateExecutorTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
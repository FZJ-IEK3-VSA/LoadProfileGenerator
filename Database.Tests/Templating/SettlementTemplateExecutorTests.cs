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
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest5)]
        public void GenerateSettlementPreviewTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var template = sim.SettlementTemplates.It.First(x => x.Name.StartsWith("H0"));
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
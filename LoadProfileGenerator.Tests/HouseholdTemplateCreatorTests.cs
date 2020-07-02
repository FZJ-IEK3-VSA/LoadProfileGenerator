using Automation;
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database;
using Database.Tests;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;

namespace LoadProfileGenerator.Tests {
    public class HouseholdTemplateCreatorTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest2)]
        public void RunHouseholdTemplateCreatorTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var hhtc = new HouseholdTemplateCreator(sim);
                while (sim.HouseholdTemplates.Items.Count > 0)
                {
                    sim.HouseholdTemplates.DeleteItem(sim.HouseholdTemplates.Items[0]);
                }

                hhtc.Run(false, sim);

                SimIntegrityChecker.Run(sim);
                db.Cleanup();
            }
        }

        public HouseholdTemplateCreatorTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
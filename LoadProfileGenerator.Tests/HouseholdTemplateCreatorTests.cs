using Automation;
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database;
using Database.Tests;
using System.Collections.Generic;
using System.Linq;
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
                HashSet<string> templatesWithoutMatchingHH = ["CHR62 Couple both Working from Home"];

                // delete all templates for which a matching modular household exists
                var toDelete = sim.HouseholdTemplates.Items.Where(t => !templatesWithoutMatchingHH.Contains(t.Name)).ToList();
                foreach (var template in toDelete)
                {
                    sim.HouseholdTemplates.DeleteItem(template);
                }

                // run the HouseholdTemplateCreator to generate all templates
                hhtc.Run(false, sim);

                SimIntegrityChecker.Run(sim, CheckingOptions.Default());
                db.Cleanup();
            }
        }

        public HouseholdTemplateCreatorTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
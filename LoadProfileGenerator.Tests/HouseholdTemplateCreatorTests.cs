using System.Threading;
using Automation;
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database;
using Database.Tests;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;

namespace LoadProfileGenerator.Tests {
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class HouseholdTemplateCreatorTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.LongTest2)]
        public void RunHouseholdTemplateCreatorTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(db.ConnectionString);
                var hhtc = new HouseholdTemplateCreator(sim);
                while (sim.HouseholdTemplates.It.Count > 0)
                {
                    sim.HouseholdTemplates.DeleteItem(sim.HouseholdTemplates.It[0]);
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
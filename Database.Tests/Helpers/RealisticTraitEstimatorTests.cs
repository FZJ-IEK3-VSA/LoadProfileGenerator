using Automation;
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database.Helpers;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;

namespace Database.Tests.Helpers
{

    public class RealisticTraitEstimatorTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                RealisticTraitEstimator.Run(sim);
                SimIntegrityChecker.Run(sim, CheckingOptions.Default());
                db.Cleanup();
            }
        }

        public RealisticTraitEstimatorTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
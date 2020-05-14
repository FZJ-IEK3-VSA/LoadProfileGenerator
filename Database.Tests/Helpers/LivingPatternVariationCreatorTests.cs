using Automation;
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database.Helpers;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;

namespace Database.Tests.Helpers
{
    [TestFixture()]
    public class LivingPatternVariationCreatorTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void RunTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                LivingPatternVariationCreator lpvc = new LivingPatternVariationCreator();
                //    lpvc.RunOfficeJobs(sim);
                //  lpvc.RunSleep(sim);
                lpvc.RunAlarm(sim);
                SimIntegrityChecker.Run(sim);
                db.Cleanup();
            }

        }

        public LivingPatternVariationCreatorTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
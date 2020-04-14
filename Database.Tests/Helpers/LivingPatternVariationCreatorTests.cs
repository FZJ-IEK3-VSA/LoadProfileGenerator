using Automation;
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database.Helpers;
using NUnit.Framework;

namespace Database.Tests.Helpers
{
    [TestFixture()]
    public class LivingPatternVariationCreatorTests : UnitTestBaseClass
    {
        [Test()]
        [Category(UnitTestCategories.BasicTest)]
        public void RunTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim  = new Simulator( db.ConnectionString);
            LivingPatternVariationCreator lpvc = new LivingPatternVariationCreator();
        //    lpvc.RunOfficeJobs(sim);
          //  lpvc.RunSleep(sim);
          lpvc.RunAlarm(sim);
            SimIntegrityChecker.Run(sim);
           db.Cleanup();

        }
    }
}
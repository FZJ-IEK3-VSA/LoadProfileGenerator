using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using Database.Tests;
using NUnit.Framework;

namespace LoadProfileGenerator.Tests
{
    [TestFixture]
    public class CompleteAffordanceAddTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void CompleteAffordanceCreatorTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.LoadProfileGenerator);
            Simulator sim = new Simulator(db.ConnectionString);
            VLoadType lt = sim.LoadTypes[0];
            DeviceCategory dc = sim.DeviceCategories[1];
            TimeBasedProfile tp = sim.Timeprofiles[1];
            Location loc = sim.Locations[0];
            TraitTag tag = sim.TraitTags[0];
            TimeLimit timeLimit = sim.TimeLimits[0];

            CompleteAffordanceAdd.CreateItems(sim, "aff", "Entertainment / desire", "device", "trait", lt, dc, tp, 1,
                10, 1, 99, loc, tag, "traitclass", timeLimit, "affcategory", null, false, "newLocation");
            SimIntegrityChecker.Run(sim);
            db.Cleanup();
        }
    }
}
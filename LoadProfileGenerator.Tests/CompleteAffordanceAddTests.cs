using Automation;
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using Database.Tests;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;

namespace LoadProfileGenerator.Tests
{
    [TestFixture]
    public class CompleteAffordanceAddTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CompleteAffordanceCreatorTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
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

        public CompleteAffordanceAddTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
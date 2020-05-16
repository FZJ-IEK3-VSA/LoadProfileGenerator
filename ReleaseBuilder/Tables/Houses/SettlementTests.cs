using Automation;
using Automation.ResultFiles;
using Common;
using Common.Tests;
using Database;
using Database.Helpers;
using Database.Tests;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;

namespace ReleaseBuilder.Tables.Houses {
    public class SettlementTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void MakeTraitStatisticsTest()
        {
            using (var dbs = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var sim = new Simulator(dbs.ConnectionString);
                var set = sim.Settlements.FindFirstByName("combination", FindMode.Partial);
                if (set == null)
                {
                    throw new LPGException("settlement not found.");
                }
                set.MakeTraitStatistics(@"e:\hhstatistics.csv", @"e:\affStatistics.csv", sim);

                dbs.Cleanup();
            }
        }

        public SettlementTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
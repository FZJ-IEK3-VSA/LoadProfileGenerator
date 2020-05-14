using Automation.ResultFiles;
using Common;
using Common.Tests;
using Database;
using Database.Helpers;
using Database.Tests;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;

namespace ReleaseBuilder.Tables.Houses {
    [TestFixture]
    public class SettlementTests : UnitTestBaseClass
    {
        [Fact]
        [Category("QuickChart")]
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
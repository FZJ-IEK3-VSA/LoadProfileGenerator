using Automation.ResultFiles;
using Common;
using Common.Tests;
using Database;
using Database.Helpers;
using Database.Tests;
using NUnit.Framework;

namespace ReleaseBuilder.Tables.Houses {
    [TestFixture]
    public class SettlementTests : UnitTestBaseClass
    {
        [Test]
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
    }
}
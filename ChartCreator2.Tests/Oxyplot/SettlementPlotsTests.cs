using Automation;
using ChartCreator2.OxyCharts;
using Common.Tests;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;

namespace ChartCreator2.Tests.Oxyplot {

    public class SettlementPlotsTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunTest() {
            var sp = new SettlementPlots();
            sp.Run(@"F:\Dissertation\Results2015-12-13\Sum.Profiles.Electricity.csv",
                @"F:\Dissertation\Results2015-12-13\test", ";");
        }

        public SettlementPlotsTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
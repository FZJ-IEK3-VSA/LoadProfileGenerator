using Automation;
using ChartCreator2.SettlementMergePlots;
using Common.Tests;
using Xunit;
using Xunit.Abstractions;

namespace ChartCreator2.Tests.SettlementCharts
{

    public class MergedActivityPlotTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            MergedActivityPlot.Run(@"e:\MergedActivities.csv", @"e:\pictures");
        }

        public MergedActivityPlotTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
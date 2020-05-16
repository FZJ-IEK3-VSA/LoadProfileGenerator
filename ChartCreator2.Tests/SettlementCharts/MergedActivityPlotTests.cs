using Automation;
using ChartCreator2.SettlementMergePlots;
using Common.Tests;
using JetBrains.Annotations;

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

        public MergedActivityPlotTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
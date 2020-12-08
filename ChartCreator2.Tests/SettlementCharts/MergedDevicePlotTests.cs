using Automation;
using ChartCreator2.SettlementMergePlots;
using Common.Tests;
using Xunit;
using Xunit.Abstractions;

namespace ChartCreator2.Tests.SettlementCharts
{

    public class MergedDevicePlotTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            MergedDevicePlot.Run(@"e:\MergedDeviceProfiles.csv", @"G:\masterbatch");
        }

        public MergedDevicePlotTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
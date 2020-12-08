using Automation;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using Xunit;
using Xunit.Abstractions;

namespace ChartCreator2.Tests.OxyCharts {
    public class CalculationDurationFlameChartTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            var cdfc = new CalculationDurationFlameChart();
            var cp =  CalculationProfiler.Read(@"C:\work\CalculationBenchmarks.ActionCarpetPlotTest\");
            cdfc.Run(cp,@"e:\", "FlameChartTest");
        }

        public CalculationDurationFlameChartTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
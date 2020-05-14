using System.Threading;
using Automation;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;

namespace ChartCreator2.Tests.OxyCharts {
    [TestFixture]
    public class CalculationDurationFlameChartTests : UnitTestBaseClass
    {
        [Fact][Apartment(ApartmentState.STA)]
        [Category(UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            var cdfc = new CalculationDurationFlameChart();
            var cp =  CalculationProfiler.Read(@"C:\work\CalculationBenchmarks.ActionCarpetPlotTest\");
            cdfc.Run(cp,@"e:\", "FlameChartTest");
        }

        public CalculationDurationFlameChartTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
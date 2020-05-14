using System.Threading;
using Automation;
using ChartCreator2.SettlementMergePlots;
using Common.Tests;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;

namespace ChartCreator2.Tests.SettlementCharts
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class MergedActivityPlotTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            MergedActivityPlot.Run(@"e:\MergedActivities.csv", @"e:\pictures");
        }

        public MergedActivityPlotTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
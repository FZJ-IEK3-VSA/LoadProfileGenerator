using System.Threading;
using Automation;
using ChartCreator2.SettlementMergePlots;
using Common.Tests;
using NUnit.Framework;

namespace ChartCreator2.Tests.SettlementCharts
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class MergedActivityPlotTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            MergedActivityPlot.Run(@"e:\MergedActivities.csv", @"e:\pictures");
        }
    }
}
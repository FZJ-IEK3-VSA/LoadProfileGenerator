using System.Threading;
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
        [Category("QuickChart")]
        public void RunTest()
        {
            MergedActivityPlot.Run(@"e:\MergedActivities.csv", @"e:\pictures");
        }
    }
}
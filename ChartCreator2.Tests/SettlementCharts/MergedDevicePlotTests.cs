using System.Threading;
using Automation;
using ChartCreator2.SettlementMergePlots;
using Common.Tests;
using NUnit.Framework;

namespace ChartCreator2.Tests.SettlementCharts
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class MergedDevicePlotTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            MergedDevicePlot.Run(@"e:\MergedDeviceProfiles.csv", @"G:\masterbatch");
        }
    }
}
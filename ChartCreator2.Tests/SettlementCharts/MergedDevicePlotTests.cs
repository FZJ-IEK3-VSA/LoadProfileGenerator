using System.Threading;
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
        [Category("QuickChart")]
        public void RunTest()
        {
            MergedDevicePlot.Run(@"e:\MergedDeviceProfiles.csv", @"G:\masterbatch");
        }
    }
}
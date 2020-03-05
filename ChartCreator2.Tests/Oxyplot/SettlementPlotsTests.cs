using System.Threading;
using ChartCreator2.OxyCharts;
using Common.Tests;
using NUnit.Framework;

namespace ChartCreator2.Tests.Oxyplot {
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SettlementPlotsTests : UnitTestBaseClass
    {
        [Test]
        [Category("QuickChart")]
        public void RunTest() {
            var sp = new SettlementPlots();
            sp.Run(@"F:\Dissertation\Results2015-12-13\Sum.Profiles.Electricity.csv",
                @"F:\Dissertation\Results2015-12-13\test", ";");
        }
    }
}
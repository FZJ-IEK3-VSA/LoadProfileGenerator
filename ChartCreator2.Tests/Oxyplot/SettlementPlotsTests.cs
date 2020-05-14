using System.Threading;
using Automation;
using ChartCreator2.OxyCharts;
using Common.Tests;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;

namespace ChartCreator2.Tests.Oxyplot {
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SettlementPlotsTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunTest() {
            var sp = new SettlementPlots();
            sp.Run(@"F:\Dissertation\Results2015-12-13\Sum.Profiles.Electricity.csv",
                @"F:\Dissertation\Results2015-12-13\test", ";");
        }

        public SettlementPlotsTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
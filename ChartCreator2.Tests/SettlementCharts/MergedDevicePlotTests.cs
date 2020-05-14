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
    public class MergedDevicePlotTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            MergedDevicePlot.Run(@"e:\MergedDeviceProfiles.csv", @"G:\masterbatch");
        }

        public MergedDevicePlotTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
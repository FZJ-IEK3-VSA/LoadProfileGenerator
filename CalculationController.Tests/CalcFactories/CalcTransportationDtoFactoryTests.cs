using System;
using System.Globalization;
using Automation;
using CalculationController.CalcFactories;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace CalculationController.Tests.CalcFactories
{
    public class CalcTransportationDtoFactoryTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void DistanceToEnergyFactorTest()
        {
            VLoadType vlt = new VLoadType("elec","","W","kWh",1000,1,new TimeSpan(1,0,0),
                1,"",LoadTypePriority.All,true,Guid.NewGuid().ToStrGuid(), 1);

           var result= CalcTransportationDtoFactory.DistanceToPowerFactor(100000, 15,vlt.ConversionFaktorPowerToSum);
            result.Should().BeApproximatelyWithinPercent(0.00185,0.1);
            double distanceGained = 15000 * result*3600;
            distanceGained.Should().BeApproximatelyWithinPercent(100000,0.001);
            Logger.Info(result.ToString(CultureInfo.InvariantCulture));
        }

        public CalcTransportationDtoFactoryTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
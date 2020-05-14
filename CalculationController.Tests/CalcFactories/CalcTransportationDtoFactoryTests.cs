using System;
using System.Globalization;
using Automation;
using CalculationController.CalcFactories;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace CalculationController.Tests.CalcFactories
{
    [TestFixture()]
    public class CalcTransportationDtoFactoryTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void DistanceToEnergyFactorTest()
        {
            VLoadType vlt = new VLoadType("elec","","W","kWh",1000,1,new TimeSpan(1,0,0),
                1,"",LoadTypePriority.All,true,Guid.NewGuid().ToStrGuid(), 1);

           var result= CalcTransportationDtoFactory.DistanceToPowerFactor(100000, 15,vlt.ConversionFaktorPowerToSum);
            Assert.That(result,Is.EqualTo(0.00185).Within(0.001));
            double distanceGained = 15000 * result*3600;
            Assert.That(distanceGained, Is.EqualTo(100000).Within(0.001));
            Logger.Info(result.ToString(CultureInfo.InvariantCulture));
        }

        public CalcTransportationDtoFactoryTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
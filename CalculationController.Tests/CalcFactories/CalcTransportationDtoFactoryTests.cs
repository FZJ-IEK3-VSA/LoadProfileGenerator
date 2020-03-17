using System;
using Automation;
using CalculationController.CalcFactories;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using NUnit.Framework;

namespace CalculationController.Tests.CalcFactories
{
    [TestFixture()]
    public class CalcTransportationDtoFactoryTests : UnitTestBaseClass
    {
        [Test()]
        [Category(UnitTestCategories.BasicTest)]
        public void DistanceToEnergyFactorTest()
        {
            VLoadType vlt = new VLoadType("elec","","W","kWh",1000,1,new TimeSpan(1,0,0),
                1,"",LoadTypePriority.All,true,Guid.NewGuid().ToString(), 1);

           var result= CalcTransportationDtoFactory.DistanceToPowerFactor(100000, 15,vlt.ConversionFaktorPowerToSum);
            Assert.That(result,Is.EqualTo(0.00185).Within(0.001));
            double distanceGained = 15000 * result*3600;
            Assert.That(distanceGained, Is.EqualTo(100000).Within(0.001));
            Console.WriteLine(result);
        }
    }
}
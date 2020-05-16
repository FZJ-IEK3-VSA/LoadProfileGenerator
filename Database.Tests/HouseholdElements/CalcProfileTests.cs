using System;
using Automation;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.Tests;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.HouseholdElements
{

    public class CalcProfileTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcProfileTest1MinPro1MinStep()
        {
            TimeSpan stepsize = new TimeSpan(0, 1, 0);
            CalcProfile cp = new CalcProfile("name", Guid.NewGuid().ToStrGuid(), stepsize, ProfileType.Absolute, "made up");
            cp.AddNewTimepoint(new TimeSpan(0, 0, 0), 100);
            cp.AddNewTimepoint(new TimeSpan(0, 1, 0), 100);
            cp.ConvertToTimesteps();
            cp.StepValues[0].Should().Be(100);
            cp.StepValues.Count.Should().Be(1);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcProfileTest2MinPro1MinStep()
        {
            TimeSpan stepsize = new TimeSpan(0, 1, 0);
            CalcProfile cp = new CalcProfile("name", Guid.NewGuid().ToStrGuid(), stepsize, ProfileType.Absolute, "made up");
            cp.AddNewTimepoint(new TimeSpan(0, 0, 0), 100);
            cp.AddNewTimepoint(new TimeSpan(0, 2, 0), 100);
            cp.ConvertToTimesteps();
            cp.StepValues[0].Should().Be(100);
            cp.StepValues[0].Should().Be(100);
            cp.StepValues.Count.Should().Be(2);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcProfileTest30SPro1MinStep()
        {
            Config.IsInUnitTesting = true;
            TimeSpan stepsize = new TimeSpan(0, 1, 0);
            CalcProfile cp = new CalcProfile("name", Guid.NewGuid().ToStrGuid(), stepsize, ProfileType.Absolute, "made up");
            cp.AddNewTimepoint(new TimeSpan(0, 0, 0), 100);
            cp.AddNewTimepoint(new TimeSpan(0, 0, 30), 50);
            cp.AddNewTimepoint(new TimeSpan(0, 1, 30), 0);
            cp.ConvertToTimesteps();
            cp.StepValues.Count.Should().Be(2);
            cp.StepValues[0].Should().Be(75);
            cp.StepValues[1].Should().Be(25);
        }

        public CalcProfileTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
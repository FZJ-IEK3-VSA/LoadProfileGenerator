using System;
using Automation;
using Common;
using Common.Tests;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;

namespace SimulationEngine.Tests
{
    public class CalculationOptionProcessorTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void DateTimeTests()
        {
            Config.CatchErrors = false;
            DateTime dt = DateTime.Now;
            Logger.Info(dt.ToShortDateString());
        }

        //[Fact]
        //public void ReadOptionsTestRandomSeed()
        //{
        //    Config.CatchErrors = false;
        //    List<string> options = new List<string>();
        //    options.Add("Calculate");
        //    options.Add("-CalcObjectNumber");
        //    options.Add("1");
        //    options.Add("-RandomSeed");
        //    options.Add("1");
        //    Program.RunOptionProcessing("", options.ToArray());
        //}

/*
        [Fact]
        public void ReadOptionsTestStartDate()
        {
            CalculationOptionProcessor cop = new CalculationOptionProcessor();
            List<string> options = new List<string>();
            options.Add("--Calculate");
            options.Add("--StartDate");
            options.Add("01.01.2014");
            cop.ReadOptions(options.ToArray());
            (cop.StartDate).Should().Be(new DateTime(2014, 1, 1));
        }

        [Fact]
        public void ReadOptionsTestEndDate()
        {
            CalculationOptionProcessor cop = new CalculationOptionProcessor();
            List<string> options = new List<string>();
            options.Add("--Calculate");
            options.Add("--EndDate");
            options.Add("01.02.2014");
            cop.ReadOptions(options.ToArray());
            (cop.EndDate).Should().Be(new DateTime(2014, 2, 1));
        }

        [Fact]
        public void ReadOptionsTestCalcObjectNumber()
        {
            CalculationOptionProcessor cop = new CalculationOptionProcessor();
            List<string> options = new List<string>();
            options.Add("--Calculate");
            options.Add("--CalcObjectNumber");
            options.Add("1");
            cop.ReadOptions(options.ToArray());
            (cop.CalcObjectNumber).Should().Be(1);
        }*/
public CalculationOptionProcessorTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
{
}
    }
}
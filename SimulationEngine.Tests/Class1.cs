using System;
using Common;
using Common.Tests;
using NUnit.Framework;

namespace SimulationEngine.Tests
{
    [TestFixture]
    public class CalculationOptionProcessorTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void DateTimeTests()
        {
            Program.CatchErrors = false;
            DateTime dt = DateTime.Now;
            Logger.Info(dt.ToShortDateString());
        }

        //[Test]
        //public void ReadOptionsTestRandomSeed()
        //{
        //    Program.CatchErrors = false;
        //    List<string> options = new List<string>();
        //    options.Add("Calculate");
        //    options.Add("-CalcObjectNumber");
        //    options.Add("1");
        //    options.Add("-RandomSeed");
        //    options.Add("1");
        //    Program.RunOptionProcessing("", options.ToArray());
        //}

/*
        [Test]
        public void ReadOptionsTestStartDate()
        {
            CalculationOptionProcessor cop = new CalculationOptionProcessor();
            List<string> options = new List<string>();
            options.Add("--Calculate");
            options.Add("--StartDate");
            options.Add("01.01.2014");
            cop.ReadOptions(options.ToArray());
            Assert.AreEqual(new DateTime(2014, 1, 1), cop.StartDate);
        }

        [Test]
        public void ReadOptionsTestEndDate()
        {
            CalculationOptionProcessor cop = new CalculationOptionProcessor();
            List<string> options = new List<string>();
            options.Add("--Calculate");
            options.Add("--EndDate");
            options.Add("01.02.2014");
            cop.ReadOptions(options.ToArray());
            Assert.AreEqual(new DateTime(2014, 2, 1), cop.EndDate);
        }

        [Test]
        public void ReadOptionsTestCalcObjectNumber()
        {
            CalculationOptionProcessor cop = new CalculationOptionProcessor();
            List<string> options = new List<string>();
            options.Add("--Calculate");
            options.Add("--CalcObjectNumber");
            options.Add("1");
            cop.ReadOptions(options.ToArray());
            Assert.AreEqual(1, cop.CalcObjectNumber);
        }*/
    }
}
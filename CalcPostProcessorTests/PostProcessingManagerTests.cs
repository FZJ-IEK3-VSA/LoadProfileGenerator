using System;
using System.Collections.Generic;
using Automation;
using CalcPostProcessor;
using CalculationController.CalcFactories;
using CalculationController.Queue;
using Common;
using Common.SQLResultLogging.InputLoggers;
using Common.Tests;
using Database;
using Database.Tests;
using NUnit.Framework;

namespace CalcPostProcessorTests
{
    public class PostProcessingManagerTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void Run()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], sim.ModularHouseholds[0], EnergyIntensityType.EnergySaving, false,
                "1", null, LoadTypePriority.Optional, null, null,null, new List<CalcOption>(), new DateTime(2018, 1, 1),
                new DateTime(2018, 1, 10),
                new TimeSpan(0, 1, 0), ";", 1, new TimeSpan(0, 1, 0), false, false, false, 3, 3,
                calculationProfiler);
            CalcManagerFactory cmf = new CalcManagerFactory();
            var cm = cmf.GetCalcManager(sim, wd.WorkingDirectory, csps,  false);
            cm.Run(null);

            var mq = new Moq.Mock<ICalculationProfiler>();
            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
            //FileFactoryAndTracker fft = new FileFactoryAndTracker(wd.WorkingDirectory,"objname",cm.Logfile.FileFactoryAndTracker);
            //fft.RegisterHouseholdKey(Constants.GeneralHouseholdKey,"general");
            //fft.RegisterHouseholdKey(Constants.GeneralHouseholdKey,"general");
            PostProcessingManager pcm  = new PostProcessingManager(mq.Object,cm.Logfile.FileFactoryAndTracker);
            //Debug.Assert(calcResult != null, nameof(calcResult) + " != null");
            pcm.Run(wd.WorkingDirectory);
            wd.CleanUp();
            db.Cleanup();
        }
    }
}

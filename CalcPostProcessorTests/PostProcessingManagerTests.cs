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
using JetBrains.Annotations;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace CalcPostProcessorTests {
    public class PostProcessingManagerTests : UnitTestBaseClass {
        public PostProcessingManagerTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void Run()
        {
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim = new Simulator(db.ConnectionString);
                    var calculationProfiler = new CalculationProfiler();
                    var csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                        sim.TemperatureProfiles[0],
                        sim.ModularHouseholds[0],
                        EnergyIntensityType.EnergySaving,
                        false,
                        null,
                        LoadTypePriority.Optional,
                        null,
                        null,
                        null,
                        new List<CalcOption>(),
                        new DateTime(2018, 1, 1),
                        new DateTime(2018, 1, 10),
                        new TimeSpan(0, 1, 0),
                        ";",
                        1,
                        new TimeSpan(0, 1, 0),
                        false,
                        false,
                        false,
                        3,
                        3,
                        calculationProfiler, wd.WorkingDirectory);
                    var cmf = new CalcManagerFactory();
                    var cm = cmf.GetCalcManager(sim, csps, false);
                    cm.Run(null);

                    var mq = new Mock<ICalculationProfiler>();
                    wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
                    //FileFactoryAndTracker fft = new FileFactoryAndTracker(wd.WorkingDirectory,"objname",cm.Logfile.FileFactoryAndTracker);
                    //fft.RegisterHouseholdKey(Constants.GeneralHouseholdKey,"general");
                    //fft.RegisterHouseholdKey(Constants.GeneralHouseholdKey,"general");
                    var pcm = new PostProcessingManager(mq.Object, cm.CalcRepo.FileFactoryAndTracker);
                    //Debug.Assert(calcResult != null, nameof(calcResult) + " != null");
                    pcm.Run(wd.WorkingDirectory);
                    db.Cleanup();
                }
                wd.CleanUp();
            }
        }
    }
}
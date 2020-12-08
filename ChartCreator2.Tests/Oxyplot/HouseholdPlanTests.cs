using System.IO;
using Automation;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace ChartCreator2.Tests.Oxyplot {

    public class HouseholdPlanTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BrokenTest)]
        public void MainHouseholdPlanTest()
        {
            CleanTestBase.RunAutomatically(false);
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                configSetter: x =>
                {
                    x.ApplyOptionDefault(OutputFileDefault.OnlyDeviceProfiles);
                    //x.Enable(CalcOption.MakeGraphics);
                    x.Enable(CalcOption.HouseholdPlan);
                }
            );
            using (FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger))
            {
                CalculationProfiler cp = new CalculationProfiler();
                ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                    2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                var hp = new HouseholdPlan(ccps, fft, cp);
                Logger.Info("Making picture");
                var di = new DirectoryInfo(cs.DstDir);
                var rfe = cs.GetRfeByFilename("HouseholdPlan.Times.CHR02 Couple Both working, over 30.HH1.csv");
                hp.MakePlot(rfe);
                Logger.Info("finished picture");
                //OxyCalculationSetup.CopyImage(resultFileEntries[0].FullFileName);
                var imagefiles = FileFinder.GetRecursiveFiles(di, "HouseholdPlan.*.png");
                imagefiles.Count.Should().BeGreaterOrEqualTo( 1);
            }
            cs.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }

        public HouseholdPlanTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
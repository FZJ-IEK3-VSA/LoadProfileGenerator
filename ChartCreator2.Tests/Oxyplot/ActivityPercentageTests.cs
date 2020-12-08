using System.IO;
using Automation;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using Database.Database;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace ChartCreator2.Tests.Oxyplot {

    [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
    public class ActivityPercentageTests : UnitTestBaseClass
    {
        [StaFact]
        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                configSetter: x => x.Enable(CalcOption.ActivationFrequencies));
            using (FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger))
            {
                fft.ReadExistingFilesFromSql();
                CalculationProfiler cp = new CalculationProfiler();
                ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                    2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                var afpm = new ActivityPercentage(ccps, fft, cp);
                Logger.Info("Making picture");
                var di = new DirectoryInfo(cs.DstDir);
                var rfe = cs.GetRfeByFilename("ActivityPercentage.HH1.csv");
                afpm.MakePlot(rfe);
                Logger.Info("finished picture");
                //OxyCalculationSetup.CopyImage(resultFileEntries[0].FullFileName);
                var imagefiles = FileFinder.GetRecursiveFiles(di, "ActivityPercentage.*.png");
                imagefiles.Count.Should().BeGreaterOrEqualTo( 2);
            }
            Logger.Warning("Leftover connections: " + Connection.ConnectionCount);
            cs.CleanUp();

            //CleanTestBase.Run(true);
        }

        public ActivityPercentageTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
using System.IO;
using Automation;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace ChartCreator2.Tests.Oxyplot {

    public class LocationStatisticsTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]

        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                configSetter: x => x.Enable(CalcOption.LocationsFile));
            //cs.Wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(cs.Wd.SqlResultLoggingService));
            using (FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger))
            {
                fft.ReadExistingFilesFromSql();
                Logger.Info("Making picture");
                var di = new DirectoryInfo(cs.DstDir);
                var files = FileFinder.GetRecursiveFiles(di, "LocationStatistics.HH1.csv");
                files.Count.Should().BeGreaterOrEqualTo( 1);
                CalculationProfiler cp = new CalculationProfiler();
                ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                    2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                var locationStatistics = new LocationStatistics(ccps, fft, cp);
                var rfe = cs.GetRfeByFilename("LocationStatistics.HH1.csv");
                locationStatistics.MakePlot(rfe);
                Logger.Info("finished picture");
                //   OxyCalculationSetup.CopyImage(resultFileEntries[0].FullFileName);
                var imagefiles = FileFinder.GetRecursiveFiles(di, "LocationStatistics.*.png");
                imagefiles.Count.Should().BeGreaterOrEqualTo( 1);
            }
            cs.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }

        public LocationStatisticsTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
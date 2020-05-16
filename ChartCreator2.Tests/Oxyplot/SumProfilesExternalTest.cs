using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation;
using Automation.ResultFiles;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using FluentAssertions;
using Xunit;


namespace ChartCreator2.Tests.Oxyplot {

    public class SumProfilesExternalTest {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BrokenTest)]
        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            using var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                LoadTypePriority.Mandatory,
                configSetter: x =>
                {
                    x.Enable(CalcOption.SumProfileExternalEntireHouse);
                    x.ExternalTimeResolution = "00:30:00";
                });
            using (FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger))
            {
                CalculationProfiler cp = new CalculationProfiler();
                ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                    2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                var aeupp = new SumProfiles(ccps, fft, cp);
                Logger.Info("Making picture");
                var di = new DirectoryInfo(cs.DstDir);
                //var files = FileFinder.GetRecursiveFiles(di, );
                ResultFileEntry rfe = cs.GetRfeByFilename("SumProfiles_1800s.Electricity.csv");
                //Assert.GreaterOrEqual(files.Count, 1);

                aeupp.MakePlot(rfe);

                Logger.Info("finished picture");
                var imagefiles = FileFinder.GetRecursiveFiles(di, "SumProfiles_1800s.*.png");
                imagefiles.Count.Should().BeGreaterOrEqualTo( 1);
                fft.ResultFileList.ResultFiles.Count.Should().BeGreaterOrEqualTo( 1);
            }
            cs.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }
    }
}
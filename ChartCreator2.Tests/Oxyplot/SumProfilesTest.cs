using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using FluentAssertions;
using Xunit;


namespace ChartCreator2.Tests.Oxyplot {

    public class SumProfilesTest {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest4)]
        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            using var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                LoadTypePriority.Mandatory,
                configSetter: x => x.Enable(CalcOption.IndividualSumProfiles));
            using (var fft = cs.GetFileTracker())
            {
                //FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger);
                CalculationProfiler cp = new CalculationProfiler();
                ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                    2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                var aeupp = new SumProfiles(ccps, fft, cp);
                Logger.Info("Making picture");
                var di = new DirectoryInfo(cs.DstDir);
                var rfe = cs.GetRfeByFilename("SumProfiles.Electricity.csv");

                aeupp.MakePlot(rfe);
                Logger.Info("finished picture");
                //OxyCalculationSetup.CopyImage(resultFileEntries[0].FullFileName);
                var imagefiles = FileFinder.GetRecursiveFiles(di, "Sumprofiles.*.png");
                imagefiles.Count.Should().BeGreaterOrEqualTo( 1);
                fft.ResultFileList.ResultFiles.Count.Should().BeGreaterOrEqualTo(1);
            }
            cs.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }
    }
}
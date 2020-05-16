using System.IO;
using Automation;
using ChartCreator2.OxyCharts;
using Common;
//using Common.SQLResultLogging;
using Common.Tests;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace ChartCreator2.Tests.Oxyplot {

    public class AffordanceEnergyUseTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BrokenTest)]
        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                configSetter: x => x.Enable(CalcOption.AffordanceEnergyUse));
            using (FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger))
            {
                fft.ReadExistingFilesFromSql();
                CalculationProfiler cp = new CalculationProfiler();
                ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                    2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                //SqlResultLoggingService srls = new SqlResultLoggingService(cs.DstDir);
                var aeupp = new AffordanceEnergyUse(ccps, fft, cp);
                Logger.Debug("Making picture");
                var di = new DirectoryInfo(cs.DstDir);
                var rfe = cs.GetRfeByFilename("AffordanceEnergyUse.HH1.Electricity.csv");
                aeupp.MakePlot(rfe);
                Logger.Debug("finished picture");
                //OxyCalculationSetup.CopyImage(resultFileEntries[0].FullFileName);
                var imagefiles = FileFinder.GetRecursiveFiles(di, "AffordanceEnergyUse.*.png");
                imagefiles.Count.Should().BeGreaterOrEqualTo(1);
            }
            cs.CleanUp();
            //CleanTestBase.RunAutomatically(true);
        }
        /*
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void MakePlotTestMini()
        {
            Config.MakePDFCharts = true;
            ChartLocalizer.ShouldTranslate = true;
            var di = new DirectoryInfo(@"E:\unittest\AffordanceEnergyUseTests");
            FileFactoryAndTracker fft = new FileFactoryAndTracker(di.FullName, "1");
            CalculationProfiler cp = new CalculationProfiler();
            ChartBase.ChartCreationParameterSet ccps = new ChartBase.ChartCreationParameterSet(4000,
                2500, 300, false, fft, GlobalConsts.CSVCharacter, cp);
            var aeupp = new AffordanceEnergyUse(ccps);
            Logger.Debug("Making picture");

            var rfe = ResultFileList.LoadAndGetByFileName(di.FullName, "AffordanceEnergyUse.HH0.Electricity.csv");
            aeupp.MakePlot(rfe, "AffordanceEnergyUse HH1 Electricity", di);
            Logger.Debug("finished picture");
        }*/
        public AffordanceEnergyUseTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
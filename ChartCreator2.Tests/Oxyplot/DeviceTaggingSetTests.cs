using System;
using System.IO;
using Automation;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace ChartCreator2.Tests.Oxyplot {

    public class DeviceTaggingSetTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BrokenTest)]
        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            var start = DateTime.Now;
            //ChartLocalizer.ShouldTranslate = true;
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(2, GlobalConsts.CSVCharacter, configSetter: x =>
            {
                x.Enable(CalcOption.TotalsPerDevice);
                x.Enable(CalcOption.HouseholdContents);
            });
            var simend = DateTime.Now;
            using (FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger))
            {
                CalculationProfiler cp = new CalculationProfiler();
                ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                    2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                var aeupp = new DeviceTaggingSet(ccps, fft, cp);
                Logger.Info("Making picture");
                var di = new DirectoryInfo(cs.DstDir);
                var rfe = cs.GetRfeByFilename("DeviceTaggingSet.Electricity.General.csv");
                aeupp.MakePlot(rfe);
                Logger.Info("finished picture");
                //OxyCalculationSetup.CopyImage(resultFileEntries[0].FullFileName);
                Logger.Info("Simulation Time:" + (simend - start));
                Logger.Info("Chart Time:" + (DateTime.Now - simend));
                var imagefiles = FileFinder.GetRecursiveFiles(di, "DeviceTaggingSet.*.png");
                imagefiles.Count.Should().BeGreaterOrEqualTo( 1);
            }
            cs.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }
        /*
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void MakePlotTestMini()
        {
            ChartLocalizer.ShouldTranslate = true;
            Config.MakePDFCharts = true;

            var di = new DirectoryInfo(@"E:\unittest\DeviceTaggingSetTests");
            FileFactoryAndTracker fft = new FileFactoryAndTracker(di.FullName, "1");
            CalculationProfiler cp = new CalculationProfiler();
            ChartBase.ChartCreationParameterSet ccps = new ChartBase.ChartCreationParameterSet(4000,
                2500, 300, false, fft, GlobalConsts.CSVCharacter, cp);
            var aeupp = new DeviceTaggingSet(ccps);

            var rfe = ResultFileList.LoadAndGetByFileName(di.FullName, "DeviceTaggingSet.Electricity.csv");
            aeupp.MakePlot(rfe, "device tagging set", di);
            var rfe2 = ResultFileList.LoadAndGetByFileName(di.FullName, "DeviceTaggingSet.Warm Water.HH0.*");
            aeupp.MakePlot(rfe2, "device tagging set", di);
            Logger.Info("finished picture");
        }*/
        public DeviceTaggingSetTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
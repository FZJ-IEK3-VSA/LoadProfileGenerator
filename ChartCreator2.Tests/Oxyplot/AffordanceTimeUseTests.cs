using System.IO;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using Database.Database;
using NUnit.Framework;

namespace ChartCreator2.Tests.Oxyplot {
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class AffordanceTimeUseTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            //ChartLocalizer.ShouldTranslate = true;
            Config.MakePDFCharts = true;
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                configSetter: x => x.Enable(CalcOption.ActivationFrequencies));
            using (FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger))
            {
                fft.ReadExistingFilesFromSql();
                CalculationProfiler cp = new CalculationProfiler();
                ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                    2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                var aeupp = new AffordanceTimeUse(ccps, fft, cp);
                Logger.Debug("Making picture");
                var di = new DirectoryInfo(cs.DstDir);
                ResultFileEntry rfe = cs.GetRfeByFilename("AffordanceTimeUse.HH1.csv");
                aeupp.MakePlot(rfe);
                Logger.Debug("finished picture");
                //OxyCalculationSetup.CopyImage(resultFileEntries[0].FullFileName);
                var imagefiles = FileFinder.GetRecursiveFiles(di, "AffordanceTimeUse.*.png");
                Assert.GreaterOrEqual(imagefiles.Count, 2);
            }
            Logger.Warning("Open threads for database: " + Connection.ConnectionCount);
            cs.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }
        /*
        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void MakePlotTestMini()
        {
            ChartLocalizer.ShouldTranslate = true;
            Config.MakePDFCharts = true;

            var di = new DirectoryInfo(@"E:\unittest\AffordanceTimeUseTests");

            var rfe = ResultFileList.LoadAndGetByFileName(di.FullName, "AffordanceTimeUse.HH0.csv");
            FileFactoryAndTracker fft = new FileFactoryAndTracker(di.FullName, "1");
            CalculationProfiler cp = new CalculationProfiler();
            ChartBase.ChartCreationParameterSet ccps = new ChartBase.ChartCreationParameterSet(4000,
                2500, 300, false, fft, GlobalConsts.CSVCharacter, cp);
            var aeupp = new AffordanceTimeUse(ccps);
            aeupp.MakePlot(rfe, "AffordanceTimeUse", di);
            Logger.Debug("finished picture");
        }*/
    }
}
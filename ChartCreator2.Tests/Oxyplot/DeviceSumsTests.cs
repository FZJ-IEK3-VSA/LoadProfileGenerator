using System;
using System.IO;
using System.Threading;
using Automation;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using NUnit.Framework;

namespace ChartCreator2.Tests.Oxyplot {
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class DeviceSumsTests : UnitTestBaseClass
    {
        [Test]
        [Category("TestToFix")]
        public void MakePlotMonthlyTest()
        {
            CleanTestBase.RunAutomatically(false);
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1, GlobalConsts.CSVCharacter, enddate: new DateTime(2012, 3, 31),
                configSetter: x => x.Enable(CalcOption.TotalsPerDevice));
            FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger);
            CalculationProfiler cp = new CalculationProfiler();
            ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                2500,  false,  GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
            var aeupp = new DeviceSums(ccps,fft,cp);
            Logger.Info("Making picture");
            var di = new DirectoryInfo(cs.DstDir);
            var rfe = cs.GetRfeByFilename("DeviceSums_Monthly.Electricity.csv");
            aeupp.MakePlotMonthly(rfe, "monthly sums", di);
            Logger.Info("finished picture");

            var imagefiles = FileFinder.GetRecursiveFiles(di, "DeviceSums_Monthly.*.png");
            Assert.GreaterOrEqual(imagefiles.Count, 1);
            cs.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }

        [Test]
        [Category("TestToFix")]
        public void MakePlotTest()
        {
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                configSetter: x => x.Enable(CalcOption.TotalsPerDevice));
            FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger);
            fft.ReadExistingFilesFromSql();
            CalculationProfiler cp = new CalculationProfiler();
            ChartCreationParameters ccps = new ChartCreationParameters(300,4000,
                2500,  false,  GlobalConsts.CSVCharacter,new DirectoryInfo(cs.DstDir));
            var aeupp = new DeviceSums(ccps,fft,cp);
            Logger.Info("Making picture");
            var di = new DirectoryInfo(cs.DstDir);
            var rfe = cs.GetRfeByFilename("DeviceSums.Electricity.csv");
            aeupp.MakePlot(rfe);
            Logger.Info("finished picture");
            //OxyCalculationSetup.CopyImage(resultFileEntries[0].FullFileName);
            var imagefiles = FileFinder.GetRecursiveFiles(di, "DeviceSums.Electricity.*.png");
            Assert.GreaterOrEqual(imagefiles.Count, 1);
            cs.CleanUp();
        }
        /*
        [Test]
        [Category("QuickChart")]
        public void MakePlotTestMini()
        {
            ChartLocalizer.ShouldTranslate = true;
            Config.MakePDFCharts = true;
            var di = new DirectoryInfo(@"E:\unittest\DeviceSumsTests");
            FileFactoryAndTracker fft = new FileFactoryAndTracker(di.FullName, "1",);
            CalculationProfiler cp = new CalculationProfiler();
            ChartBase.ChartCreationParameterSet ccps = new ChartBase.ChartCreationParameterSet(4000,
                2500, 300, false, fft, GlobalConsts.CSVCharacter, cp);
            var aeupp = new DeviceSums(ccps);
            Logger.Info("Making picture");

            var rfe = ResultFileList.LoadAndGetByFileName(di.FullName, "DeviceSums.Electricity.csv");
            aeupp.MakePlot(rfe, "sums", di);
            Logger.Info("finished picture");
        }*/
    }
}
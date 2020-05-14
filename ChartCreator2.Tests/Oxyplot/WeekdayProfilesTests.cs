using System;
using System.IO;
using System.Threading;
using Automation;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace ChartCreator2.Tests.Oxyplot {
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class WeekdayProfilesTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.LongTest5)]
        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                LoadTypePriority.Mandatory, enddate: new DateTime(2012, 12, 31),
                x => x.Enable(CalcOption.WeekdayProfiles));
            using (FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger))
            {
                fft.ReadExistingFilesFromSql();
                CalculationProfiler cp = new CalculationProfiler();
                ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                    2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                var aeupp = new WeekdayProfiles(ccps, fft, cp);
                Logger.Info("Making picture");
                var di = new DirectoryInfo(cs.DstDir);
                var rfe = cs.GetRfeByFilename("WeekdayProfiles.Electricity.csv");
                aeupp.MakePlot(rfe);
                Logger.Info("finished picture");
                //OxyCalculationSetup.CopyImage(resultFileEntries[0].FullFileName);

                var imagefiles = FileFinder.GetRecursiveFiles(di, "WeekdayProfiles.*.png");
                Assert.GreaterOrEqual(imagefiles.Count, 1);
            }
            cs.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }
        /*
        [Fact]
        [Category(UnitTestCategories.ManualOnly)]
        public void MakePlotTestMini()
        {
            var di = new DirectoryInfo(@"E:\unittest\WeekdayProfilesTests");
            FileFactoryAndTracker fft = new FileFactoryAndTracker(di.FullName, "1", CompressedStack.);
            ChartLocalizer.ShouldTranslate = true;
            Config.MakePDFCharts = true;
            CalculationProfiler cp = new CalculationProfiler();
            ChartBase.ChartCreationParameterSet ccps = new ChartBase.ChartCreationParameterSet(4000,
                2500, 300, false, fft, GlobalConsts.CSVCharacter, cp);
            var aeupp = new WeekdayProfiles(ccps);
            Logger.Info("Making picture");

            var rfe = ResultFileList.LoadAndGetByFileName(di.FullName, "WeekdayProfiles.Electricity.csv");
            aeupp.MakePlot(rfe, "Weekday", di);
            Logger.Info("finished picture");
        }*/
        public WeekdayProfilesTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
using System;
using System.IO;
using Automation;
using ChartCreator2.OxyCharts;
using Common;
using Common.SQLResultLogging.InputLoggers;
using Common.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace ChartCreator2.Tests.Oxyplot {

    public class DeviceProfilesTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest5)]
        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(2, GlobalConsts.CSVCharacter, LoadTypePriority.Mandatory,
                configSetter: x =>
                {
                    x.Enable(CalcOption.DeviceProfilesIndividualHouseholds);
                    x.Enable(CalcOption.HouseholdContents);
                    x.SelectedEnergyIntensity = EnergyIntensityType.EnergySavingPreferMeasured;
                    x.EndDateDateTime = new DateTime(2012, 12, 31);
                }, energyIntensity: EnergyIntensityType.EnergySavingPreferMeasured);
            using (FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger))
            {
                fft.ReadExistingFilesFromSql();
                DeviceProfiles.DaysToMake = 365;
                CalculationProfiler cp = new CalculationProfiler();
                ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                    2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                var calcParameters = new CalcParameterLogger(cs.Wd.SqlResultLoggingService).Load();
                var aeupp = new DeviceProfiles(ccps, fft, cp, cs.Wd.SqlResultLoggingService,calcParameters);
                Logger.Info("Making picture");
                var di = new DirectoryInfo(cs.DstDir);
                var rfe = cs.GetRfeByFilename("DeviceProfiles.HH1.Electricity.csv");
                aeupp.MakePlot(rfe);
                Logger.Info("finished picture");
                var imagefiles = FileFinder.GetRecursiveFiles(di, "DeviceProfiles.*.png");
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
            var oldCharts = new DirectoryInfo(@"E:\unittest\DeviceProfilesTest\Charts");
            var filesToDelete = oldCharts.GetFiles("*.*");
            Config.SpecialChartFontSize = 26;
            foreach (var oldChart in filesToDelete) {
                Logger.Warning("Deleting " + oldChart.Name);
                oldChart.Delete();
            }
            DeviceProfiles.DaysToMake = 5;
            ChartLocalizer.ShouldTranslate = true;
            Config.MakePDFCharts = false;
            FileFactoryAndTracker fft = new FileFactoryAndTracker(oldCharts.FullName,"1");
            CalculationProfiler cp = new CalculationProfiler();
            ChartBase.ChartCreationParameterSet ccps = new ChartBase.ChartCreationParameterSet(4000,
                2500, 300, false, fft, GlobalConsts.CSVCharacter, cp);
            var aeupp = new DeviceProfiles(ccps);
            Logger.Info("Making picture");
            var di = new DirectoryInfo(@"E:\unittest\DeviceProfilesTest");
            var rfe = ResultFileList.LoadAndGetByFileName(di.FullName, "DeviceProfiles.Electricity.csv");
            aeupp.MakePlot(rfe, "dev profiles", di);
            Logger.Info("finished picture");
        }*/
        public DeviceProfilesTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
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
    public class DeviceProfilesExternalTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BrokenTest)]
        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(2,  GlobalConsts.CSVCharacter, LoadTypePriority.Mandatory,
                configSetter: x => {
                    x.CSVCharacter = ";";
                    x.ApplyOptionDefault(OutputFileDefault.None);
                    x.Enable(CalcOption.DeviceProfileExternalEntireHouse);
                    x.Enable(CalcOption.HouseholdContents);
                    x.ExternalTimeResolution = "01:00:00";
                    x.SelectedEnergyIntensity = EnergyIntensityType.EnergySavingPreferMeasured;
                    x.EndDateString = "31.01.2012";
                }, energyIntensity: EnergyIntensityType.EnergySavingPreferMeasured);
            FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir,"1",cs.Wd.InputDataLogger);
            CalculationProfiler cp = new CalculationProfiler();
            ChartCreationParameters ccps = new ChartCreationParameters(300,4000,
                2500,  false,  GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
            var aeupp = new DeviceProfilesExternal(ccps,fft,cp);
            Logger.Info("Making picture");
            var di = new DirectoryInfo(cs.DstDir);
            var rfe = cs.GetRfeByFilename("DeviceProfiles_3600s.Electricity.csv");
            aeupp.MakePlot(rfe);
            Logger.Info("finished picture");
            var imagefiles = FileFinder.GetRecursiveFiles(di, "DeviceProfiles*.png");
            Assert.GreaterOrEqual(imagefiles.Count, 1);
            cs.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }

        [Test]
        [Category(UnitTestCategories.BrokenTest)]
        public void MakePlotTestForHouse()
        {
            CleanTestBase.RunAutomatically(false);
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1,  GlobalConsts.CSVCharacter,
                LoadTypePriority.RecommendedForHouses,
                configSetter: x => {
                    x.Enable(CalcOption.DeviceProfileExternalEntireHouse);
                    x.ExternalTimeResolution = "00:30:00";
                    x.CSVCharacter = ";";
                }, useHouse: true);
            FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger);
            CalculationProfiler cp = new CalculationProfiler();
            ChartCreationParameters ccps = new ChartCreationParameters(300,4000,
                2500,  false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
            var aeupp = new DeviceProfilesExternal(ccps,fft,cp);
            Logger.Info("Making picture");
            var di = new DirectoryInfo(cs.DstDir);

            var rfe = cs.GetRfeByFilename("DeviceProfiles_1800s.Electricity.csv");
            aeupp.MakePlot(rfe);
            Logger.Info("finished picture");
            var imagefiles = FileFinder.GetRecursiveFiles(di, "DeviceProfiles*.png");
            Assert.GreaterOrEqual(imagefiles.Count, 1);
            cs.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }
        /*
        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void MakePlotTestMini()
        {
            var oldCharts = new DirectoryInfo(@"F:\DissCalcs_2\Chr03ht04CalcIntensive2\Charts");
            var filesToDelete = oldCharts.GetFiles("*.*");
            foreach (var oldChart in filesToDelete) {
                Logger.Warning("Deleting " + oldChart.Name);
                oldChart.Delete();
            }
            DeviceProfiles.DaysToMake = 5;
            ChartLocalizer.ShouldTranslate = true;
            Config.MakePDFCharts = true;
            FileFactoryAndTracker fft = new FileFactoryAndTracker(oldCharts.FullName, "1");
            CalculationProfiler cp = new CalculationProfiler();
            ChartBase.ChartCreationParameterSet ccps = new ChartBase.ChartCreationParameterSet(4000,
                2500, 300, false, fft, GlobalConsts.CSVCharacter, cp);
            var aeupp = new DeviceProfiles(ccps);
            Logger.Info("Making picture");
            var di = new DirectoryInfo(@"F:\DissCalcs_2\Chr03ht04CalcIntensive2");
            var rfe = ResultFileList.LoadAndGetByFileName(di.FullName, "DeviceProfiles.Electricity.csv");
            aeupp.MakePlot(rfe, "dev profiles", di);
            Logger.Info("finished picture");
        }*/
    }
}
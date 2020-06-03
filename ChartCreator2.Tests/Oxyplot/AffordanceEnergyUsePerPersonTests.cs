using System.IO;
using Automation;
using Automation.ResultFiles;
using ChartCreator2.OxyCharts;
using Common;
using Common.SQLResultLogging;
using Common.Tests;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace ChartCreator2.Tests.Oxyplot {

    public class AffordanceEnergyUsePerPersonTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            Config.MakePDFCharts = false;
            //ChartLocalizer.ShouldTranslate = false;
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            var rep = cs.StartHousehold(2, GlobalConsts.CSVCharacter,
                configSetter: x =>
                {
                    x.Enable(CalcOption.AffordanceEnergyUse);
                    x.Enable(CalcOption.HouseSumProfilesFromDetailedDats);
                    x.Enable(CalcOption.DeviceProfilesIndividualHouseholds);
                });
            if (rep == null)
            {
                throw new LPGException("Failed to simulate");
            }

            using (FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger))
            {
                fft.ReadExistingFilesFromSql();
                ChartCreationParameters ccps = new ChartCreationParameters(144, 4000, 2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                CalculationProfiler cp = new CalculationProfiler();

                var affordanceEnergyUsePerPerson = new AffordanceEnergyUsePerPerson(ccps, fft, cp, rep);
                Logger.Info("Making picture");
                var di = new DirectoryInfo(cs.DstDir);
                //var rfe = cs.GetRfeByFilename("AffordanceEnergyUsePerPerson.HH1.Electricity.csv");
                var keys = rep.HouseholdKeys;
                rep.FindTableByKey(ResultTableID.AffordanceEnergyUse, keys[1].HouseholdKey);
                affordanceEnergyUsePerPerson.MakePlot(keys[1]);
                Logger.Info("finished picture");
                //OxyCalculationSetup.CopyImage(resultFileEntries[0].FullFileName);
                var imagefiles = FileFinder.GetRecursiveFiles(di, "AffordanceEnergyUsePerPerson.*.png");
                imagefiles.Count.Should().BeGreaterOrEqualTo(1);
            }
            cs.CleanUp();
            //CleanTestBase.Run(true);
        }

        /*
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void MakePlotTestMini()
        {
            Config.MakePDFCharts = true;
            ChartLocalizer.ShouldTranslate = true;
            var di = new DirectoryInfo(@"E:\unittest\AffordanceEnergyUsePerPersonTests");
            var rfe = ResultFileList.LoadAndGetByFileName(di.FullName,
                "AffordanceEnergyUsePerPerson.HH0.Electricity.csv");
            FileFactoryAndTracker fft = new FileFactoryAndTracker(di.FullName, "1",);
            CalculationProfiler cp = new CalculationProfiler();
            ChartBase.ChartCreationParameterSet ccps = new ChartBase.ChartCreationParameterSet(4000,
                2500, 300, false, fft, GlobalConsts.CSVCharacter, cp);
            var affordanceEnergyUsePerPerson = new AffordanceEnergyUsePerPerson(ccps);

            affordanceEnergyUsePerPerson.MakePlot(rfe, "AffordanceEnergyUsePerPerson HH1 Electricity",
                di);
            Logger.Info("finished picture");
        }*/
        public AffordanceEnergyUsePerPersonTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
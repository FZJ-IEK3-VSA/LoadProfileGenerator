using System.IO;
using Automation;
using Automation.ResultFiles;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using Database.Database;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace ChartCreator2.Tests.Oxyplot {

    public class AffordanceTaggingSetTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            //ChartLocalizer.ShouldTranslate = true;
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(2, GlobalConsts.CSVCharacter,
                configSetter: x => x.Enable(CalcOption.ActivationFrequencies));
            using (FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger))
            {
                fft.ReadExistingFilesFromSql();
                CalculationProfiler cp = new CalculationProfiler();
                ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                    2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                var aeupp = new AffordanceTaggingSet(ccps, fft, cp);
                Logger.Debug("Making picture");
                var di = new DirectoryInfo(cs.DstDir);
                ResultFileEntry rfe = cs.GetRfeByFilename("AffordanceTaggingSet.Wo bleibt die Zeit.HH1.csv");

                aeupp.MakePlot(rfe);
                Logger.Debug("finished picture");
                //OxyCalculationSetup.CopyImage(resultFileEntries[0].FullFileName);
                var imagefiles = FileFinder.GetRecursiveFiles(di, "AffordanceTaggingSet.*.png");
                imagefiles.Count.Should().BeGreaterOrEqualTo(1);
            }
            Logger.Warning("Open threads for database: " + Connection.ConnectionCount);
            Command.PrintOpenConnections();
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
            var di = new DirectoryInfo(@"E:\unittest\AffordanceTaggingSetTests");
            FileFactoryAndTracker fft = new FileFactoryAndTracker(di.FullName, "1");
            CalculationProfiler cp = new CalculationProfiler();
            ChartBase.ChartCreationParameterSet ccps = new ChartBase.ChartCreationParameterSet(4000,
                2500, 300, false, fft, GlobalConsts.CSVCharacter, cp);
            var aeupp = new AffordanceTaggingSet(ccps);

            var rfe = ResultFileList.LoadAndGetByFileName(di.FullName,
                "AffordanceTaggingSet.Wo bleibt die Zeit.HH0.csv");
            aeupp.MakePlot(rfe, "affordance tagging set", di);
            Logger.Debug("finished picture");
        }*/
        public AffordanceTaggingSetTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
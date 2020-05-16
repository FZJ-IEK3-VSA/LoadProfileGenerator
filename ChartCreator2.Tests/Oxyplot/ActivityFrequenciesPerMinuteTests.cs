//using System.Collections.Generic;

using System.IO;
using Automation;
using Automation.ResultFiles;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace ChartCreator2.Tests.Oxyplot {
    public class ActivityFrequenciesPerMinuteTests : UnitTestBaseClass {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest3)]
        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                LoadTypePriority.Mandatory, configSetter: x => x.Enable(CalcOption.ActivationFrequencies));
            using (FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger))
            {
                fft.ReadExistingFilesFromSql();
                CalculationProfiler cp = new CalculationProfiler();
                ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                    2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                var activityFrequenciesPerMinute = new ActivityFrequenciesPerMinute(ccps, fft, cp);
                Logger.Info("Making picture");
                var di = new DirectoryInfo(cs.DstDir);
                ResultFileEntry rfe = cs.GetRfeByFilename("ActivityFrequenciesPerMinute.HH1.csv");

                activityFrequenciesPerMinute.MakePlot(rfe);
                Logger.Info("finished picture");
                //OxyCalculationSetup.CopyImage(resultFileEntries[0].FullFileName);
                var imagefiles = FileFinder.GetRecursiveFiles(di, "ActivityFrequenciesPerMinute.*.png");
                imagefiles.Count.Should().BeGreaterOrEqualTo( 2);
            }
            cs.CleanUp();

            //CleanTestBase.Run(true);
        }

        public ActivityFrequenciesPerMinuteTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
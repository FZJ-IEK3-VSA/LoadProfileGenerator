using System.IO;
using Automation;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace ChartCreator2.Tests.Oxyplot {

    public class VariableLogFileChartTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BrokenTest)]
        public void MakePlotTest()
        {
            CleanTestBase.RunAutomatically(false);
            var cs = new OxyCalculationSetup(Utili.GetCurrentMethodAndClass());
            cs.StartHousehold(1, GlobalConsts.CSVCharacter,
                LoadTypePriority.Mandatory, null,
                x => x.Enable(CalcOption.VariableLogFile));
            using (FileFactoryAndTracker fft = new FileFactoryAndTracker(cs.DstDir, "1", cs.Wd.InputDataLogger))
            {
                CalculationProfiler cp = new CalculationProfiler();
                ChartCreationParameters ccps = new ChartCreationParameters(300, 4000,
                    2500, false, GlobalConsts.CSVCharacter, new DirectoryInfo(cs.DstDir));
                var aeupp = new VariableLogFileChart(ccps, fft, cp);
                Logger.Info("Making picture");
                var di = new DirectoryInfo(cs.DstDir);

                var rfe = cs.GetRfeByFilename("Variablelogfile.HH1.csv");
                aeupp.MakePlot(rfe);

                Logger.Info("finished picture");
                //OxyCalculationSetup.CopyImage(ffe);

                var imagefiles = FileFinder.GetRecursiveFiles(di, "Variablelogfile.*.png");
                imagefiles.Count.Should().BeGreaterOrEqualTo(1);
            }
            cs.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }

        public VariableLogFileChartTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
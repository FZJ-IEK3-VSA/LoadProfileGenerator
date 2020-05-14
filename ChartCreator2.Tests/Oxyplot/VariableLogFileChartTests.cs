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
    public class VariableLogFileChartTests : UnitTestBaseClass
    {
        [Fact]
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
                Assert.GreaterOrEqual(imagefiles.Count, 1);
            }
            cs.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }

        public VariableLogFileChartTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using Database;
using Database.Tests;
using NUnit.Framework;

namespace ChartCreator2.Tests.Oxyplot {
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class MakeNRWChartTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void MakeNRWChartTest() {
            CleanTestBase.RunAutomatically(false);
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.ChartCreator);
            var sim = new Simulator(db.ConnectionString);
            var versions = sim.CalculationOutcomes.It.Select(x => x.LPGVersion).Distinct().ToList();
            versions.Sort();
            var dstversion = versions.Last();
            //ChartLocalizer.ShouldTranslate = false;
            var se1 = new MakeNRWChart.SeriesEntry(0.1, "EnergyIntensivePreferMeasured",
                "Energieintensive Gerätewahl", dstversion);
            var se2 = new MakeNRWChart.SeriesEntry(-0.1, "EnergySavingPreferMeasured",
                "Energiesparende Gerätewahl", dstversion);
            //MakeNRWChart.SeriesEntry se3 = new MakeNRWChart.SeriesEntry(-0.1, "EnergyIntensive","EnergyIntensive Gerätewahl", dstversion);
            //MakeNRWChart.SeriesEntry se4 = new MakeNRWChart.SeriesEntry(-0.1, "EnergySaving", "EnergySaving Gerätewahl", dstversion);
            var seriesEntries = new List<MakeNRWChart.SeriesEntry>
            {
                se1,
                se2
            };
            //    seriesEntries.Add(se3);
            //  seriesEntries.Add(se4);
            CalculationProfiler cp = new CalculationProfiler();
            foreach (var version in versions) {
                var v = version;
                var versionEntries =
                    sim.CalculationOutcomes.It.Where(x => x.LPGVersion == v).ToList();
                if (versionEntries.Count == 0) {
                    throw new LPGException("No Entries Found");
                }
                var mnc = new MakeNRWChart(150, 1000, 1600,cp);
                var pngname = Path.Combine(wd.WorkingDirectory, "testchart." + version + ".png");
                // string pdfname = Path.Combine(wd.WorkingDirectory, "testchart." + version + ".pdf");

                mnc.MakeScatterChart(versionEntries, pngname, seriesEntries);
            }

            var imagefiles = FileFinder.GetRecursiveFiles(new DirectoryInfo(wd.WorkingDirectory),
                "testchart.*.png");
            Assert.GreaterOrEqual(imagefiles.Count, 1);
            db.Cleanup();
            wd.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using ChartCreator2.OxyCharts;
using Common;
using Common.Tests;
using Database;
using Database.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace ChartCreator2.Tests.Oxyplot {

    public class MakeNRWChartTests : UnitTestBaseClass
    {
        [StaFact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MakeNRWChartTest()
        {
            CleanTestBase.RunAutomatically(false);
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim = new Simulator(db.ConnectionString);
                    var versions = sim.CalculationOutcomes.Items.Select(x => x.LPGVersion).Distinct().ToList();
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
                    foreach (var version in versions)
                    {
                        var v = version;
                        var versionEntries =
                            sim.CalculationOutcomes.Items.Where(x => x.LPGVersion == v).ToList();
                        if (versionEntries.Count == 0)
                        {
                            throw new LPGException("No Entries Found");
                        }
                        var mnc = new MakeNRWChart(150, 1000, 1600, cp);
                        var pngname = Path.Combine(wd.WorkingDirectory, "testchart." + version + ".png");
                        // string pdfname = Path.Combine(wd.WorkingDirectory, "testchart." + version + ".pdf");

                        mnc.MakeScatterChart(versionEntries, pngname, seriesEntries);
                    }

                    var imagefiles = FileFinder.GetRecursiveFiles(new DirectoryInfo(wd.WorkingDirectory),
                        "testchart.*.png");
                    imagefiles.Count.Should().BeGreaterOrEqualTo(1);
                    db.Cleanup();
                }
                wd.CleanUp();
            }
            CleanTestBase.RunAutomatically(true);
        }

        public MakeNRWChartTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
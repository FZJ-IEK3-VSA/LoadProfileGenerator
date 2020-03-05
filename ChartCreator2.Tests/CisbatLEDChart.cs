/*using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ChartCreator.SettlementMergePlots;
using CommonDataWPF;
using NUnit.Framework;
using SettlementProcessing.CSVProcessing;

namespace ChartCreator.Tests {
    internal static class CisbatLEDChart {
        public class Durchschnittsprofil {
            [Test]
            [Category("QuickChart")]
            public void Run()
            {
                const string glue = "hh1000200w.csv";
                const string led = "hh1000led.csv";
                var dstdir = @"Z:\cisbat\chart";
                Config.CultureInfo = CultureInfo.InvariantCulture;

                var summedFile1 = Path.Combine(@"Z:\cisbat", glue);
           //     var values200 = TotalsReader.ReadDataFiles(null, summedFile1);
                var summedFile2 = Path.Combine(@"Z:\cisbat", led);
         //       var valuesLED = TotalsReader.ReadDataFiles(null, summedFile2);
                var mwp = new MergedWeekdayProfiles();
                //var weekdayEntriesGlue1 = mwp.ConvertToEntries(values200,"Glühlampen", false, MergedWeekdayProfiles.ConversionMode.PerSeasonAndDaytype,60); // 60 wg. kwh zu kw
                //var weekdayEntriesLED = mwp.ConvertToEntries(valuesLED, "LEDs",false, MergedWeekdayProfiles.ConversionMode.PerSeasonAndDaytype, 60);
                /*
                mwp.MakeChart(Path.Combine(dstdir, "Weekday.Lights.All.pdf"), weekdayEntriesGlue1, "kW");
                var mergedList = new List<MergedWeekdayProfiles.WeekdayEntry>();
                mergedList.AddRange(weekdayEntriesGlue1);
                mergedList.AddRange(weekdayEntriesLED);

                var filteredList =
                    mergedList.Where(x => x.ChartName == "Wochentag").ToList();
                var finalList = new List<MergedWeekdayProfiles.WeekdayEntry>();
                foreach (var weekdayEntry in filteredList) {
                    finalList.Add(weekdayEntry.ReduceTo15Min(15));
                }

                foreach (var entry in finalList) {
                    if (entry.LineName == "Glühlampen") {
                        entry.LineName = "incandescent light bulb";
                    }
                }
                const string yaxislabel = "Average Power in ";
                var entries =
                    new Dictionary<string, Dictionary<string, MergedWeekdayProfiles.WeekdayEntry>>();
                var key = "";
                entries.Add(key, new Dictionary<string, MergedWeekdayProfiles.WeekdayEntry>());
                foreach (var entry in finalList) {
                    if (entry.DataSourceName == "Glühlampen" && entry.ChartName == "Wochentag" &&
                        entry.LineName == "Winter") {
                        entries[key].Add("Incandescent", entry);
                    }
                    else if (entry.DataSourceName == "LEDs" && entry.ChartName == "Wochentag" &&
                             entry.LineName == "Winter") {
                        entries[key].Add("LED", entry);
                    }
                }

                mwp.MakeOneDayChart(Path.Combine(dstdir, "Light.pdf"), entries, "kW", yaxislabel, 30);
            }
            
            [Test]
            [Category("QuickChart")]
            public void RunOnlyMerge()
            {
                const string dstDir = @"X:\ResultDir";
                const string dir200W = @"X:\200W1";
                const string srcFileName = "200W.csv";
                var mwp = new MergedWeekdayProfiles();
                var srcFullName = Path.Combine(dstDir, srcFileName);
                var values200 = TotalsReader.ReadDataFiles(dir200W, srcFullName);
                var weekdayEntriesGlue1 = mwp.ConvertToEntries(values200,
                    "Glühlampen", false, MergedWeekdayProfiles.ConversionMode.PerSeasonAndDaytype);

                foreach (var entry in weekdayEntriesGlue1) {
                    var filename = entry.ChartName + "." + entry.LineName + "." + entry.LineName + ".csv";
                    var file = Path.Combine(dstDir, filename);
                    using (var sw = new StreamWriter(file)) {
                        foreach (var valEntry in entry.Values) {
                            sw.WriteLine(valEntry.Key + ";" + valEntry.Value);
                        }
                        sw.Close();
                    }
                }
            }
        }
    }
}
*/
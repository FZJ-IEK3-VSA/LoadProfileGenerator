using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation;
using ChartPDFCreator;
using Common;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.SettlementMergePlots {
    public class MergedAffordanceEnergy {
        private const int Fontsize = 20;

        private static void MakeGeneralBarChart([ItemNotNull] [NotNull] List<AffordanceEntry> entries, [NotNull] string dstDir) {
            var householdNames = entries.Select(x => x.HouseholdName.Trim()).Distinct().ToList();
            // make absolute values
            var plotModel1 = MakePlotmodel(householdNames,
                ChartLocalizer.Get().GetTranslation("Electricity") + " in kWh");
            var affNames = entries.Select(x => x.AffordanceName).Distinct().ToList();
            affNames.Sort((x, y) => string.Compare(x, y, StringComparison.Ordinal));
            OxyPalette p;
            if (affNames.Count < 2) {
                p = OxyPalettes.Hue64;
            }
            else {
                p = OxyPalettes.HueDistinct(affNames.Count);
            }
            for (var i = 0; i < affNames.Count; i++) {
                var tag = affNames[i];
                var columnSeries2 = new ColumnSeries
                {
                    IsStacked = true,
                    StackGroup = "1",
                    StrokeThickness = 1,
                    StrokeColor = OxyColors.White,
                    Title = ChartLocalizer.Get().GetTranslation(tag),
                    LabelPlacement = LabelPlacement.Middle,
                    FillColor = p.Colors[i]
                };
                foreach (var householdName in householdNames) {
                    var te =
                        entries.FirstOrDefault(x => x.AffordanceName == tag && x.HouseholdName == householdName);
                    if (te != null) {
                        columnSeries2.Items.Add(new ColumnItem(te.Value));
                    }
                    else {
                        columnSeries2.Items.Add(new ColumnItem(0));
                    }
                }
                plotModel1.Series.Add(columnSeries2);
            }

            var fileName = Path.Combine(dstDir, "MergedAffordanceEnergyUse.pdf");
            OxyPDFCreator.Run(plotModel1, fileName);
            var hhSums = new Dictionary<string, double>();
            var households = entries.Select(x => x.HouseholdName).Distinct().ToList();
            foreach (var household in households) {
                var sum = entries.Where(x => x.HouseholdName == household).Select(x => x.Value).Sum();
                hhSums.Add(household, sum);
            }
            foreach (var affordanceName in affNames) {
                var plotModel2 = MakePlotmodel(householdNames, "Anteil am Gesamtverbrauch in Prozent");
                plotModel2.LegendFontSize = Fontsize;
                var columnSeries2 = new ColumnSeries
                {
                    IsStacked = true,
                    StackGroup = "1",
                    StrokeThickness = 1,
                    Title = ChartLocalizer.Get().GetTranslation(affordanceName),
                    LabelPlacement = LabelPlacement.Middle,
                    FillColor = OxyColors.LightBlue
                };
                var averageValue =
                    entries.Where(x => x.AffordanceName == affordanceName)
                        .Select(x => x.Value / hhSums[x.HouseholdName] * 100)
                        .Average();
                var ls = new LineSeries
                {
                    Color = OxyColors.Red,
                    Title = "Durchschnitt"
                };
                for (var i = 0; i < householdNames.Count; i++) {
                    var householdName = householdNames[i];
                    ls.Points.Add(new DataPoint(i, averageValue));
                    var te =
                        entries.FirstOrDefault(
                            x => x.AffordanceName == affordanceName && x.HouseholdName == householdName);

                    if (te != null) {
                        columnSeries2.Items.Add(new ColumnItem(te.Value / hhSums[householdName] * 100));
                    }
                    else {
                        columnSeries2.Items.Add(new ColumnItem(0));
                    }
                }
                plotModel2.Series.Add(columnSeries2);
                plotModel2.Series.Add(ls);
                var cleanTag = AutomationUtili.CleanFileName(affordanceName);
                var relfileName = Path.Combine(dstDir, "MergedAffordanceTaggingEnergyUse." + cleanTag + ".pdf");
                OxyPDFCreator.Run(plotModel2, relfileName);
            }
        }

        [NotNull]
        private static PlotModel MakePlotmodel([ItemNotNull] [NotNull] List<string> householdNames, [NotNull] string yaxislabel) {
            var plotModel1 = new PlotModel
            {
                DefaultFontSize = Fontsize,
                LegendFontSize = 10,
                LegendBorderThickness = 0,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendSymbolMargin = 20
            };

            var categoryAxis1 = new CategoryAxis
            {
                MajorStep = 1,
                Minimum = -0.5,
                Angle = 45,
                MaximumPadding = 0.03,
                AxisTickToLabelDistance = 60
            };
            for (var i = 0; i < householdNames.Count; i++) {
                if (i % 5 == 0) {
                    var householdName = householdNames[i];
                    categoryAxis1.ActualLabels.Add(householdName.Substring(0, 5));
                }
                else {
                    categoryAxis1.ActualLabels.Add(string.Empty);
                }
            }
            categoryAxis1.GapWidth = 0;
            plotModel1.Axes.Add(categoryAxis1);
            var linearAxis1 = new LinearAxis
            {
                AbsoluteMinimum = 0,
                MaximumPadding = 0.06,
                MinimumPadding = 0,
                Title = yaxislabel
            };
            plotModel1.Axes.Add(linearAxis1);
            return plotModel1;
        }

        public void Run([NotNull] Dictionary<string, List<AffordanceEntry>> entries, [NotNull] string dstDir) {
            var allEntries = new List<AffordanceEntry>();
            foreach (var pair in entries) {
                allEntries.AddRange(pair.Value);
            }
            MakeGeneralBarChart(allEntries, dstDir);
        }

        public class AffordanceEntry {
            public AffordanceEntry([NotNull] string affName, double value, [NotNull] string householdName) {
                AffordanceName = affName;
                Value = value;
                HouseholdName = householdName;
            }

            [NotNull]
            public string AffordanceName { get; }

            [NotNull]
            public string HouseholdName { get; }
            public double Value { get; }
        }
    }
}
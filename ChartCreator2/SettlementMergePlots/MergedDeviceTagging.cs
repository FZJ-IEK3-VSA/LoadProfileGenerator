using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class MergedDeviceTagging {
        private const int Fontsize = 30;

        private static void MakeBarCharts([JetBrains.Annotations.NotNull] string setName, [ItemNotNull] [JetBrains.Annotations.NotNull] List<TagEntry> entries, [JetBrains.Annotations.NotNull] string dstDirectory) {
            var householdNames = entries.Select(x => x.HouseholdName.Trim()).Distinct().ToList();
            // make absolute values
            var plotModel1 = MakePlotmodel(householdNames,
                ChartLocalizer.Get().GetTranslation("Electricity") + " in kWh");
            var tagNames = entries.Select(x => x.TagName).Distinct().ToList();
            OxyPalette p;
            if (tagNames.Count < 2) {
                p = OxyPalettes.Hue64;
            }
            else {
                p = OxyPalettes.HueDistinct(tagNames.Count);
            }
            for (var i = 0; i < tagNames.Count; i++) {
                var tag = tagNames[i];

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
                    var te = entries.FirstOrDefault(x => x.TagName == tag && x.HouseholdName == householdName);
                    if (te != null) {
                        columnSeries2.Items.Add(new ColumnItem(te.Value));
                    }
                    else {
                        columnSeries2.Items.Add(new ColumnItem(0));
                    }
                }
                plotModel1.Series.Add(columnSeries2);
            }
            var fileName = "MergedDeviceTagging." + setName + ".pdf";
            OxyPDFCreator.Run(plotModel1, fileName);
            var hhSums = new Dictionary<string, double>();
            foreach (var householdName in householdNames) {
                var sum = entries.Where(x => x.HouseholdName == householdName).Select(x => x.Value).Sum();
                hhSums.Add(householdName, sum);
            }
            foreach (var tagName in tagNames) {
                var plotModel2 = MakePlotmodel(householdNames, "Anteil am Gesamtverbrauch in Prozent");
                var columnSeries2 = new ColumnSeries
                {
                    IsStacked = true,
                    StackGroup = "1",
                    StrokeThickness = 1,
                    StrokeColor = OxyColors.White,
                    Title = ChartLocalizer.Get().GetTranslation(tagName),
                    LabelPlacement = LabelPlacement.Middle,
                    FillColor = OxyColors.LightBlue
                };
                foreach (var householdName in householdNames) {
                    var te =
                        entries.FirstOrDefault(x => x.TagName == tagName && x.HouseholdName == householdName);

                    if (te != null) {
                        columnSeries2.Items.Add(new ColumnItem(te.Value / hhSums[householdName] * 100));
                    }
                    else {
                        columnSeries2.Items.Add(new ColumnItem(0));
                    }
                }
                plotModel2.Series.Add(columnSeries2);
                var cleanTag = AutomationUtili.CleanFileName(tagName);
                var relfileName = Path.Combine(dstDirectory,
                    "MergedDeviceTagging." + setName + "." + cleanTag + ".pdf");
                OxyPDFCreator.Run(plotModel2, relfileName);
            }
        }

        [JetBrains.Annotations.NotNull]
        private static PlotModel MakePlotmodel([ItemNotNull] [JetBrains.Annotations.NotNull] List<string> householdNames, [JetBrains.Annotations.NotNull] string yaxislabel) {
            var plotModel1 = new PlotModel
            {
                DefaultFontSize = Fontsize,
                LegendFontSize = Fontsize,
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
                var householdName = householdNames[i];
                if (i % 5 == 0) {
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

        public void Run([JetBrains.Annotations.NotNull] Dictionary<string, List<TagEntry>> consumption, [JetBrains.Annotations.NotNull] string dstDirectory) {
            foreach (var pair in consumption) {
                MakeBarCharts(pair.Key, pair.Value, dstDirectory);
            }
        }

        public class TagEntry {
            public TagEntry([JetBrains.Annotations.NotNull] string tagName, double value, [JetBrains.Annotations.NotNull] string householdName) {
                TagName = tagName;
                Value = value;
                HouseholdName = householdName;
            }

            [JetBrains.Annotations.NotNull]
            public string HouseholdName { get; }
            [JetBrains.Annotations.NotNull]
            public string TagName { get; }
            public double Value { get; }
        }
    }
}
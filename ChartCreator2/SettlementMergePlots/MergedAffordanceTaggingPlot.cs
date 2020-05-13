using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    public class MergedAffordanceTaggingPlot {
        private const int Fontsize = 30;

        private static void MakeBarCharts([ItemNotNull] [JetBrains.Annotations.NotNull] List<AffTagEntry> entries, [JetBrains.Annotations.NotNull] string dstDirectory) {
            var personNames = entries.Select(x => x.PersonName.Trim()).Distinct().ToList();
            // make absolute values
            var plotModel1 = MakePlotmodel(personNames, "Simulationszeit in Prozent");
            var tagNames = entries.Select(x => x.AffTagName).Distinct().ToList();
            OxyPalette p;
            if (tagNames.Count < 2) {
                p = OxyPalettes.Hue64;
            }
            else {
                p = OxyPalettes.HueDistinct(tagNames.Count);
            }
            var personSums = new Dictionary<string, double>();
            foreach (var personName in personNames) {
                var sum = entries.Where(x => x.PersonName == personName).Select(x => x.Value).Sum();
                personSums.Add(personName, sum);
            }

            for (var i = 0; i < tagNames.Count; i++) {
                var tag = tagNames[i];
                var columnSeries2 = new ColumnSeries
                {
                    IsStacked = true,
                    StackGroup = "1",
                    XAxisKey = "N",
                    StrokeThickness = 1,
                    Title = ChartLocalizer.Get().GetTranslation(tag),
                    LabelPlacement = LabelPlacement.Middle,
                    StrokeColor = OxyColors.White,
                    FillColor = p.Colors[i]
                };
                foreach (var personName in personNames) {
                    var te = entries.FirstOrDefault(x => x.AffTagName == tag && x.PersonName == personName);
                    if (te != null) {
                        columnSeries2.Items.Add(new ColumnItem(te.Value / personSums[te.PersonName] * 100));
                    }
                    else {
                        columnSeries2.Items.Add(new ColumnItem(0));
                    }
                }
                plotModel1.Series.Add(columnSeries2);
            }
            const string fileName = "MergedAffordanceTaggingSet.WoBleibtDieZeit.Absolute.pdf";
            OxyPDFCreator.Run(plotModel1, fileName);
            foreach (var tagName in tagNames) {
                var plotModel2 = MakePlotmodel(personNames, "Anteil an der Gesamtzeit in Prozent");
                var columnSeries2 = new ColumnSeries
                {
                    IsStacked = true,
                    StackGroup = "1",
                    StrokeThickness = 1,
                    StrokeColor = OxyColors.White,
                    XAxisKey = "N",
                    Title = ChartLocalizer.Get().GetTranslation(tagName),
                    LabelPlacement = LabelPlacement.Middle,
                    FillColor = OxyColors.LightBlue
                };
                var averageValue =
                    entries.Where(x => x.AffTagName == tagName)
                        .Select(x => x.Value / personSums[x.PersonName] * 100)
                        .Average();
                var ls = new LineSeries
                {
                    Color = OxyColors.Red,
                    Title = "Durchschnitt"
                };
                for (var i = 0; i < personNames.Count; i++) {
                    var personName = personNames[i];
                    ls.Points.Add(new DataPoint(i, averageValue));
                    var te =
                        entries.FirstOrDefault(x => x.AffTagName == tagName && x.PersonName == personName);

                    if (te != null) {
                        columnSeries2.Items.Add(new ColumnItem(te.Value / personSums[personName] * 100));
                    }
                    else {
                        columnSeries2.Items.Add(new ColumnItem(0));
                    }
                }
                plotModel2.Series.Add(columnSeries2);
                plotModel2.Series.Add(ls);
                var cleanTag = AutomationUtili.CleanFileName(tagName);
                var relfileName = Path.Combine(dstDirectory,
                    "MergedAffordanceTaggingSet.WoBleibtDieZeit." + cleanTag + ".pdf");
                OxyPDFCreator.Run(plotModel2, relfileName);
            }
        }

        [JetBrains.Annotations.NotNull]
        private static PlotModel MakePlotmodel([ItemNotNull] [JetBrains.Annotations.NotNull] List<string> personNames, [JetBrains.Annotations.NotNull] string yaxislabel) {
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
                MaximumPadding = 0.02,
                Title = "Personen",
                Key = "N",
                MinimumPadding = 0,
                FontSize = Fontsize
            };
            var categoryAxis2 = new CategoryColorAxis
            {
                Position = AxisPosition.Top,
                Minimum = -0.5,
                MinimumPadding = 0,
                MajorStep = 1
            };
            categoryAxis2.MinimumPadding = 0;
            categoryAxis2.TicklineColor = OxyColors.White;
            categoryAxis2.MaximumPadding = 0.02;
            categoryAxis2.MajorTickSize = 20;
            categoryAxis2.Key = "coloraxis";
            categoryAxis2.Title = "Haushalt";
            var lastHH = string.Empty;
            var distancecount = 30;
            var colors = new List<OxyColor>();
            var lastColor = OxyColors.LightBlue;

            for (var i = 0; i < personNames.Count; i++) {
                distancecount++;
                var hhName = personNames[i].Substring(0, 5);
                if (lastHH != hhName) {
                    lastHH = hhName;
                    if (lastColor == OxyColors.LightBlue) {
                        lastColor = OxyColors.Green;
                    }
                    else {
                        lastColor = OxyColors.LightBlue;
                    }
                    if (distancecount > 20) {
                        categoryAxis2.Labels.Add(hhName);
                        distancecount = 0;
                    }
                    else {
                        categoryAxis2.Labels.Add(" ");
                    }
                }
                else {
                    categoryAxis2.Labels.Add(" ");
                }
                colors.Add(lastColor);
                if (i % 10 == 0) {
                    categoryAxis1.ActualLabels.Add(i.ToString(CultureInfo.CurrentCulture));
                }
                else {
                    categoryAxis1.ActualLabels.Add(" ");
                }
            }
            Logger.Info("Category labels: " + categoryAxis2.Labels.Count);
            Logger.Info("Total number of colors: " + colors.Count);
            colors.Add(OxyColors.White);
            var p2 = new OxyPalette(colors);
            categoryAxis2.Palette = p2;
            plotModel1.Axes.Add(categoryAxis2);
            categoryAxis1.GapWidth = 0;
            plotModel1.Axes.Add(categoryAxis1);
            var linearAxis1 = new LinearAxis
            {
                AbsoluteMinimum = 0,
                MaximumPadding = 0.01,
                MinimumPadding = 0,
                MinorTickSize = 0,
                Title = yaxislabel
            };
            plotModel1.Axes.Add(linearAxis1);
            return plotModel1;
        }

        public void Run([ItemNotNull] [JetBrains.Annotations.NotNull] List<AffTagEntry> consumption, [JetBrains.Annotations.NotNull] string dstDir) {
            MakeBarCharts(consumption, dstDir);
        }

        public class AffTagEntry {
            public AffTagEntry([JetBrains.Annotations.NotNull] string affTagName, double value, [JetBrains.Annotations.NotNull] string personName) {
                AffTagName = affTagName;
                Value = value;
                //  HouseholdName = householdName;
                PersonName = personName;
            }

            //private string HouseholdName { get; }
            [JetBrains.Annotations.NotNull]
            public string AffTagName { get; }

            [JetBrains.Annotations.NotNull]
            public string PersonName { get; }
            public double Value { get; }
        }
    }
}
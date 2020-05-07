using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Database.Tables.Validation;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using CategoryAxis = OxyPlot.Axes.CategoryAxis;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;
using ScatterSeries = OxyPlot.Series.ScatterSeries;

namespace ChartCreator2.OxyCharts {
    internal class MakeNRWChart {
        private readonly int _dpi;
        private const int FontSize = 24;
        private readonly int _height;
        private readonly int _width;
        [JetBrains.Annotations.NotNull] private readonly CalculationProfiler _calculationProfiler;
        private const string Averagelabel = "Average Energy Consumption in Germany";
        private const string Xaxislabel = "Number of Persons in the Household";

        private const string Yaxislabel = "Energy Consumption in kWh";

        public MakeNRWChart(int dpi, int height, int width, [JetBrains.Annotations.NotNull] CalculationProfiler calculationProfiler) {
            _dpi = dpi;
            _height = height;
            _width = width;
            _calculationProfiler = calculationProfiler;
        }

        private static void AddNRWPoints([JetBrains.Annotations.NotNull] LineSeries sc, int multiplicator) {
            sc.Points.Add(new DataPoint(1 * multiplicator, 1798));
            sc.Points.Add(new DataPoint(2 * multiplicator, 2850));
            sc.Points.Add(new DataPoint(3 * multiplicator, 3733));
            sc.Points.Add(new DataPoint(4 * multiplicator, 4480));
            sc.Points.Add(new DataPoint(5 * multiplicator, 5311));
            sc.Points.Add(new DataPoint(6 * multiplicator, 5816));
        }

        public void MakeScatterChart([ItemNotNull] [JetBrains.Annotations.NotNull] List<CalculationOutcome> outcomes, [JetBrains.Annotations.NotNull] string pngfullName, [ItemNotNull] [JetBrains.Annotations.NotNull] List<SeriesEntry> series) {
            _calculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            var plotModel1 = new PlotModel
            {
                LegendBorderThickness = 0,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                DefaultFontSize = FontSize,
                LegendFontSize = FontSize,
                DefaultFont = "Arial",
                LegendFont = "Arial"
            };
            var ca = new CategoryAxis
            {
                Minimum = 0.4,
                Title = "Number of Persons",
                Position = AxisPosition.Bottom
            };
            ca.Title = Xaxislabel;
            ca.Labels.Add("0");
            ca.Labels.Add("1");
            ca.Labels.Add("2");
            ca.Labels.Add("3");
            ca.Labels.Add("4");
            ca.Labels.Add("5");
            ca.Labels.Add("6");
            ca.MaximumPadding = 0.02;
            plotModel1.Axes.Add(ca);

            var la = new LinearAxis
            {
                Minimum = 0,
                Position = AxisPosition.Left,
                Title = Yaxislabel,
                MaximumPadding = 0.02
            };
            plotModel1.Axes.Add(la);

            var sc = new LineSeries
            {
                LineStyle = LineStyle.Dash,
                MarkerFill = OxyColors.SkyBlue,
                MarkerSize = 5,
                MarkerType = MarkerType.Circle,
                StrokeThickness = 3
            };

            AddNRWPoints(sc, 1);
            sc.Title = Averagelabel;
            plotModel1.Series.Add(sc);

            var energyIntensities = outcomes.Select(x => x.EnergyIntensity).Distinct().ToList();
            energyIntensities.Sort((x, y) => string.Compare(x, y, StringComparison.Ordinal));
            series.Sort((x, y) => string.Compare(x.Version, y.Version, StringComparison.Ordinal));
            OxyPalette p = OxyPalettes.Hue64;
            if (series.Count > 1) {
                p = OxyPalettes.HueDistinct(series.Count);
            }

            var i = 0;
            for (var index = 0; index < series.Count; index++) {
                var seriesEntry = series[index];
                var entrylist = outcomes
                    .Where(x => x.EnergyIntensity == seriesEntry.EnergyIntensity &&
                                x.LPGVersion == seriesEntry.Version)
                    .ToList();
                var sc1 = new ScatterSeries();
                foreach (var entry in entrylist) {
                    sc1.Points.Add(
                        new ScatterPoint(entry.NumberOfPersons + seriesEntry.Offset, entry.ElectricityDouble));
                }

                sc1.Title = seriesEntry.DisplayName;
                var oc = p.Colors[i++];
                sc1.MarkerStroke = OxyColor.FromAColor(128, oc);
                sc1.MarkerFill = oc;
                if (index == 0) {
                    sc1.MarkerType = MarkerType.Diamond;
                }
                else {
                    sc1.MarkerType = MarkerType.Star;
                }
                sc1.MarkerStrokeThickness = 1;
                plotModel1.Series.Add(sc1);
            }
            PngExporter.Export(plotModel1, pngfullName, _width, _height, OxyColor.FromRgb(255, 255, 255), _dpi);
            _calculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            //ChartPDFCreator.OxyPDFCreator.Run(plotModel1, pdfFullName);
        }

        internal class SeriesEntry {
            public SeriesEntry(double offset, [JetBrains.Annotations.NotNull] string energyIntensity, [JetBrains.Annotations.NotNull] string displayName, [JetBrains.Annotations.NotNull] string version) {
                Offset = offset;
                EnergyIntensity = energyIntensity;
                DisplayName = displayName;
                Version = version;
            }

            [JetBrains.Annotations.NotNull]
            public string DisplayName { get; }
            [JetBrains.Annotations.NotNull]
            public string EnergyIntensity { get; }

            public double Offset { get; }
            [JetBrains.Annotations.NotNull]
            public string Version { get; }
        }
    }
}
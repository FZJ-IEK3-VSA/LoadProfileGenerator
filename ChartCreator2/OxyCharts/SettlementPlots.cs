using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using CategoryAxis = OxyPlot.Axes.CategoryAxis;
using ColumnSeries = OxyPlot.Series.ColumnSeries;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;

namespace ChartCreator2.OxyCharts {
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    internal class SettlementPlots {
        private const bool BarPlots = true;
        private const bool LinePlots = true;
        private const bool TotalLinePlot = true;

        private static void MakeBarPlot([JetBrains.Annotations.NotNull] string outputPath, [ItemNotNull] [JetBrains.Annotations.NotNull] List<Column> columns, int position, int day,
            int minutesToSum) {
            var plotModel2 = new PlotModel();
            var p = OxyPalettes.HueDistinct(columns.Count);
            plotModel2.LegendPosition = LegendPosition.BottomCenter;
            plotModel2.LegendPlacement = LegendPlacement.Outside;
            plotModel2.LegendOrientation = LegendOrientation.Horizontal;
            plotModel2.Title = "Day " + day;
            // axes
            var categoryAxis = new CategoryAxis
            {
                AbsoluteMinimum = 0,
                MinimumPadding = 0,
                GapWidth = 0,
                MajorStep = 60,
                Title = "Energy"
            };
            plotModel2.Axes.Add(categoryAxis);

            var linearAxis2 = new LinearAxis
            {
                AbsoluteMinimum = 0,
                MaximumPadding = 0.06,
                MinimumPadding = 0,
                Title = "Minutes"
            };
            plotModel2.Axes.Add(linearAxis2);

            for (var i = 1; i < columns.Count; i++) {
                var columnSeries2 = new ColumnSeries
                {
                    IsStacked = true,
                    StrokeThickness = 0,
                    Title = columns[i].HHNumber
                };
                for (var j = position; j < position + 1440; j += minutesToSum) {
                    columnSeries2.Items.Add(new ColumnItem(columns[i].MakeSum(j, minutesToSum)));
                }
                columnSeries2.FillColor = p.Colors[i];
                plotModel2.Series.Add(columnSeries2);
            }
            var path2 = Path.Combine(outputPath, "Plot." + day + "." + minutesToSum + "min.bar.png");
            PngExporter.Export(plotModel2, path2, 3200, 1600, OxyColor.FromRgb(255, 255, 255), 100);
        }

        private static void MakeLinePlot([JetBrains.Annotations.NotNull] string outputPath, [ItemNotNull] [JetBrains.Annotations.NotNull] List<Column> columns, int position, int day) {
            var p = OxyPalettes.HueDistinct(columns.Count);
            var plotModel1 = new PlotModel
            {
                LegendPosition = LegendPosition.BottomCenter,
                LegendPlacement = LegendPlacement.Outside,
                LegendOrientation = LegendOrientation.Horizontal,
                Title = "Day " + day
            };
            var linearAxis1 = new LinearAxis
            {
                Position = AxisPosition.Bottom
            };
            plotModel1.Axes.Add(linearAxis1);

            for (var i = 1; i < columns.Count; i++) {
                var lineSeries1 = new LineSeries
                {
                    Title = columns[i].HHNumber,
                    Color = p.Colors[i]
                };
                for (var j = position; j < position + 1440; j++) {
                    lineSeries1.Points.Add(new DataPoint(j, columns[i].Values[j]));
                }
                plotModel1.Series.Add(lineSeries1);
            }
            var path = Path.Combine(outputPath, "Plot." + day + ".line.png");
            PngExporter.Export(plotModel1, path, 3200, 1600, OxyColor.FromRgb(255, 255, 255), 100);
        }

        private static void MakeTotalLinePlot([JetBrains.Annotations.NotNull] string outputPath, [JetBrains.Annotations.NotNull] List<double> sums) {
            var plotModel1 = new PlotModel
            {
                LegendPosition = LegendPosition.BottomCenter,
                LegendPlacement = LegendPlacement.Outside,
                LegendOrientation = LegendOrientation.Horizontal,
                Title = "Total"
            };
            var linearAxis1 = new LinearAxis
            {
                Position = AxisPosition.Bottom
            };
            plotModel1.Axes.Add(linearAxis1);
            var lineSeries1 = new LineSeries
            {
                Title = "Sum"
            };
            for (var j = 0; j < sums.Count; j++) {
                lineSeries1.Points.Add(new DataPoint(j, sums[j]));
            }
            plotModel1.Series.Add(lineSeries1);
            var path = Path.Combine(outputPath, "Sum.line.png");
            PngExporter.Export(plotModel1, path, 3200, 1600, OxyColor.FromRgb(255, 255, 255), 100);
        }

        private static void ReadFile([JetBrains.Annotations.NotNull] string fullName,
                                     [ItemNotNull] [JetBrains.Annotations.NotNull] List<Column> columns,
                                    [CanBeNull] int? limit,
                                     [ItemNotNull] [JetBrains.Annotations.NotNull] string[] csvArr)
        {
            using (var sr = new StreamReader(fullName)) {
                var header = sr.ReadLine();
                if (header == null) {
                    throw new LPGException("Readline failed");
                }
                var headers = header.Split(csvArr, StringSplitOptions.None);
                foreach (var s in headers) {
                    var col = new Column(s);
                    columns.Add(col);
                }
                var line = 0;
                var maxline = int.MaxValue;
                if (limit != null) {
                    maxline = (int) limit;
                }
                while (!sr.EndOfStream && line < maxline) {
                    line++;
                    if (line % 1000 == 0) {
                        Logger.Info("Line: " + line);
                    }
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }
                    var cols = s.Split(';');
                    for (var i = 0; i < cols.Length; i++) {
                        if (i != 1 && i != 500) {
                            if (!string.IsNullOrWhiteSpace(cols[i].Trim())) {
                                var col = columns[i];
                                var val = cols[i];
                                var mydouble = Utili.ConvertToDoubleWithMessage(val, "Col " + i);
                                col.Values.Add(mydouble);
                            }
                        }
                    }
                }
                sr.Close();
            }
            columns.RemoveAt(columns.Count - 1); // last column is empty
            columns.RemoveAt(1); // 2nd column is dates
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public void Run([JetBrains.Annotations.NotNull] string fullName, [JetBrains.Annotations.NotNull] string outputPath, [JetBrains.Annotations.NotNull] string csvChar) {
            var columns = new List<Column>();

            ReadFile(fullName, columns, null, new[] {csvChar});
            var sums = new List<double>();
            for (var i = 0; i < columns[0].Values.Count - 1; i++) {
                double sum = 0;
                for (var j = 1; j < columns.Count; j++) {
                    sum += columns[j].Values[i];
                }
                sums.Add(sum);
            }
            if (TotalLinePlot) {
                MakeTotalLinePlot(outputPath, sums);
            }

            var position = 0;
            var day = 1;
            while (position + 1440 < columns[0].Values.Count) {
                if (LinePlots) {
                    MakeLinePlot(outputPath, columns, position, day);
                }
                // stacked bars
                if (BarPlots) {
                    MakeBarPlot(outputPath, columns, position, day, 60);
                }
                day++;
                position += 1440;
            }
        }

        private class Column {
            [JetBrains.Annotations.NotNull] private readonly string _name;

            public Column([JetBrains.Annotations.NotNull] string name) {
                _name = name;
                Values = new List<double>();
            }

            [JetBrains.Annotations.NotNull]
            public string HHNumber {
                get {
                    var name = _name;
                    name = name.Substring(name.IndexOf(" House ", StringComparison.CurrentCulture) + 6);
                    name = name.Trim();
                    return name.Substring(0, name.IndexOf(" ", StringComparison.CurrentCulture)).Trim();
                }
            }

            [JetBrains.Annotations.NotNull]
            public List<double> Values { get; }

            public double MakeSum(int start, int count) {
                double sum = 0;
                for (var i = start; i < start + count; i++) {
                    sum += Values[i];
                }
                return sum;
            }
        }
    }
}
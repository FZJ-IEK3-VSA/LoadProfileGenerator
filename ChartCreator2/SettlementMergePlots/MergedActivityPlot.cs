using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
using CategoryAxis = OxyPlot.Axes.CategoryAxis;
using ColumnSeries = OxyPlot.Series.ColumnSeries;
using LinearAxis = OxyPlot.Axes.LinearAxis;

namespace ChartCreator2.SettlementMergePlots {
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public static class MergedActivityPlot {
        private static void MakeBarPlot([JetBrains.Annotations.NotNull] string outputPath, [ItemNotNull] [JetBrains.Annotations.NotNull] List<Column> columns, int position, int day) {
            var plotModel2 = new PlotModel();
            // filter significant columns
            var allColumns = new List<Tuple<string, double>>();
            for (var i = 1; i < columns.Count; i++) {
                allColumns.Add(new Tuple<string, double>(columns[i].Name, columns[i].MakeSum(position, 1440)));
            }
            allColumns.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            var topCols = allColumns.Select(x => x.Item1).Take(20).ToList();
            var p = OxyPalettes.HueDistinct(topCols.Count);
            var oxc = OxyColor.FromArgb(255, 50, 50, 50);
            plotModel2.LegendPosition = LegendPosition.BottomCenter;
            plotModel2.LegendPlacement = LegendPlacement.Outside;
            plotModel2.LegendOrientation = LegendOrientation.Horizontal;
            plotModel2.Title = "Day " + day;
            // axes
            var cate = new CategoryAxis
            {
                AbsoluteMinimum = 0,
                MinimumPadding = 0,
                GapWidth = 0,
                MajorStep = 60,
                Title = "Activities"
            };
            plotModel2.Axes.Add(cate);

            var linearAxis2 = new LinearAxis
            {
                AbsoluteMinimum = 0,
                MaximumPadding = 0.06,
                MinimumPadding = 0,
                Title = "Minutes"
            };
            plotModel2.Axes.Add(linearAxis2);
            const int minutesToSum = 15;
            for (var i = 1; i < columns.Count; i++) {
                if (columns[i].MakeSum(position, 1440) > 0) {
                    var columnSeries2 = new ColumnSeries
                    {
                        IsStacked = true,
                        StrokeThickness = 0,
                        Title = columns[i].Name
                    };
                    for (var j = position; j < position + 1440; j += minutesToSum) {
                        columnSeries2.Items.Add(new ColumnItem(columns[i].MakeSum(j, minutesToSum) / minutesToSum));
                    }
                    if (topCols.Contains(columns[i].Name)) {
                        var coloridx = topCols.IndexOf(columns[i].Name);
                        columnSeries2.FillColor = p.Colors[coloridx];
                    }
                    else {
                        columnSeries2.FillColor = oxc;
                    }
                    plotModel2.Series.Add(columnSeries2);
                }
            }
            var path2 = Path.Combine(outputPath, "ActivityPlot." + day + ".bar.png");
            PngExporter.Export(plotModel2, path2, 3200, 1600, OxyColor.FromRgb(255, 255, 255), 100);
        }

        private static void ReadFile([JetBrains.Annotations.NotNull] string fullName, [ItemNotNull] [JetBrains.Annotations.NotNull] List<Column> columns, [CanBeNull] int? limit) {
            using (var sr = new StreamReader(fullName)) {
                var header = sr.ReadLine();
                if (header == null) {
                    throw new LPGException("File " + fullName + " was null");
                }
                var headers = header.Split(';');
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
                        Logger.Info("Merged Activity Plot Reading Line: " + line);
                    }
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("File " + fullName + " was empty.");
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
            // columns.RemoveAt(columns.Count - 1); // last column is empty
            columns.RemoveAt(1); // 2nd column is dates
            var colsTodelete = columns.Where(x => string.IsNullOrWhiteSpace(x.Name)).ToList();
            foreach (var column in colsTodelete) {
                columns.Remove(column);
            }
        }

        public static void Run([JetBrains.Annotations.NotNull] string fullName, [JetBrains.Annotations.NotNull] string outputPath) {
            var columns = new List<Column>();

            ReadFile(fullName, columns, null);
            var position = 0;
            var day = 1;
            var totaldays = (int) (columns[0].Values.Count / 1440.0);
            Logger.Info("Merged Activity Plots: Found " + totaldays + " days. ");
            while (position + 1440 < columns[0].Values.Count) {
                Logger.Info("Creating Bar Plot for " + day + "/" + totaldays + " days. ");
                MakeBarPlot(outputPath, columns, position, day);
                day++;
                position += 1440;
            }
        }

        private class Column {
            public Column([JetBrains.Annotations.NotNull] string name) {
                Name = name;
                Values = new List<double>();
            }

            [JetBrains.Annotations.NotNull]
            public string Name { get; }
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
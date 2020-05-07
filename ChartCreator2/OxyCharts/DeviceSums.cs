using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    internal class DeviceSums : ChartBaseFileStep
    {
        public DeviceSums([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                          [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                          [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.DeviceSums
            },
            "Device Sums", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcEntry)
        {
            return FileProcessingResult.NoFilesTocreate;
            /*
            string plotName = "Device Sums " + srcEntry.HouseholdNumberString + " " + srcEntry.LoadTypeInformation?.Name;
            _CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            const int numberOfFixedColumnsBeforeFirstTaggingSet = 6;
            var consumption = new List<Tuple<string, double>>();
            var taggingSets = new List<ChartTaggingSet>();
            using (var sr = new StreamReader(srcEntry.FullFileName)) {
                var top = sr.ReadLine();
                if (top == null) {
                    throw new LPGException("Empty file:" + srcEntry.FullFileName);
                }
                var headerArr = top.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                for (var i = numberOfFixedColumnsBeforeFirstTaggingSet; i < headerArr.Length; i++) {
                    if (!string.IsNullOrWhiteSpace(headerArr[i])) {
                        taggingSets.Add(new ChartTaggingSet(headerArr[i]));
                    }
                }
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }
                    if (!s.StartsWith("Sums" + _Parameters.CSVCharacter, StringComparison.Ordinal)) {
                        var cols = s.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                        var d = Math.Abs(Convert.ToDouble(cols[1], CultureInfo.CurrentCulture));
                        consumption.Add(new Tuple<string, double>(cols[0], d));
                        for (var i = numberOfFixedColumnsBeforeFirstTaggingSet; i < cols.Length - 1; i++) {
                            taggingSets[i - numberOfFixedColumnsBeforeFirstTaggingSet].AffordanceToCategories.Add(
                                cols[0], cols[i]);
                        }
                    }
                }
            }
            var filtered = consumption.Where(x => x.Item2 > 0.05).ToList();
            foreach (var set in taggingSets) {
                MakeIntervalBars(srcEntry, plotName, _Parameters.BaseDirectory, filtered, set, "." + set.Name);
            }
            _CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            if (taggingSets.Count > 0) {
                return FileProcessingResult.ShouldCreateFiles;
            }
            return FileProcessingResult.NoFilesTocreate;*/
        }

        public void MakePlotMonthly([JetBrains.Annotations.NotNull] ResultFileEntry rfe, [JetBrains.Annotations.NotNull] string plotName, [JetBrains.Annotations.NotNull] DirectoryInfo basisPath) {
            _CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            var consumption = new List<Tuple<string, List<double>>>();
            var months = 0;
            double totalSum = 0;
            using (var sr = new StreamReader(rfe.FullFileName)) {
                sr.ReadLine(); // header
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed.");
                    }
                    if (!s.StartsWith("Sums;", StringComparison.Ordinal)) {
                        var cols = s.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                        var l = new List<double>();
                        for (var i = 1; i < cols.Length; i++) {
                            if (cols[i].Length > 0) {
                                var d = Convert.ToDouble(cols[i], CultureInfo.CurrentCulture);
                                totalSum += d;
                                l.Add(d);
                            }
                        }
                        consumption.Add(new Tuple<string, List<double>>(cols[0], l));
                        if (l.Count > months) {
                            months = l.Count;
                        }
                    }
                }
            }
            if (consumption.Count == 0) {
                return;
            }
            var plotModel1 = new PlotModel
            {
                // general
                LegendBorderThickness = 0,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                Title = plotName
            };
            // axes
            var categoryAxis1 = new CategoryAxis
            {
                MinorStep = 1
            };
            var colSums = new Dictionary<int, double>();
            for (var i = 0; i < months; i++) {
                categoryAxis1.Labels.Add("Month " + (i + 1));
                colSums.Add(i, 0);
            }

            plotModel1.Axes.Add(categoryAxis1);
            var linearAxis1 = new LinearAxis
            {
                AbsoluteMinimum = 0,
                MaximumPadding = 0.06,
                MinimumPadding = 0
            };

            plotModel1.Axes.Add(linearAxis1);
            // generate plot
            var p = OxyPalettes.Hue64;
            if (consumption.Count > 1) {
                p = OxyPalettes.HueDistinct(consumption.Count);
            }
            var series = 0;
            foreach (var pair in consumption) {
                // main columns
                var columnSeries2 = new ColumnSeries
                {
                    IsStacked = true,
                    StackGroup = "1",
                    StrokeThickness = 1,
                    Title = pair.Item1,
                    LabelPlacement = LabelPlacement.Middle,
                    FillColor = p.Colors[series++]
                };
                var col = 0;
                foreach (var d in pair.Item2) {
                    var ci = new ColumnItem(d);
                    columnSeries2.Items.Add(ci);
                    colSums[col] += d;

                    if (d / totalSum > 0.025) {
                        var textAnnotation1 = new TextAnnotation();
                        var shortendName = pair.Item1;
                        if (shortendName.Length > 15) {
                            shortendName = shortendName.Substring(0, 15) + "...";
                        }
                        textAnnotation1.Text = shortendName + Environment.NewLine + d;
                        textAnnotation1.TextPosition = new DataPoint(col + 0.3, colSums[col] - d / 2);
                        textAnnotation1.TextHorizontalAlignment = HorizontalAlignment.Left;
                        textAnnotation1.TextVerticalAlignment = VerticalAlignment.Middle;
                        plotModel1.Annotations.Add(textAnnotation1);
                    }
                    col++;
                }
                plotModel1.Series.Add(columnSeries2);
            }
            Save(plotModel1, plotName, rfe.FullFileName, basisPath);
            _CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
        }
    }
}
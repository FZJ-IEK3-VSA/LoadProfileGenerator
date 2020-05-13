using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    internal class DeviceTaggingSet : ChartBaseFileStep
    {
        public DeviceTaggingSet([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                                [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                                [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.DeviceTaggingSetFiles
            },
            "Device Tagging Set", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        private void MakeBarCharts([JetBrains.Annotations.NotNull] ResultFileEntry rfe, [JetBrains.Annotations.NotNull] string plotName, [JetBrains.Annotations.NotNull] DirectoryInfo basisPath,
            [JetBrains.Annotations.NotNull] Dictionary<string, List<TagEntry>> consumption) {
            Profiler.StartPart(Utili.GetCurrentMethodAndClass());
            foreach (var pair in consumption) {
                var hasReferenceValue = pair.Value.Any(x => x.ReferenceValues.Count > 0);
                if (!hasReferenceValue) {
                    continue;
                }
                var plotModel1 = new PlotModel();
                pair.Value.Sort((x, y) => x.Value.CompareTo(y.Value));
                plotModel1.LegendBorderThickness = 0;
                plotModel1.LegendOrientation = LegendOrientation.Horizontal;
                plotModel1.LegendPlacement = LegendPlacement.Outside;
                plotModel1.LegendPosition = LegendPosition.BottomCenter;
                var labelFontSize = 12;
                if (Config.MakePDFCharts) {
                    plotModel1.DefaultFontSize = Parameters.PDFFontSize;
                    plotModel1.LegendFontSize = Parameters.PDFFontSize;
                    labelFontSize = 16;
                }
                plotModel1.LegendSymbolMargin = 20;
                if (Parameters.ShowTitle) {
                    plotModel1.Title = plotName;
                }

                var categoryAxis1 = new CategoryAxis
                {
                    MinorStep = 1,
                    Minimum = -0.5
                };
                categoryAxis1.Labels.Add(ChartLocalizer.Get().GetTranslation("Simulated"));
                var firstEntry = pair.Value[0];
                var referenceCount = firstEntry.ReferenceHeaders.Count;
                for (var i = 0; i < referenceCount; i++) {
                    categoryAxis1.Labels.Add(ChartLocalizer.Get().GetTranslation(firstEntry.ReferenceHeaders[i]));
                }
                categoryAxis1.GapWidth = 1;
                categoryAxis1.MaximumPadding = 0.02;
                categoryAxis1.Position = AxisPosition.Left;
                plotModel1.Axes.Add(categoryAxis1);

                var sum1 = pair.Value.Select(x => x.Value).Sum();
                var sums = new List<double>();
                foreach (var entry in pair.Value) {
                    for (var i = 0; i < entry.ReferenceValues.Count; i++) {
                        if (sums.Count < i + 1) {
                            sums.Add(0);
                        }
                        sums[i] += entry.ReferenceValues[i];
                    }
                }
                var sum2 = sums.Max();
                var totalSum = Math.Max(sum1, sum2);
                string s2 = rfe.LoadTypeInformation?.Name??"";
                var linearAxis1 = new LinearAxis
                {
                    AbsoluteMinimum = 0,
                    MaximumPadding = 0.02,
                    MinimumPadding = 0,
                    MajorStep = totalSum / 5,
                    MinorTickSize = 0,
                    Position = AxisPosition.Bottom,
                    Title = ChartLocalizer.Get().GetTranslation(s2) + " in " + rfe.LoadTypeInformation?.UnitOfSum +
                                    string.Empty
                };
                plotModel1.Axes.Add(linearAxis1);
                OxyPalette p;
                if (pair.Value.Count > 1) {
                    p = OxyPalettes.HueDistinct(pair.Value.Count);
                }
                else {
                    p = OxyPalettes.Hue64;
                }
                var colSums = new Dictionary<int, double>
                {
                    { 0, 0 },
                    { 1, 0 }
                };
                var count = 0;
                foreach (var tagentry in pair.Value) {
                    var columnSeries2 = new BarSeries
                    {
                        FillColor = p.Colors[count]
                    };
                    count++;
                    columnSeries2.IsStacked = true;
                    columnSeries2.StackGroup = "1";
                    columnSeries2.StrokeThickness = 1;
                    columnSeries2.StrokeColor = OxyColor.FromArgb(255, 255, 255, 255);
                    columnSeries2.StrokeThickness = 0.1;
                    columnSeries2.Title = ChartLocalizer.Get().GetTranslation(tagentry.TagName);
                    columnSeries2.LabelPlacement = LabelPlacement.Middle;
                    var coli = new BarItem(tagentry.Value);
                    columnSeries2.Items.Add(coli);
                    foreach (var referenceValue in tagentry.ReferenceValues) {
                        var coli2 = new BarItem(referenceValue);
                        columnSeries2.Items.Add(coli2);
                    }
                    var col = 0;
                    if (tagentry.Value / sum1 > 0.2) {
                        var d = tagentry.Value;
                        var valuetext = d.ToString("N0", CultureInfo.CurrentCulture) + " " + rfe.LoadTypeInformation?.UnitOfSum + " (" +
                                        (d / sum1 * 100).ToString("N1", CultureInfo.CurrentCulture) + " %)";
                        SetRectangelAnnotation(col, colSums, plotModel1, valuetext, d, 0.25, 0.35, labelFontSize);
                        var shortendName = ChartLocalizer.Get().GetTranslation(tagentry.TagName).Trim();
                        if (shortendName.Length > 20) {
                            shortendName = shortendName.Substring(0, 17) + "...";
                        }
                        SetRectangelAnnotation(col, colSums, plotModel1, shortendName, d, 0.35, 0.45, labelFontSize);
                    }
                    col++;
                    double refValue = 0;
                    if (tagentry.ReferenceValues.Count > 0) {
                        refValue = tagentry.ReferenceValues[0];
                    }
                    if (refValue / sum2 > 0.15) {
                        var valueText = refValue.ToString("N0", CultureInfo.CurrentCulture) + " " + rfe.LoadTypeInformation?.UnitOfSum +
                                        " (" + (refValue / sum2 * 100).ToString("N1", CultureInfo.CurrentCulture) +
                                        " %)";
                        SetRectangelAnnotation(col, colSums, plotModel1, valueText, refValue, 0.25, 0.35,
                            labelFontSize);
                        var labelText = ChartLocalizer.Get().GetTranslation(tagentry.TagName);
                        SetRectangelAnnotation(col, colSums, plotModel1, labelText, refValue, 0.35, 0.45,
                            labelFontSize);
                    }
                    colSums[0] += tagentry.Value;
                    if (tagentry.ReferenceValues.Count > 0) {
                        colSums[1] += tagentry.ReferenceValues[0];
                    }
                    plotModel1.Series.Add(columnSeries2);
                }
                var fi = new FileInfo(rfe.FullFileName);
                var modifiedName = fi.Name.Substring(0, fi.Name.Length - 3) + AutomationUtili.CleanFileName(pair.Key) +
                                   ".bars";
                if(fi.DirectoryName == null) {
                    throw new LPGException("File was not assigned to a directory");
                }

                var cleanedfullname = Path.Combine(fi.DirectoryName, modifiedName);
                Save(plotModel1, plotName, cleanedfullname, basisPath);
            }
            Profiler.StopPart(Utili.GetCurrentMethodAndClass());
        }

        private void MakePieCharts([JetBrains.Annotations.NotNull] string fileName, [JetBrains.Annotations.NotNull] string plotName, [JetBrains.Annotations.NotNull] DirectoryInfo basisPath,
            [JetBrains.Annotations.NotNull] Dictionary<string, List<TagEntry>> consumption) {
            foreach (var pair in consumption) {
                var plotModel1 = new PlotModel();
                pair.Value.Sort((x, y) => x.Value.CompareTo(y.Value));
                plotModel1.LegendBorderThickness = 0;
                plotModel1.LegendOrientation = LegendOrientation.Horizontal;
                plotModel1.LegendPlacement = LegendPlacement.Outside;
                plotModel1.LegendPosition = LegendPosition.BottomCenter;
                if (Parameters.ShowTitle) {
                    plotModel1.Title = plotName;
                }

                var pieSeries1 = new PieSeries
                {
                    InsideLabelColor = OxyColors.White,
                    InsideLabelPosition = 0.8,
                    StrokeThickness = 2,
                    AreInsideLabelsAngled = true
                };
                foreach (var tuple in pair.Value) {
                    var name = tuple.TagName.Trim();
                    if (name.Length > 30) {
                        name = name.Substring(0, 20) + "...";
                    }
                    var slice = new PieSlice(name, tuple.Value);

                    pieSeries1.Slices.Add(slice);
                }

                plotModel1.Series.Add(pieSeries1);
                var fi = new FileInfo(fileName);
                var modifiedName = fi.Name.Substring(0, fi.Name.Length - 3) + AutomationUtili.CleanFileName(pair.Key);
                if(fi.DirectoryName == null) {
                    throw new LPGException("Directory name was null");
                }

                var cleanedfullname = Path.Combine(fi.DirectoryName, modifiedName);
                Save(plotModel1, plotName, cleanedfullname, basisPath);
            }
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcEntry) {
            var consumption = new Dictionary<string, List<TagEntry>>();
            var tagname = string.Empty;
            var referenceHeaders = new List<string>();
            if (srcEntry.FullFileName == null)
            {
                throw new LPGException("filename was null");
            }
            using (var sr = new StreamReader(srcEntry.FullFileName)) {
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }
                    if (s == "-----") {
                        referenceHeaders.Clear();
                        tagname = sr.ReadLine();
                        if (tagname == null) {
                            throw new LPGException("File " + srcEntry.FullFileName + " was null");
                        }
                        consumption.Add(tagname, new List<TagEntry>());
                        var header = sr.ReadLine(); // header
                        if (header == null) {
                            throw new LPGException("File was empty: " + srcEntry.FullFileName);
                        }
                        var arr = header.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                        for (var i = 3; i < arr.Length && !string.IsNullOrWhiteSpace(arr[i]); i++) {
                            referenceHeaders.Add(arr[i]);
                        }
                    }
                    if (!s.StartsWith("Sum;", StringComparison.Ordinal) && s.Length > 0) {
                        var cols = s.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                        if (cols.Length > 1) {
                            var tagValue = Math.Abs(Convert.ToDouble(cols[1], CultureInfo.CurrentCulture));
                            var name = cols[0];
                            var te = new TagEntry(name, tagValue);
                            te.ReferenceHeaders.AddRange(referenceHeaders);
                            if (cols.Length > 3) {
                                for (var i = 3; i < cols.Length && !string.IsNullOrWhiteSpace(cols[i]); i++) {
                                    if (!string.IsNullOrEmpty(cols[i])) {
                                        var refVal = Math.Abs(Convert.ToDouble(cols[i], CultureInfo.CurrentCulture));
                                        te.ReferenceValues.Add(refVal);
                                    }
                                }
                            }
                            consumption[tagname].Add(te);
                        }
                    }
                }
            }

            string plotName = "Device Tagging Set " + srcEntry.HouseholdNumberString + " " +
                              srcEntry.LoadTypeInformation?.Name;
            MakePieCharts(srcEntry.FullFileName, plotName, Parameters.BaseDirectory, consumption);
            MakeBarCharts(srcEntry, plotName, Parameters.BaseDirectory, consumption);
            return FileProcessingResult.ShouldCreateFiles;
        }

        private static void SetRectangelAnnotation(int col, [JetBrains.Annotations.NotNull] Dictionary<int, double> colSums, [JetBrains.Annotations.NotNull] PlotModel plotModel1,
            [JetBrains.Annotations.NotNull] string text, double value, double yMin, double yMax, int labelFontSize) {
            var textAnnotation1 = new RectangleAnnotation
            {
                Text = text,
                TextHorizontalAlignment = HorizontalAlignment.Left,
                TextVerticalAlignment = VerticalAlignment.Top,
                StrokeThickness = 0,
                MinimumY = col + yMin,
                MaximumY = col + yMax,
                MinimumX = colSums[col],
                MaximumX = colSums[col] + value,
                Fill = OxyColors.Transparent,
                FontSize = labelFontSize
            };
            plotModel1.Annotations.Add(textAnnotation1);
        }

        private class TagEntry {
            public TagEntry([JetBrains.Annotations.NotNull] string tagName, double value) {
                TagName = tagName;
                Value = value;
            }

            [ItemNotNull]
            [JetBrains.Annotations.NotNull]
            public List<string> ReferenceHeaders { get; } = new List<string>();
            [JetBrains.Annotations.NotNull]
            public List<double> ReferenceValues { get; } = new List<double>();

            [JetBrains.Annotations.NotNull]
            public string TagName { get; }
            public double Value { get; }
        }
    }
}
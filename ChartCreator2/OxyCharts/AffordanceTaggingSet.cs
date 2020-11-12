using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;
using BarSeries = OxyPlot.Series.BarSeries;
using CategoryAxis = OxyPlot.Axes.CategoryAxis;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using RectangleAnnotation = OxyPlot.Annotations.RectangleAnnotation;

namespace ChartCreator2.OxyCharts {
    internal class AffordanceTaggingSet : ChartBaseFileStep
    {
        public AffordanceTaggingSet([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                                    [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                                    [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.AffordanceTaggingSetFiles
            },
            "Affordance Tagging Sets", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry rfe)
        {
            string plotName = "Affordance Tagging Set " + rfe.HouseholdNumberString;
            var consumption = new Dictionary<string, List<double>>();
            var colNames = new Dictionary<int, string>();
            var colSums = new Dictionary<int, double>();
            double totalSum = 0;
            if (rfe.FullFileName == null) {
                throw new LPGException("filename was null");
            }
            using (var sr = new StreamReader(rfe.FullFileName)) {
                // read data
                var header = sr.ReadLine();
                if (header == null) {
                    throw new LPGException("Readline failed.");
                }
                var colheaders = header.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                for (var index = 1; index < colheaders.Length; index++) {
                    if (colheaders[index].Length > 0) {
                        colNames.Add(index, colheaders[index]);
                        colSums.Add(index - 1, 0);
                    }
                }
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed.");
                    }
                    var cols = s.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                    var list = new List<double>();
                    consumption.Add(cols[0], list);

                    for (var index = 1; index < cols.Length - 1; index++) {
                        var d = Convert.ToDouble(cols[index], CultureInfo.CurrentCulture);
                        list.Add(d);
                        totalSum += d;
                    }
                }
            }

            var plotModel1 = new PlotModel();
            Legend l = new Legend();
            plotModel1.Legends.Add(l);

            // general
            l.LegendBorderThickness = 0;
            l.LegendOrientation = LegendOrientation.Horizontal;
            l.LegendPlacement = LegendPlacement.Outside;
            l.LegendPosition = LegendPosition.BottomCenter;
            l.LegendSymbolMargin = 20;
            var labelFontSize = 11;
            var pngOffset = 0.1;
            // in the pdfs the vertical text gets moved a little. this is the offset in the png to counter that.
            if (Config.MakePDFCharts) {
                plotModel1.DefaultFontSize = Parameters.PDFFontSize;
                l.LegendFontSize = Parameters.PDFFontSize;
                labelFontSize = 16;
                pngOffset = 0;
            }
            if (Parameters.ShowTitle) {
                plotModel1.Title = plotName;
            }
            // axes
            var categoryAxis1 = new CategoryAxis
            {
                MinorStep = 1,
                MaximumPadding = 0.02,
                GapWidth = 1,
                FontSize = 18
            };
            foreach (var s in colNames.Values) {
                categoryAxis1.Labels.Add(ChartLocalizer.Get().GetTranslation(s));
            }
            plotModel1.Axes.Add(categoryAxis1);
            var linearAxis1 = new LinearAxis
            {
                AbsoluteMinimum = 0,
                MaximumPadding = 0.03,
                MinimumPadding = 0,
                MinorTickSize = 0
            };
            linearAxis1.MajorStep *= 2;
            linearAxis1.Title = ChartLocalizer.Get().GetTranslation("Time in Percent");
            plotModel1.Axes.Add(linearAxis1);

            OxyPalette p;
            if (consumption.Count > 1) {
                p = OxyPalettes.HueDistinct(consumption.Count);
            }
            else {
                p = OxyPalettes.Hue64;
            }
            // generate plot
            var colheight = totalSum / consumption.Values.First().Count;
            var count = 0;
            foreach (var keyValuePair in consumption) {
                // main columns
                var columnSeries2 = new BarSeries
                {
                    IsStacked = true,
                    StackGroup = "1",
                    StrokeThickness = 0.5,
                    StrokeColor =OxyColors.White,
                    Title = ChartLocalizer.Get().GetTranslation(keyValuePair.Key),
                    LabelPlacement = LabelPlacement.Middle
                };
                var col = 0;
                foreach (var minutes in keyValuePair.Value) {
                    var d = minutes / colheight * 100;
                    var ci = new BarItem(d);
                    columnSeries2.Items.Add(ci);

                    if (d > 15) {
                        {
                            var textAnnotation1 = new RectangleAnnotation
                            {
                                Text = minutes.ToString("N0", CultureInfo.CurrentCulture) + " min (" +
                                                   d.ToString("N1", CultureInfo.CurrentCulture) + " %)",
                                TextHorizontalAlignment = HorizontalAlignment.Left,
                                TextVerticalAlignment = VerticalAlignment.Top,
                                StrokeThickness = 0,
                                MinimumY = colSums[col],
                                MaximumY = colSums[col] + d,
                                MinimumX = col + 0.28 + pngOffset,
                                MaximumX = col + 0.40 + pngOffset,
                                Fill = OxyColors.Transparent,
                                TextRotation = 270,
                                FontSize = labelFontSize
                            };
                            plotModel1.Annotations.Add(textAnnotation1);
                        }
                        {
                            var textAnnotation1 = new RectangleAnnotation();
                            var shortendName = ChartLocalizer.Get().GetTranslation(keyValuePair.Key);
                            if (shortendName.Length > 20) {
                                shortendName = shortendName.Substring(0, 17) + "...";
                            }
                            textAnnotation1.Text = shortendName.Trim();
                            textAnnotation1.TextHorizontalAlignment = HorizontalAlignment.Left;
                            textAnnotation1.TextVerticalAlignment = VerticalAlignment.Top;
                            textAnnotation1.StrokeThickness = 0;
                            textAnnotation1.MinimumY = colSums[col];
                            textAnnotation1.MaximumY = colSums[col] + d;
                            textAnnotation1.MinimumX = col + 0.20 + pngOffset;
                            textAnnotation1.MaximumX = col + 0.30 + pngOffset;
                            textAnnotation1.Fill = OxyColors.Transparent;
                            textAnnotation1.TextRotation = 270;
                            textAnnotation1.FontSize = labelFontSize;
                            plotModel1.Annotations.Add(textAnnotation1);
                        }
                    }
                    colSums[col] += d;
                    col++;
                }
                columnSeries2.FillColor = p.Colors[count];
                count++;
                plotModel1.Series.Add(columnSeries2);
            }
            Save(plotModel1, plotName, rfe.FullFileName, Parameters.BaseDirectory, CalcOption.HouseholdContents);
            return FileProcessingResult.ShouldCreateFiles;
        }
    }
}
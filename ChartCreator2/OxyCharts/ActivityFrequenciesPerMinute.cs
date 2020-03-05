using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    public class ActivityFrequenciesPerMinute : ChartBaseFileStep {
        public ActivityFrequenciesPerMinute([NotNull] ChartCreationParameters parameters,
                                            [NotNull] FileFactoryAndTracker fft,
                                            [NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler,new List<ResultFileID>() { ResultFileID.ActivationFrequencies},
            "Activity Frequencies per Minute ",FileProcessingResult.ShouldCreateFiles
            ) {
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcEntry)
        {
            string plotName = "Activity Frequencies per Minute " + srcEntry.HouseholdNumberString;
            _CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            var consumption =
                new Dictionary<string, List<Tuple<string, List<double>>>>();
            var lastname = string.Empty;
            using (var sr = new StreamReader(srcEntry.FullFileName)) {
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }
                    var cols = s.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                    if (cols.Length == 1) {
                        consumption.Add(cols[0], new List<Tuple<string, List<double>>>());
                        lastname = cols[0];
                        sr.ReadLine();
                    }
                    else {
                        var values = new List<double>();
                        for (var i = 1; i < cols.Length; i++) {
                            var d = Convert.ToDouble(cols[i], CultureInfo.CurrentCulture);
                            values.Add(d);
                        }
                        consumption[lastname].Add(new Tuple<string, List<double>>(cols[0], values));
                    }
                }
            }
            foreach (var pair in consumption) {
                var plotModel1 = new PlotModel
                {
                    // general
                    LegendBorderThickness = 0,
                    LegendOrientation = LegendOrientation.Horizontal,
                    LegendPlacement = LegendPlacement.Outside,
                    LegendPosition = LegendPosition.BottomCenter
                };
                var personName = pair.Key;
                if (_Parameters.ShowTitle) {
                    plotModel1.Title = plotName + " " + personName;
                }
                // axes
                var categoryAxis = new CategoryAxis
                {
                    AbsoluteMinimum = 0,
                    MinimumPadding = 0,
                    GapWidth = 0,
                    MajorStep = 60,
                    Title = "Minutes"
                };
                plotModel1.Axes.Add(categoryAxis);

                var linearAxis2 = new LinearAxis
                {
                    AbsoluteMinimum = 0,
                    MaximumPadding = 0.06,
                    MinimumPadding = 0,
                    Title = "Days"
                };
                plotModel1.Axes.Add(linearAxis2);
                // data
                OxyPalette p;
                if (pair.Value.Count < 2) {
                    p = OxyPalettes.Hue64;
                }
                else {
                    p = OxyPalettes.HueDistinct(pair.Value.Count);
                }

                for (var i = 0; i < pair.Value.Count; i++) {
                    var columnSeries2 = new ColumnSeries
                    {
                        IsStacked = true,
                        StrokeThickness = 0,

                        Title = pair.Value[i].Item1
                    };
                    var values = pair.Value[i].Item2;
                    for (var j = 0; j < values.Count; j++) {
                        columnSeries2.Items.Add(new ColumnItem(values[j]));
                    }
                    columnSeries2.FillColor = p.Colors[i];
                    plotModel1.Series.Add(columnSeries2);
                }
                Save(plotModel1, plotName, srcEntry.FullFileName + "." + personName, _Parameters.BaseDirectory);
            }
            _CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }
    }
}
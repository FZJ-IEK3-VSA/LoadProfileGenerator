using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Automation;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    internal class ExecutedActionsOverviewCount : ChartBaseFileStep
    {
        public ExecutedActionsOverviewCount([NotNull] ChartCreationParameters parameters,
                                            [NotNull] FileFactoryAndTracker fft,
                                            [NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.ExecutedActionsOverview
            },
            "Executed Actions Overview Count", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        private static int Comparison([NotNull] Tuple<string, double> x, [NotNull] Tuple<string, double> y)
        {
            if (string.Compare(x.Item1, y.Item1, StringComparison.Ordinal) != 0) {
                return string.Compare(x.Item1, y.Item1, StringComparison.Ordinal);
            }
            return x.Item2.CompareTo(y.Item2);
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry rfe)
        {
            _CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            string plotName = "Execution Count for " + rfe.HouseholdNumberString;
            var consumption =
                new Dictionary<string, List<Tuple<string, double>>>();
            var lastname = string.Empty;
            using (var sr = new StreamReader(rfe.FullFileName)) {
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed.");
                    }
                    var cols = s.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                    if (s == "-----") {
                        var name = sr.ReadLine();
                        if (name == null) {
                            throw new LPGException("Readline failed.");
                        }
                        consumption.Add(name, new List<Tuple<string, double>>());
                        lastname = name;
                        sr.ReadLine(); // header
                    }
                    else {
                        var d = Convert.ToDouble(cols[1], CultureInfo.CurrentCulture);
                        consumption[lastname].Add(new Tuple<string, double>(cols[0], d));
                    }
                }
            }
            foreach (var pair in consumption) {
                var mylist = pair.Value;
                mylist.Sort(Comparison);
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
                plotModel1.IsLegendVisible = false;
                // axes
                var cate = new CategoryAxis
                {
                    AbsoluteMinimum = 0,
                    MinimumPadding = 0,
                    GapWidth = 0,
                    MinorStep = 1,
                    Title = " ",
                    Angle = 90,

                    AxisTitleDistance = 150,
                    ClipTitle = false
                };
                plotModel1.Axes.Add(cate);

                var linearAxis2 = new LinearAxis
                {
                    AbsoluteMinimum = 0,
                    MaximumPadding = 0.06,
                    MinimumPadding = 0,
                    Title = "Times of execution"
                };
                plotModel1.Axes.Add(linearAxis2);
                // data
                OxyPalette p;
                if (pair.Value.Count > 1) {
                    p = OxyPalettes.HueDistinct(pair.Value.Count);
                }
                else {
                    p = OxyPalettes.Hue64;
                }

                var columnSeries2 = new ColumnSeries
                {
                    StrokeThickness = 0,
                    Title = "Actions"
                };
                for (var i = 0; i < pair.Value.Count; i++) {
                    var label = pair.Value[i].Item1;
                    if (label.Length > 40) {
                        label = label.Substring(0, 40);
                    }
                    cate.Labels.Add(label);
                    var ci = new ColumnItem(pair.Value[i].Item2)
                    {
                        Color = p.Colors[i]
                    };
                    columnSeries2.Items.Add(ci);
                }
                plotModel1.Series.Add(columnSeries2);
                var fi = new FileInfo(rfe.FullFileName);
                var pn = fi.Name.Substring(0, fi.Name.Length - 3);
                var cleanedName = AutomationUtili.CleanFileName(pair.Key);
                if (fi.DirectoryName == null) {
                    throw new LPGException("Directory Name was null");
                }
                var correctfilename = Path.Combine(fi.DirectoryName, pn + cleanedName + ".png");
                Save(plotModel1, plotName, correctfilename, _Parameters.BaseDirectory);
            }
            _CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }
    }
}
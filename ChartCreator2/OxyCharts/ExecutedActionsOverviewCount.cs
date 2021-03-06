﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Automation;
using Automation.ResultFiles;
using Common;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    internal class ExecutedActionsOverviewCount : ChartBaseFileStep
    {
        public ExecutedActionsOverviewCount([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                                            [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                                            [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.ExecutedActionsOverview
            },
            "Executed Actions Overview Count", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        private static int Comparison([JetBrains.Annotations.NotNull] Tuple<string, double> x, [JetBrains.Annotations.NotNull] Tuple<string, double> y)
        {
            if (string.Compare(x.Item1, y.Item1, StringComparison.Ordinal) != 0) {
                return string.Compare(x.Item1, y.Item1, StringComparison.Ordinal);
            }
            return x.Item2.CompareTo(y.Item2);
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry rfe)
        {
            Profiler.StartPart(Utili.GetCurrentMethodAndClass());
            string plotName = "Execution Count for " + rfe.HouseholdNumberString;
            var consumption =
                new Dictionary<string, List<Tuple<string, double>>>();
            var lastname = string.Empty;
            if (rfe.FullFileName == null)
            {
                throw new LPGException("filename was null");
            }
            using (var sr = new StreamReader(rfe.FullFileName)) {
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed.");
                    }
                    var cols = s.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
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
                var plotModel1 = new PlotModel();
                var l = new Legend();
                plotModel1.Legends.Add(l);
                    // general
                    l.LegendBorderThickness = 0;
                    l.LegendOrientation = LegendOrientation.Horizontal;
                    l.LegendPlacement = LegendPlacement.Outside;
                    l.LegendPosition = LegendPosition.BottomCenter;
                var personName = pair.Key;
                if (Parameters.ShowTitle) {
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

                var columnSeries2 = new BarSeries
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
                    var ci = new BarItem(pair.Value[i].Item2)
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
                Save(plotModel1, plotName, correctfilename, Parameters.BaseDirectory, CalcOption.ActivationsPerHour);
            }
            Profiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }
    }
}
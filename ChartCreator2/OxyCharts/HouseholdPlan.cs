using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Automation;
using Automation.ResultFiles;
using Common;
using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;
using CategoryAxis = OxyPlot.Axes.CategoryAxis;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using BarSeries = OxyPlot.Series.BarSeries;
namespace ChartCreator2.OxyCharts {
    internal class HouseholdPlan : ChartBaseFileStep
    {
        public HouseholdPlan([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                             [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                             [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.HouseholdPlanTime
            },
            "Household Plan Results", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        [JetBrains.Annotations.NotNull]
        private PlotModel MakePlot([JetBrains.Annotations.NotNull] string title, [JetBrains.Annotations.NotNull] out CategoryAxis cate, [JetBrains.Annotations.NotNull] string axistitle)
        {
            var plotModel1 = new PlotModel();
            var l = new Legend();
            plotModel1.Legends.Add(l);
                // general
                l.LegendBorderThickness = 0;
                l.LegendOrientation = LegendOrientation.Horizontal;
                l.LegendPlacement = LegendPlacement.Outside;
                l.LegendPosition = LegendPosition.BottomCenter;
                plotModel1.PlotMargins = new OxyThickness(double.NaN, double.NaN, double.NaN, 200);
            if (Parameters.ShowTitle) {
                plotModel1.Title = title;
            }
            // axes
            cate = new CategoryAxis
            {
                AbsoluteMinimum = 0,
                MinimumPadding = 0,
                GapWidth = 0,
                Angle = 90
            };
            plotModel1.Axes.Add(cate);
            var linearAxis2 = new LinearAxis
            {
                AbsoluteMinimum = 0,
                MaximumPadding = 0.06,
                MinimumPadding = 0,
                Title = axistitle
            };
            plotModel1.Axes.Add(linearAxis2);
            // data
            return plotModel1;
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcEntry) {
            Profiler.StartPart(Utili.GetCurrentMethodAndClass());
            if (srcEntry.FullFileName == null)
            {
                throw new LPGException("filename was null");
            }
            using (var sr = new StreamReader(srcEntry.FullFileName)) {
                var currentSection = string.Empty;
                BarSeries plan = null;
                BarSeries actual = null;
                BarSeries relativeSeries = null;
                PlotModel pmAbsolute = null;
                PlotModel pmRelative = null;
                CategoryAxis cate = null;
                CategoryAxis cateRel = null;
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }
                    if (s.StartsWith("#####", StringComparison.Ordinal)) {
                        // do nothing
                    }
                    else if (s.StartsWith("-----", StringComparison.Ordinal)) {
                        if (pmAbsolute != null) {
                            pmAbsolute.Series.Add(plan);
                            pmAbsolute.Series.Add(actual);
                            Save(pmAbsolute, "Household Plan " + currentSection, srcEntry.FullFileName + "." + currentSection,
                                Parameters.BaseDirectory, CalcOption.HouseholdPlan);
                            pmRelative.Series.Add(relativeSeries);
                            Save(pmRelative, "Household Plan Percentage " + currentSection,
                                srcEntry.FullFileName + "." + currentSection + ".Percent", Parameters.BaseDirectory, CalcOption.HouseholdPlan);
                        }
                        currentSection = s.Substring(6, s.Length - 12);
                        var axistitle = "Hours";
                        if (currentSection.ToUpperInvariant().Contains("ACTIVATION")) {
                            axistitle = "Activations";
                        }
                        pmAbsolute = MakePlot(currentSection, out cate, axistitle);
                        pmRelative = MakePlot(currentSection, out cateRel, "Percent [%]");

                        plan = new BarSeries
                        {
                            Title = "Planned",
                            IsStacked = false,
                            StrokeThickness = 1
                        };
                        actual = new BarSeries
                        {
                            Title = "Actual",
                            IsStacked = false,
                            StrokeThickness = 1
                        };
                        relativeSeries = new BarSeries
                        {
                            IsStacked = false,
                            StrokeThickness = 1
                        };
                    }
                    else {
                        var cols = s.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                        var values = new List<double>();
                        var success = double.TryParse(cols[1], out _);
                        // check if it's a line with numbers and otherwise skip
                        if (success) {
                            for (var i = 1; i < cols.Length; i++) {
                                var d = Convert.ToDouble(cols[i], CultureInfo.CurrentCulture);
                                values.Add(d);
                            }
                            if (cate == null) {
                                throw new LPGException("category was null");
                            }
                            cate.Labels.Add(cols[0]);
                            cateRel.Labels.Add(cols[0]);
                            plan.Items.Add(new BarItem(values[2]));
                            actual.Items.Add(new BarItem(values[1]));
                            relativeSeries.Items.Add(new BarItem(values[3]));
                        }
                    }
                }
            }
            Profiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    public class ActivityPercentage : ChartBaseFileStep
    {
        public ActivityPercentage([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
            [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
        [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
        calculationProfiler,new List<ResultFileID>() { ResultFileID.ActivityPercentages
        },
        "Activity Percentages",FileProcessingResult.ShouldCreateFiles
        ) {
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcResultFileEntry) {
            string plotName = "Activity Percentages " + srcResultFileEntry.HouseholdKey + " " +
                              srcResultFileEntry.LoadTypeInformation?.Name;

            _CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            var consumption =
                new Dictionary<string, List<Tuple<string, double>>>();
            var lastname = string.Empty;
            using (var sr = new StreamReader(srcResultFileEntry.FullFileName)) {
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }

                    var cols = s.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                    if (cols.Length == 1) {
                        consumption.Add(cols[0], new List<Tuple<string, double>>());
                        lastname = cols[0];
                        sr.ReadLine();
                    }
                    else {
                        var val = Convert.ToDouble(cols[1], CultureInfo.CurrentCulture);
                        consumption[lastname].Add(new Tuple<string, double>(cols[0], val));
                    }
                }
            }
            foreach (var pair in consumption) {
                var personname = pair.Key;

                var plotModel1 = new PlotModel
                {
                    LegendBorderThickness = 0,
                    LegendOrientation = LegendOrientation.Horizontal,
                    LegendPlacement = LegendPlacement.Outside,
                    LegendPosition = LegendPosition.BottomCenter
                };
                if (_Parameters.ShowTitle) {
                    plotModel1.Title = plotName + " " + personname;
                }

                var pieSeries1 = new PieSeries
                {
                    InsideLabelColor = OxyColors.White,
                    InsideLabelPosition = 0.8,
                    StrokeThickness = 2,
                    AreInsideLabelsAngled = true
                };
                OxyPalette p;
                if (pair.Value.Count > 2) {
                    p = OxyPalettes.HueDistinct(pair.Value.Count);
                }
                else {
                    p = OxyPalettes.Hue64;
                }
                pieSeries1.InsideLabelColor = OxyColor.FromRgb(0, 0, 0);
                var i = 0;
                foreach (var tuple in pair.Value) {
                    var name = tuple.Item1.Trim();
                    if (name.Length > 30) {
                        name = name.Substring(0, 20) + "...";
                    }
                    var slice = new PieSlice(name, tuple.Item2)
                    {
                        Fill = p.Colors[i++]
                    };
                    pieSeries1.Slices.Add(slice);
                }

                plotModel1.Series.Add(pieSeries1);
                Save(plotModel1, plotName, srcResultFileEntry.FullFileName + "." + personname, _Parameters.BaseDirectory);
            }
            _CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }
    }
}
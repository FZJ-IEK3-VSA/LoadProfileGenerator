using System;
using System.Collections.Generic;
using System.IO;
using Automation.ResultFiles;
using Common;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    internal class CriticalThresholdViolations : ChartBaseFileStep
    {
        public CriticalThresholdViolations([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                                           [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                                           [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.CriticalThresholdViolations
            },
            "Critical Threshold Violations", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcEntry)
        {
            string plotName = "Critical desire threshhold violations for " + srcEntry.HouseholdNumberString;
            Profiler.StartPart(Utili.GetCurrentMethodAndClass());
            var headers = new List<string>();
            var values = new List<double[]>();
            if (srcEntry.FullFileName == null) {
                throw new LPGException("Filename was null");
            }
            using (var sr = new StreamReader(srcEntry.FullFileName)) {
                var top = sr.ReadLine();
                if (top == null) {
                    throw new LPGException("Readline failed");
                }
                var header1 = top.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                headers.AddRange(header1);
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }
                    var cols = s.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                    var result = new double[headers.Count];
                    for (var index = 0; index < cols.Length; index++) {
                        var col = cols[index];
                        var success = double.TryParse(col, out double d);
                        if (success) {
                            result[index] = d;
                        }
                    }
                    values.Add(result);
                }
            }
            var plotModel1 = new PlotModel();
            if (Parameters.ShowTitle) {
                plotModel1.Title = plotName;
            }
            var linearAxis1 = new LinearAxis
            {
                Position = AxisPosition.Bottom
            };
            plotModel1.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis();
            plotModel1.Axes.Add(linearAxis2);
            plotModel1.IsLegendVisible = false;
            for (var i = 2; i < headers.Count; i++) {
                var lineSeries1 = new LineSeries
                {
                    Title = headers[i]
                };
                for (var j = 0; j < values.Count; j++) {
                    lineSeries1.Points.Add(new DataPoint(j, values[j][i]));
                }
                //lineSeries1.Smooth = true;

                plotModel1.Series.Add(lineSeries1);
                var pointAnnotation1 = new PointAnnotation
                {
                    X = values.Count - 1,
                    Y = values[values.Count - 1][i],
                    Text = headers[i],
                    TextHorizontalAlignment = HorizontalAlignment.Right,
                    TextVerticalAlignment = VerticalAlignment.Middle,
                    TextColor = lineSeries1.Color
                };
                plotModel1.Annotations.Add(pointAnnotation1);
            }
            Save(plotModel1, plotName, srcEntry.FullFileName, Parameters.BaseDirectory);
            Profiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }
    }
}
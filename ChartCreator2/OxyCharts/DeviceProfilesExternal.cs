using System;
using System.Collections.Generic;
using System.IO;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    internal class DeviceProfilesExternal : ChartBaseFileStep
    {
        public DeviceProfilesExternal([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                                      [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                                      [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.DeviceProfileCSVExternal
            },
            "Device Profiles Externals", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcResultFileEntry)
        {
           string plotName = "Device Profiles External Time Resolution  " + srcResultFileEntry.HouseholdNumberString + " " +
                       srcResultFileEntry.LoadTypeInformation?.Name;
            _CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            var headers = new List<string>();
            var values = new List<double[]>();
            using (var sr = new StreamReader(srcResultFileEntry.FullFileName)) {
                var topLine = sr.ReadLine();
                if (topLine == null) {
                    throw new LPGException("Readline failed");
                }
                var header1 = topLine.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                headers.AddRange(header1);
                while (!sr.EndOfStream && values.Count < 5000) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }
                    var cols = s.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
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
            var plotModel1 = new PlotModel
            {
                // general
                LegendBorderThickness = 0,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter
            };
            if (_Parameters.ShowTitle) {
                plotModel1.Title = plotName;
            }
            // axes
            var linearAxis2 = new LinearAxis
            {
                AbsoluteMinimum = 0,
                MaximumPadding = 0.06,
                MinimumPadding = 0,
                Title = "2"
            };
            plotModel1.Axes.Add(linearAxis2);
            // data
            OxyPalette p;
            if (headers.Count < 2) {
                p = OxyPalettes.Hue64;
            }
            else {
                p = OxyPalettes.HueDistinct(headers.Count);
            }

            for (var i = 2; i < headers.Count; i++) {
                // main columns
                var columnSeries2 = new AreaSeries
                {
                    StrokeThickness = 0,
                    Title = headers[i]
                };

                for (var j = 0; j < values.Count; j++) {
                    double sum = 0;
                    for (var k = i - 1; k > 0; k--) {
                        sum += values[j][k];
                    }

                    var bottom = new DataPoint(j, sum);
                    columnSeries2.Points.Add(bottom);

                    var top = new DataPoint(j, sum + values[j][i]);
                    columnSeries2.Points2.Add(top);
                }
                columnSeries2.Color = p.Colors[i];
                plotModel1.Series.Add(columnSeries2);
            }

            Save(plotModel1, plotName, srcResultFileEntry.FullFileName, _Parameters.BaseDirectory);
            _CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }
    }
}
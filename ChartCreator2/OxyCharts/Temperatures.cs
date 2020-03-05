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
    internal class Temperatures : ChartBaseFileStep
    {
        public Temperatures([NotNull] ChartCreationParameters parameters,
                            [NotNull] FileFactoryAndTracker fft,
                            [NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.Temperatures
            },
            "Temperatures", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry rfe)
        {
            _CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            const string plotName = "Temperatures";
            var values = new List<double>();
            using (var sr = new StreamReader(rfe.FullFileName)) {
                sr.ReadLine();
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }
                    var cols = s.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                    var col = cols[cols.Length - 1];
                    var success = double.TryParse(col, out double d);
                    if (!success) {
                        throw new LPGException("Double Trouble!");
                    }
                    values.Add(d);
                }
            }
            var plotModel1 = new PlotModel();
            if (_Parameters.ShowTitle) {
                plotModel1.Title = plotName;
            }
            var linearAxis1 = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time in Minutes"
            };
            plotModel1.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis
            {
                Title = "Temperature in °C"
            };
            plotModel1.Axes.Add(linearAxis2);
            plotModel1.IsLegendVisible = false;
            var lineSeries1 = new LineSeries
            {
                Title = "Temperatures"
            };
            for (var j = 0; j < values.Count; j++) {
                lineSeries1.Points.Add(new DataPoint(j, values[j]));
            }
            plotModel1.Series.Add(lineSeries1);
            Save(plotModel1, plotName, rfe.FullFileName, _Parameters.BaseDirectory);
            _CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }
    }
}
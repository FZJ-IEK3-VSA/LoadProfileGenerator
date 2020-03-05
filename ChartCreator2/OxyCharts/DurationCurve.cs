using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    internal class DurationCurve : ChartBaseFileStep
    {
        public DurationCurve([NotNull] ChartCreationParameters parameters,
                             [NotNull] FileFactoryAndTracker fft,
                             [NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.DurationCurveSums
            },
            "Duration Curve", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcEntry) {
            _CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            string plotName = "Duration Curve " + srcEntry.HouseholdNumberString + " " + srcEntry.LoadTypeInformation?.Name;
            try {
                var values = new List<double>();
                using (var sr = new StreamReader(srcEntry.FullFileName)) {
                    sr.ReadLine();
                    while (!sr.EndOfStream) {
                        var s = sr.ReadLine();
                        if (s == null) {
                            throw new LPGException("Readline failed.");
                        }
                        var cols = s.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                        var result = Convert.ToDouble(cols[2], CultureInfo.CurrentCulture);
                        values.Add(result);
                    }
                }
                var plotModel1 = new PlotModel();
                if (_Parameters.ShowTitle) {
                    plotModel1.Title = plotName;
                }
                var linearAxis1 = new LinearAxis
                {
                    Title = "Time in Minutes",
                    Position = AxisPosition.Bottom
                };
                plotModel1.Axes.Add(linearAxis1);
                var linearAxis2 = new LinearAxis
                {
                    Title = srcEntry.LoadTypeInformation?.Name + " in " + srcEntry.LoadTypeInformation?.UnitOfPower
                };
                plotModel1.Axes.Add(linearAxis2);
                plotModel1.IsLegendVisible = false;
                var lineSeries1 = new LineSeries
                {
                    Title = "Duration Curve"
                };
                for (var j = 0; j < values.Count; j++) {
                    lineSeries1.Points.Add(new DataPoint(j, values[j]));
                }
                //lineSeries1.Smooth = false;

                plotModel1.Series.Add(lineSeries1);
                Save(plotModel1, plotName, srcEntry.FullFileName, _Parameters.BaseDirectory);
            }
            catch (Exception e) {
                Logger.Error("Error in Duration Curve Chart: " + e.Message);
                Logger.Exception(e);
            }
            _CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }
    }
}
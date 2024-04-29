using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.JSON;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    internal class SumProfilesExternal : ChartBaseFileStep
    {
        private readonly CalcParameters _calcParameters;

        public SumProfilesExternal([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                                   [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                                   [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler, CalcParameters calcParameters) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.CSVSumProfileExternal
            },
            "Sim Profiles External Time Resolution", FileProcessingResult.ShouldCreateFiles
        )
        {
            _calcParameters = calcParameters;
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry rfe)
        {
            try
            {
                Profiler.StartPart(Utili.GetCurrentMethodAndClass());
                string plotName = "Sum Profile for " + rfe.HouseholdNumberString + " " + rfe.LoadTypeInformation?.Name;
                var values = new List<double>();
                if (rfe.FullFileName == null)
                {
                    throw new LPGException("filename was null");
                }
                // specify the number format to use for parsing
                NumberFormatInfo formatInfo = new NumberFormatInfo();
                formatInfo.NumberDecimalSeparator = _calcParameters.DecimalSeperator;

                using (var sr = new StreamReader(rfe.FullFileName))
                {
                    sr.ReadLine();
                    while (!sr.EndOfStream)
                    {
                        var s = sr.ReadLine();
                        if (s == null)
                        {
                            throw new LPGException("Readline failed");
                        }
                        var cols = s.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                        var col = cols[cols.Length - 1];
                        var success = double.TryParse(col, NumberStyles.Float, formatInfo, out var d);
                        if (!success)
                        {
                            throw new LPGException("Error reading file '" + rfe.FullFileName + "': Could not parse double from '" + col + "'");
                        }
                        values.Add(d);
                    }
                }
                var plotModel1 = new PlotModel();
                if (Parameters.ShowTitle)
                {
                    plotModel1.Title = plotName;
                }
                var linearAxis1 = new LinearAxis
                {
                    Position = AxisPosition.Bottom
                };
                plotModel1.Axes.Add(linearAxis1);
                linearAxis1.Title = "Day";
                var linearAxis2 = new LinearAxis
                {
                    Title = rfe.LoadTypeInformation?.Name + " in " + rfe.LoadTypeInformation?.UnitOfPower
                };
                plotModel1.Axes.Add(linearAxis2);
                plotModel1.IsLegendVisible = false;
                var lineSeries1 = new LineSeries
                {
                    Title = "Energy"
                };
                for (var j = 0; j < values.Count; j++)
                {
                    lineSeries1.Points.Add(new DataPoint(j, values[j]));
                }
                plotModel1.Series.Add(lineSeries1);
                Save(plotModel1, plotName, rfe.FullFileName, Parameters.BaseDirectory, CalcOption.SumProfileExternalIndividualHouseholds);
            } finally
            {
                Profiler.StopPart(Utili.GetCurrentMethodAndClass());
            }
            return FileProcessingResult.ShouldCreateFiles;
        }
    }
}
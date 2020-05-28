using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    internal class SumProfiles : ChartBaseFileStep
    {
        public SumProfiles([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                           [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                           [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.CSVSumProfile
            },
            "Sum Profiles", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        private void MakeBoxPlot([JetBrains.Annotations.NotNull] string fileName, [JetBrains.Annotations.NotNull] string plotName, [JetBrains.Annotations.NotNull] DirectoryInfo basisPath, [JetBrains.Annotations.NotNull] List<double> values,
            [JetBrains.Annotations.NotNull] LoadTypeInformation lti)
        {
            const int stepsize = 24 * 60;
            var dayEntries = new List<DayEntry>();
            for (var i = 0; i < values.Count; i += stepsize) {
                var oneDay = new List<double>();
                for (var j = 0; j < stepsize && i + j < values.Count; j++) {
                    oneDay.Add(values[i + j]);
                }
                if (oneDay.Count > 5) {
                    var min = oneDay.Min();
                    var max = oneDay.Max();
                    oneDay.Sort();
#pragma warning disable VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
                    var idx = oneDay.Count / 2;
#pragma warning restore VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
                    var median = oneDay[idx];
                    dayEntries.Add(new DayEntry(min, max, median, Percentile(oneDay, 0.25),
                        Percentile(oneDay, 0.75)));
                }
            }

            var plotModel1 = new PlotModel
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                IsLegendVisible = true
            };
            if (Parameters.ShowTitle) {
                plotModel1.Title = plotName;
            }
            var linearAxis1 = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Day"
            };
            plotModel1.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis
            {
                Title = lti.Name + " in " + lti.UnitOfPower
            };
            plotModel1.Axes.Add(linearAxis2);
            var bps = new BoxPlotSeries();
            for (var i = 0; i < dayEntries.Count; i++) {
                bps.Items.Add(new BoxPlotItem(i, dayEntries[i].MinValue, dayEntries[i].Percentile25,
                    dayEntries[i].Median, dayEntries[i].Percentile75, dayEntries[i].MaxValue));
            }
            plotModel1.Series.Add(bps);
            plotModel1.LegendBackground = OxyColor.FromArgb(200, 255, 255, 255);
            var fi = new FileInfo(fileName);
            var dstFileName = fi.Name.Insert(fi.Name.Length - 4, "MinMax.");
            Save(plotModel1, plotName, fileName, basisPath,CalcOption.IndividualSumProfiles, dstFileName);
        }

        private void MakeLinePlot([JetBrains.Annotations.NotNull] string fileName, [JetBrains.Annotations.NotNull] string plotName, [JetBrains.Annotations.NotNull] DirectoryInfo basisPath, [JetBrains.Annotations.NotNull] List<double> values,
            [JetBrains.Annotations.NotNull] LoadTypeInformation lti)
        {
            var plotModel1 = new PlotModel();
            if (Parameters.ShowTitle) {
                plotModel1.Title = plotName;
            }
            var linearAxis1 = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Timestep"
            };
            plotModel1.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis
            {
                Title = lti.Name + " in " + lti.UnitOfPower
            };
            plotModel1.Axes.Add(linearAxis2);
            plotModel1.IsLegendVisible = true;
            var lineSeries1 = new LineSeries
            {
                Title = lti.Name + " in " + lti.UnitOfPower
            };
            for (var j = 0; j < values.Count; j++) {
                lineSeries1.Points.Add(new DataPoint(j, values[j]));
            }
            plotModel1.Series.Add(lineSeries1);
            Save(plotModel1, plotName, fileName, basisPath, CalcOption.IndividualSumProfiles);
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcEntry)
        {
            Profiler.StartPart(Utili.GetCurrentMethodAndClass());
            string plotName = "Sum Profile for " + srcEntry.HouseholdNumberString + " " + srcEntry.LoadTypeInformation?.Name;
            var values = new List<double>();
            if (srcEntry.FullFileName == null)
            {
                throw new LPGException("filename was null");
            }
            using (var sr = new StreamReader(srcEntry.FullFileName)) {
                sr.ReadLine();
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }
                    var cols = s.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                    var col = cols[cols.Length - 1];
                    var success = double.TryParse(col, out double d);
                    if (!success) {
                        throw new LPGException("Double Trouble reading the file " + srcEntry.FileName);
                    }
                    if(srcEntry.LoadTypeInformation == null) {
                        throw new LPGException("Lti was null");
                    }

                    values.Add(d / srcEntry.LoadTypeInformation.ConversionFaktor);
                }
            }
            MakeBoxPlot(srcEntry.FullFileName, plotName, Parameters.BaseDirectory, values, srcEntry.LoadTypeInformation ?? throw new InvalidOperationException());
            MakeLinePlot(srcEntry.FullFileName, plotName, Parameters.BaseDirectory, values, srcEntry.LoadTypeInformation);
            Profiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }

        private static double Percentile([JetBrains.Annotations.NotNull] List<double> sequence, double excelPercentile)
        {
            sequence.Sort();
            var sequenceCount = sequence.Count;
            var n = (sequenceCount - 1) * excelPercentile + 1;
            // Another method: double n = (N + 1) * excelPercentile
            if (Math.Abs(n - 1d) < 0.00001) {
                return sequence[0];
            }
            if (Math.Abs(n - sequenceCount) < 0.000001) {
                return sequence[sequenceCount - 1];
            }
            var k = (int) n;
            var d = n - k;
            return sequence[k - 1] + d * (sequence[k] - sequence[k - 1]);
        }

        private class DayEntry {
            public DayEntry(double minValue, double maxValue, double median, double percentile25,
                double percentile75)
            {
                MinValue = minValue;
                MaxValue = maxValue;
                Median = median;
                Percentile25 = percentile25;
                Percentile75 = percentile75;
            }

            public double MaxValue { get; }
            public double Median { get; }

            public double MinValue { get; }
            public double Percentile25 { get; }
            public double Percentile75 { get; }
        }
    }
}
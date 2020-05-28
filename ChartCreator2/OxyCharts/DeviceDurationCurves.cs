using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    internal class DeviceDurationCurves : ChartBaseFileStep
    {
        public DeviceDurationCurves([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                                    [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                                    [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.DurationCurveDevices
            },
            "Duration Curves per Device", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcEntry)
        {
            string plotName = "Device Duration Curve " + srcEntry.HouseholdNumberString + " " + srcEntry.LoadTypeInformation?.Name;
            Profiler.StartPart(Utili.GetCurrentMethodAndClass());

            var entries = new Dictionary<int, ValueEntry>();
            if (srcEntry.FullFileName == null)
            {
                throw new LPGException("Filename was null");
            }
            ReadFile(srcEntry.FullFileName, entries);
            foreach (var keyValuePair in entries) {
                keyValuePair.Value.Values.Sort((x, y) => y.CompareTo(x));
            }
            var plotModel1 = new PlotModel();
            if (Parameters.ShowTitle) {
                plotModel1.Title = plotName;
            }
            var linearAxis1 = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time"
            };
            plotModel1.Axes.Add(linearAxis1);
            var linearAxis2 = new LinearAxis
            {
                Title = srcEntry.LoadTypeInformation?.Name + " [" + srcEntry.LoadTypeInformation?.UnitOfPower + "]"
            };
            plotModel1.Axes.Add(linearAxis2);
            plotModel1.IsLegendVisible = true;
            var globalMax = entries.Values.Where(x => x.Index > 2).Select(x => x.Values.Max()).Max();

            var filtered =
                entries.Values.Where(x => x.Index > 2 && x.Sum != 0 && x.Values.Max() > globalMax * 0.01).ToList();
            if (filtered.Count == 0) {
                filtered = entries.Values.Where(x => x.Index > 2).ToList();
            }
            var filtered2 = filtered.OrderByDescending(x => x.Sum).ToList();

            if (filtered2.Count == 0 && entries.Count > 0) {
                throw new LPGException("Bug in the filtering for the device duration curves!");
            }
            var labeled = filtered2.Take(10).ToList();
            foreach (var entry in filtered2) {
                var lineSeries1 = new LineSeries
                {
                    Title = entry.Name
                };
                for (var j = 0; j < entry.Values.Count; j++) {
                    lineSeries1.Points.Add(new DataPoint(j, entry.Values[j]));
                }
                //lineSeries1.Smooth = false;

                plotModel1.Series.Add(lineSeries1);
                if (labeled.Contains(entry)) {
                    var pointAnnotation1 = new PointAnnotation
                    {
                        X = 10,
                        Y = entry.Values[0],
                        Text = entry.Name,
                        TextHorizontalAlignment = HorizontalAlignment.Left,
                        TextVerticalAlignment = VerticalAlignment.Middle,
                        TextColor = lineSeries1.Color
                    };
                    plotModel1.Annotations.Add(pointAnnotation1);
                }
            }
            Save(plotModel1, plotName, srcEntry.FullFileName, Parameters.BaseDirectory, CalcOption.DurationCurve);
            Profiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }

        private void ReadFile([CanBeNull] string fileName, [JetBrains.Annotations.NotNull] Dictionary<int, ValueEntry> entries) {
            if (fileName == null) {
                throw new LPGException("filename was nul" +
                                       "l");
            }
            using (var sr = new StreamReader(fileName)) {
                var top = sr.ReadLine();
                if (top == null) {
                    throw new LPGException("Readline failed.");
                }
                var header1 = top.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                for (var i = 0; i < header1.Length; i++) {
                    entries.Add(i, new ValueEntry(header1[i], i));
                }
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }
                    var cols = s.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                    var result = new double[entries.Count];
                    for (var index = 0; index < cols.Length; index++) {
                        var col = cols[index];
                        var success = double.TryParse(col, out double d);
                        if (success) {
                            result[index] = d;
                        }
                    }
                    for (var i = 0; i < result.Length; i++) {
                        entries[i].Values.Add(result[i]);
                    }
                }
            }
        }

        private class ValueEntry {
            public ValueEntry([JetBrains.Annotations.NotNull] string name, int index) {
                Name = name;
                Index = index;
            }

            public int Index { get; }

            [JetBrains.Annotations.NotNull]
            public string Name { get; }

            public double Sum => Values.Sum();
            [JetBrains.Annotations.NotNull]
            public List<double> Values { get; } = new List<double>();

            public override string ToString() => Name + " Sum:" + Sum + " Max:" + Values.Max();
        }
    }
}
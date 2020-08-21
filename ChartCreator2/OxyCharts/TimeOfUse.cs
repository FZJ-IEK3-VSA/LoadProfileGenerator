using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    internal class TimeOfUse : ChartBaseFileStep
    {
        public TimeOfUse([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                         [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                         [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.TimeOfUseEnergy, ResultFileID.TimeOfUse
            },
            "Activity Percentages", FileProcessingResult.ShouldCreateFiles
        )
        { }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry rfe)
        {
            Profiler.StartPart(Utili.GetCurrentMethodAndClass());
            string plotName = "Time of Use " + rfe.HouseholdNumberString + " " + rfe.LoadTypeInformation?.Name;
            bool isEnergy = rfe.ResultFileID == ResultFileID.TimeOfUseEnergy;
            var devices = new List<Device>();
            if (rfe.FullFileName == null)
            {
                throw new LPGException("filename was null");
            }
            using (var sr = new StreamReader(rfe.FullFileName)) {
                var s = sr.ReadLine();
                if (s == null) {
                    throw new LPGException("Readline failed");
                }
                var header1 = s.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                foreach (var header in header1) {
                    devices.Add(new Device(header));
                }
                while (!sr.EndOfStream && s.Length > 0) {
                    s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }
                    var cols = s.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                    for (var index = 2; index < cols.Length; index++) {
                        var col = cols[index];
                        if (col.Length > 0) {
                            if (col.Length > 0) {
                                var success = double.TryParse(col, out double d);
                                if (!success) {
                                    throw new LPGException("Double Trouble! " + rfe.FileName);
                                }
                                devices[index].Values.Add(d);
                            }
                        }
                    }
                }
            }
            devices.RemoveAt(0);
            devices.RemoveAt(0);
            devices.Sort((x, y) => y.Sum.CompareTo(x.Sum));
            double max = 0;
            for (var i = 0; i < devices[0].Values.Count; i++) {
                double sum = 0;
                foreach (var device in devices) {
                    if (device.Values.Count > i) {
                        sum += device.Values[i];
                    }
                }
                if (sum > max) {
                    max = sum;
                }
            }

            var plotModel1 = new PlotModel();
            var l = new Legend();
            plotModel1.Legends.Add(l);
                // general
                l.LegendBorderThickness = 0;
                l.LegendOrientation = LegendOrientation.Horizontal;
                l.LegendPlacement = LegendPlacement.Outside;
                l.LegendPosition = LegendPosition.BottomCenter;

            if (Parameters.ShowTitle) {
                plotModel1.Title = plotName;
            }
            // axes
            var cate = new CategoryAxis
            {
                AbsoluteMinimum = 0,
                MinimumPadding = 0,
                GapWidth = 0,
                MajorStep = 60,

                Title = "Minutes"
            };

            plotModel1.Axes.Add(cate);

            var linearAxis2 = new LinearAxis
            {
                AbsoluteMinimum = 0,
                MaximumPadding = 0.06,
                MinimumPadding = 0
            };

            if (isEnergy) {
                linearAxis2.Title = rfe.LoadTypeInformation?.Name + " in " + rfe.LoadTypeInformation?.UnitOfPower;
            }
            else {
                linearAxis2.Title = "Minutes/(Household-Year)";
            }
            linearAxis2.Minimum = 0;
            linearAxis2.Maximum = max * 1.05;
            plotModel1.Axes.Add(linearAxis2);
            // data
            var p = OxyPalettes.HueDistinct(devices.Count);

            for (var i = 0; i < devices.Count; i++) {
                // main columns
                var columnSeries2 = new BarSeries
                {
                    IsStacked = true,
                    StrokeThickness = 0,

                    Title = devices[i].Name
                };
                var myvalues = devices[i].Values;
                for (var j = 0; j < myvalues.Count; j++) {
                    columnSeries2.Items.Add(new BarItem(myvalues[j]));
                }
                columnSeries2.FillColor = p.Colors[i];
                plotModel1.Series.Add(columnSeries2);
            }
            Save(plotModel1, plotName, rfe.FullFileName, Parameters.BaseDirectory, CalcOption.TimeOfUsePlot);
            Profiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }

        private class Device {
            [JetBrains.Annotations.NotNull] private readonly List<double> _values = new List<double>();

            public Device([JetBrains.Annotations.NotNull] string name) => Name = name;

            [JetBrains.Annotations.NotNull]
            public string Name { get; }

            public double Sum => _values.Sum();

            [JetBrains.Annotations.NotNull]
            public List<double> Values => _values;
        }
    }
}
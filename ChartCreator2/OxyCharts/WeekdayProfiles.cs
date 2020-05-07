using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation.ResultFiles;
using Common;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    internal class WeekdayProfiles : ChartBaseFileStep
    {
        public WeekdayProfiles([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                               [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                               [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.WeekdayLoadProfileID
            },
            "Weekday Profiles", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        [JetBrains.Annotations.NotNull]
        private PlotModel MakeChart([JetBrains.Annotations.NotNull] string plotName,
                                    [ItemNotNull] [JetBrains.Annotations.NotNull] List<Entry> entries,
                                    bool showTitle,
                                    [CanBeNull] LoadTypeInformation lti)
        {
            var days = new HashSet<string>();
            foreach (var entry in entries) {
                days.Add(entry.Day);
            }

            var seasons = entries.Select(x => x.Season).Distinct().ToList();
            var maxTimeValue = 0;
            double maxVal = 0;
            foreach (var entry in entries) {
                if (entry.Values.Count > maxTimeValue) {
                    maxTimeValue = entry.Values.Count;
                }
                if (entry.Values.Max() > maxVal) {
                    maxVal = entry.Values.Max();
                }
            }
            seasons.Sort();
            var plotModel1 = new PlotModel
            {
                LegendPosition = LegendPosition.BottomCenter,
                LegendPlacement = LegendPlacement.Outside,
                LegendOrientation = LegendOrientation.Horizontal
            };
            var strokeThickness = 1;
            if (Config.MakePDFCharts) {
                plotModel1.DefaultFontSize = _Parameters.PDFFontSize;
                plotModel1.LegendFontSize = _Parameters.PDFFontSize;
                strokeThickness = 1;
            }
            if (showTitle) {
                plotModel1.Title = plotName;
            }
            var linearAxis1 = new TimeSpanAxis
            {
                Position = AxisPosition.Bottom
            };
            var min = entries.Select(x => x.Values.Min()).Min();
            if (min > 0) {
                min = -0.0001;
            }

            linearAxis1.Minimum = 0;
            linearAxis1.MinimumPadding = 0;
            linearAxis1.MinorTickSize = 0;
            linearAxis1.MaximumPadding = 0.03;
            linearAxis1.MajorStep = 60 * 60 * 6;
            plotModel1.Axes.Add(linearAxis1);
            double start = 0;
            var step = 1.0 / days.Count;
            foreach (var day in days) {
                var ls = new LineSeries();
                ls.Points.Add(new DataPoint(TimeSpanAxis.ToDouble(new TimeSpan(0)), 0));
                ls.Points.Add(new DataPoint(TimeSpanAxis.ToDouble(new TimeSpan(24, 0, 0)), 0));
                ls.Color = OxyColors.LightGray;
                ls.YAxisKey = day;
                plotModel1.Series.Add(ls);
            }
            foreach (var daytype in days) {
                var linearAxis2 = new LinearAxis
                {
                    StartPosition = start,
                    EndPosition = start + step * 0.95,
                    Key = daytype,
                    Title = ChartLocalizer.Get().GetTranslation(daytype + " in " + lti?.UnitOfPower),
                    MinorTickSize = 0
                };
                linearAxis1.Minimum = min;
                linearAxis2.MinimumPadding = 0.005;
                linearAxis2.MaximumPadding = 0.1;
                linearAxis2.Maximum = maxVal;
                plotModel1.Axes.Add(linearAxis2);

                var ls = new LineSeries
                {
                    StrokeThickness = 0.5,
                    Color = OxyColors.Black,
                    YAxisKey = daytype
                };
                ls.Points.Add(new DataPoint(0, 0));
                ls.Points.Add(new DataPoint(maxTimeValue, 0));
                start += step;
            }
            var colorList = new List<OxyColor>
            {
                OxyColors.Green,
                OxyColors.Red,
                OxyColors.Blue
            };
            var seasonColors = new Dictionary<string, int>();
            var seasonCount = 0;
            foreach (var season in seasons) {
                seasonColors.Add(season, seasonCount);
                seasonCount++;
            }
            var labeledSeasons = new List<string>();
            for (var i = 0; i < entries.Count; i++) {
                var ts = new TimeSpan(0);
                var oneDay = new TimeSpan(24, 0, 0);
#pragma warning disable VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
                var stepsize = new TimeSpan(oneDay.Ticks / entries[i].Values.Count);
#pragma warning restore VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
                var lineSeries1 = new LineSeries();
                var seasonLabel = ChartLocalizer.Get().GetTranslation(entries[i].Season);
                if (!labeledSeasons.Contains(seasonLabel)) {
                    lineSeries1.Title = seasonLabel;
                    labeledSeasons.Add(seasonLabel);
                }
                lineSeries1.YAxisKey = entries[i].Day;
                var seasonColor = seasonColors[entries[i].Season];
                lineSeries1.Color = colorList[seasonColor];
                lineSeries1.StrokeThickness = strokeThickness;
                for (var j = 0; j < entries[i].Values.Count; j++) {
                    lineSeries1.Points.Add(new DataPoint(TimeSpanAxis.ToDouble(ts), entries[i].Values[j]));
                    ts = ts.Add(stepsize);
                }
                plotModel1.Series.Add(lineSeries1);
            }

            return plotModel1;
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry srcEntry)
        {
            const string plotName = "Weekday Profiles";
            var entries = new List<Entry>();
            using (var sr = new StreamReader(srcEntry.FullFileName)) {
                var s = sr.ReadLine();
                while (s != "By Season" && !sr.EndOfStream) {
                    s = sr.ReadLine();
                }
                if (s != "By Season") {
                    throw new LPGException("Wrong file!");
                }
                for (var i = 0; i < 3; i++) {
                    s = sr.ReadLine();
                }
                if (s == null) {
                    throw new LPGException("Readline failed");
                }
                var header = s.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);

                foreach (var s1 in header) {
                    entries.Add(new Entry(s1));
                }

                while (!sr.EndOfStream) {
                    var s1 = sr.ReadLine();
                    if (s1 == null) {
                        throw new LPGException("Readline failed");
                    }
                    var cols = s1.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                    for (var index = 0; index < cols.Length; index++) {
                        var col = cols[index];
                        if (col.Length > 0) {
                            var success = double.TryParse(col, out double d);
                            if (!success) {
                                throw new LPGException("Double Trouble");
                            }
                            entries[index].Values.Add(d);
                        }
                    }
                }
            }
            entries.RemoveAt(0);
            entries.RemoveAt(entries.Count - 1);

            var plotModel1 = MakeChart(plotName, entries, _Parameters.ShowTitle, srcEntry.LoadTypeInformation);
            Save(plotModel1, plotName, srcEntry.FullFileName, _Parameters.BaseDirectory);
            return FileProcessingResult.ShouldCreateFiles;
        }

        private class Entry {
            public Entry([JetBrains.Annotations.NotNull] string s) {
                var substr = s.Split(' ');
                Season = substr[0];
                if (substr.Length > 1) {
                    Day = substr[1];
                }
                else {
                    Day = "";
                }
            }

            [JetBrains.Annotations.NotNull]
            public string Day { get; }

            [JetBrains.Annotations.NotNull]
            public string Season { get; }

            [JetBrains.Annotations.NotNull]
            public List<double> Values { get; } = new List<double>();
        }
    }
}
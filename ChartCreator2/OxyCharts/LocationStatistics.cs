using System;
using System.Collections.Generic;
using System.IO;
using Automation;
using Automation.ResultFiles;
using Common;
using OxyPlot;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    internal class LocationStatistics : ChartBaseFileStep
    {
        public LocationStatistics([JetBrains.Annotations.NotNull] ChartCreationParameters parameters,
                                  [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft,
                                  [JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.LocationStatistic
            },
            "Location Statistics", FileProcessingResult.ShouldCreateFiles
        )
        {
        }

        protected override FileProcessingResult MakeOnePlot(ResultFileEntry rfe) {
            Profiler.StartPart(Utili.GetCurrentMethodAndClass());
            string plotName = "Location Statistics " + rfe.HouseholdNumberString;
            var persons = new List<PersonEntry>();
            if (rfe.FullFileName == null)
            {
                throw new LPGException("filename was null");
            }
            using (var sr = new StreamReader(rfe.FullFileName)) {
                PersonEntry lastPerson = null;
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("File " + rfe.FullFileName + " was empty.");
                    }
                    if (s.StartsWith("----", StringComparison.CurrentCulture)) {
                        var s2 = sr.ReadLine();
                        if (s2 == null) {
                            throw new LPGException("readline failed");
                        }
                        lastPerson = new PersonEntry(s2);
                        persons.Add(lastPerson);
                    }
                    else {
                        if (lastPerson == null) {
                            throw new LPGException("lastperson was null");
                        }
                        var cols = s.Split(Parameters.CSVCharacterArr, StringSplitOptions.None);
                        var val = Utili.ConvertToDoubleWithMessage(cols[2], "LocationStatisticsPlot");
                        if (val > 0) {
                            lastPerson.Percentages.Add(cols[0], val);
                        }
                    }
                }
            }
            foreach (var entry in persons) {
                var plotModel1 = new PlotModel
                {
                    LegendBorderThickness = 0,
                    LegendOrientation = LegendOrientation.Horizontal,
                    LegendPlacement = LegendPlacement.Outside,
                    LegendPosition = LegendPosition.BottomCenter
                };
                if (Parameters.ShowTitle) {
                    plotModel1.Title = plotName;
                }

                var pieSeries1 = new PieSeries
                {
                    InsideLabelColor = OxyColors.White,
                    InsideLabelPosition = 0.8,
                    StrokeThickness = 2,
                    AreInsideLabelsAngled = true
                };
                foreach (var tuple in entry.Percentages) {
                    var name = tuple.Key.Trim();
                    if (name.Length > 30) {
                        name = name.Substring(0, 20) + "...";
                    }
                    var slice = new PieSlice(name, tuple.Value);

                    pieSeries1.Slices.Add(slice);
                }

                plotModel1.Series.Add(pieSeries1);
                var newfilename = "LocationStatistics." + entry.CleanName;
                Save(plotModel1, plotName, rfe.FullFileName, Parameters.BaseDirectory,CalcOption.LocationsFile, newfilename);
            }
            Profiler.StopPart(Utili.GetCurrentMethodAndClass());
            return FileProcessingResult.ShouldCreateFiles;
        }

        private class PersonEntry {
            public PersonEntry([JetBrains.Annotations.NotNull] string personName) => PersonName = personName;

            [JetBrains.Annotations.NotNull]
            public string CleanName => AutomationUtili.CleanFileName(PersonName);

            [JetBrains.Annotations.NotNull]
            public Dictionary<string, double> Percentages { get; } = new Dictionary<string, double>();

            [JetBrains.Annotations.NotNull]
            private string PersonName { get; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ChartCreator2.OxyCharts {
    internal class DeviceProfiles : ChartBaseFileStep
    {
        [NotNull] private readonly SqlResultLoggingService _srls;

        public DeviceProfiles([NotNull] ChartCreationParameters parameters,
                              [NotNull] FileFactoryAndTracker fft,
                              [NotNull] ICalculationProfiler calculationProfiler,
                              [NotNull] SqlResultLoggingService srls) : base(parameters, fft,
            calculationProfiler, new List<ResultFileID>() { ResultFileID.DeviceProfileCSV
            },
            "Device Profiles", FileProcessingResult.ShouldCreateFiles
        )
        {
            _srls = srls;
        }

        [UsedImplicitly]
        public static int DaysToMake { get; set; } = 5;

        private static DateTime GetDay(DateTime x) => new DateTime(x.Year, x.Month, x.Day);

        private void GetFirstAndLastDate([NotNull] string fileName, out DateTime firstDate, out DateTime lastDate)
        {
            firstDate = DateTime.MinValue;
            lastDate = DateTime.MinValue;
            using (var sr = new StreamReader(fileName)) {
                sr.ReadLine();
                var line = 0;
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed.");
                    }
                    var cols = s.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                    var success1 = DateTime.TryParse(cols[1], CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dt);
                    if (success1) {
                        if (line == 0) {
                            firstDate = dt;
                        }
                        lastDate = dt;
                        line++;
                    }
                }
            }
            firstDate = GetDay(firstDate);
            lastDate = GetDay(lastDate);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void MakeChartFromDay([NotNull] string fileName, [NotNull] string plotName, [NotNull] DirectoryInfo basisPath, [NotNull] string yaxisLabel,
            TimeSpan timestep,
            [ItemNotNull] [NotNull] List<string> headers,
            [NotNull] DayEntry day,
            [NotNull] DeviceTaggingSetInformation taggingSet,
            bool makePng)
        {
            // process results
            var columns = new List<MyColumn>();
            for (var i = 0; i < headers.Count; i++) {
                var header = headers[i];
                columns.Add(new MyColumn(header, i, taggingSet, basisPath,FFT));
            }
            foreach (var valueArr in day.Values) {
                foreach (var column in columns) {
                    column.Values.Add(valueArr[column.Column]);
                    column.Sum += valueArr[column.Column];
                }
            }
            columns.RemoveAt(0); // remove first two columns with the time stamps
            columns.RemoveAt(0);
            var newColumns = new List<MyColumn>();
            var tags = columns.Select(x => x.Tag).Distinct().ToList();
            var tagNumber = 0;
            foreach (var tag in tags) {
                var myc = new MyColumn(tag, tagNumber++, null, basisPath,FFT);
                newColumns.Add(myc);
            }
            for (var j = 0; j < columns[0].Values.Count; j++) {
                foreach (var newColumn in newColumns) {
                    var colsForTag = columns.Where(x => x.Tag == newColumn.RawName).ToList();
                    var sum = colsForTag.Sum(x => x.Values[j]);
                    newColumn.Values.Add(sum);
                }
                var newSum = newColumns.Sum(x => x.Values[j]);
                var oldsum = columns.Sum(x => x.Values[j]);
                if (Math.Abs(newSum - oldsum) > 0.001) {
                    throw new LPGException("Missing values");
                }
            }
            foreach (var column in newColumns) {
                column.Sum = column.Values.Sum();
            }
            newColumns.Sort((x, y) => y.Sum.CompareTo(x.Sum));
            var plotModel1 = new PlotModel
            {
                // general
                LegendBorderThickness = 0,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,

                LegendPosition = LegendPosition.BottomCenter
            };
            if (Config.MakePDFCharts) {
                plotModel1.LegendFontSize =_Parameters.PDFFontSize;
                plotModel1.DefaultFontSize = _Parameters.PDFFontSize;
            }
            if (Config.SpecialChartFontSize != null) {
                plotModel1.LegendFontSize = Config.SpecialChartFontSize.Value;
                plotModel1.DefaultFontSize = Config.SpecialChartFontSize.Value;
            }
            if (_Parameters.ShowTitle) {
                plotModel1.Title = plotName;
            }
            var dateTimeAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd.MM. HH:mm",
                MajorStep = 0.25,
                MaximumPadding = 0.05
            };
            plotModel1.Axes.Add(dateTimeAxis);

            // axes
            var linearAxis2 = new LinearAxis
            {
                AbsoluteMinimum = 0,
                MaximumPadding = 0.06,
                MinimumPadding = 0,
                Title = yaxisLabel
            };
            plotModel1.Axes.Add(linearAxis2);
            // data
            var p = OxyPalettes.Hue64;
            if (newColumns.Count > 1) {
                p = OxyPalettes.Jet(newColumns.Count);
            }

            var currentTime = new TimeSpan(0);
            for (var i = 0; i < newColumns.Count; i++) {
                // main columns
                var column = newColumns[i];
                var columnSeries2 = new AreaSeries
                {
                    StrokeThickness = 0,
                    Title = ChartLocalizer.Get().GetTranslation(column.RawName)
                };
                for (var j = 0; j < column.Values.Count; j++) {
                    currentTime = currentTime.Add(timestep);
                    double sum = 0;
                    for (var k = i - 1; k >= 0; k--) {
                        sum += newColumns[k].Values[j];
                    }
                    var dt = day.Times[j];
                    var bottom = new DataPoint(DateTimeAxis.ToDouble(dt), sum);
                    columnSeries2.Points.Add(bottom);
                    var top = new DataPoint(DateTimeAxis.ToDouble(dt), sum + column.Values[j]);
                    columnSeries2.Points2.Add(top);
                }
                columnSeries2.Color2 = p.Colors[i];
                columnSeries2.Color = p.Colors[i];
                columnSeries2.Fill = p.Colors[i];
                plotModel1.Series.Add(columnSeries2);
            }
            var thisname = fileName + "." + taggingSet.Name + "." + day.Day.Year + "." + day.Day.Month + "." +
                           day.Day.Day;
            Save(plotModel1, plotName, thisname, basisPath, makePng: makePng);
        }

        protected override FileProcessingResult MakeOnePlot([NotNull] ResultFileEntry srcEntry)
        {
            string plotName = "Devices " + srcEntry.HouseholdKey + " " + srcEntry.LoadTypeInformation?.Name;
            _CalculationProfiler.StartPart(Utili.GetCurrentMethodAndClass());
            var lti = srcEntry.LoadTypeInformation;
            if(lti == null) {
                throw new LPGException("LTI was null");
            }

            var timestep = srcEntry.TimeResolution;
            var unitName = lti.Name + " in " + lti.UnitOfPower + string.Empty;
            var yaxisLabel = ChartLocalizer.Get().GetTranslation(unitName);
            var conversionfactor = lti.ConversionFaktor;
            GetFirstAndLastDate(srcEntry.FullFileName, out DateTime first, out var last);
            var selectedDateTimes = new List<DateTime>();
            var r = new Random();
            var allDays = new List<DateTime>();
            var curr = first;
            while (curr <= last) {
                allDays.Add(curr);
                curr = curr.AddDays(1);
            }
            for (var i = 0; i < DaysToMake && allDays.Count > 0; i++) {
                var idx = r.Next(allDays.Count);
                selectedDateTimes.Add(allDays[idx]);
                allDays.RemoveAt(idx);
            }
            selectedDateTimes.Sort();
            //var tagFiles =_Parameters.BaseDirectory.GetFiles(Constants.DeviceTaggingSetFileName);
            var taggingSets = DeviceTaggingSetList.Read(_srls);
           var x = ReadAllDays(srcEntry, conversionfactor, selectedDateTimes,
               taggingSets, plotName, _Parameters.BaseDirectory,
                yaxisLabel, timestep);
            _CalculationProfiler.StopPart(Utili.GetCurrentMethodAndClass());
            return x;
        }

        private FileProcessingResult ReadAllDays([NotNull] ResultFileEntry rfe, double conversionfactor, [NotNull] List<DateTime> selectedDateTimes,
            [NotNull] DeviceTaggingSetList taggingSets, [NotNull] string plotName, [NotNull] DirectoryInfo basisPath, [NotNull] string yaxisLabel,
            TimeSpan timestep)
        {
            var pngCount = 0;
            try {
                var headers = new List<string>();
                using (var sr = new StreamReader(rfe.FullFileName)) {
                    var topLine = sr.ReadLine();
                    if (topLine == null) {
                        throw new LPGException("Readline failed.");
                    }
                    var header1 = topLine.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                    if (header1.Length < 2) {
                        throw new LPGException("Could not split the line from the device profiles properly: " +
                                               header1);
                    }
                    headers.AddRange(header1);
                    var dayEntry = new DayEntry(DateTime.MinValue);
                    while (!sr.EndOfStream) {
                        var s = sr.ReadLine();
                        if (s == null) {
                            throw new LPGException("Readline failed.");
                        }
                        var cols = s.Split(_Parameters.CSVCharacterArr, StringSplitOptions.None);
                        if (cols.Length < 2) {
                            throw new LPGException("Could not split the line from the device profiles properly: " + s);
                        }

                        var success1 = DateTime.TryParse(cols[1], CultureInfo.CurrentCulture, DateTimeStyles.None,
                            out var dt);
                        if (!success1) {
                            Logger.Error("Parsing failed in Deviceprofile.MakePlot.");
                            continue;
                        }
                        if (GetDay(dt) != dayEntry.Day) {
                            if (selectedDateTimes.Contains(dayEntry.Day)) {
                                var loadtypeNames =
                                    taggingSets.TaggingSets.SelectMany(x => x.LoadTypesForThisSet).Select(y => y.Name)
                                        .ToList();
                                foreach (var deviceTaggingSetInfo in taggingSets.TaggingSets) {
                                    {
                                        var sum = dayEntry.Values.Select(x => x.Sum()).Sum();
                                        if (loadtypeNames.Contains(rfe.LoadTypeInformation?.Name) && Math.Abs(sum) > Constants.Ebsilon) {
                                            var makePng = pngCount <= 8;
                                            MakeChartFromDay(rfe.FullFileName, plotName, basisPath, yaxisLabel,
                                                timestep,
                                                headers,
                                                dayEntry, deviceTaggingSetInfo, makePng);
                                            GC.WaitForPendingFinalizers();
                                            GC.Collect();

                                            pngCount++;
                                        }
                                    }
                                }
                            }
                            dayEntry = new DayEntry(GetDay(dt));
                        }
                        dayEntry.Times.Add(dt);
                        var result = new double[headers.Count];
                        for (var index = 0; index < cols.Length; index++) {
                            var col = cols[index];
                            var success = double.TryParse(col, out double d);
                            if (success) {
                                result[index] = d / conversionfactor;
                            }
                        }
                        dayEntry.Values.Add(result);
                    }
                }
            }
            catch (Exception ex) {
                Logger.Exception(ex);
                throw;
            }
            if(pngCount > 0) {
                return FileProcessingResult.ShouldCreateFiles;
            }
            return FileProcessingResult.NoFilesTocreate;
        }

        private class DayEntry {
            public DayEntry(DateTime day) => Day = day;

            public DateTime Day { get; }
            [NotNull]
            public List<DateTime> Times { get; } = new List<DateTime>();

            [ItemNotNull]
            [NotNull]
            public List<double[]> Values { get; } = new List<double[]>();
        }

        private class MyColumn {
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
            public MyColumn([NotNull] string name, int column, [CanBeNull] DeviceTaggingSetInformation taggingSet,
                [NotNull] DirectoryInfo basisPath, [NotNull] FileFactoryAndTracker fft)
            {
                Tag = "";
                RawName = name;
                Column = column;
                if (name.Contains(" [")) {
                    var deviceWithRoom = name.Substring(name.IndexOf(" - ", StringComparison.Ordinal) + 3);
                    deviceWithRoom =
                        deviceWithRoom.Substring(0, deviceWithRoom.IndexOf(" [", StringComparison.Ordinal)).Trim();
                    var device =
                        deviceWithRoom.Substring(deviceWithRoom.IndexOf(" - ", StringComparison.Ordinal) + 3).Trim();

                    try {
                        if (taggingSet == null) {
                            throw new LPGException("Tagging set was null");
                        }
                        if (!taggingSet.TagByDeviceName.ContainsKey(device)) {
                            var fileName = Path.Combine(basisPath.FullName, "missingCategories.txt");
                            if (!fft.CheckForFile(ResultFileID.MissingTags, Constants.GeneralHouseholdKey)) {
                                fft.RegisterFile(fileName, "Devices that are missing device tags", false,
                                    ResultFileID.MissingTags,
                                    Constants.GeneralHouseholdKey, TargetDirectory.Root);
                            }
                            taggingSet.TagByDeviceName.Add(device, "Other");
                            using (var sw = new StreamWriter(fileName, true)) {
                                sw.WriteLine(device);
                            }
                            Logger.Error("Missing entry in the device tagging set " + taggingSet.Name + " for the device " + device);
                        }
                    }
                    catch (Exception ex) {
                        Logger.Error(
                            "Couldn't write to missing categories file: " + basisPath + " Error message was:" +
                            ex.Message);
                        Logger.Exception(ex);
                    }
                    if (taggingSet == null) {
                        throw new LPGException("Tagging set was null");
                    }
                    Tag = taggingSet.TagByDeviceName[device];
                }
            }

            public int Column { get; }
            [NotNull]
            public string RawName { get; }
            public double Sum { get; set; }
            [NotNull]
            public string Tag { get; }

            [NotNull]
            public List<double> Values { get; } = new List<double>();

            public override string ToString() => "Tag: " + Tag + " Name: " + RawName;
        }
    }
}
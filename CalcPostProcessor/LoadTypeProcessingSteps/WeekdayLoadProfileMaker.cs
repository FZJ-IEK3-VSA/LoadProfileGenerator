//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.LoadTypeProcessingSteps {
    internal class WeekdayLoadProfileMaker : LoadTypeStepBase
    {
        [NotNull]
        private readonly IFileFactoryAndTracker _fft;

        public WeekdayLoadProfileMaker(
                                       [NotNull] IFileFactoryAndTracker fft,
                                       [NotNull] CalcDataRepository repository,
                                       [NotNull] ICalculationProfiler profiler
                                       ):base(repository, AutomationUtili.GetOptionList(CalcOption.WeekdayProfiles),profiler,"Weekday Profiles")
        {
            _fft = fft;
        }

        [NotNull]
        private static string GetHeaderForSeasonKey(int key) {
            switch (key) {
                case 0: return "Winter Sunday";
                case 1: return "Winter Saturday";
                case 2: return "Winter Weekday";
                case 10: return "Spring/Autumn Sunday";
                case 11: return "Spring/Autumn Saturday";
                case 12: return "Spring/Autumn Weekday";
                case 20: return "Summer Sunday";
                case 21: return "Summer Saturday";
                case 22: return "Summer Weekday";
                default: throw new LPGException("unknown season");
            }
        }

        private static int GetSeasonID(int month, int day) {
            // just some numbers to create a clear mapping
            var season = -1;
            if (month == 1 || month == 2 || month == 12) // winter
            {
                season = 0;
            }
            if (month == 3 || month == 4 || month == 5 || month == 9 || month == 10 || month == 11) {
                season = 10;
            }
            if (month == 6 || month == 7 || month == 8) // summer
            {
                season = 20;
            }
            var daytype = -1;
            if (day == 0) {
                daytype = 0;
            }
            if (day == 6) {
                daytype = 1;
            }
            if (day == 1 || day == 2 || day == 3 || day == 4 || day == 5) {
                daytype = 2;
            }
            if (season == -1 || day == -1) {
                throw new LPGException("unkown season ID");
            }
            return season + daytype;
        }

        [NotNull]
        private  string MakeWriteableString(int timestep)
        {
            var calcParameters = Repository.CalcParameters;
            var sb = new StringBuilder();
            sb.Append(timestep);
            sb.Append(calcParameters.CSVCharacter);
            var ts = new TimeSpan(timestep * calcParameters.InternalStepsize.Ticks);
            sb.Append(ts.ToString(@"hh\:mm\:ss", Config.CultureInfo));
            sb.Append(calcParameters.CSVCharacter + "'");
            sb.Append(ts.ToString(@"hh\:mm\:ss", Config.CultureInfo));
            sb.Append(calcParameters.CSVCharacter);
            return sb.ToString();
        }

        private  void ReadFile([NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
            [NotNull] Dictionary<int, Dictionary<int, double>> dailyValues,
            [NotNull] Dictionary<int, Dictionary<int, Dictionary<int, double>>> dailyValuesbyMonth,
            [NotNull] Dictionary<int, Dictionary<int, double>> dailyValuesbySeason, [NotNull] Dictionary<DayOfWeek, int> dayCount,
            [NotNull] Dictionary<int, int> seasonDayCount) {
            var calcParameters = Repository.CalcParameters;
            var curTime = calcParameters.OfficialStartTime;
            var curDate = calcParameters.OfficialStartTime;
            dayCount.Add(curTime.DayOfWeek, 1);
            if (!seasonDayCount.ContainsKey(GetSeasonID(curTime.Month, (int) curTime.DayOfWeek))) {
                seasonDayCount.Add(GetSeasonID(curTime.Month, (int) curTime.DayOfWeek), 0);
            }
            seasonDayCount[GetSeasonID(curTime.Month, (int) curTime.DayOfWeek)] = 1;

            foreach (var efr in energyFileRows) {
                var day = (int) curTime.DayOfWeek;
                var timestep = (int) ((curTime - curDate).TotalSeconds /
                                      calcParameters.InternalStepsize.TotalSeconds);
                if (!dailyValues.ContainsKey(day)) {
                    dailyValues.Add(day, new Dictionary<int, double>());
                }
                if (!dailyValues[day].ContainsKey(timestep)) {
                    dailyValues[day].Add(timestep, 0);
                }
                var sum = efr.SumCached;
                dailyValues[day][timestep] += sum;
                if (!dailyValuesbyMonth.ContainsKey(curTime.Month)) {
                    dailyValuesbyMonth.Add(curTime.Month, new Dictionary<int, Dictionary<int, double>>());
                }
                if (!dailyValuesbyMonth[curTime.Month].ContainsKey(day)) {
                    dailyValuesbyMonth[curTime.Month].Add(day, new Dictionary<int, double>());
                }
                if (!dailyValuesbyMonth[curTime.Month][day].ContainsKey(timestep)) {
                    dailyValuesbyMonth[curTime.Month][day].Add(timestep, 0);
                }
                dailyValuesbyMonth[curTime.Month][day][timestep] += sum;

                if (!dailyValuesbySeason.ContainsKey(GetSeasonID(curTime.Month, day))) {
                    dailyValuesbySeason.Add(GetSeasonID(curTime.Month, day), new Dictionary<int, double>());
                }
                if (!dailyValuesbySeason[GetSeasonID(curTime.Month, day)].ContainsKey(timestep)) {
                    dailyValuesbySeason[GetSeasonID(curTime.Month, day)].Add(timestep, 0);
                }
                dailyValuesbySeason[GetSeasonID(curTime.Month, day)][timestep] += sum;
                curTime += calcParameters.InternalStepsize;

                if (curDate.Day != curTime.Day) {
                    if (!seasonDayCount.ContainsKey(GetSeasonID(curDate.Month, day))) {
                        seasonDayCount.Add(GetSeasonID(curDate.Month, day), 0);
                    }
                    seasonDayCount[GetSeasonID(curDate.Month, day)] += 1;
                    if (!dayCount.ContainsKey(curDate.DayOfWeek)) {
                        dayCount.Add(curDate.DayOfWeek, 0);
                    }
                    dayCount[curDate.DayOfWeek] += 1;
                    curDate = new DateTime(curTime.Year, curTime.Month, curTime.Day);
                }
            }
        }

        private  void Run([NotNull] CalcLoadTypeDto dstLoadType, [NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
            [NotNull] IFileFactoryAndTracker fft) {
            var calcParameters = Repository.CalcParameters;
            var dailyValues = new Dictionary<int, Dictionary<int, double>>();
            var dailyValuesbyMonth =
                new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();
            var dailyValuesbySeason =
                new Dictionary<int, Dictionary<int, double>>();
            var dayCount = new Dictionary<DayOfWeek, int>();

            var seasonDayCount = new Dictionary<int, int>();

            ReadFile(energyFileRows, dailyValues, dailyValuesbyMonth, dailyValuesbySeason, dayCount, seasonDayCount);

            var resultfile = fft.MakeFile<StreamWriter>("WeekdayProfiles." + dstLoadType.Name + ".csv",
                "Averaged profiles for each weekday for " + dstLoadType.Name, true,
                ResultFileID.WeekdayLoadProfileID, Constants.GeneralHouseholdKey, TargetDirectory.Reports,
                calcParameters.InternalStepsize,CalcOption.WeekdayProfiles,
                dstLoadType.ConvertToLoadTypeInformation());
            var valuecount = WriteNormalPart(dstLoadType, resultfile, dailyValues, dayCount);
            WriteByMonth(resultfile, dailyValuesbyMonth, valuecount);
            WriteBySeason(resultfile, valuecount, dailyValuesbySeason, seasonDayCount);
            resultfile.Flush();
            resultfile.Close();
        }

        private  void WriteByMonth([NotNull] StreamWriter resultfile,
            [NotNull] Dictionary<int, Dictionary<int, Dictionary<int, double>>> dailyValuesbyMonth, int valuecount) {
            var calcParameters = Repository.CalcParameters;
            resultfile.WriteLine();
            resultfile.WriteLine("---------------------------------------");
            resultfile.WriteLine("By Month");
            resultfile.WriteLine("---------------------------------------");
            resultfile.WriteLine();
            var header = "Time" + calcParameters.CSVCharacter;
            foreach (var keyValuePair in dailyValuesbyMonth) {
                var month = keyValuePair.Key;
                var dailydict = keyValuePair.Value;
                foreach (var valuePair in dailydict) {
                    var weekday = valuePair.Key;
                    var wd = (DayOfWeek) weekday;
                    header += "Month " + month + ", weekday" + wd + calcParameters.CSVCharacter;
                }
            }
            resultfile.WriteLine(header);

            for (var i = 0; i < valuecount; i++) {
                var line = i + calcParameters.CSVCharacter;
                foreach (var keyValuePair in dailyValuesbyMonth) {
                    var dailydict = keyValuePair.Value;
                    foreach (var valuePair in dailydict) {
                        var minutedict = valuePair.Value;
                        line += minutedict[i] + calcParameters.CSVCharacter;
                    }
                }
                resultfile.WriteLine(line);
            }
        }

        private  void WriteBySeason([NotNull] StreamWriter resultfile, int valuecount,
            [NotNull] Dictionary<int, Dictionary<int, double>> dailyValuesbySeason, [NotNull] Dictionary<int, int> seasonDayCount) {
            var calcParameters = Repository.CalcParameters;
            resultfile.WriteLine();
            resultfile.WriteLine("---------------------------------------");
            resultfile.WriteLine("By Season");
            resultfile.WriteLine("---------------------------------------");
            resultfile.WriteLine();
            var header = "Time" + calcParameters.CSVCharacter;

            int[] allseasons = {0, 1, 2, 10, 11, 12, 20, 21, 22};
            foreach (var season in allseasons) {
                header += GetHeaderForSeasonKey(season) + calcParameters.CSVCharacter;
            }
            resultfile.WriteLine(header);

            for (var i = 0; i < valuecount; i++) {
                var line = i + calcParameters.CSVCharacter;
                foreach (var season in allseasons) {
                    if (dailyValuesbySeason.ContainsKey(season)) {
                        line += dailyValuesbySeason[season][i] / seasonDayCount[season] +
                                calcParameters.CSVCharacter;
                    }
                    else {
                        line += "0" + calcParameters.CSVCharacter;
                    }
                }
                resultfile.WriteLine(line);
            }
        }

        private  int WriteNormalPart([NotNull] CalcLoadTypeDto dstLoadType, [NotNull] StreamWriter resultfile,
            [NotNull] Dictionary<int, Dictionary<int, double>> dailyValues, [NotNull] Dictionary<DayOfWeek, int> dayCount) {
            var calcParameters = Repository.CalcParameters;
            var headerdays = string.Empty;
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek))) {
                headerdays += day.ToString();
                headerdays += calcParameters.CSVCharacter;
            }
            resultfile.WriteLine(dstLoadType.Name + ".Time;Calender;Calender for Excel" +
                                 calcParameters.CSVCharacter + headerdays);

            var valuecount = dailyValues.Values.First().Count;
            for (var i = 0; i < valuecount; i++) {
                var time = MakeWriteableString(i);
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek))) {
                    double value = 0;
                    if (dailyValues.ContainsKey((int) day)) {
                        value = dailyValues[(int) day][i];
                    }
                    if (dayCount.ContainsKey(day)) {
                        time += value / dayCount[day] + calcParameters.CSVCharacter;
                    }
                    else {
                        time += calcParameters.CSVCharacter;
                    }
                }
                resultfile.WriteLine(time);
            }
            return valuecount;
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            LoadtypeStepParameters p = (LoadtypeStepParameters)parameters;
            Run(p.LoadType,p.EnergyFileRows,_fft);
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>();
    }
}
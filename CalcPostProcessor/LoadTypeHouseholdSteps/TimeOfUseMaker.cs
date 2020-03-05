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
using System.Text;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.LoadTypeHouseholdSteps {
    public  class TimeOfUseMaker: LoadTypeStepBase
    {
        [NotNull]
        private readonly CalcParameters _calcParameters;
        [NotNull]
        private readonly FileFactoryAndTracker _fft;

        public TimeOfUseMaker(
                              [NotNull] CalcDataRepository repository,
                              [NotNull] ICalculationProfiler profiler,
                              [NotNull] FileFactoryAndTracker fft):base(repository,
            AutomationUtili.GetOptionList(CalcOption.TimeOfUsePlot), profiler, "Time of Use Averages")
        {
            _calcParameters = Repository.CalcParameters;
            _fft = fft;
        }

        [NotNull]
        private StringBuilder MakeWriteableString(int timestep) {
            var sb = new StringBuilder();
            sb.Append(timestep);
            sb.Append(_calcParameters.CSVCharacter);
            var ts = new TimeSpan(timestep * _calcParameters.InternalStepsize.Ticks);
            sb.Append(ts.ToString(@"hh\:mm\:ss", Config.CultureInfo));
            sb.Append(_calcParameters.CSVCharacter);
            if (_calcParameters.WriteExcelColumn) {
                sb.Append("'");
                sb.Append(ts.ToString(@"hh\:mm\:ss", Config.CultureInfo));
                sb.Append(_calcParameters.CSVCharacter);
            }
            return sb;
        }

        private  void Run([NotNull] CalcLoadTypeDto dstLoadType, [NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
            [NotNull] FileFactoryAndTracker fft, [NotNull] EnergyFileColumns efcs) {
            var comma = _calcParameters.CSVCharacter;
            var dailyMinute = new Dictionary<string, double[]>();
            var dailyEnergys = new Dictionary<string, double[]>();
            var dailyEnergysByDay =
                new Dictionary<DayOfWeek, Dictionary<string, double[]>>();
            var efc = efcs.ColumnEntriesByColumn[dstLoadType];
            var headerNames = new List<string>();
            var dayofWeekDaycount = new Dictionary<DayOfWeek, int>();
            foreach (DayOfWeek value in Enum.GetValues(typeof(DayOfWeek))) {
                dailyEnergysByDay.Add(value, new Dictionary<string, double[]>());
                dayofWeekDaycount.Add(value, 0);
            }

            foreach (var colEntry in efc) {
                if (!dailyMinute.ContainsKey(colEntry.Value.Name)) {
                    dailyMinute.Add(colEntry.Value.Name, new double[1440]);
                    dailyEnergys.Add(colEntry.Value.Name, new double[1440]);
                    headerNames.Add(colEntry.Value.Name);
                    foreach (DayOfWeek value in Enum.GetValues(typeof(DayOfWeek))) {
                        dailyEnergysByDay[value].Add(colEntry.Value.Name, new double[1440]);
                    }
                }
            }

            var curTime = _calcParameters.OfficialStartTime;
            var curDate = _calcParameters.OfficialStartTime;
            var daycount = 0;

            foreach (var efr in energyFileRows) {
                // calculate the time step
                var timestep = (int) ((curTime - curDate).TotalSeconds / 60.0); // 60s in one minute
                // for each column make sums
                for (var i = 0; i < efr.EnergyEntries.Count; i++) {
                    var name = efc[i].Name;
                    if (efr.EnergyEntries[i] > 0 && efc[i].LocationName != "(autonomous device)")
                        // don't count standby for the time
                    {
                        dailyMinute[name][timestep]++;
                    }
                    dailyEnergysByDay[curTime.DayOfWeek][name][timestep] += efr.EnergyEntries[i];
                    dailyEnergys[name][timestep] += efr.EnergyEntries[i];
                }
                curTime += _calcParameters.InternalStepsize;
                if (curDate.Day != curTime.Day) {
                    dayofWeekDaycount[curDate.DayOfWeek]++;
                    curDate = new DateTime(curTime.Year, curTime.Month, curTime.Day);
                    daycount++;
                }
            }

            var resultfileMinute = fft.MakeFile<StreamWriter>("TimeOfUseProfiles." + dstLoadType.Name + ".csv",
                "Time of Use Profiles for all devices with " + dstLoadType.Name, true,
                 ResultFileID.TimeOfUse, Constants.GeneralHouseholdKey, TargetDirectory.Reports, _calcParameters.InternalStepsize,
                dstLoadType.ConvertToLoadTypeInformation());
            var resultfileEnergy = fft.MakeFile<StreamWriter>(
                "TimeOfUseEnergyProfiles." + dstLoadType.Name + ".csv",
                "Time of Use Profiles with the power consumption for all devices with " + dstLoadType.Name, true,
                ResultFileID.TimeOfUseEnergy, Constants.GeneralHouseholdKey, TargetDirectory.Reports, _calcParameters.InternalStepsize,
                dstLoadType.ConvertToLoadTypeInformation());
            var headerdays = string.Empty;
            foreach (var colEntry in headerNames) {
                headerdays += colEntry + comma;
            }
            var header = dstLoadType.Name + ".Time" + comma + "Calender" + comma;
            if (_calcParameters.WriteExcelColumn) {
                header += "Calender for Excel" + comma;
            }
            header += headerdays;
            resultfileMinute.WriteLine(header);
            resultfileEnergy.WriteLine(header);
            for (var i = 0; i < 1440; i++) {
                var time = MakeWriteableString(i);
                var time2 = new StringBuilder(time.ToString());
                foreach (var colEntry in dailyMinute) {
                    var value = colEntry.Value[i];
                    time.Append(value);
                    time.Append(comma);
                }
                resultfileMinute.WriteLine(time);
                foreach (var colEntry in dailyEnergys) {
                    var value = colEntry.Value[i];
                    time2.Append(value / daycount);
                    time2.Append(comma);
                }
                resultfileEnergy.WriteLine(time2);
            }
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek))) {
                resultfileEnergy.WriteLine();
                resultfileEnergy.WriteLine("----");
                resultfileEnergy.WriteLine(day.ToString());
                resultfileEnergy.WriteLine("----");
                resultfileEnergy.WriteLine();
                resultfileEnergy.WriteLine(header);
                for (var i = 0; i < 1440; i++) {
                    var time = MakeWriteableString(i);

                    foreach (var colEntry in dailyEnergysByDay[day]) {
                        var value = colEntry.Value[i];
                        time.Append(value / dayofWeekDaycount[day]);
                        time.Append(comma);
                    }
                    resultfileEnergy.WriteLine(time);
                }
            }
            resultfileMinute.Flush();
            resultfileMinute.Close();
            resultfileEnergy.Flush();
            resultfileEnergy.Close();
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            LoadtypeStepParameters p = (LoadtypeStepParameters)parameters;
            var efc = Repository.ReadEnergyFileColumns(Constants.GeneralHouseholdKey);
            Run(p.LoadType, p.EnergyFileRows, _fft, efc);
        }
    }
}
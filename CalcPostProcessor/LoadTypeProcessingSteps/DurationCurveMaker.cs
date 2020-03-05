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
    public class DurationCurveMaker: LoadTypeStepBase
    {
        [NotNull]
        private readonly CalcParameters _calcParameters;
        [NotNull]
        private readonly FileFactoryAndTracker _fft;

        public DurationCurveMaker([NotNull] CalcDataRepository repository,
                                  [NotNull] ICalculationProfiler calculationProfiler,
                                  [NotNull] FileFactoryAndTracker fft)
            :base(repository, AutomationUtili.GetOptionList(CalcOption.DurationCurve),calculationProfiler,"Duration Curves")
        {
            _calcParameters = Repository.CalcParameters;
            _fft = fft;
        }
        private void Run([NotNull] CalcLoadTypeDto dstLoadType, [NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
                        [NotNull] EnergyFileColumns efc) {
            var comma = _calcParameters.CSVCharacter;
            var entries = new Dictionary<string, List<double>>();
            var sums = new List<double>();

            var columns = efc.ColumnEntriesByColumn[dstLoadType].Values.ToList();
            var curline = 0;
            foreach (var efr in energyFileRows) {
                for (var i = 0; i < columns.Count; i++) {
                    var name = columns[i].Name;
                    if (!entries.ContainsKey(name)) {
                        entries.Add(name, new List<double>());
                    }
                    if (entries[name].Count > curline) {
                        entries[name][curline] += efr.EnergyEntries[i];
                    }
                    else {
                        entries[name].Add(efr.EnergyEntries[i]);
                    }
                }
                sums.Add(efr.SumCached);
                curline++;
            }
            foreach (var keyValuePair in entries) {
                if (keyValuePair.Value.Count != sums.Count) {
                    throw new LPGException("Uneven Timestep count while creating the duration curves. This is a bug!");
                }
            }
            sums.Sort((x, y) => y.CompareTo(x));
            var ts = new TimeSpan(0, 0, 0);
            var timestep = 0;
            // sums
            var sumfile = _fft.MakeFile<StreamWriter>("DurationCurve." + dstLoadType.Name + ".csv",
                "Summed up household duration curve for " + dstLoadType.Name, true,
                 ResultFileID.DurationCurveSums, Constants.GeneralHouseholdKey, TargetDirectory.Reports, _calcParameters.InternalStepsize,
                dstLoadType.ConvertToLoadTypeInformation());
            sumfile.WriteLine("Timestep" + comma + "Time span" + comma + "Sum [" + dstLoadType.UnitOfPower + "]");
            var sumsb = new StringBuilder();
            foreach (var sum in sums) {
                sumsb.Clear();
                sumsb.Append(timestep);
                sumsb.Append(comma);
                sumsb.Append(ts);
                sumsb.Append(comma);
                sumsb.Append(sum);
                sumfile.WriteLine(sumsb.ToString());
                timestep++;
                ts = ts.Add(_calcParameters.InternalStepsize);
            }
            sumfile.Close();
            // individual devices
            var normalfile = _fft.MakeFile<StreamWriter>("DeviceDurationCurves." + dstLoadType.Name + ".csv",
                "Duration curve for each device for " + dstLoadType.Name, true,
                 ResultFileID.DurationCurveDevices, Constants.GeneralHouseholdKey, TargetDirectory.Reports, _calcParameters.InternalStepsize,
                dstLoadType.ConvertToLoadTypeInformation());

            var header = string.Empty;
            foreach (var keyValuePair in entries) {
                header += keyValuePair.Key + " [" + dstLoadType.UnitOfPower + "]" + comma;
            }
            normalfile.WriteLine(dstLoadType.Name + ".Time" + comma + "Time span" + comma + header);
            foreach (var keyValuePair in entries) {
                keyValuePair.Value.Sort();
            }
            timestep = 0;
            ts = new TimeSpan(0, 0, 0);
            var sb = new StringBuilder();
            var values = entries.Values.ToArray();
            for (var i = 0; i < sums.Count; i++) {
                sb.Clear();
                sb.Append(timestep);
                sb.Append(comma);
                sb.Append(ts);
                sb.Append(comma);
                for (var index = 0; index < values.Length; index++) {
                    sb.Append(values[index][i].ToString(Config.CultureInfo));
                    sb.Append(comma);
                }
                normalfile.WriteLine(sb.ToString());
                timestep++;
                ts = ts.Add(_calcParameters.InternalStepsize);
            }
            normalfile.Close();
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            LoadtypeStepParameters p = (LoadtypeStepParameters)parameters;
            var efc = Repository.ReadEnergyFileColumns(Constants.GeneralHouseholdKey);
            if (!efc.ColumnCountByLoadType.ContainsKey(p.LoadType)) {
                //this household / house has no devices of this load type
                return;
            }
            Run(p.LoadType,p.EnergyFileRows,efc);
        }
    }
}
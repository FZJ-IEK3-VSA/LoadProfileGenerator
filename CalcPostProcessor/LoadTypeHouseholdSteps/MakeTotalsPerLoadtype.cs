//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
// All advertising materials mentioning features or use of this software must display the following acknowledgement:
// “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
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

#region

//using System;
using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
//using System.IO;

#endregion

namespace CalcPostProcessor.LoadTypeHouseholdSteps {
    public class MakeTotalsPerLoadtype : LoadTypeStepBase
    {
        [NotNull] private readonly IInputDataLogger _inputDataLogger;

        //[JetBrains.Annotations.NotNull]private readonly Dictionary<HouseholdKey, StreamWriter> _files;

        public MakeTotalsPerLoadtype(
                                     [NotNull] CalcDataRepository repository,
                                     [NotNull] ICalculationProfiler profiler,
                                     [NotNull] IInputDataLogger inputDataLogger)
            :base(repository, AutomationUtili.GetOptionList(CalcOption.TotalsPerLoadtype),profiler,
                "Totals per LoadType")
        {
            _inputDataLogger = inputDataLogger;
            //   _files = new Dictionary<HouseholdKey, StreamWriter>();
        }
        /*
        public double CalculateTotal([JetBrains.Annotations.NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows, [JetBrains.Annotations.NotNull] CalcLoadTypeDto dstLoadType,
            [JetBrains.Annotations.NotNull] Dictionary<CalcLoadTypeDto, double> allSums, int numberOfPersons)
        {
            throw new LPGNotImplementedException("calculate house totals");
         /*   //if (!_files.ContainsKey(Constants.TotalsKey)) {
                //WriteHeader(_fft, Constants.TotalsKey);
            //}
            double total = 0;
            var min = double.MaxValue;
            var max = double.MinValue;
            foreach (var efr in energyFileRows) {
                var sum = efr.SumCached;
                if (sum < min) {
                    min = sum;
                }
                if (sum > max) {
                    max = sum;
                }
                total += sum;
            }
            //WriteToGeneralFile(total, dstLoadType, min, max, numberOfPersons);
            if (!allSums.ContainsKey(dstLoadType)) {
                allSums.Add(dstLoadType, 0);
            }

            //var hhe = Repository.TotalInformation.HouseholdEntries.FirstOrDefault(x => x.HouseholdKey == Constants.TotalsKey);
            //if (hhe == null) {hhe = new TotalsInformation.HouseholdEntry{Name = "HouseSum"
              //  };
//                Repository.TotalInformation.HouseholdEntries.Add(hhe);
            }
            hhe.HouseholdKey = Constants.TotalsKey;
            var lte = new TotalsInformation.LoadTypeEntry(dstLoadType.ConvertToLoadTypeInformation(), total * dstLoadType.ConversionFactor);
            hhe.LoadTypeEntries.Add(lte);

            allSums[dstLoadType] += total * dstLoadType.ConversionFactor;
            return total;
        }*/

        public void RunIndividualHouseholds([NotNull] CalcLoadTypeDto dstLoadType, [NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
             [NotNull] EnergyFileColumns efc, [NotNull] Dictionary<CalcLoadTypeDto, double> allSums)
        {
            var householdKeys =
                efc.ColumnEntriesByColumn[dstLoadType].Values.Select(entry => entry.HouseholdKey).Distinct()
                    .ToList();
         //   if (householdKeys.Count > 1) {
            foreach (var hhnum in householdKeys) {
                //if (!_files.ContainsKey(hhnum)) {
//                        WriteHeader(fft, hhnum);
                //                  }
                //                var sw = _files[hhnum];
                var columns =
                    efc.ColumnEntriesByColumn[dstLoadType]
                        .Values.Where(entry => entry.HouseholdKey == hhnum)
                        .Select(entry => entry.Column).ToList();
                double totalSum = 0;
                var min = double.MaxValue;
                var max = double.MinValue;
                foreach (var efr in energyFileRows) {
                    if (!efr.Timestep.DisplayThisStep) {
                        continue;
                    }
                    var sum = efr.GetSumForCertainCols(columns);
                    if (sum < min) {
                        min = sum;
                    }

                    if (sum > max) {
                        max = sum;
                    }

                    totalSum += sum;
                }
/*
                var s = dstLoadType.Name + _calcParameters.CSVCharacter;
                s += totalSum + _calcParameters.CSVCharacter;
                s += dstLoadType.UnitOfPower + "*" +
                     _calcParameters.InternalStepsize.TotalSeconds +
                     " seconds" + _calcParameters.CSVCharacter;
                s += totalSum * dstLoadType.ConversionFactor + _calcParameters.CSVCharacter +
                     dstLoadType.UnitOfSum;
             /*
                s += _calcParameters.CSVCharacter +
                     totalSum * dstLoadType.ConversionFactor / totaldays +
                     _calcParameters.CSVCharacter + dstLoadType.UnitOfSum;
                     
                s += _calcParameters.CSVCharacter + min +
                     _calcParameters.CSVCharacter + dstLoadType.UnitOfPower;
                s += _calcParameters.CSVCharacter + max +
                     _calcParameters.CSVCharacter + dstLoadType.UnitOfPower;*/
                // sw.WriteLine(s);
                if (!allSums.ContainsKey(dstLoadType)) {
                    allSums.Add(dstLoadType, 0);
                }

                allSums[dstLoadType] += totalSum * dstLoadType.ConversionFactor;
                /*
                    var hhe = Repository.TotalInformation.HouseholdEntries.FirstOrDefault(x => x.HouseholdKey == hhnum);
                    if (hhe == null) {
                         hhe = new TotalsInformation.HouseholdEntry();
                        var entry =Repository.HouseholdKeys.FirstOrDefault(x => x.HouseholdKey == hhnum);
                        if (entry!= null) {
                            hhe.Name = entry.HouseholdName;
                            if (hhnum.Key.StartsWith("HH",StringComparison.CurrentCulture)) {
                                hhe.PureName = hhe.Name?.Substring(0, hhe.Name.Length - hhnum.Key.Length).Trim();
                            }
                        }
                        else {
                            throw new LPGException("Unknown household key");
                        }
                        hhe.HouseholdKey = hhnum;
                        //Repository.TotalInformation.HouseholdEntries.Add(hhe);
                        //if (_repository.CalcParameters.IsSet(CalcOption.TotalsPerLoadtype))
                        //{
                            //todo: check alternative sums
                          //  var sw = _fft.MakeFile<StreamWriter>(Constants.TotalsJsonName, "Totals per load type as JSON", false,
                                //ResultFileID.TotalsJson, Constants.GeneralHouseholdKey, TargetDirectory.Reports,
                                //_repository.CalcParameters.InternalStepsize);
                            //_repository.TotalInformation.WriteResultEntries(sw);
                            //sw.Close();
                        }
                }
                    //var lte = new TotalsInformation.LoadTypeEntry(dstLoadType.ConvertToLoadTypeInformation(), totalSum * dstLoadType.ConversionFactor);
                //hhe.LoadTypeEntries.Add(lte);
                }
            //}*/
            }
        }
        /*
        private void WriteHeader([JetBrains.Annotations.NotNull] FileFactoryAndTracker fft, [JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            StreamWriter sw;
            if (householdKey == Constants.TotalsKey) {
                sw = fft.MakeFile<StreamWriter>("TotalsPerLoadtype.csv", "Totals per load type", true,
                    ResultFileID.Totals, Constants.TotalsKey, TargetDirectory.Reports,
                    _calcParameters.InternalStepsize);
            }
            else {
                sw = fft.MakeFile<StreamWriter>("TotalsPerLoadtype." + householdKey + ".csv",
                    "Totals per load type for " + householdKey, true, ResultFileID.TotalsPerHousehold,
                    householdKey, TargetDirectory.Reports, _calcParameters.InternalStepsize);
            }
            _files.Add(householdKey, sw);
            sw.WriteLine("Load type" + _calcParameters.CSVCharacter + "Sum" +
                         _calcParameters.CSVCharacter + "Units" +
                         _calcParameters.CSVCharacter + "Readable" +
                         _calcParameters.CSVCharacter + "Units" +
                         _calcParameters.CSVCharacter + "Per Day" +
                         _calcParameters.CSVCharacter + "Units" +
                         _calcParameters.CSVCharacter + "Minimum Values" +
                         _calcParameters.CSVCharacter + "Minmum Value Unit" +
                         _calcParameters.CSVCharacter + "Maximum Values" +
                         _calcParameters.CSVCharacter + "Maximum Value Unit" +
                         _calcParameters.CSVCharacter + "Per Person" +
                         _calcParameters.CSVCharacter + "Unit" +
                         _calcParameters.CSVCharacter + "Per Person and Day" +
                         _calcParameters.CSVCharacter + "Unit");
        }*/
    /*
        private void WriteToGeneralFile(double sum, [JetBrains.Annotations.NotNull] CalcLoadTypeDto dstLoadType, double min, double max, int numberOfPersons)
        {
            var s = dstLoadType.Name + _calcParameters.CSVCharacter;
            s += sum + _calcParameters.CSVCharacter;
            s += dstLoadType.UnitOfPower + "*" + _calcParameters.InternalStepsize.TotalSeconds +
                 " seconds" + _calcParameters.CSVCharacter;
            s += sum * dstLoadType.ConversionFactor + _calcParameters.CSVCharacter +
                 dstLoadType.UnitOfSum;
            var totaldays =
                (_calcParameters.OfficialEndTime - _calcParameters.OfficialStartTime)
                .TotalDays;
            s += _calcParameters.CSVCharacter + sum * dstLoadType.ConversionFactor / totaldays +
                 _calcParameters.CSVCharacter + dstLoadType.UnitOfSum;
            s += _calcParameters.CSVCharacter + min + _calcParameters.CSVCharacter +
                 dstLoadType.UnitOfPower;
            s += _calcParameters.CSVCharacter + max + _calcParameters.CSVCharacter +
                 dstLoadType.UnitOfPower;
            s += _calcParameters.CSVCharacter + sum * dstLoadType.ConversionFactor / numberOfPersons +
                 _calcParameters.CSVCharacter + dstLoadType.UnitOfSum;
            s += _calcParameters.CSVCharacter +
                 sum * dstLoadType.ConversionFactor / numberOfPersons / totaldays +
                 _calcParameters.CSVCharacter + dstLoadType.UnitOfSum;
            _fft.GetResultFileEntry(ResultFileID.Totals, null, Constants.TotalsKey, null, null).StreamWriter?
                .WriteLine(s);
            _fft.GetResultFileEntry(ResultFileID.Totals, null, Constants.TotalsKey, null, null).StreamWriter?
                .Flush();
        }*/

        protected override void PerformActualStep(IStepParameters parameters)
        {
            LoadtypeStepParameters p = (LoadtypeStepParameters)parameters;
            var totaldays =
                (Repository.CalcParameters.OfficialEndTime -
                 Repository.CalcParameters.OfficialStartTime).TotalDays;
            Dictionary<CalcLoadTypeDto, double> totalsPerLoadType = new Dictionary<CalcLoadTypeDto, double>();
            //CalculateTotal(energyFileRows, loadType, totalsPerLoadType, Repository.GetPersons(householdKey).Count);
            var efc = Repository.ReadEnergyFileColumns(Constants.GeneralHouseholdKey);
            RunIndividualHouseholds(p.LoadType,p.EnergyFileRows, efc, totalsPerLoadType);
            List<TotalsPerLoadtypeEntry> totals = new List<TotalsPerLoadtypeEntry>();
            foreach (HouseholdKeyEntry entry in Repository.HouseholdKeys) {
                if(entry.KeyType == HouseholdKeyType.General) {
                    continue;
                }

                HouseholdKey key = entry.HouseholdKey;
                int personscount = 0;
                if (entry.KeyType == HouseholdKeyType.Household)
                {
                    personscount = Repository.GetPersons(key).Count;
                }
                foreach (KeyValuePair<CalcLoadTypeDto, double> pair in totalsPerLoadType) {
                    totals.Add(new TotalsPerLoadtypeEntry(key, pair.Key, pair.Value, personscount, totaldays));
                }
            }
            _inputDataLogger.SaveList(totals.ConvertAll(x=> (IHouseholdKey) x));
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {CalcOption.DetailedDatFiles};
    }
}
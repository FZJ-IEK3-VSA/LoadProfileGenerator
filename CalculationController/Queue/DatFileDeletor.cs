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
using Automation.ResultFiles;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;

//using CalcController.CalcFactories;
//using Calculation.HouseholdElements;
//using Calculation.PostProcessing;
// ReSharper disable All

//using JetBrains.Annotations;

namespace CalculationController.Queue {
    public class DatFileDeletor {
        [NotNull]
        public string CalcObjectName { get; }

        [ItemNotNull]
        [NotNull]
        //private readonly ObservableCollection<CalculationEntry> _calculationEntries;

        private readonly string _resultPath;
        private readonly bool _deleteDatFiles;
        //[NotNull]
        //private readonly CalcStartParameterSet _csps;
        //[NotNull]
        //private readonly CalculationResult _results;

        public DatFileDeletor(//[NotNull] CalculationResult results,
                                 [NotNull] CalcParameters parameters,
            //[NotNull][ItemNotNull] ObservableCollection<CalculationEntry> calculationEntries, 
                                 [NotNull] string resultPath, [NotNull] string calcObjectName) {
            CalcObjectName = calcObjectName;
            //_results = results;
            //_calculationEntries = calculationEntries;
            _resultPath = resultPath;
            _deleteDatFiles = parameters.DeleteDatFiles;
        }

        private static void DeleteDatFiles([ItemNotNull] [NotNull] List<ResultFileEntry> resultFiles) {
            foreach (var rfe in resultFiles) {
                if (rfe.FileName.ToUpperInvariant().EndsWith(".DAT", StringComparison.Ordinal)) {
                    File.Delete(rfe.FullFileName);
                }
            }
        }
/*
        private  void ProcessResultfilesForSettlement(List<CalculationResult> results, string path,ObservableCollection<ResultFileEntry> settlementResults, [CanBeNull] Dispatcher dispatcher,Simulator sim) {
            try {
                {
                    //todo: change this to read the ltdict from the database
                    var ltdict = CalcLoadTypeFactory.MakeLoadTypes(
                        sim.LoadTypes.MyItems, _internalStepSize, LoadTypePriority.All);
                    //var calcLoadTypes = new List<CalcLoadType>(ltdict.LtDict.Values);
                    throw new NotImplementedException("xxx");
  //                  var srfg = new SettlementResultFileGenerator(results,new FileFactoryAndTracker(path, "(none)"), settlementResults, calcLoadTypes.ToArray(),dispatcher);
  //                  srfg.Run();
                }
            }
            catch (Exception e) {
                MessageWindows.ShowDebugMessage(e);
                Logger.Exception(e);
                if (Config.IsInUnitTesting) {
                    throw;
                }
            }
        }
*/
        public void ProcessResults() {
            SqlResultLoggingService srls = new SqlResultLoggingService(_resultPath);

            // for a single household / house
            //if (_calculationEntries.Count == 1) // && (_calculationEntries[0].CalcObject.GetType() == typeof (Household)
            {
                if (_deleteDatFiles) {
                    ResultFileEntryLogger rfel = new ResultFileEntryLogger(srls);
                    var resultFileEntries = rfel.Load();
                    DeleteDatFiles(resultFileEntries);
                }
                //
                return;
            }
            // for a settlement
            //throw new LPGException("xxx - no settlement calculation anymore");
            //var results = new List<CalculationResult>();
            //foreach (var entry in _calculationEntries) {
            //results.Add(entry.CalculationResult);
            //}

            //var resultFileEntries = new ObservableCollection<ResultFileEntry>();
            //var everythingok = false;
            /*            if (results.Count > 0 && results[0] != null) {
                            everythingok = true;
                            throw new LPGException("xxx");
                            //ProcessResultfilesForSettlement(results, _resultPath, resultFileEntries,_csps.Dispatcher, sim);

                        }*/

            /*
            if (_deleteDatFiles) {
                foreach (var entry in _calculationEntries) {
                    DeleteDatFiles(entry);
                }
            }
            if (_calculationEntries.Count > 0) {
                if (_csps.Dispatcher != null && Thread.CurrentThread != _csps.Dispatcher.Thread) {
                    _csps.Dispatcher.BeginInvoke(DispatcherPriority.Normal, _csps.ReportFinishFuncForHouseAndSettlement,
                         everythingok, _csps.CalcTarget.Name, resultFileEntries);
                }
                else {
                    _csps.ReportFinishFuncForHouseAndSettlement( everythingok, _csps.CalcTarget.Name,
                        resultFileEntries);
                }
            }
            else {
                if (_csps.Dispatcher != null) {
                    _csps.Dispatcher.BeginInvoke(DispatcherPriority.Normal, _csps.ReportFinishFuncForHouseAndSettlement,
                        null, false, string.Empty, resultFileEntries);
                }
                else {
                    _csps.ReportFinishFuncForHouseAndSettlement(null, false, string.Empty, resultFileEntries);
                }
            }*/
        }
    }
}
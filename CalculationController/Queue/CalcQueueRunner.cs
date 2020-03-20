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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.DtoFactories;
using CalculationEngine;
using Common;
//using Common.SQLResultLogging.InputLoggers;
using Database;
using Database.Tables.Houses;
using JetBrains.Annotations;

namespace CalculationController.Queue {
    internal class CalcQueueRunner {
        [ItemNotNull]
        [NotNull]
        private static readonly List<CalcManager> _calcManagers = new List<CalcManager>();
        //[CanBeNull] private CalculationResult _results;

        public static void CloseLogfilesAfterError() {
            while (_calcManagers.Count > 0) {
                _calcManagers[0].CloseLogfile();
                _calcManagers.RemoveAt(0);
            }
        }

        //
        //Func<object, bool> openTabFunc,[CanBeNull] Dispatcher dispatcher,
            //GeographicLocation geographicLocation, bool forceRandom, TemperatureProfile temperatureProfile,
        //, , Func<bool> reportCancelFunc, string fileVersion,
            //LoadTypePriority loadTypePriority, [CanBeNull]
        //DeviceSelection deviceSelection,
          //  TransportationDeviceSet transportationDeviceSet, TravelRouteSet travelRouteSet

        [SuppressMessage("ReSharper", "RedundantAssignment")]
        private bool RunOneCalcEntry([NotNull] CalcStartParameterSet csps, [NotNull] CalculationEntry calcEntry, [NotNull] Simulator sim, bool forceRandom)
        {
            CalcManager.StartRunning();
            Logger.Info("Running the simulation for " + calcEntry.CalcObject.Name + " from " +
                        sim.MyGeneralConfig.StartDateDateTime.ToShortDateString() + " to " +
                        sim.MyGeneralConfig.EndDateDateTime.ToShortDateString());
            CalcManager calcManager = null;
            calcEntry.StartTime = DateTime.Now;
            try {
                var cmf = new CalcManagerFactory();
                calcManager = cmf.GetCalcManager(sim, calcEntry.Path,csps, calcEntry.CalcObject, forceRandom);
                    //, forceRandom,
                    //temperatureProfile,  geographicLocation, calcEntry.EnergyIntensity,
                    //fileVersion, loadTypePriority, deviceSelection, transportationDeviceSet, travelRouteSet);
                _calcManagers.Add(calcManager);
                /*CalcObjectType cot;
                if (calcEntry.CalcObject.GetType() == typeof(ModularHousehold)) {
                    cot = CalcObjectType.ModularHousehold;
                }
                else if (calcEntry.CalcObject.GetType() == typeof(House)) {
                    cot = CalcObjectType.House;
                }
                else if (calcEntry.CalcObject.GetType() == typeof(Settlement)) {
                    cot = CalcObjectType.Settlement;
                }
                else {
                    throw new LPGException("Forgotten Calc Object Type. Please report!");
                }*/
                calcManager.Run(csps.ReportCancelFunc);

                Logger.Info("Finished the simulation...");
                _calcManagers.Remove(calcManager);
            }
            catch (DataIntegrityException die) {
                calcManager?.CloseLogfile();
                if (die.Element != null) {
                    if (csps.OpenTabFunc == null)
                    {
                        throw new LPGException("OpenTabFunc was null");
                    }
                    if (csps.Dispatcher != null && !csps.Dispatcher.IsCorrectThread()) {
                        csps.Dispatcher.BeginInvoke(csps.OpenTabFunc, die.Element);
                    }
                    else {
                        csps.OpenTabFunc(die.Element);
                    }
                }
                csps.ReportCancelFunc?.Invoke();

                throw;
            }
            finally {
                if (calcManager != null) {
                    calcManager.Dispose();
                    // ReSharper disable once RedundantAssignment
#pragma warning disable S1854 // Dead stores should be removed
#pragma warning disable IDE0059 // Value assigned to symbol is never used
                    calcManager = null;
#pragma warning restore IDE0059 // Value assigned to symbol is never used
#pragma warning restore S1854 // Dead stores should be removed
#pragma warning disable S1215 // "GC.Collect" should not be called
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
#pragma warning restore S1215 // "GC.Collect" should not be called
                    if (Logger.Get().Errors.Count == 0) {
                        calcEntry.EndTime = DateTime.Now;
                        string finishedFile = Path.Combine(calcEntry.Path, Constants.FinishedFileFlag);
                        TimeSpan duration = DateTime.Now - calcEntry.StartTime;
                        using (var sw = new StreamWriter(finishedFile)) {
                            sw.WriteLine("Finished at " + DateTime.Now);
                            sw.WriteLine("Duration in seconds:");
                            sw.WriteLine(duration.TotalSeconds);
                        }
                    }
                }
            }

            return true;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        private void SaveRun(bool forceRandom,
                             [NotNull][ItemNotNull] ObservableCollection<CalculationEntry> calculationEntries,
                             [NotNull] Simulator sim,
            [NotNull] CalcStartParameterSet csps, [NotNull] string resultPath)
        {
            bool allgood = true;
            foreach (var entry in calculationEntries) {
                /*
                ResultFileEntryLogger rfel = new 
                var resultFullFilename = Path.Combine(entry.Path, Constants.ResultFileName);
                var runCalculation = true;
                if (File.Exists(resultFullFilename)) {
                    var xs = new XmlSerializer(typeof(CalculationResult));
                    CalculationResult cr;
                    using (var sr = new StreamReader(resultFullFilename)) {
                        cr = (CalculationResult) xs.Deserialize(sr);
                    }
                    if (cr == null) {
                        throw new LPGException("No Calculation Result in CalcQueueRunner. Please report.");
                    }
                    entry.CalculationResult = cr;
                    var resultFileEntries = new List<ResultFileEntry>();
                    resultFileEntries.AddRange(cr.HiddenResultFileEntries);
                    resultFileEntries.AddRange(cr.ResultFileEntries);
                    var allfilesexist = true;
                    foreach (var rfe in resultFileEntries) {
                        if (!File.Exists(rfe.FullFileName)) {
                            allfilesexist = false;
                        }
                    }
                    _results = cr;
                    if (allfilesexist) {
                        runCalculation = false;
                    }
                }*/
                //if (runCalculation) {
#pragma warning disable 162
                bool success =  RunOneCalcEntry(csps,entry,sim, forceRandom);
                if (!success) {
                    allgood = false;
                }
                //}

                if (!CalcStarter.ContinueRunning) {
                    csps.ReportCancelFunc?.Invoke();
                    return;
                }
#pragma warning restore 162
            }
            if (allgood) {
                var cpf = new CalcParametersFactory(sim);
                var calcParameters = cpf.MakeCalculationParametersFromConfig(csps, forceRandom);
                var cpp = new DatFileDeletor( calcParameters,resultPath, calculationEntries[0].CalcObject.Name);
                cpp.ProcessResults();
                SaveCallFunction(csps.ReportFinishFuncForHousehold,csps,resultPath);
            }
            else {
                throw new LPGException("No Results in CalcQueRunner! Please report this bug.");
            }
        }

        private void SaveCallFunction([CanBeNull] Func<bool, string, string, bool> reportFinishFuncForHousehold,
                                      [NotNull] CalcStartParameterSet csps,
                                      [NotNull] string resultPath)
        {
            if (reportFinishFuncForHousehold == null) {
                return;
            }

            Logger.Get()
                .SafeExecuteWithWait(
                    () => reportFinishFuncForHousehold(true,csps.CalcTarget.Name, resultPath));
        }
        [SuppressMessage("ReSharper", "ReplaceWithSingleAssignment.False")]
        [SuppressMessage("ReSharper", "ConvertIfToOrExpression")]
        public void Start([NotNull] CalcStartParameterSet csps, [NotNull][ItemNotNull] ObservableCollection<CalculationEntry> calculationEntries,
            [NotNull] Simulator sim, [NotNull] string resultPath) {
            csps.SetCalculationEntries?.Invoke(calculationEntries);
            try {
                var forceRandom = false;
                if (csps.CalcTarget.GetType() == typeof(Settlement)) {
                    forceRandom = true;
                }
                SaveRun(forceRandom, calculationEntries, sim,csps, resultPath);
            }
            catch (DataIntegrityException e) {
                Logger.Error("DataIntegrityException:"+ Environment.NewLine + e.Message);
                CloseLogfilesAfterError();
                if (!Config.IsInUnitTesting && !Config.IsInHeadless) {
                    MessageWindowHandler.Mw.ShowDataIntegrityMessage(e);
                    csps.ReportCancelFunc?.Invoke();
                }
                else {
                    throw;
                }
            }
        }
    }
}
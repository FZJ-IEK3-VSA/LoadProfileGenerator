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
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using CalculationController.Queue;
using ChartCreator2.OxyCharts;
using Common;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using Database;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.Transportation;
using Database.Tables.Validation;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.SpecialViews;

namespace LoadProfileGenerator.Presenters.SpecialViews {
    [SuppressMessage("ReSharper", "CatchAllClause")]
    public class CalculationOutcomesPresenter : PresenterBaseWithAppPresenter<CalculationOutcomesView> {
        //private static readonly List<CalculationResult> _result = new List<CalculationResult>();

        [NotNull] private readonly CategoryOutcome _outcomes;
        [CanBeNull] private string _csvPath;

        [CanBeNull] private string _filterString;

        private bool _inCalculation;

        [CanBeNull] private CalculationOutcome _selectedOutcome;

        private bool _showOnlyErrors;

        public CalculationOutcomesPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] CalculationOutcomesView view) : base(view,
            "HeaderString",
            applicationPresenter)
        {
            _outcomes = Sim.CalculationOutcomes;
            OperatingPath = @"e:\TemporaryCalculations";
            RefreshFiltered();
        }

        [CanBeNull]
        [UsedImplicitly]
        public string CSVPath {
            get => _csvPath;
            set {
                _csvPath = value;
                OnPropertyChanged(nameof(CSVPath));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<CalculationOutcome> Entries => _outcomes.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<CalculationOutcome> FilteredEntries { get; } = new ObservableCollection<CalculationOutcome>();

        [CanBeNull]
        [UsedImplicitly]
        public string FilterString {
            get => _filterString;
            set {
                _filterString = value;
                OnPropertyChanged(nameof(FilterString));
                RefreshFiltered();
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string HeaderString => "Calculation Outcomes";

        [UsedImplicitly]
        public bool InCalculation {
            get => _inCalculation;
            set {
                _inCalculation = value;
                OnPropertyChanged(nameof(InCalculation));
                OnPropertyChanged(nameof(NotInCalculation));
            }
        }

        [UsedImplicitly]
        public bool NotInCalculation => !_inCalculation;

        [NotNull]
        [UsedImplicitly]
        public string OperatingPath { get; set; }

        [CanBeNull]
        [UsedImplicitly]
        public CalculationOutcome SelectedOutcome {
            get => _selectedOutcome;
            set {
                if (Equals(value, _selectedOutcome)) {
                    return;
                }

                _selectedOutcome?.AffordanceTimeUses.Sort(AffTimeUseComparer);
                _selectedOutcome = value;
                OnPropertyChanged(nameof(SelectedOutcome));
            }
        }

        [UsedImplicitly]
        public bool ShowOnlyErrors {
            get => _showOnlyErrors;
            set {
                _showOnlyErrors = value;
                OnPropertyChanged(nameof(ShowOnlyErrors));
                RefreshFiltered();
            }
        }

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            InCalculation = false;
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public static int CountMissingEntries([NotNull] Simulator sim)
        {
            var count = 0;
            var geographicLocation = sim.GeographicLocations.FindFirstByName("berlin", FindMode.Partial);
            if (geographicLocation == null) {
                Logger.Error("Location Berlin not found. Can't count missing entries.");
                return 0;
            }

            var temperatureProfile = sim.TemperatureProfiles[0];

            var co = new List<ICalcObject>();
            for (var i = 0; i < sim.ModularHouseholds.It.Count; i++) {
                if (sim.ModularHouseholds[i].GeneratorID == null) {
                    co.Add(sim.ModularHouseholds[i]);
                }
            }

            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var intensities = new List<EnergyIntensityType> {
                EnergyIntensityType.EnergySavingPreferMeasured,
                EnergyIntensityType.EnergyIntensivePreferMeasured,
                EnergyIntensityType.EnergyIntensive,
                EnergyIntensityType.EnergySaving
            };
            foreach (var intensity in intensities) {
                foreach (var calcObject in co) {
                    if (!sim.CalculationOutcomes.ItemExists(calcObject, geographicLocation, temperatureProfile, intensity, version)) {
                        count++;
                    }
                }
            }

            return count;
        }

        public void DeleteEmptyOutcomes()
        {
            var toDelete = Entries.Where(x => x.Entries.Count == 0).ToList();
            foreach (var calculationOutcome in toDelete) {
                Sim.CalculationOutcomes.DeleteItem(calculationOutcome);
            }

            RefreshFiltered();
        }

        public void DeleteOutcome([NotNull] CalculationOutcome co)
        {
            Sim.CalculationOutcomes.DeleteItem(co);
            RefreshFiltered();
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as CalculationOutcomesPresenter;
            return presenter?.HeaderString.Equals(HeaderString) == true;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void ExportToCSV()
        {
            try {
                if (CSVPath == null) {
                    Logger.Error("CSV Path was null");
                    return;
                }

                Logger.Info("Starting export...");
                using (var sw = new StreamWriter(CSVPath)) {
                    var h = string.Empty;
                    h += "HouseholdName;"; // 0
                    h += "EnergyIntensity;"; // 1
                    h += "CalculationDuration;"; // 2
                    h += "ErrorMessage;"; // 3
                    h += "GeographicLocationName;"; // 4
                    h += "LPGVersion;"; // 5
                    h += "NumberOfPersons;"; // 6
                    h += "RandomSeed;"; // 7
                    h += "SimulationStartTime;"; // 8
                    h += "SimulationEndTime;"; // 9
                    h += "TemperatureProfile;"; // 10
                    var columns = new Dictionary<string, int>();
                    var column = 0;
                    foreach (var outcome in _outcomes.It) {
                        foreach (var entry in outcome.Entries) {
                            if (entry.LoadTypeName != null) {
                                if (!columns.ContainsKey(entry.LoadTypeName)) {
                                    columns.Add(entry.LoadTypeName, column);
                                    h += entry.LoadTypeName + ";";
                                    column++;
                                }
                            }
                        }
                    }

                    foreach (var keyValuePair in columns) {
                        h += keyValuePair.Key + "/Person;";
                    }

                    foreach (var keyValuePair in columns) {
                        h += keyValuePair.Key + "/Person/Day;";
                    }

                    sw.WriteLine(h);
                    var arr = new string[11 + column * 3];
                    foreach (var outcome in _outcomes.It) {
                        var col = 0;
                        arr[col++] = outcome.HouseholdName;
                        arr[col++] = outcome.EnergyIntensity;
                        arr[col++] = outcome.CalculationDuration.ToString(@"hh\:mm\:ss", CultureInfo.CurrentCulture);
                        arr[col++] = outcome.ErrorMessage;
                        arr[col++] = outcome.GeographicLocationName;
                        arr[col++] = outcome.LPGVersion;
                        arr[col++] = outcome.NumberOfPersons.ToString(CultureInfo.CurrentCulture);
                        arr[col++] = outcome.RandomSeed.ToString(CultureInfo.CurrentCulture);
                        arr[col++] = outcome.SimulationStartTime.ToShortDateString();
                        arr[col++] = outcome.SimulationEndTime.ToShortDateString();
                        arr[col] = outcome.TemperatureProfile;
                        foreach (var entry in outcome.Entries) {
                            if (entry.LoadTypeName != null) {
                                col = columns[entry.LoadTypeName] + 11;
                                arr[col] = entry.Value.ToString(CultureInfo.CurrentCulture);
                                col = columns[entry.LoadTypeName] + columns.Count + 11;
                                arr[col] = (entry.Value / outcome.NumberOfPersons).ToString(CultureInfo.CurrentCulture);
                                col = columns[entry.LoadTypeName] + columns.Count * 2 + 11;
                                arr[col] = (entry.Value / outcome.NumberOfPersons / 365).ToString(CultureInfo.CurrentCulture);
                            }
                        }

                        var s = string.Empty;
                        foreach (var s1 in arr) {
                            s += s1 + ";";
                        }

                        for (var i = 0; i < arr.Length; i++) {
                            arr[i] = string.Empty;
                        }

                        s = s.Replace(Environment.NewLine, " ");
                        sw.WriteLine(s);
                    }
                }

                Logger.Info("Finished export.");
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        public void FindDuplicates()
        {
            var outcomes = new List<CalculationOutcome>();
            foreach (var outcome in _outcomes.It) {
                var count = Sim.CalculationOutcomes.CountItems(outcome.HouseholdName,
                    outcome.GeographicLocationName,
                    outcome.TemperatureProfile,
                    outcome.EnergyIntensity,
                    outcome.LPGVersion);
                outcomes.Add(outcome);
                if (count != 1) {
                    Logger.Warning("Found duplicate item:" + outcome.HouseholdName + ", " + outcome.EnergyIntensity);
                }
            }

            foreach (var outcome in outcomes) {
                var count = Sim.CalculationOutcomes.CountItems(outcome.HouseholdName,
                    outcome.GeographicLocationName,
                    outcome.TemperatureProfile,
                    outcome.EnergyIntensity,
                    outcome.LPGVersion);
                if (count > 1) {
                    _outcomes.DeleteItem(outcome);
                }
            }

            Logger.Info("Finished duplicate item check.");
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                const int hash = 17;
                // Suitable nullity checks etc, of course :)
                return hash * 23 + TabHeaderPath.GetHashCode();
            }
        }

        public static void MakeVersionComparisonChart([NotNull] Simulator sim)
        {
            var dstPath = Path.Combine(sim.MyGeneralConfig.DestinationPath, "CalculationOutcomes");
            if (!Directory.Exists(dstPath)) {
                Directory.CreateDirectory(dstPath);
            }

            CalculationProfiler profiler = new CalculationProfiler();
            //make Version Comparison Charts
            var versions = sim.CalculationOutcomes.It.Select(x => x.LPGVersion).Distinct().ToList();
            var energyIntensities = sim.CalculationOutcomes.It.Select(x => x.EnergyIntensity).Distinct().ToList();
            foreach (var energyIntensity in energyIntensities) {
                var seriesEntries = new List<MakeNRWChart.SeriesEntry>();
                var offset = 0.1;
                foreach (var entry in versions) {
                    var se1 = new MakeNRWChart.SeriesEntry(offset, energyIntensity, energyIntensity + " - " + entry, entry);

                    offset += 0.1;

                    seriesEntries.Add(se1);
                }

                var pngname = Path.Combine(dstPath, "NRWScatter." + energyIntensity + ".png");
                var mnc = new MakeNRWChart(150, 1000, 1600, profiler);
                mnc.MakeScatterChart(sim.CalculationOutcomes.It.ToList(), pngname, seriesEntries);
                Logger.Info("Finished creating the file " + pngname);
            }

            foreach (var version in versions) {
                var seriesEntries = new List<MakeNRWChart.SeriesEntry>();
                var offset = 0.1;
                foreach (var energyIntensity in energyIntensities) {
                    var se1 = new MakeNRWChart.SeriesEntry(offset, energyIntensity, energyIntensity + " - " + version, version);

                    offset += 0.1;

                    seriesEntries.Add(se1);
                }

                var pngname = Path.Combine(dstPath, "NRWScatter." + version + ".png");
                var mnc = new MakeNRWChart(150, 1000, 1600, profiler);
                mnc.MakeScatterChart(sim.CalculationOutcomes.It.ToList(), pngname, seriesEntries);
                Logger.Info("Finished creating the file " + pngname);
            }
        }

        [NotNull]
        public static Dictionary<string, Dictionary<string, OutcomeStatistic>> ReadResult([NotNull] SingleTimestepActionEntryLogger atael,
                                                                                          [NotNull] HouseholdKey key,
                                                                                          [NotNull] ActionEntryLogger ael)
        {
            var singleTimeActionEntries = atael.Read(key);
            var actionEntries = ael.Read(key);
            //1. person 2. affname
            Dictionary<string, Dictionary<string, OutcomeStatistic>> resultDict = new Dictionary<string, Dictionary<string, OutcomeStatistic>>();
            Dictionary<StrGuid, ActionEntry> entriesByGuid = new Dictionary<StrGuid, ActionEntry>();
            foreach (ActionEntry entry in actionEntries) {
                entriesByGuid.Add(entry.ActionEntryGuid, entry);
            }

            var persons = actionEntries.Select(x => x.PersonName).Distinct().ToList();

            var prevActivities = new Dictionary<string, string>(); //for counting the activations for each activity
            foreach (var p in persons) {
                resultDict.Add(p, new Dictionary<string, OutcomeStatistic>());
                prevActivities.Add(p, string.Empty);
            }

            foreach (var singleAction in singleTimeActionEntries) {
                var action = entriesByGuid[singleAction.ActionEntryGuid];
                var dict = resultDict[action.PersonName];
                var affName = action.AffordanceName;
                if (!dict.ContainsKey(affName)) {
                    dict.Add(affName, new OutcomeStatistic());
                }

                dict[affName].Minutes++;

                if (prevActivities[action.PersonName] != affName) {
                    prevActivities[action.PersonName] = affName;
                    resultDict[action.PersonName][affName].Executions += 1;
                }
            }

            return resultDict;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void StartCalculations(bool countOnly)
        {
            try {
                InCalculation = true;
                var geographicLocation = Sim.GeographicLocations.FindFirstByName("berlin", FindMode.Partial);
                if (geographicLocation == null) {
                    Logger.Error("Could not find Berlin. Can't calculate.");
                    return;
                }

                var temperatureProfile = Sim.TemperatureProfiles[0];

                var co = new List<ICalcObject>();
                for (var i = 0; i < Sim.ModularHouseholds.It.Count; i++) {
                    if (Sim.ModularHouseholds[i].GeneratorID == null) {
                        co.Add(Sim.ModularHouseholds[i]);
                    }
                }

                var intensities = new List<EnergyIntensityType> {
                    EnergyIntensityType.EnergySavingPreferMeasured,
                    EnergyIntensityType.EnergyIntensivePreferMeasured,
                    EnergyIntensityType.EnergyIntensive,
                    EnergyIntensityType.EnergySaving
                };
                var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                var versionArray = version.Split('.');
                version = versionArray[0] + "." + versionArray[1] + "." + versionArray[2];

                var t = new Thread(() => {
                    try {
                        var count = 0;
                        foreach (var intensity in intensities) {
                            Logger.Info("Processing all households for " + intensity);
                            foreach (var calcObject in co) {
                                if (!InCalculation) {
                                    return;
                                }

                                //_result.Clear();
                                if (!Sim.CalculationOutcomes.ItemExists(calcObject, geographicLocation, temperatureProfile, intensity, version)) {
                                    if (!countOnly) {
                                        StartOneCalculation(calcObject,
                                            geographicLocation,
                                            temperatureProfile,
                                            intensity,
                                            OperatingPath,
                                            Sim,
                                            true,
                                            null,
                                            null,
                                            null);
                                        Logger.Get().SafeExecuteWithWait(Sim.CalculationOutcomes.It.Sort);
                                        Logger.Get().SafeExecuteWithWait(RefreshFiltered);
                                    }
                                    else {
                                        count++;
                                    }
                                }
                            }
                        }

                        InCalculation = false;
                        Logger.Error("Found " + count + " missing entries!");
                        MessageWindowHandler.Mw.ShowInfoMessage("All done!", "Success");
                    }
                    catch (Exception ex) {
                        MessageWindowHandler.Mw.ShowDebugMessage(ex);
                        InCalculation = false;
                        Logger.Exception(ex);
                    }
                });
                t.Start();
            }
            catch (Exception ex) {
                MessageWindowHandler.Mw.ShowDebugMessage(ex);
                InCalculation = false;
                Logger.Exception(ex);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void StartOneCalculation([NotNull] ICalcObject mycalcObject,
                                               [NotNull] GeographicLocation geographicLocation,
                                               [NotNull] TemperatureProfile temperatureProfile,
                                               EnergyIntensityType intensity,
                                               [NotNull] string operatingPath,
                                               [NotNull] Simulator sim,
                                               bool deleteEverything,
                                               [CanBeNull] TransportationDeviceSet transportationDeviceSet,
                                               [CanBeNull] TravelRouteSet travelRouteSet,
                                               [CanBeNull] ChargingStationSet chargingStationSet)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (sim.CalculationOutcomes.ItemExists(mycalcObject, geographicLocation, temperatureProfile, intensity, version)) {
                return;
            }

            if (operatingPath.Length < 4) {
                MessageWindowHandler.Mw.ShowInfoMessage("Please use a longer path. Don't use the root of a drive.", "Fail!");
                return;
            }

            if (Directory.Exists(operatingPath)) {
                Directory.Delete(operatingPath, true);
            }

            Directory.CreateDirectory(operatingPath);
            var start = DateTime.Now;
            Config.IsInUnitTesting = true;
            Logger.Threshold = Severity.Information;

            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.NoFiles);
            var startDate = new DateTime(2019, 1, 1);
            sim.MyGeneralConfig.StartDateUIString = startDate.ToShortDateString();
            var endDate = new DateTime(2019, 12, 31);
            sim.MyGeneralConfig.EndDateUIString = endDate.ToShortDateString();
            sim.MyGeneralConfig.ExternalTimeResolution = sim.MyGeneralConfig.InternalTimeResolution;
            sim.MyGeneralConfig.RandomSeed = -1; // always randomseed
            sim.MyGeneralConfig.ShowSettlingPeriod = "False";
            sim.MyGeneralConfig.Enable(CalcOption.TotalsPerLoadtype);
            sim.MyGeneralConfig.Enable(CalcOption.ActionsLogfile);
            sim.MyGeneralConfig.Enable(CalcOption.ActionsEachTimestep);
            //sim.MyGeneralConfig.Enable(CalcOption.ac);
            var cs = new CalcStarter(sim);
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            var numberOfPersons = mycalcObject.CalculatePersonCount();
            //Dictionary<string, Dictionary<string, OutcomeStatistic>> timeUseDict = null;
            try {
                var csps = new CalcStartParameterSet(ReportFinishFuncForHouseAndSettlement,
                    ReportFinishFuncForHousehold,
                    OpenTabFunc,
                    null,
                    geographicLocation,
                    temperatureProfile,
                    mycalcObject,
                    intensity,
                    ReportCancelFunc,
                    false,
                    null,
                    LoadTypePriority.All,
                    transportationDeviceSet,
                    travelRouteSet,
                    sim.MyGeneralConfig.AllEnabledOptions(),
                    startDate,
                    endDate,
                    new TimeSpan(0, 1, 0),
                    ";",
                    -1,
                    sim.MyGeneralConfig.ExternalStepSize,
                    sim.MyGeneralConfig.DeleteDatFilesBool,
                    sim.MyGeneralConfig.WriteExcelColumnBool,
                    sim.MyGeneralConfig.ShowSettlingPeriodBool,
                    3,
                    sim.MyGeneralConfig.RepetitionCount,
                    calculationProfiler,
                    chargingStationSet,
                    null,
                    sim.MyGeneralConfig.DeviceProfileHeaderMode,
                    false, operatingPath);

                cs.Start(csps);
                ChartMaker.MakeChartsAndPDF(calculationProfiler,operatingPath);
                Thread.Sleep(3000);
                /*if (string.IsNullOrWhiteSpace(errormessage) && _result.Count == 0) {
                    throw new LPGException("No results, but no error message. Something is wrong. Please report.");
                }*/

                //timeUseDict = ReadResultFiles(operatingPath);
                var duration = DateTime.Now - start;
                var errormessage = string.Empty;
                ReadResultDataAndSaveToDb(mycalcObject,
                    geographicLocation,
                    temperatureProfile,
                    intensity,
                    operatingPath,
                    sim,
                    version,
                    errormessage,
                    duration,
                    startDate,
                    endDate,
                    numberOfPersons);
                if (Directory.Exists(operatingPath) && deleteEverything) {
                    bool success = false;
                    int trycount = 0;
                    while (!success && trycount < 100) {
                        try {
                            GC.WaitForPendingFinalizers();
                            GC.Collect();
                            Directory.Delete(operatingPath, true);
                            success = true;
                        }
                        catch (Exception ex) {
                            Logger.Error(ex.Message);
                            trycount++;
                            Logger.Info("Files still open, try " + trycount + ", waiting 500 ms: " + ex.Message);
                            Thread.Sleep(500);
                        }
                    }
                }

                Logger.ImportantInfo("Calculation duration:" + duration);
            }
            catch (Exception ex) {
                // errormessage = ex.Message;
                Logger.Exception(ex);
            }

            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.Reasonable);
        }

        private static int AffTimeUseComparer([NotNull] AffordanceTimeUseOutcome x, [NotNull] AffordanceTimeUseOutcome y)
        {
            var result = string.Compare(x.PersonName, y.PersonName, StringComparison.Ordinal);
            if (result != 0) {
                return result;
            }

            return string.Compare(x.AffordanceName, y.AffordanceName, StringComparison.Ordinal);
        }

        private static bool OpenTabFunc([NotNull] object o) => true;

        private static void ReadResultDataAndSaveToDb([NotNull] ICalcObject mycalcObject,
                                                      [NotNull] GeographicLocation geographicLocation,
                                                      [NotNull] TemperatureProfile temperatureProfile,
                                                      EnergyIntensityType intensity,
                                                      [NotNull] string operatingPath,
                                                      [NotNull] Simulator sim,
                                                      [NotNull] string version,
                                                      [NotNull] string errormessage,
                                                      TimeSpan duration,
                                                      DateTime startDate,
                                                      DateTime endDate,
                                                      int numberOfPersons)
        {
            var mainversion = version.Substring(0, version.LastIndexOf(".", StringComparison.Ordinal));
            var calculationOutcome = new CalculationOutcome("Calculation for " + mycalcObject.Name,
                mycalcObject.Name,
                mainversion,
                temperatureProfile.Name,
                geographicLocation.Name,
                errormessage,
                sim.MyGeneralConfig.InternalTimeResolution,
                intensity.ToString(),
                duration,
                startDate,
                endDate,
                sim.ConnectionString,
                numberOfPersons,
                0,
                null,
                Guid.NewGuid().ToStrGuid());
            calculationOutcome.SaveToDB();

            SqlResultLoggingService srls = new SqlResultLoggingService(operatingPath);
            //ResultFileEntryLogger rfel = new ResultFileEntryLogger(srls);
            var keyLogger = new HouseholdKeyLogger(srls);
            var keys = keyLogger.Load();
            TotalsPerLoadtypeEntryLogger tel = new TotalsPerLoadtypeEntryLogger(srls);
            SingleTimestepActionEntryLogger stael = new SingleTimestepActionEntryLogger(srls);
            ActionEntryLogger ael = new ActionEntryLogger(srls);
            foreach (var householdKeyEntry in keys) {
                if (householdKeyEntry.KeyType == HouseholdKeyType.Household) {
                    var totalsEntries = tel.Read(householdKeyEntry.HouseholdKey);
                    if (totalsEntries.Count == 0) {
                        throw new LPGException();
                    }

                    foreach (TotalsPerLoadtypeEntry entry in totalsEntries) {
                        calculationOutcome.AddLoadType(entry.Loadtype.Name, entry.Value);
                        Logger.Info("Importing outcome for " + entry.Loadtype.Name + ": " + entry.Value);
                        if (double.IsNaN(entry.Value)) {
                            throw new LPGException("Invalid result: " + mycalcObject.Name + " " + entry.Value + " is NaN.");
                        }
                    }

                    var timeUseDict = ReadResult(stael, householdKeyEntry.HouseholdKey, ael);
                    foreach (var personDict in timeUseDict) {
                        foreach (var pair in personDict.Value) {
                            calculationOutcome.AddAffordanceTimeUse(pair.Key, personDict.Key, pair.Value.Minutes, pair.Value.Executions);
                        }
                    }
                }
            }

            calculationOutcome.SaveToDB();
            Logger.Get().SafeExecuteWithWait(() => sim.CalculationOutcomes.It.Add(calculationOutcome));
        }

        private void RefreshFiltered()
        {
            var newlist = new List<CalculationOutcome>();
            if (_showOnlyErrors) {
                newlist.AddRange(Entries.Where(x => x.ErrorMessage.Length > 0));
            }
            else {
                newlist.AddRange(Entries);
            }

            if (!string.IsNullOrEmpty(_filterString)) {
                newlist = newlist.Where(x =>
                    x.HouseholdName.ToUpperInvariant().Contains(_filterString.ToUpperInvariant()) || x.LPGVersion.Contains(_filterString)).ToList();
            }

            foreach (var calculationOutcome in newlist) {
                if (!FilteredEntries.Contains(calculationOutcome)) {
                    FilteredEntries.Add(calculationOutcome);
                }
            }

            var todelete = new List<CalculationOutcome>();
            foreach (var filteredItem in FilteredEntries) {
                if (!newlist.Contains(filteredItem)) {
                    todelete.Add(filteredItem);
                }
            }

            foreach (var calculationOutcome in todelete) {
                FilteredEntries.Remove(calculationOutcome);
            }

            FilteredEntries.Sort();
        }

        private static bool ReportCancelFunc() => true;

        private static bool ReportFinishFuncForHouseAndSettlement(bool a2,
                                                                  [NotNull] string a3,
                                                                  [ItemNotNull] [NotNull] ObservableCollection<ResultFileEntry> a4) =>
            /*foreach (var calculationResult in a1) {
                _result.Add(calculationResult);
            }*/
            true;

        //TODO: read the results
        //_result.Add(a1);
        private static bool ReportFinishFuncForHousehold(bool a2, [NotNull] string a3, [NotNull] string resultPath) => true;


        public class OutcomeStatistic {
            public int Executions { get; set; }
            public double Minutes { get; set; }
        }
    }
}
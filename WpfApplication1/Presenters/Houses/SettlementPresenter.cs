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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using Automation;
using Common;
using Common.Enums;
using Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Houses;

namespace LoadProfileGenerator.Presenters.Houses {
    public class LoadTypeSelection {
        [NotNull] private readonly Action _refreshSettlement;
        private bool _enabled;


        public LoadTypeSelection([NotNull] Action refreshSettlement, [NotNull] VLoadType loadType, bool enabled)
        {
            _refreshSettlement = refreshSettlement;
            LoadType = loadType;
            _enabled = enabled;
        }

        public bool Export {
            get => _enabled;

            set {
                if (_enabled == value) {
                    return;
                }

                _enabled = value;
                _refreshSettlement();
            }
        }

        [NotNull]
        public VLoadType LoadType { get; }

        [NotNull]
        public string Name => LoadType.PrettyName;
    }

    public class SettlementPresenter : PresenterBaseDBBase<SettlementView> {
        [NotNull] private readonly EnergyIntensityConverter _eic = new EnergyIntensityConverter();
        [NotNull] private readonly Settlement _settlement;
        private CalcObjectType _selectedCalcObjectType;

        public SettlementPresenter([NotNull] ApplicationPresenter applicationPresenter,
                                   [NotNull] SettlementView view,
                                   [NotNull] Settlement settlement) : base(view, "ThisSettlement.HeaderString", settlement, applicationPresenter)
        {
            _settlement = settlement;
            UpdateInternalTimesteps();
            UpdateExternalTimeResolution(_settlement.InternalTimeResolution);
            CalcObjectTypes = new ObservableCollection<CalcObjectType> {
                CalcObjectType.ModularHousehold,
                CalcObjectType.House
            };
            RefreshAgeEntries();
            RefreshLivingPatternEntries();
            RefreshCalcOptions();
            RefreshLoadTypeList();
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Settlement.AgeEntry> AgeEntries { get; } = new ObservableCollection<Settlement.AgeEntry>();

        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<CalcObjectType> CalcObjectTypes { get; }

        [NotNull]
        [UsedImplicitly]
        public Dictionary<CreationType, string> CreationTypes => CreationTypeHelper.CreationTypeDictionary;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> DefaultTimeSteps { get; } = new ObservableCollection<string>();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<EnergyIntensityConverter.EnergyIntensityForDisplay> EnergyIntensities => _eic.All;

        [NotNull]
        [UsedImplicitly]
        public EnergyIntensityConverter.EnergyIntensityForDisplay EnergyIntensity {
            get => _eic.GetAllDisplayElement(ThisSettlement.EnergyIntensityType);
            set {
                ThisSettlement.EnergyIntensityType = value.EnergyIntensityType;
                OnPropertyChanged(nameof(EnergyIntensity));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public string ExternalTimeResolution {
            get => _settlement.ExternalTimeResolution;

            set => _settlement.ExternalTimeResolution = value;
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> ExternalTimeSteps { get; } = new ObservableCollection<string>();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<GeographicLocation> GeographicLocations => Sim.GeographicLocations.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<House> Houses => Sim.Houses.MyItems;

        [CanBeNull]
        [UsedImplicitly]
        public string InternalTimeResolution {
            get => _settlement.InternalTimeResolution;

            set {
                _settlement.InternalTimeResolution = value;
                UpdateExternalTimeResolution(_settlement.InternalTimeResolution);
            }
        }

        [UsedImplicitly]
        public Visibility IsHouseVisible {
            get {
                if (_selectedCalcObjectType == CalcObjectType.House) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        public Visibility IsModularHouseholdVisible {
            get {
                if (_selectedCalcObjectType == CalcObjectType.ModularHousehold) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TagEntry> LivingPatternEntries { get; } = new ObservableCollection<TagEntry>();


        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<LoadTypeSelection> LoadTypeList { get; } = new ObservableCollection<LoadTypeSelection>();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ModularHousehold> ModularHouseholds => Sim.ModularHouseholds.MyItems;


        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> NotSelectedOptions { get; } = new ObservableCollection<string>();

        [NotNull]
        [UsedImplicitly]
        public Dictionary<OutputFileDefault, string> OutputFileDefaults => OutputFileDefaultHelper.OutputFileDefaultDictionary;

        [UsedImplicitly]
        public int PersonCount { get; set; }

        [UsedImplicitly]
        public CalcObjectType SelectedCalcObjectType {
            get => _selectedCalcObjectType;
            set {
                _selectedCalcObjectType = value;
                OnPropertyChanged(nameof(SelectedCalcObjectType));
                OnPropertyChanged(nameof(IsModularHouseholdVisible));
                OnPropertyChanged(nameof(IsHouseVisible));
            }
        }

        [UsedImplicitly]
        public OutputFileDefault SelectedOptionDefault { get; set; }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> SelectedOptions { get; } = new ObservableCollection<string>();

        [NotNull]
        [UsedImplicitly]
        public string Statistics {
            get {
                var houses = _settlement.Households.Count(x => x.CalcObjectType == CalcObjectType.House);
                var modularHouseholdsCount = _settlement.Households.Count(x => x.CalcObjectType == CalcObjectType.ModularHousehold);
                var persons = 0;
                foreach (var hh in _settlement.Households) {
                    if (hh.CalcObjectType == CalcObjectType.ModularHousehold) {
                        var ch = (ModularHousehold)hh.CalcObject;
                        if (ch != null) {
                            persons += ch.Persons.Count;
                        }
                    }

                    if (hh.CalcObjectType == CalcObjectType.House) {
                        var ch = (House)hh.CalcObject;
                        if (ch != null) {
                            persons += ch.CalculatePersonCount();
                        }
                    }
                }

                var s = "Houses: " + houses + ", Modular Households: " + modularHouseholdsCount + ", Persons:" + persons;
                return s;
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TagEntry> TagEntries { get; } = new ObservableCollection<TagEntry>();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TemperatureProfile> TemperatureProfiles => Sim.TemperatureProfiles.MyItems;

        [NotNull]
        [UsedImplicitly]
        public Settlement ThisSettlement => _settlement;

        [UsedImplicitly]
        public int TotalHouseholds {
            get {
                var count = 0;
                foreach (var hh in _settlement.Households) {
                    count += hh.Count;
                }

                return count;
            }
        }

        [NotNull]
        [UsedImplicitly]
        public SettlementHH AddCalcObject([NotNull] ICalcObject calcObject, int count)
        {
            var shh = _settlement.AddHousehold(calcObject, count);
            _settlement.SaveToDB();
            _settlement.Households.Sort();
            OnPropertyChanged(nameof(TotalHouseholds));
            OnPropertyChanged(nameof(Statistics));
            return shh;
        }

        public void AddOption(CalcOption calcOption)
        {
            ThisSettlement.AddCalcOption(calcOption);
            RefreshCalcOptions();
        }


        public void ApplyOptionDefault(OutputFileDefault selectedOptionDefault)
        {
            var selected = OutputFileDefaultHelper.GetOptionsForDefault(selectedOptionDefault);
            foreach (var calcOption in selected) {
                if (ThisSettlement.CalcOptions == null || !ThisSettlement.CalcOptions.Contains(calcOption)) {
                    ThisSettlement.AddCalcOption(calcOption);
                }
            }

            if (ThisSettlement.CalcOptions != null) {
                foreach (var option in ThisSettlement.CalcOptions.ToList()) {
                    if (!selected.Contains(option)) {
                        ThisSettlement.RemoveOption(option);
                    }
                }
            }

            ThisSettlement.OutputFileDefault = OutputFileDefault.NoFiles;
            RefreshCalcOptions();
        }

        public void Delete()
        {
            Sim.Settlements.DeleteItem(_settlement);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as SettlementPresenter;
            return presenter?.ThisSettlement.Equals(_settlement) == true;
        }

        public void ExportCalculationJson([NotNull] string dstDirectory)
        {
            //copy entire directory and db3 file
            if (!Directory.Exists(dstDirectory)) {
                Directory.CreateDirectory(dstDirectory);
                Thread.Sleep(500);
            }

            string lpgPath = Assembly.GetExecutingAssembly().Location;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (lpgPath != null) {
                Logger.Info("Current LPG Path: " + lpgPath);
                FileInfo fileInfo = new FileInfo(lpgPath);
                if (fileInfo.Directory != null) {
                    string simExePath = Path.Combine(fileInfo.Directory.FullName, "simulationengine.exe");
                    if (File.Exists(simExePath)) {
                        DirectoryInfo di = fileInfo.Directory;
                        var files = di.GetFiles("*.*", SearchOption.AllDirectories);
                        foreach (var srcFile in files) {
                            string dstPath = Path.Combine(dstDirectory, srcFile.Name);
                            srcFile.CopyTo(dstPath, true);
                            Logger.Info("Copying " + srcFile.Name + " to " + dstPath);
                        }
                    }
                    else {
                        Logger.Error("Could not determine the correct path for the simulationengine.exe. It was not at " + simExePath + ".");
                    }
                }
            }

            const string simulationEngine = "simulationengine.exe";
            ThisSettlement.WriteJsonCalculationSpecs(dstDirectory, simulationEngine);
            Logger.Info("Finished writing the json calculation specifications to " + dstDirectory);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + TabHeaderPath.GetHashCode();
                return hash;
            }
        }

        public void MakeCopy()
        {
            var newSettlement = Sim.Settlements.CreateNewItem(Sim.ConnectionString);
            newSettlement.ImportFromExisting(_settlement);
            ApplicationPresenter.OpenItem(newSettlement);
        }

        public void RefreshAgeEntries()
        {
            AgeEntries.Clear();
            var aes = _settlement.CalculateAgeEntries();
            PersonCount = _settlement.AllPersons.Count;
            AgeEntries.SynchronizeWithList(aes);
        }

        public void RefreshCalcOptions()
        {
            List<string> selectedOptions = new List<string>();
            List<string> notselectedOptions = new List<string>();
            foreach (var cod in CalcOptionHelper.CalcOptionDictionary) {
                if (ThisSettlement.CalcOptions == null || !ThisSettlement.CalcOptions.Contains(cod.Key)) {
                    notselectedOptions.Add(cod.Value);
                }
                else {
                    selectedOptions.Add(cod.Value);
                }
            }

            NotSelectedOptions.SynchronizeWithList(notselectedOptions);
            SelectedOptions.SynchronizeWithList(selectedOptions);
        }

        public void RefreshLivingPatternEntries()
        {
            LivingPatternEntries.Clear();
            var counts = _settlement.CalculateLivingPatternCounts();

            var entries = new List<TagEntry>();
            foreach (var pair in counts) {
                entries.Add(new TagEntry(pair.Key.Name, pair.Value));
            }

            entries.Sort((x, y) => string.CompareOrdinal(x.Tag, y.Tag));
            foreach (var tagEntry in entries) {
                LivingPatternEntries.Add(tagEntry);
            }
        }

        public void RefreshLoadTypeList()
        {
            foreach (var lt in Sim.LoadTypes.It) {
                bool enabled = ThisSettlement.EnabledLoadtypes.Contains(lt.Name);

                LoadTypeSelection lts = new LoadTypeSelection(RefreshSettlementLoadTypeList, lt, enabled);
                LoadTypeList.Add(lts);
            }
        }

        public void RefreshTagEntries()
        {
            TagEntries.Clear();
            var counts = _settlement.CalculateHouseholdTagCounts();

            var entries = new List<TagEntry>();
            foreach (var pair in counts) {
                entries.Add(new TagEntry(pair.Key.Name, pair.Value));
            }

            entries.Sort((x, y) => string.CompareOrdinal(x.Tag, y.Tag));
            foreach (var tagEntry in entries) {
                TagEntries.Add(tagEntry);
            }
        }

        public void RemoveHousehold([NotNull] SettlementHH shh)
        {
            _settlement.DeleteSettlementHHFromDB(shh);
            _settlement.SaveToDB();
            OnPropertyChanged(nameof(TotalHouseholds));
            OnPropertyChanged(nameof(Statistics));
        }

        public void RemoveOption(CalcOption calcOption)
        {
            if (ThisSettlement.CalcOptions == null || ThisSettlement.CalcOptions.Contains(calcOption)) {
                ThisSettlement.RemoveOption(calcOption);
            }

            RefreshCalcOptions();
        }

        public void ResetLoadtypeSelection()
        {
            foreach (LoadTypeSelection selection in LoadTypeList) {
                if (selection.Export) {
                    selection.Export = false;
                }
            }
        }

        private void AddSingleTimespan(int min, int second)
        {
            var ts = new TimeSpan(0, 0, min, second);
            DefaultTimeSteps.Add(ts.ToString());
        }

        private void RefreshSettlementLoadTypeList()
        {
            foreach (var lts in LoadTypeList) {
                if (lts.Export && !ThisSettlement.EnabledLoadtypes.Contains(lts.Name)) {
                    ThisSettlement.AddLoadtypeForPostProcessing(lts.LoadType.Name);
                }

                if (!lts.Export && ThisSettlement.EnabledLoadtypes.Contains(lts.Name)) {
                    ThisSettlement.RemoveLoadtypeForPostProcessing(lts.LoadType.Name);
                }
            }
        }

        private void UpdateExternalTimeResolution([CanBeNull] string tsstr)
        {
            bool success = TimeSpan.TryParse(tsstr, out var internalTimeResolution);
            ExternalTimeSteps.Clear();
            if (!success) {
                return;
            }

            ExternalTimeSteps.Add(internalTimeResolution.ToString());
            var ts2 = new TimeSpan(internalTimeResolution.Ticks * 2);
            ExternalTimeSteps.Add(ts2.ToString());
            ts2 = new TimeSpan(internalTimeResolution.Ticks * 3);
            ExternalTimeSteps.Add(ts2.ToString());
            ts2 = new TimeSpan(internalTimeResolution.Ticks * 5);
            ExternalTimeSteps.Add(ts2.ToString());
            ts2 = new TimeSpan(internalTimeResolution.Ticks * 10);
            ExternalTimeSteps.Add(ts2.ToString());
            ts2 = new TimeSpan(internalTimeResolution.Ticks * 15);
            ExternalTimeSteps.Add(ts2.ToString());
            ts2 = new TimeSpan(internalTimeResolution.Ticks * 60);
            ExternalTimeSteps.Add(ts2.ToString());
            ts2 = new TimeSpan(internalTimeResolution.Ticks * 3600);
            ExternalTimeSteps.Add(ts2.ToString());
            ts2 = new TimeSpan(internalTimeResolution.Ticks * 86400);
            ExternalTimeSteps.Add(ts2.ToString());
        }

        private void UpdateInternalTimesteps()
        {
            DefaultTimeSteps.Clear();
            AddSingleTimespan(0, 1);
            AddSingleTimespan(0, 10);
            AddSingleTimespan(1, 0);
            AddSingleTimespan(5, 0);
            AddSingleTimespan(15, 0);
        }

        public class TagEntry {
            public TagEntry([NotNull] string tag, int count)
            {
                Tag = tag;
                Count = count;
            }

            [UsedImplicitly]
            public int Count { get; }

            [NotNull]
            public string Tag { get; }
        }
    }
}
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Automation;
using Common;
using Common.Enums;
using Database.Tables.BasicElements;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Houses;

namespace LoadProfileGenerator.Presenters.Houses {
    public class SettlementTemplatePresenter : PresenterBaseDBBase<SettlementTemplateView>,
        IEquatable<SettlementTemplatePresenter> {
        [ItemNotNull] [NotNull] private readonly ObservableCollection<House> _generatedHouses = new ObservableCollection<House>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<Settlement> _generatedSettlements =
            new ObservableCollection<Settlement>();

        [NotNull] private readonly SettlementTemplate _template;

        [NotNull] private EnergyIntensityConverter.EnergyIntensityForDisplay _hhdIntensity;

        private int _hhdMaxPersons;
        private int _hhdMinPersons;
        private double _hhdPercent;

        [CanBeNull] private HouseholdTemplate _householdTemplateSelection;

        private int _housetypeMaximumSize;
        private int _housetypeMinimumSize;
        private int _hsdMaximum;
        private int _hsdMinimum;
        private double _hsdPercent;

        [CanBeNull] private HouseType _htSelection;

        [CanBeNull] private string _limitFilter;

        [CanBeNull] private HouseholdTrait _limitHouseholdTrait;

        private int _limitMaximum;

        public SettlementTemplatePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] SettlementTemplateView view,
            [NotNull] SettlementTemplate template) : base(view, "ThisTemplate.HeaderString", template, applicationPresenter)
        {
            _hhdIntensity = new EnergyIntensityConverter.EnergyIntensityForDisplay(EnergyIntensityType.EnergyIntensive,"Energy Intensive");
            _template = template;
            RefreshGeneratedSettlements();
            RefreshGeneratedHouses();
            foreach (var tag in Sim.HouseholdTags.It) {
                var te = new TagEntry(tag, false);
                AllTags.Add(te);
            }
            RefreshTraits();
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TagEntry> AllTags { get; } = new ObservableCollection<TagEntry>();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseholdTrait> AllTraits { get; } = new ObservableCollection<HouseholdTrait>();

        [NotNull]
        public EnergyIntensityConverter Eic { get; } = new EnergyIntensityConverter();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<EnergyIntensityConverter.EnergyIntensityForDisplay> EnergyIntensities => Eic.All;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<House> GeneratedHouses => _generatedHouses;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Settlement> GeneratedSettlements => _generatedSettlements;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<GeographicLocation> GeographicLocations => Sim.GeographicLocations.It;

        [NotNull]
        public EnergyIntensityConverter.EnergyIntensityForDisplay HHDIntensity {
            get => _hhdIntensity;
            set {
                if (Equals(value, _hhdIntensity)) {
                    return;
                }
                _hhdIntensity = value;
                OnPropertyChanged(nameof(HHDIntensity));
            }
        }

        public int HHDMaxPersons {
            get => _hhdMaxPersons;
            set {
                if (value == _hhdMaxPersons) {
                    return;
                }
                _hhdMaxPersons = value;
                OnPropertyChanged(nameof(HHDMaxPersons));
            }
        }

        public int HHDMinPersons {
            get => _hhdMinPersons;
            set {
                if (value == _hhdMinPersons) {
                    return;
                }
                _hhdMinPersons = value;
                OnPropertyChanged(nameof(HHDMinPersons));
            }
        }

        public double HHDPercent {
            get => _hhdPercent;
            set {
                if (Math.Abs(value - _hhdPercent) < 0.00000001) {
                    return;
                }
                _hhdPercent = value;
                OnPropertyChanged(nameof(HHDPercent));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseholdTemplate> HouseholdTemplates => Sim.HouseholdTemplates.MyItems;

        [CanBeNull]
        public HouseholdTemplate HouseholdTemplateSelection {
            get => _householdTemplateSelection;
            set {
                if (Equals(value, _householdTemplateSelection)) {
                    return;
                }
                _householdTemplateSelection = value;
                OnPropertyChanged(nameof(HouseholdTemplateSelection));
            }
        }

        [UsedImplicitly]
        public int HousetypeMaximumSize {
            get => _housetypeMaximumSize;
            set {
                if (value == _housetypeMaximumSize) {
                    return;
                }
                _housetypeMaximumSize = value;
                OnPropertyChanged(nameof(HousetypeMaximumSize));
            }
        }

        [UsedImplicitly]
        public int HousetypeMinimumSize {
            get => _housetypeMinimumSize;
            set {
                if (value == _housetypeMinimumSize) {
                    return;
                }
                _housetypeMinimumSize = value;
                OnPropertyChanged(nameof(HousetypeMinimumSize));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseType> HouseTypes => Sim.HouseTypes.MyItems;

        public int HSDMaximum {
            get => _hsdMaximum;
            set {
                if (value.Equals(_hsdMaximum)) {
                    return;
                }
                _hsdMaximum = value;
                OnPropertyChanged(nameof(HSDMaximum));
            }
        }

        public int HSDMinimum {
            get => _hsdMinimum;
            set {
                if (value.Equals(_hsdMinimum)) {
                    return;
                }
                _hsdMinimum = value;
                OnPropertyChanged(nameof(HSDMinimum));
            }
        }

        public double HSDPercent {
            get => _hsdPercent;
            set {
                if (value.Equals(_hsdPercent)) {
                    return;
                }
                _hsdPercent = value;
                OnPropertyChanged(nameof(HSDPercent));
            }
        }

        [CanBeNull]
        public HouseType HTSelection {
            get => _htSelection;
            set {
                if (Equals(value, _htSelection)) {
                    return;
                }
                _htSelection = value;
                OnPropertyChanged(nameof(HTSelection));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public string LimitFilter {
            get => _limitFilter;
            set {
                if (value == _limitFilter) {
                    return;
                }
                _limitFilter = value;
                OnPropertyChanged(nameof(LimitFilter));
                RefreshTraits();
            }
        }

        [CanBeNull]
        public HouseholdTrait LimitHouseholdTrait {
            get => _limitHouseholdTrait;
            set {
                if (Equals(value, _limitHouseholdTrait)) {
                    return;
                }
                _limitHouseholdTrait = value;
                OnPropertyChanged(nameof(LimitHouseholdTrait));
            }
        }

        public int LimitMaximum {
            get => _limitMaximum;
            set {
                if (value == _limitMaximum) {
                    return;
                }
                _limitMaximum = value;
                OnPropertyChanged(nameof(LimitMaximum));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TemperatureProfile> TemperatureProfiles => Sim.TemperatureProfiles.MyItems;

        [NotNull]
        [UsedImplicitly]
        public SettlementTemplate ThisTemplate => _template;

        public bool Equals([CanBeNull] SettlementTemplatePresenter other) => other?.ThisTemplate.Equals(_template) == true;

        public void AddAllTemplates()
        {
            foreach (var template in Sim.HouseholdTemplates.It) {
                _template.AddHouseholdTemplate(template);
            }
        }

        public void AddManyHousetypes()
        {
            foreach (var houseType in Sim.HouseTypes.It) {
                if (houseType.MinimumHouseholdCount >= _housetypeMinimumSize &&
                    houseType.MaximumHouseholdCount <= _housetypeMaximumSize) {
                    _template.AddHouseType(houseType);
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void CreateSettlement()
        {
            try {
                _template.CreateSettlementFromPreview(Sim);
                RefreshGeneratedHouses();
                RefreshGeneratedSettlements();
            }
            catch (Exception ex) {
                Logger.Error("Error:" + ex.Message);
                MessageWindows.ShowDebugMessage(ex);
                Logger.Exception(ex);
            }
        }

        public void Delete()
        {
            Sim.SettlementTemplates.DeleteItem(_template);
            Close(false);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        public void DeleteGeneratedHouses()
        {
            var count = _generatedHouses.Count;
            if (count == 0) {
                return;
            }
            var dr = MessageWindows.ShowYesNoMessage("Delete these " + count + " houses?", "Delete?");
            if (dr != MessageBoxResult.Yes) {
                return;
            }
            foreach (var house in GeneratedHouses) {
                Sim.Houses.DeleteItem(house);
            }
            RefreshGeneratedHouses();
        }

        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        public void DeleteGeneratedSettlements()
        {
            var count = _generatedSettlements.Count;
            if (count == 0) {
                return;
            }
            var dr = MessageWindows.ShowYesNoMessage("Delete these " + count + " settlements?", "Delete?");
            if (dr != MessageBoxResult.Yes) {
                return;
            }
            foreach (var sett in GeneratedSettlements) {
                Sim.Settlements.DeleteItem(sett);
            }
            RefreshGeneratedSettlements();
        }

        public override bool Equals([CanBeNull] object obj)
        {
            var presenter = obj as SettlementTemplatePresenter;
            return presenter?.ThisTemplate.Equals(_template) == true;
        }

        public void GeneratePreview()
        {
            _template.GenerateSettlementPreview(Sim);
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

        public void MakeCopy()
        {
            var newtemplate = Sim.SettlementTemplates.CreateNewItem(Sim.ConnectionString);
            newtemplate.ImportFromExisting(_template);
            newtemplate.SaveToDB();
            ApplicationPresenter.OpenItem(newtemplate);
        }

        public void RefreshGeneratedHouses()
        {
            Logger.Info("Refreshing houses...");
            var houses = new List<House>();
            foreach (var house in Sim.Houses.It) {
                if (_template.IsHouseGeneratedByThis(house)) {
                    houses.Add(house);
                }
            }
            _generatedHouses.SynchronizeWithList(houses);
        }

        public void RefreshGeneratedSettlements()
        {
            Logger.Info("Refreshing settlements...");
            var settlements = new List<Settlement>();
            foreach (var sett in Sim.Settlements.It) {
                if (_template.IsSettlementGeneratedByThis(sett)) {
                    settlements.Add(sett);
                }
            }
            _generatedSettlements.SynchronizeWithList(settlements);
        }

        private void RefreshTraits()
        {
            var filteredTraits = new List<HouseholdTrait>();
            if (string.IsNullOrWhiteSpace(LimitFilter)) {
                filteredTraits.AddRange(Sim.HouseholdTraits.It);
            }
            else {
                filteredTraits = Sim.HouseholdTraits.It
                    .Where(x => x.PrettyName.ToUpperInvariant().Contains(LimitFilter.ToUpperInvariant()))
                    .ToList();
            }
            AllTraits.SynchronizeWithList(filteredTraits);
        }

        public class TagEntry : INotifyPropertyChanged {
            private bool _isChecked;

            public TagEntry([NotNull] HouseholdTag tag, bool isChecked)
            {
                Tag = tag;
                IsChecked = isChecked;
            }

            public bool IsChecked {
                get => _isChecked;
                set {
                    if (value == _isChecked) {
                        return;
                    }
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }

            [NotNull]
            public HouseholdTag Tag { get; }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CanBeNull] [CallerMemberName] string propertyName = null)
            {
                PropertyChanged
                    ?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
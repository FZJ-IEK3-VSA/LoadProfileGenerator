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
using System.Linq;
using Common;
using Common.Enums;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

namespace LoadProfileGenerator.Presenters.Households {
    public class ModularHouseholdPresenter : PresenterBaseDBBase<ModularHouseholdView> {
        [CanBeNull] private static TraitTag _selectedFilterTag;

        [NotNull] private readonly EnergyIntensityConverter _eic = new EnergyIntensityConverter();
        [ItemNotNull] [NotNull] private readonly ObservableCollection<HouseholdTrait> _householdTraits;
        [NotNull] private readonly ModularHousehold _modularHousehold;

        [ItemNotNull] [NotNull] private readonly ObservableCollection<UsedIn> _usedIns = new ObservableCollection<UsedIn>();

        [CanBeNull] private string _filterText;

        private ModularHouseholdTrait.ModularHouseholdTraitAssignType _selectedAssigningType;

        [CanBeNull] private ModularHouseholdTrait _selectedCHHTrait;

        [CanBeNull] private Person _selectedPerson;

        [CanBeNull] private HouseholdTrait _selectedTrait;

        private bool _showOnlySuitableTraits;
        private bool _useFilter;
        private bool _useTags = true;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public ModularHouseholdPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] ModularHouseholdView view,
            [NotNull] ModularHousehold modularHousehold)
            : base(view, "ThisModularHousehold.HeaderString", modularHousehold, applicationPresenter)
        {
            _modularHousehold = modularHousehold;
            AssignTypes = new ObservableCollection<object>();
            foreach (var value in Enum.GetValues(typeof(ModularHouseholdTrait.ModularHouseholdTraitAssignType))) {
                AssignTypes.Add(value);
            }

            if (_modularHousehold.DeviceSelection == null && DeviceSelections.Count > 0) {
                _modularHousehold.DeviceSelection = DeviceSelections[0];
            }

            _householdTraits = new ObservableCollection<HouseholdTrait>();
            FilterTraits(string.Empty, null);
            RefreshLivingPatterns();
            RefreshUsedIn();
            try {
                if (ThisModularHousehold.Persons.Count > 0) {
                    ThisModularHousehold.RefreshPersonTimeEstimates();
                }
            }
            catch (Exception ex) {
                Logger.Exception(ex);
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseholdTag> AllTags => Sim.HouseholdTags.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TraitTag> AllTraitTags => Sim.TraitTags.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<object> AssignTypes { get; }

        [NotNull]
        [UsedImplicitly]
        public Dictionary<CreationType, string> CreationTypes => CreationTypeHelper.CreationTypeDictionary;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceSelection> DeviceSelections
            => Sim.DeviceSelections.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<EnergyIntensityConverter.EnergyIntensityForDisplay> EnergyIntensities
            => _eic.ForHouseholds;

        [NotNull]
        [UsedImplicitly]
        public EnergyIntensityConverter.EnergyIntensityForDisplay EnergyIntensity {
            get => _eic.GetHHDisplayElement(_modularHousehold.EnergyIntensityType);
            set {
                _modularHousehold.EnergyIntensityType = value.EnergyIntensityType;
                OnPropertyChanged(nameof(EnergyIntensity));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ModularHouseholdTrait> FilteredTraits { get; } =
            new ObservableCollection<ModularHouseholdTrait>();

        [CanBeNull]
        [UsedImplicitly]
        public string FilterText {
            get => _filterText;
            set {
                if (value == _filterText) {
                    return;
                }

                _filterText = value;
                OnPropertyChanged(nameof(FilterText));
            }
        }

        [UsedImplicitly]
        public int HouseholdCount => _modularHousehold.Traits.Count;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseholdTrait> HouseholdTraits => _householdTraits;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TraitTag> LivingPatternTags { get; } = new ObservableCollection<TraitTag>();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Person> Persons => Sim.Persons.Items;

        [UsedImplicitly]
        public ModularHouseholdTrait.ModularHouseholdTraitAssignType SelectedAssigningType {
            get => _selectedAssigningType;
            set {
                _selectedAssigningType = value;
                OnPropertyChanged(nameof(SelectedAssigningType));
            }
        }

        [CanBeNull]
        public ModularHouseholdTrait SelectedChTrait {
            get => _selectedCHHTrait;
            set {
                if (Equals(value, _selectedCHHTrait)) {
                    return;
                }

                _selectedCHHTrait = value;
                OnPropertyChanged(nameof(SelectedChTrait));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public TraitTag SelectedFilterTag {
            get { return _selectedFilterTag; }
            set {
                if (Equals(value, _selectedFilterTag)) {
                    return;
                }
#pragma warning disable S2696 // Instance members should not write to "static" fields
                _selectedFilterTag = value;
#pragma warning restore S2696 // Instance members should not write to "static" fields
                OnPropertyChanged(nameof(SelectedFilterTag));
            }
        }

        [CanBeNull]
        public HouseholdTag SelectedHouseholdTag { get; set; }

        [UsedImplicitly]
        [CanBeNull]
        public Person SelectedPerson {
            get => _selectedPerson;
            set {
                _selectedPerson = value;
                OnPropertyChanged(nameof(SelectedPerson));
                FilterTraits(FilterText, SelectedFilterTag);
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public HouseholdTrait SelectedTrait {
            get => _selectedTrait;
            set {
                _selectedTrait = value;
                OnPropertyChanged(nameof(SelectedTrait));
            }
        }

        [UsedImplicitly]
        public bool ShowOnlySuitableTraits {
            get => _showOnlySuitableTraits;
            set {
                _showOnlySuitableTraits = value;
                FilterTraits(FilterText, SelectedFilterTag);
            }
        }

        [NotNull]
        public ModularHousehold ThisModularHousehold => _modularHousehold;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> UsedIns => _usedIns;

        [UsedImplicitly]
        public bool UseFilter {
            get => _useFilter;
            set {
                if (value == _useFilter) {
                    return;
                }

                _useFilter = value;
                OnPropertyChanged(nameof(UseFilter));
                FilterTraits(FilterText, SelectedFilterTag);
            }
        }

        [UsedImplicitly]
        public bool UseTags {
            get => _useTags;
            set {
                if (value == _useTags) {
                    return;
                }

                _useTags = value;
                OnPropertyChanged(nameof(UseTags));
                FilterTraits(FilterText, SelectedFilterTag);
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Vacation> Vacations => Sim.Vacations.Items;

        public void AddHouseholdTrait([NotNull] HouseholdTrait trait,
            ModularHouseholdTrait.ModularHouseholdTraitAssignType modularHouseholdTraitAssignType,
            [CanBeNull] Person p)
        {
            _modularHousehold.AddTrait(trait, modularHouseholdTraitAssignType, p);
            OnPropertyChanged(nameof(HouseholdCount));
            FilterTraits(FilterText, SelectedFilterTag);
        }

        public void AddPerson([NotNull] Person person, [NotNull] LivingPatternTag tag)
        {
            _modularHousehold.AddPerson(person, tag);
        }

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB) {
                _modularHousehold.SaveToDB();
            }

            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public void CreateNewModularHousehold()
        {
            var newChh =
                Sim.ModularHouseholds.CreateNewItem(
                    Sim.ConnectionString);
            var s = ThisModularHousehold.Name;
            var name = ThisModularHousehold.Name + " (Copy)";
            var lastspace = s.LastIndexOf(" ", StringComparison.Ordinal);
            if (lastspace > 0) {
                var number = s.Substring(lastspace);
                var success = int.TryParse(number, out int output);
                if (success) {
                    var newname = name.Substring(0, lastspace);
                    output++;
                    while (Sim.ModularHouseholds.IsNameTaken(newname + " " + output)) {
                        output++;
                    }

                    name = newname + " " + output;
                }
            }

            newChh.Name = name;
            newChh.ImportModularHousehold(ThisModularHousehold);
            ApplicationPresenter.OpenItem(newChh);
            Sim.ModularHouseholds.Items.Sort();
        }

        public void Delete()
        {
            Sim.ModularHouseholds.DeleteItem(_modularHousehold);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as ModularHouseholdPresenter;
            return presenter?.ThisModularHousehold.Equals(_modularHousehold) == true;
        }

        public void FilterTraits([CanBeNull] string text, [CanBeNull] TraitTag selectedTag)
        {
            var sourceList = new List<HouseholdTrait>(Sim.HouseholdTraits.Items);
            if (ShowOnlySuitableTraits && SelectedPerson != null) {
                sourceList = sourceList.Where(x => x.IsValidForPerson(SelectedPerson)).ToList();
            }

            if (UseFilter) {
                if (string.IsNullOrEmpty(text)) {
                    var alltraits = ThisModularHousehold.Traits.ToList();
                    FilteredTraits.SynchronizeWithList(alltraits);
                    _householdTraits.SynchronizeWithList(sourceList);
                    return;
                }

                var upper = text.ToUpperInvariant();
                var traits =
                    ThisModularHousehold.Traits.Where(
                        x => x.HouseholdTrait.PrettyName.ToUpperInvariant().Contains(upper)).ToList();
                FilteredTraits.SynchronizeWithList(traits);
                var availableTraits =
                    sourceList.Where(x => x.PrettyName.ToUpperInvariant().Contains(upper)).ToList();
                if (SelectedPerson != null) {
                    availableTraits = availableTraits.Where(x => x.IsValidForPerson(SelectedPerson)).ToList();
                }

                _householdTraits.SynchronizeWithList(availableTraits);
            }

            if (UseTags) {
                if (selectedTag == null) {
                    var alltraits = ThisModularHousehold.Traits.ToList();
                    FilteredTraits.SynchronizeWithList(alltraits);
                    _householdTraits.SynchronizeWithList(sourceList);
                    return;
                }

                var traits =
                    ThisModularHousehold.Traits.Where(x => x.HouseholdTrait.Tags.Any(y => y.Tag == selectedTag))
                        .ToList();
                FilteredTraits.SynchronizeWithList(traits);
                var availableTraits =
                    sourceList.Where(x => x.Tags.Any(y => y.Tag == selectedTag)).ToList();
                if (SelectedPerson != null) {
                    availableTraits = availableTraits.Where(x => x.IsValidForPerson(SelectedPerson)).ToList();
                }

                _householdTraits.SynchronizeWithList(availableTraits);
            }
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

        [UsedImplicitly]
        public void RefreshLivingPatterns()
        {
            var list = Sim.TraitTags.Items
                .Where(x => x.Name.StartsWith("Living Pattern", StringComparison.InvariantCulture)).ToList();
            LivingPatternTags.SynchronizeWithList(list);
        }

        public void RefreshTimeEstimates()
        {
            ThisModularHousehold.RefreshPersonTimeEstimates();
        }

        public void RefreshUsedIn()
        {
            var usedIn = _modularHousehold.CalculateUsedIns(Sim);
            _usedIns.Clear();
            foreach (var dui in usedIn) {
                _usedIns.Add(dui);
            }
        }

        public void RemoveHouseholdTrait([NotNull] ModularHouseholdTrait trait)
        {
            _modularHousehold.DeleteTraitFromDB(trait);
            OnPropertyChanged(nameof(HouseholdCount));
            FilterTraits(FilterText, SelectedFilterTag);
        }

        public void RemovePerson([NotNull] ModularHouseholdPerson chp)
        {
            _modularHousehold.RemovePerson(chp);
        }

        public void SwapPersons([NotNull] ModularHouseholdPerson srcPerson, [NotNull] Person dstPerson, [NotNull] LivingPatternTag tag)
        {
            _modularHousehold.SwapPersons(srcPerson, dstPerson, tag);
            FilterTraits(FilterText, SelectedFilterTag);
        }
    }
}
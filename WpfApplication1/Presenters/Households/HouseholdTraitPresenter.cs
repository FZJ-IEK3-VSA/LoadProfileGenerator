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
using System.Globalization;
using System.Linq;
using System.Windows;
using Common;
using Common.Enums;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

namespace LoadProfileGenerator.Presenters.Households {
    public class HouseholdTraitPresenter : PresenterBaseDBBase<HouseholdTraitView> {
        [ItemNotNull] [NotNull] private readonly ObservableCollection<string> _classifications = new ObservableCollection<string>();
        [NotNull] private readonly HouseholdTrait _hht;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<Affordance> _relevantAffordances = new ObservableCollection<Affordance>();
        private int _affordanceWeight;
        [NotNull] private string _autoSelectedAddCategory;
        [CanBeNull] private string _currentAffordanceDesireString;
        private int _endMinusTime;
        private int _endPlusTime;
        private bool _overwriteTimeLimitOnAffordance;
        [NotNull] private string _selectedAddCategory;

        [CanBeNull] private HHTLocation _selectedAffLocation;

        [CanBeNull] private Affordance _selectedAffordance;

        private decimal _selectedDecayRate;

        [CanBeNull] private Desire _selectedDesire;

        private PermittedGender _selectedGender;

        [CanBeNull] private string _selectedHealthStatus;

        private double _selectedMaxAge;
        private double _selectedMinAge;
        private decimal _selectedThreshold;
        [CanBeNull] private TimeLimit _selectedTimeLimit;
        private decimal _selectedWeight;

        private int _startMinusTime;
        private int _startPlusTime;

        public HouseholdTraitPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] HouseholdTraitView view,
            [NotNull] HouseholdTrait householdTrait)
            : base(view, "ThisHouseholdTrait.HeaderString", householdTrait, applicationPresenter)
        {
            _hht = householdTrait;
            _selectedAddCategory = CategoryOrDevice[0];
            _autoSelectedAddCategory = CategoryOrDevice[0];
            if (_hht.Locations.Count > 0) {
                SelectedAffLocation = _hht.Locations[0];
            }

            RefreshRelevantAffordancesForAdding();

            if (_relevantAffordances.Count > 0) {
                SelectedAffordance = _relevantAffordances[0];
            }

            SelectedHouseholdTrait = Sim.HouseholdTraits[0];
            RefreshUses();
            RefreshClassifications();
            foreach (TimeType type in Enum.GetValues(typeof(TimeType))) {
                TimeTypes.Add(type);
            }
        }

        [UsedImplicitly]
        public int AffordanceWeight {
            get => _affordanceWeight;
            set {
                if (value == _affordanceWeight) {
                    return;
                }

                _affordanceWeight = value;
                OnPropertyChanged(nameof(AffordanceWeight));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<VLoadType> AllLoadTypes => Sim.LoadTypes.MyItems;

        [NotNull]
        [UsedImplicitly]
        public string AutoSelectedAddCategory {
            get => _autoSelectedAddCategory;
            set {
                _autoSelectedAddCategory = value;
                OnPropertyChanged(nameof(AutoSelectedAddCategory));
                OnPropertyChanged(nameof(ShowAutoCategoryDropDown));
                OnPropertyChanged(nameof(ShowAutoDeviceDropDown));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> CategoryOrDevice => Sim.CategoryOrDevice;

        [NotNull]
        [UsedImplicitly]
        public string Classification {
            get => ThisHouseholdTrait.Classification;
            set => ThisHouseholdTrait.Classification = value;
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> Classifications => _classifications;

        [CanBeNull]
        [UsedImplicitly]
        public string CurrentAffordanceDesireString {
            get => _currentAffordanceDesireString;
            set {
                if (value == _currentAffordanceDesireString) {
                    return;
                }

                _currentAffordanceDesireString = value;
                OnPropertyChanged(nameof(CurrentAffordanceDesireString));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Desire> Desires => Sim.Desires.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceActionGroup> DeviceActionGroups
            => Sim.DeviceActionGroups.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceAction> DeviceActions => Sim.DeviceActions.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceCategory> DeviceCategories
            => Sim.DeviceCategories.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<RealDevice> Devices => Sim.RealDevices.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> DeviceTypeStrings => DeviceTypeSelectorHelper.DeviceTypeStrings;

        public int EndMinusTime {
            get => _endMinusTime;
            set {
                if (value == _endMinusTime) {
                    return;
                }

                _endMinusTime = value;
                OnPropertyChanged(nameof(EndMinusTime));
            }
        }

        public int EndPlusTime {
            get => _endPlusTime;
            set {
                if (value == _endPlusTime) {
                    return;
                }

                _endPlusTime = value;
                OnPropertyChanged(nameof(EndPlusTime));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> HealthStatusStrings { get; } = HHTDesire.HealthStatusStrings;

        [UsedImplicitly]
        public bool HideAffordancesOnAllLocations { get; set; }

        [UsedImplicitly]
        public bool HideAffordancesOnThisLocation { get; set; }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseholdTrait> HouseholdTraits
            => Sim.HouseholdTraits.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Location> Locations => Sim.Locations.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<UsedIn> ModularHouseholds { get; set; } = new ObservableCollection<UsedIn>();

        [NotNull]
        [UsedImplicitly]
        public IEnumerable<PermittedGender> MyPermittedGenders {
            get {
                var lt = new List<PermittedGender>();
                for (var i = 0; i < Constants.PermittedGenderNumber; i++) {
                    lt.Add((PermittedGender) i);
                }

                return lt;
            }
        }

        [UsedImplicitly]
        public bool OverwriteTimeLimitOnAffordance {
            get => _overwriteTimeLimitOnAffordance;
            set {
                if (value == _overwriteTimeLimitOnAffordance) {
                    return;
                }

                _overwriteTimeLimitOnAffordance = value;
                OnPropertyChanged(nameof(OverwriteTimeLimitOnAffordance));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Person> Persons => Sim.Persons.MyItems;

        [NotNull]
        [UsedImplicitly]
        public string PracticalEstimate {
            get {
                if (ThisHouseholdTrait.EstimateType == EstimateType.FromCalculations) {
                    return "In previous calculations this trait was executed " +
                           ThisHouseholdTrait.EstimatedTimes2.ToString(CultureInfo.CurrentCulture) +
                           "x/" + ThisHouseholdTrait.EstimatedTimeType2 + ", for an average of " +
                           (ThisHouseholdTrait.EstimatedDuration2InMinutes / 60).ToString("N1",
                               CultureInfo.CurrentCulture) +
                           "h for a total per year of " +
                           ThisHouseholdTrait.EstimatedTimePerYearInH.ToString("N1", CultureInfo.CurrentCulture) + "h";
                }

                return "For this trait no previous calculations for an estimate could be found.";
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Affordance> RelevantAffordances => _relevantAffordances;

        [NotNull]
        [UsedImplicitly]
        public string SelectedAddCategory {
            get => _selectedAddCategory;
            set {
                _selectedAddCategory = value;
                OnPropertyChanged(nameof(SelectedAddCategory));
                OnPropertyChanged(nameof(ShowDeviceCategoryDropDown));
                OnPropertyChanged(nameof(ShowDeviceDropDown));
                OnPropertyChanged(nameof(ShowDeviceActionDropDown));
                OnPropertyChanged(nameof(ShowDeviceActionGroupDropDown));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public HHTLocation SelectedAffLocation {
            get => _selectedAffLocation;
            set {
                _selectedAffLocation = value;
                OnPropertyChanged(nameof(SelectedAffLocation));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public Affordance SelectedAffordance {
            get => _selectedAffordance;
            set {
                _selectedAffordance = value;
                OnPropertyChanged(nameof(SelectedAffordance));
            }
        }

        [UsedImplicitly]
        public decimal SelectedDecayRate {
            get => _selectedDecayRate;
            set {
                _selectedDecayRate = value;
                OnPropertyChanged(nameof(SelectedDecayRate));
            }
        }

        [CanBeNull]
        public Desire SelectedDesire {
            get => _selectedDesire;
            set {
                _selectedDesire = value;
                OnPropertyChanged(nameof(SelectedDesire));
            }
        }

        [UsedImplicitly]
        public PermittedGender SelectedGender {
            get => _selectedGender;
            set {
                _selectedGender = value;
                OnPropertyChanged(nameof(SelectedGender));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public string SelectedHealthStatus {
            get => _selectedHealthStatus;
            set {
                _selectedHealthStatus = value;
                OnPropertyChanged(nameof(SelectedHealthStatus));
            }
        }

        [NotNull]
        [UsedImplicitly]
        public HouseholdTrait SelectedHouseholdTrait { get; set; }

        [CanBeNull]
        [UsedImplicitly]
        public HouseholdTrait SelectedImportHouseholdTrait { get; set; }

        [UsedImplicitly]
        public double SelectedMaxAge {
            get => _selectedMaxAge;
            set {
                _selectedMaxAge = value;
                OnPropertyChanged(nameof(SelectedMaxAge));
            }
        }

        [UsedImplicitly]
        public double SelectedMinAge {
            get => _selectedMinAge;
            set {
                _selectedMinAge = value;
                OnPropertyChanged(nameof(SelectedMinAge));
            }
        }

        [UsedImplicitly]
        public decimal SelectedThreshold {
            get => _selectedThreshold;
            set {
                _selectedThreshold = value;
                OnPropertyChanged(nameof(SelectedThreshold));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public TimeLimit SelectedTimeLimit {
            get => _selectedTimeLimit;
            set {
                if (Equals(value, _selectedTimeLimit)) {
                    return;
                }

                _selectedTimeLimit = value;
                OnPropertyChanged(nameof(SelectedTimeLimit));
            }
        }

        [UsedImplicitly]
        public decimal SelectedWeight {
            get => _selectedWeight;
            set {
                _selectedWeight = value;
                OnPropertyChanged(nameof(SelectedWeight));
            }
        }

        [UsedImplicitly]
        public bool ShowAllAffordances { get; set; }

        [UsedImplicitly]
        public Visibility ShowAutoCategoryDropDown {
            get {
                if (_autoSelectedAddCategory == "Device") {
                    return Visibility.Hidden;
                }

                return Visibility.Visible;
            }
        }

        [UsedImplicitly]
        public Visibility ShowAutoDeviceDropDown {
            get {
                if (_autoSelectedAddCategory == "Device") {
                    return Visibility.Visible;
                }

                return Visibility.Hidden;
            }
        }

        [UsedImplicitly]
        public Visibility ShowDeviceActionDropDown {
            get {
                if (DeviceTypeSelectorHelper.DeviceTypeDict[_selectedAddCategory] ==
                    AssignableDeviceType.DeviceAction) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        public Visibility ShowDeviceActionGroupDropDown {
            get {
                if (DeviceTypeSelectorHelper.DeviceTypeDict[_selectedAddCategory] ==
                    AssignableDeviceType.DeviceActionGroup) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        public Visibility ShowDeviceCategoryDropDown {
            get {
                if (DeviceTypeSelectorHelper.DeviceTypeDict[_selectedAddCategory] ==
                    AssignableDeviceType.DeviceCategory) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        [UsedImplicitly]
        public Visibility ShowDeviceDropDown {
            get {
                if (DeviceTypeSelectorHelper.DeviceTypeDict[_selectedAddCategory] == AssignableDeviceType.Device) {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        public int StartMinusTime {
            get => _startMinusTime;
            set {
                if (value == _startMinusTime) {
                    return;
                }

                _startMinusTime = value;
                OnPropertyChanged(nameof(StartMinusTime));
            }
        }

        public int StartPlusTime {
            get => _startPlusTime;
            set {
                if (value == _startPlusTime) {
                    return;
                }

                _startPlusTime = value;
                OnPropertyChanged(nameof(StartPlusTime));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TraitTag> Tags => Sim.TraitTags.It;

        [NotNull]
        public HouseholdTrait ThisHouseholdTrait => _hht;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeLimit> TimeLimits => Sim.TimeLimits.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeBasedProfile> Timeprofiles
            => Sim.Timeprofiles.MyItems;

        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeType> TimeTypes { get; } = new ObservableCollection<TimeType>();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseholdTrait> Traits => Sim.HouseholdTraits.MyItems;

        public void AddAffordanceToLocation()
        {
            TimeLimit tl = null;
            if (OverwriteTimeLimitOnAffordance) {
                tl = SelectedTimeLimit;
            }

            if (SelectedAffordance == null) {
                Logger.Warning("No affordance selected");
                return;
            }

            if (SelectedAffLocation == null) {
                Logger.Error("Selected location was null");
                return;
            }
            _hht.AddAffordanceToLocation(SelectedAffLocation, SelectedAffordance, tl, AffordanceWeight, _startMinusTime,
                _startPlusTime, _endMinusTime, _endPlusTime);
        }

        public void AddAutoDev([NotNull] IAssignableDevice db, [CanBeNull] TimeBasedProfile tp,
            decimal timeStandardDeviation,
            [CanBeNull] VLoadType vLoadType, [NotNull] TimeLimit timeLimit, [CanBeNull] Location loc,
            double variableValue,
            VariableCondition variableCondition, [CanBeNull] Variable variable)
        {
            _hht.AddAutomousDevice(db, tp, timeStandardDeviation, vLoadType, timeLimit, loc, variableValue,
                variableCondition, variable);

            _hht.SaveToDB();
        }

        public void AddDesire([NotNull] Desire desire, decimal decayTime, [NotNull] string healthStatus, decimal threshold, decimal weight,
            int minAge, int maxAge, PermittedGender gender)
        {
            _hht.AddDesire(desire, decayTime, healthStatus, threshold, weight, minAge, maxAge, gender);
            _hht.CalculateEstimatedTimes();
        }

        [NotNull]
        public HHTLocation AddLocation([NotNull] Location l)
        {
            var hhtl = _hht.AddLocation(l);
            _hht.SaveToDB();
            return hhtl;
        }

        public void AddTag([NotNull] TraitTag tag)
        {
            ThisHouseholdTrait.AddTag(tag);
        }

        public void CreateNewTrait()
        {
            var newTrait = ThisHouseholdTrait.MakeCopy(Sim);
            ApplicationPresenter.OpenItem(newTrait);
            Sim.HouseholdTraits.It.Sort();
        }

        public void Delete()
        {
            Sim.HouseholdTraits.DeleteItem(_hht);
            Close(false);
        }

        public override bool Equals(object obj)
        {
            var presenter = obj as HouseholdTraitPresenter;
            return presenter?.ThisHouseholdTrait.Equals(_hht) == true;
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

        public void ImportHousehold()
        {
            if(SelectedImportHouseholdTrait!= null) {
                _hht.ImportHouseholdTrait(SelectedImportHouseholdTrait);
            }
            else
            {
                Logger.Warning("No household trait selected.");
            }
        }

        public void RefreshClassifications()
        {
            foreach (var trait in Sim.HouseholdTraits.It) {
                if (!_classifications.Contains(trait.Classification)) {
                    _classifications.Add(trait.Classification);
                }
            }
        }

        public void RefreshRelevantAffordancesForAdding()
        {
            RelevantAffordances.Clear();
            var alldesires = new Dictionary<Desire, bool>();
            foreach (var desire in _hht.Desires) {
                if (!alldesires.ContainsKey(desire.Desire)) {
                    alldesires.Add(desire.Desire, true);
                }
            }

            foreach (var affordance in Sim.Affordances.MyItems) {
                foreach (var affordanceDesire in affordance.AffordanceDesires) {
                    if (alldesires.ContainsKey(affordanceDesire.Desire)) {
                        _relevantAffordances.Add(affordance);
                        break;
                    }
                }
            }

            if (HideAffordancesOnAllLocations) {
                Sim.DeviceCategories.MyItems.ToList().ForEach(dc => dc.RefreshSubDevices());
                foreach (var hhLocation in _hht.Locations) {
                    foreach (var aff in hhLocation.AffordanceLocations) {
                        if (_relevantAffordances.Contains(aff.Affordance)) {
                            _relevantAffordances.Remove(aff.Affordance);
                        }
                    }
                }
            }

            if (HideAffordancesOnThisLocation) {
                if(SelectedAffLocation != null) {
                    foreach (var aff in SelectedAffLocation.AffordanceLocations) {
                        if (_relevantAffordances.Contains(aff.Affordance)) {
                            _relevantAffordances.Remove(aff.Affordance);
                        }
                    }
                }
            }
        }

        public void RefreshUses()
        {
            var usedInHH = _hht.CalculateUsedIns(Sim);
            ModularHouseholds.Clear();
            foreach (var lui in usedInHH) {
                ModularHouseholds.Add(lui);
            }
        }

        public void RemoveAutoDev([NotNull] HHTAutonomousDevice ad)
        {
            _hht.DeleteHHTAutonomousDeviceFromDB(ad);
            _hht.SaveToDB();
        }

        public void RemoveDesire([NotNull] HHTDesire hhtDesire)
        {
            _hht.RemoveDesire(hhtDesire);
            _hht.CalculateEstimatedTimes();
        }

        public void RemoveLocation([NotNull] HHTLocation l)
        {
            _hht.DeleteHHTLocationFromDB(l);
            _hht.SaveToDB();
        }

        public void ShowOtherPossibleDesires()
        {
            var referencedDesires = new Dictionary<Desire, string>();
            var mydesires = ThisHouseholdTrait.Desires.Select(x => x.Desire).ToList();
            var affs = ThisHouseholdTrait.CollectAffordances(false);
            foreach (var aff in affs) {
                foreach (var desire in aff.AffordanceDesires) {
                    if (!referencedDesires.ContainsKey(desire.Desire) && !mydesires.Contains(desire.Desire)) {
                        referencedDesires.Add(desire.Desire, string.Empty);
                    }

                    if (referencedDesires.ContainsKey(desire.Desire)) {
                        referencedDesires[desire.Desire] += aff.Name + ", ";
                    }
                }
            }

            Logger.Info("The following desires would also be possible:");
            foreach (var desire in referencedDesires) {
                SelectedDesire = desire.Key;
                var allaffs = desire.Value;
                allaffs = allaffs.Substring(0, allaffs.Length - 2);
                var desirename = desire.Key.PrettyName;
                Logger.Info(desirename + " (" + allaffs + ")");
            }
        }
    }
}
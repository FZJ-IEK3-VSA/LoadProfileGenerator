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

#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Common;
using Common.Enums;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

#endregion

namespace LoadProfileGenerator.Presenters.Households {
    public class AffordancePresenter : PresenterBaseDBBase<AffordanceView> {
        [NotNull] private readonly Affordance _aff;

        [NotNull] private readonly ObservableCollection<VariableAction> _allActions =
            new ObservableCollection<VariableAction>();

        [ItemNotNull] [NotNull]
        private readonly ObservableCollection<string> _allConditions = new ObservableCollection<string>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<string> _executionTimes;

        [ItemNotNull] [NotNull] private readonly ObservableCollection<UsedIn> _usedIns;

        private DateTime _endtime;

        [CanBeNull] private string _selectedAddCategory;

        private DateTime _starttime;

        public AffordancePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] AffordanceView view,
            [NotNull] Affordance aff)
            : base(view, "ThisAffordance.HeaderString", aff, applicationPresenter)
        {
            _aff = aff;
            SelectedAddCategory = Sim.CategoryOrDevice[0];
            Sim.Affordances.RefreshAllAffordanceCategory();
            _usedIns = new ObservableCollection<UsedIn>();
            RefreshUsedIn();
            LocationModes.Add(VariableLocationMode.CurrentLocation);
            LocationModes.Add(VariableLocationMode.OtherLocation);
            _allActions.Add(VariableAction.SetTo);
            _allActions.Add(VariableAction.Add);
            _allActions.Add(VariableAction.Subtract);
            _allConditions.SynchronizeWithList(VariableConditionHelper.CollectAllStrings());
            _executionTimes = new ObservableCollection<string>();
            _executionTimes.SynchronizeWithList(VariableExecutionTimeHelper.CollectAllStrings());
            ActionsAfterInterruptions.SynchronizeWithList(ActionAfterInterruptionHelper.CollectAllStrings());
        }

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<string> ActionsAfterInterruptions { get; } = new ObservableCollection<string>();

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Affordance> Affordances => Sim.Affordances.Items;

        [UsedImplicitly]
        [NotNull]
        public ObservableCollection<VariableAction> AllActions => _allActions;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<string> AllAffordanceCategories
            =>Sim.Affordances.AllAffordanceCategories;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<string> AllConditions => _allConditions;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Location> AllLocations => Sim.Locations.Items;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Variable> AllVariables => Sim.Variables.Items;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<string> CategoryOrDevice => Sim.CategoryOrDevice;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<Desire> Desires => Sim.Desires.Items;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<DeviceCategory> DeviceCategories
            => Sim.DeviceCategories.Items;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<RealDevice> Devices => Sim.RealDevices.Items;

        [UsedImplicitly]
        public DateTime Endtime {
            get => _endtime;
            set {
                if (value.Equals(_endtime)) {
                    return;
                }

                _endtime = value;
                OnPropertyChanged(nameof(Endtime));
            }
        }

        [NotNull]
        [UsedImplicitly]
        public Dictionary<BodilyActivityLevel, string> BodilyActivityLevelDict => BodilyActivityLevelHelper.BodilyActivityLevelEnumDictionary;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<string> ExecutionTimes => _executionTimes;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<GeographicLocation> GeographicLocations
            => Sim.GeographicLocations.Items;

        [UsedImplicitly]
        [NotNull]
        public ObservableCollection<VariableLocationMode> LocationModes { get; } =
            new ObservableCollection<VariableLocationMode>();

        [UsedImplicitly]
        [NotNull]
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
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<VLoadType> MyVLoadTypes => Sim.LoadTypes.Items;

        [UsedImplicitly]
        [CanBeNull]
        public string SelectedAddCategory {
            get => _selectedAddCategory;
            set {
                _selectedAddCategory = value;
                OnPropertyChanged(nameof(SelectedAddCategory));
                OnPropertyChanged(nameof(ShowCategoryDropDown));
                OnPropertyChanged(nameof(ShowDeviceDropDown));
            }
        }

        [UsedImplicitly]
        [CanBeNull]
        public Affordance SelectedImportAffordance { get; set; }

        [UsedImplicitly]
        public Visibility ShowCategoryDropDown {
            get {
                if (_selectedAddCategory == "Device") {
                    return Visibility.Hidden;
                }

                return Visibility.Visible;
            }
        }

        [UsedImplicitly]
        public Visibility ShowDeviceDropDown {
            get {
                if (_selectedAddCategory == "Device") {
                    return Visibility.Visible;
                }

                return Visibility.Hidden;
            }
        }


        [UsedImplicitly]
        public DateTime Starttime {
            get => _starttime;
            set {
                if (value.Equals(_starttime)) {
                    return;
                }

                _starttime = value;
                OnPropertyChanged(nameof(Starttime));
            }
        }

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<SubAffordance> SubAffordances
            => Sim.SubAffordances.Items;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TemperatureProfile> TemperaturProfiles
            => Sim.TemperatureProfiles.Items;

        [NotNull]
        public Affordance ThisAffordance => _aff;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TimeLimit> TimeLimits => Sim.TimeLimits.Items;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TimeBasedProfile> Timeprofiles
            => Sim.Timeprofiles.Items;

        [UsedImplicitly]
        [ItemNotNull]
        [NotNull]
        public ObservableCollection<UsedIn> UsedIns => _usedIns;

        public void AddDesire([NotNull] Desire d, decimal satisfactionvalue)
        {
            _aff.AddDesire(d, satisfactionvalue, Sim.Desires.Items);
            _aff.SaveToDB();
        }

        public void AddDeviceProfile([NotNull] IAssignableDevice pdev, [CanBeNull] TimeBasedProfile tp,
            decimal timeoffset,
            [CanBeNull] VLoadType vLoadType, double probability)
        {
            _aff.AddDeviceProfile(pdev, tp, timeoffset, Sim.RealDevices.Items,
                Sim.DeviceCategories.Items, vLoadType, probability);
            _aff.SaveToDB();
        }

        public void AddSubAffordance([NotNull] SubAffordance subAff, decimal delayTime)
        {
            _aff.AddSubAffordance(subAff, delayTime);
            _aff.SaveToDB();
        }

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB) {
                _aff.SaveToDB();
            }

            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public void CreateNewSubaffordance()
        {
            var subaff = Sim.SubAffordances.CreateNewItem(Sim.ConnectionString);
            subaff.Name = "Sub-Affordance for " + _aff.Name;
            subaff.SaveToDB();
            foreach (var desire in _aff.AffordanceDesires) {
                subaff.AddDesire(desire.Desire, 1, Sim.Desires.Items);
            }

            subaff.MinimumAge = _aff.MinimumAge;
            subaff.MaximumAge = _aff.MaximumAge;
            subaff.PermittedGender = _aff.PermittedGender;
            subaff.SaveToDB();
            _aff.AddSubAffordance(subaff, 0);
            ApplicationPresenter.OpenItem(subaff);
        }

        public void CreateTrait()
        {
            var trait = Sim.HouseholdTraits.CreateNewItem(Sim.ConnectionString);
            trait.Name = _aff.Name + " Trait";
            foreach (var affdes in _aff.AffordanceDesires) {
                trait.AddDesire(affdes.Desire, affdes.Desire.DefaultDecayRate, "All", 0.5m, affdes.Desire.DefaultWeight,
                    18, 99, PermittedGender.All);
            }

            var loc = Sim.Locations.FindFirstByName("Kitchen");
            if (loc == null) {
                Logger.Error("Could not find the kitchen, which is needed for the trait creation. Please fix.");
                return;
            }

            var hhtloc = trait.AddLocation(loc);
            trait.AddAffordanceToLocation(hhtloc, _aff, null, 100, 0, 0, 0, 0);
            var dt = Sim.TimeLimits.FindFirstByName("Standby", FindMode.Partial);
            foreach (var standby in _aff.AffordanceStandbys) {
                if (standby.Device == null) {
                    continue;
                }

                trait.AddAutomousDevice(standby.Device, null, 0.1m, null, dt, loc, 0, VariableCondition.Equal, null);
            }

            trait.SaveToDB();
            ApplicationPresenter.OpenItem(trait);
        }

        public void Delete()
        {
            Sim.Affordances.DeleteItem(_aff);
            Close(false);
        }

        [SuppressMessage("ReSharper", "NotAllowedAnnotation")]
        public override bool Equals(object obj)
        {
            if (obj == null) {
                return false;
            }

            var presenter = obj as AffordancePresenter;
            return presenter?.ThisAffordance.Equals(_aff) == true;
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

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public void ImportAffordance()
        {
            if (SelectedImportAffordance == null) {
                return;
            }

            ThisAffordance.ImportFromOtherAffordance(SelectedImportAffordance,
                Sim.Desires.Items, Sim.RealDevices.Items,
                Sim.DeviceCategories.Items);
        }

        public void MakeAffordanceCopy()
        {
            var aff = Sim.Affordances.CreateNewItem(Sim.ConnectionString);
            aff.ImportFromOtherAffordance(_aff, Sim.Desires.Items, Sim.RealDevices.Items, Sim.DeviceCategories.Items);
            aff.SaveToDB();
            ApplicationPresenter.OpenItem(aff);
        }

        public void RefreshUsedIn()
        {
            var usedIn = _aff.CalculateUsedIns(Sim);
            _usedIns.Clear();
            foreach (var dui in usedIn) {
                _usedIns.Add(dui);
            }
        }

        public void RemoveDesire([NotNull] AffordanceDesire ad)
        {
            _aff.DeleteAffordanceDesireFromDB(ad);
            _aff.SaveToDB();
        }

        public void RemoveDeviceAndTimeprofile([NotNull] AffordanceDevice dtp)
        {
            _aff.DeleteDeviceFromDB(dtp);
            _aff.SaveToDB();
        }

        public void RemoveSubAffordance([NotNull] AffordanceSubAffordance subaff)
        {
            _aff.DeleteAffordanceSubAffFromDB(subaff);
            _aff.SaveToDB();
        }
    }
}
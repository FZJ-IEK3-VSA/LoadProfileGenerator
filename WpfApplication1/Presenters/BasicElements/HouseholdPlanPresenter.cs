using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Views.SpecialViews;

namespace LoadProfileGenerator.Presenters.BasicElements {
    public class HouseholdPlanPresenter : PresenterBaseDBBase<HouseholdPlanView> {
        private static int _refreshCount;

        private static int _statisticsRefreshCount;

        [ItemNotNull] [NotNull] private readonly ObservableCollection<ModularHouseholdTrait> _alAssignments =
            new ObservableCollection<ModularHouseholdTrait>();

        [NotNull] private readonly HouseholdPlan _householdPlan;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<Person> _newPersons = new ObservableCollection<Person>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<StatisticsEntry>
            _statistics = new ObservableCollection<StatisticsEntry>();

        [CanBeNull] private Person _aLPerson;

        [CanBeNull] private HouseholdTrait _aLTrait;

        [ItemNotNull] [NotNull] private ObservableCollection<PersonAssignments> _personAssignments =
            new ObservableCollection<PersonAssignments>();

        private CalcObjectType _selectedCalcObjectType;

        [CanBeNull] private string _selectedColorScheme;

        [CanBeNull] private string _sortBy;

        public HouseholdPlanPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] HouseholdPlanView view,
            [NotNull] HouseholdPlan householdPlan)
            : base(view, "ThisHouseholdPlan.HeaderString", householdPlan, applicationPresenter)
        {
            SuspendStatistics = true;
            CalcObjectTypes = new ObservableCollection<CalcObjectType>
            {
                CalcObjectType.ModularHousehold
            };

            _householdPlan = householdPlan;
            foreach (var entry in _householdPlan.Entries) {
                entry.AllDeviceActions = Sim.DeviceActions.It;
            }

            foreach (var dc in Sim.DeviceCategories.MyItems) {
                dc.RefreshSubDevices();
            }

            _householdPlan.RefreshTagCategories(Sim.Affordances.It);

            SortByOptions.Add("By Name");
            SortByOptions.Add("By Person");

            SortByOptions.Add("By Activiations");
            SortByOptions.Add("By Desire");
            SortByOptions.Add("By Total Time");
            SortByOptions.Add("By Category");
            SortBy = SortByOptions[1];
            Resort();
            if (householdPlan.CalcObject != null) {
                if (householdPlan.CalcObject.GetType() == typeof(ModularHousehold)) {
                    SelectedCalcObjectType = CalcObjectType.ModularHousehold;
                }
                else {
                    throw new LPGException("Unknown ");
                }
            }
            else {
                SelectedCalcObjectType = CalcObjectType.ModularHousehold;
            }

            ColorSchemes.Add("By Activations");
            ColorSchemes.Add("By Traits");
            SelectedColorScheme = ColorSchemes[0];
            RefreshPlan();
            RefreshAlTraits();
            RefreshAlAssignments();
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<AffordanceTaggingSet> AffordanceTaggingSets
            => Sim.AffordanceTaggingSets.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ModularHouseholdTrait> ALAssignments => _alAssignments;

        [CanBeNull]
        [UsedImplicitly]
        public Person ALPerson {
            get => _aLPerson;
            set {
                if (Equals(value, _aLPerson)) {
                    return;
                }

                _aLPerson = value;
                OnPropertyChanged(nameof(ALPerson));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Person> ALPersons { get; set; } = new ObservableCollection<Person>();

        [CanBeNull]
        [UsedImplicitly]
        public HouseholdTrait ALTrait {
            get => _aLTrait;
            set {
                if (Equals(value, _aLTrait)) {
                    return;
                }

                _aLTrait = value;
                OnPropertyChanged(nameof(ALTrait));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseholdTrait> ALTraits { get; set; } = new ObservableCollection<HouseholdTrait>();

        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<CalcObjectType> CalcObjectTypes { get; }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<string> ColorSchemes { get; set; } = new ObservableCollection<string>();

        [CanBeNull]
        [UsedImplicitly]
        public ModularHousehold EditHousehold { get; set; }

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
        public ObservableCollection<ModularHousehold> ModularHouseholds
            => Sim.ModularHouseholds.MyItems;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<PersonAssignments> PersonAssignments {
            get => _personAssignments;
            set {
                _personAssignments = value;
                RefreshPersons();
            }
        }

        [UsedImplicitly]
        public CalcObjectType SelectedCalcObjectType {
            get => _selectedCalcObjectType;
            set {
                _selectedCalcObjectType = value;
                OnPropertyChanged(nameof(SelectedCalcObjectType));
                OnPropertyChanged(nameof(IsModularHouseholdVisible));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public string SelectedColorScheme {
            get => _selectedColorScheme;
            set {
                _selectedColorScheme = value;
                HouseholdPlanEntry.ColorScheme scheme;
                if (_selectedColorScheme == ColorSchemes[0]) {
                    scheme = HouseholdPlanEntry.ColorScheme.ForPlanning;
                }
                else if (_selectedColorScheme == ColorSchemes[1]) {
                    scheme = HouseholdPlanEntry.ColorScheme.ForModular;
                }
                else {
                    throw new LPGException("Forgotten scheme!");
                }

                foreach (var entry in _householdPlan.Entries) {
                    entry.SelectedColorScheme = scheme;
                }
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public ModularHousehold SelectedModularHousehold {
            get {
                if (_householdPlan.CalcObject != null &&
                    _householdPlan.CalcObject.CalcObjectType == CalcObjectType.ModularHousehold) {
                    return (ModularHousehold) _householdPlan.CalcObject;
                }

                return null;
            }
            set {
                _householdPlan.CalcObject = value;
                OnPropertyChanged(nameof(SelectedModularHousehold));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public string SortBy {
            get => _sortBy;
            set {
                _sortBy = value;
                Resort();
                OnPropertyChanged(nameof(SortBy));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public List<string> SortByOptions { get; } = new List<string>();

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<StatisticsEntry> Statistics => _statistics;

        [UsedImplicitly]
        public bool SuspendStatistics { get; set; }

        [NotNull]
        [UsedImplicitly]
        public HouseholdPlan ThisHouseholdPlan => _householdPlan;

        public void AddAlTrait()
        {
            if (_householdPlan.CalcObject is ModularHousehold chh)
            {
                if (ALTrait == null)
                {
                    throw new LPGException("Trait was null");
                }
                chh.AddTrait(ALTrait, ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name, ALPerson);
                RefreshAlAssignments();
            }
        }

        public void AddTraitToModularHousehold([NotNull] HouseholdPlanEntry entry)
        {
            if (_householdPlan.CalcObject?.GetType() == typeof(ModularHousehold) && entry.SelectedTrait !=null) {
                var chh = (ModularHousehold) _householdPlan.CalcObject;
                chh.AddTrait(entry.SelectedTrait, ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name,
                    entry.Person);
                RefreshEntries(false);
                Logger.Info("Added " + entry.SelectedTrait + " to " + entry.Person?.PrettyName);
                entry.TimeCount = entry.SelectedTrait.EstimatedTimeCount;
                entry.TimeType = entry.SelectedTrait.EstimatedTimeType;
                entry.Times = entry.SelectedTrait.EstimatedTimes;
                entry.SaveToDB();
            }
        }

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB) {
                _householdPlan.SaveToDB();
            }

            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public void Delete()
        {
            Sim.HouseholdPlans.DeleteItem(_householdPlan);
            Close(false);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            return obj is HouseholdPlanPresenter presenter && presenter.ThisHouseholdPlan.Equals(_householdPlan);
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

        public void RefreshAlAssignments()
        {
            if (_householdPlan.CalcObject is ModularHousehold chh)
            {
                _alAssignments.Clear();
                foreach (var trait in chh.Traits)
                {
                    if (ALTraits.Contains(trait.HouseholdTrait))
                    {
                        _alAssignments.Add(trait);
                    }
                }
            }
        }

        public void RefreshCHousehold()
        {
            RefreshPersons();
            RefreshEntries(true);
        }

        public void RefreshFromTraits()
        {
            _householdPlan.RefreshFromTraits();
        }

        /// <summary>
        ///     Refreshes the plan.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void RefreshPlan()
        {
            var maxcount = ThisHouseholdPlan.Entries.Count;
            var pbw = new ProgressbarWindow("Refreshing household plan...", "Refreshing...", maxcount);
            pbw.Show();

            try {
                _householdPlan.Refresh(pbw.UpdateValue);
                RefreshPersons();
                RefreshEntries(true);
                Logger.Get().SafeExecute(pbw.Close);
                _householdPlan.SaveToDB();
            }
            catch (Exception e) {
                MessageWindows.ShowInfoMessage("Error while refreshing:" + e.Message, "Error");
                Logger.Exception(e);
            }
        }

        public void RefreshStatistics()
        {
            if (SuspendStatistics) {
                return;
            }

            if (UnloadingStarted) {
                return;
            }
#pragma warning disable S2696 // Instance members should not write to "static" fields
            _statisticsRefreshCount++;
#pragma warning restore S2696 // Instance members should not write to "static" fields
            Logger.Debug("StatisticsRefresh:" + _statisticsRefreshCount);
            Statistics.Clear();
            var energyPerLoadtype = new Dictionary<VLoadType, double>();
            var timePerPerson = new Dictionary<Person, double>();
            foreach (var tags in _householdPlan.Entries) {
                if (tags.Person == null)
                {
                    throw new LPGException("Person was null");
                }

                if (!timePerPerson.ContainsKey(tags.Person)) {
                    timePerPerson.Add(tags.Person, 0);
                }

                timePerPerson[tags.Person] += tags.TotalTime.TotalSeconds;
                var individualuses =
                    tags.CollectTotalEnergyUses(Sim.DeviceActions.MyItems);
                foreach (var energyUse in individualuses) {
                    if (!energyPerLoadtype.ContainsKey(energyUse.Key)) {
                        energyPerLoadtype.Add(energyUse.Key, 0);
                    }

                    energyPerLoadtype[energyUse.Key] += energyUse.Value;
                }
            }

            foreach (var persontime in timePerPerson) {
                var entry = new StatisticsEntry
                {
                    Name = persontime.Key.Name,
                    Value = new TimeSpan(0, 0, (int)persontime.Value).ToString()
                };
                _statistics.Add(entry);
            }

            foreach (var pair in energyPerLoadtype) {
                var entry = new StatisticsEntry
                {
                    Name = pair.Key.Name,
                    Value = pair.Value.ToString("N2", CultureInfo.CurrentCulture) + " " + pair.Key.UnitOfSum
                };
                _statistics.Add(entry);
            }
        }

        public void RemoveAlTrait([NotNull] ModularHouseholdTrait ala)
        {
            var chh = _householdPlan.CalcObject as ModularHousehold;
            chh?.DeleteTraitFromDB(ala);
        }

        public void Resort()
        {
            ThisHouseholdPlan.Entries.Sort(Comparison);
        }

        private int Comparison([NotNull] HouseholdPlanEntry x, [NotNull] HouseholdPlanEntry y)
        {
            if (x.Person == null)
            {
                throw new LPGException("Person was null");
            }
            if (y.Person == null)
            {
                throw new LPGException("Person was null");
            }
            if (x.Tag == null)
            {
                throw new LPGException("Tag was null");
            }
            switch (SortBy) {
                case "By Person": {
                        var result = string.Compare(x.Person.Name, y.Person.Name, StringComparison.Ordinal);
                    if (result != 0) {
                        return result;
                    }

                    return x.Tag.CompareTo(y.Tag);
                }
                case "By Name": {
                    var result = x.Tag.CompareTo(y.Tag);
                    if (result == 0) {
                        result = string.Compare(x.Person.Name, y.Person.Name, StringComparison.Ordinal);
                    }

                    return result;
                }
                case "By Activiations": {
                    var result = x.TotalActivations.CompareTo(y.TotalActivations);
                    if (result == 0) {
                        result = x.Tag.CompareTo(y.Tag);
                    }

                    if (result == 0) {
                        result = string.Compare(x.Person.Name, y.Person.Name, StringComparison.Ordinal);
                    }

                    return result;
                }
                case "By Desire": {
                    var result = string.Compare(x.Desires, y.Desires, StringComparison.Ordinal);
                    if (result == 0) {
                        result = x.Tag.CompareTo(y.Tag);
                    }

                    if (result == 0) {
                        result = string.Compare(x.Person.Name, y.Person.Name, StringComparison.Ordinal);
                    }

                    return result;
                }
                case "By Total Time": {
                    var result = y.TotalTime.CompareTo(x.TotalTime);
                    if (result == 0) {
                        result = x.Tag.CompareTo(y.Tag);
                    }

                    if (result == 0) {
                        result = string.Compare(x.Person.Name, y.Person.Name, StringComparison.Ordinal);
                    }

                    return result;
                }
                case "By Category": {
                    var result = string.Compare(x.AffordanceCategory, y.AffordanceCategory, StringComparison.Ordinal);
                    if (result == 0) {
                        result = x.Tag.CompareTo(y.Tag);
                    }

                    if (result == 0) {
                        result = string.Compare(x.Person.Name, y.Person.Name, StringComparison.Ordinal);
                    }

                    return result;
                }
                default: {
                    throw new LPGException("Unknown Sort By");
                }
            }
        }

        private void RefreshAlTraits()
        {
            foreach (var trait in Sim.HouseholdTraits.It) {
                if (trait.CollectAffordances(true).Count == 0) {
                    if (!ALTraits.Contains(trait)) {
                        ALTraits.Add(trait);
                    }
                }
            }
        }

        private void RefreshEntries(bool refreshDropdowns)
        {
#pragma warning disable S2696 // Instance members should not write to "static" fields
            _refreshCount++;
#pragma warning restore S2696 // Instance members should not write to "static" fields
            Logger.Debug("Refresh:" + _refreshCount);
            for (var index = 0; index < _householdPlan.Entries.Count; index++) {
                var entry = _householdPlan.Entries[index];
                entry.AllTraits = Sim.HouseholdTraits.It;
                entry.Assignments = _personAssignments;
                if (_householdPlan.CalcObject != null) {
                    entry.RefreshAllRelevantTraits((ModularHousehold) _householdPlan.CalcObject, refreshDropdowns);
                }
            }
        }

        private void RefreshPersons()
        {
            if (_householdPlan.CalcObject == null) {
                return;
            }

            if (EditHousehold?.Persons != null) {
                foreach (var persons in EditHousehold.Persons) {
                    if (!_newPersons.Contains(persons.Person)) {
                        _newPersons.Add(persons.Person);
                    }
                }
            }

            foreach (var assignment in _personAssignments) {
                if (assignment.AllNewPersons != _newPersons) {
                    assignment.AllNewPersons = _newPersons;
                }
            }

            Logger.Get().SafeExecuteWithWait(() => {
                foreach (var person in _householdPlan.CalcObject.AllPersons) {
                    if (!ALPersons.Contains(person)) {
                        ALPersons.Add(person);
                    }
                }
            });
        }

        public class StatisticsEntry {
            [CanBeNull]
            [UsedImplicitly]
            public string Name { get; set; }

            [CanBeNull]
            [UsedImplicitly]
            public string Value { get; set; }
        }
    }
}
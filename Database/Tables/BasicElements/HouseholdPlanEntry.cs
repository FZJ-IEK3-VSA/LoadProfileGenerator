using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.BasicElements {
    public enum TimeType {
        Hour,
        Day,
        Week,
        Month,
        Year
    }

    public class PersonAssignments : INotifyPropertyChanged {
        [CanBeNull] private Person _newPerson;

        [ItemCanBeNull] [CanBeNull] private ObservableCollection<Person> _newPersons;

        [CanBeNull] private Person _oldPerson;

        public PersonAssignments([CanBeNull] Person oldPerson, [CanBeNull] Person newPerson)
        {
            NewPerson = newPerson;
            OldPerson = oldPerson;
        }

        [ItemNotNull]
        [CanBeNull]
        public ObservableCollection<Person> AllNewPersons {
            get => _newPersons;
            set {
                _newPersons = value;
                OnPropertyChanged(nameof(AllNewPersons));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public Person NewPerson {
            get => _newPerson;
            set {
                _newPerson = value;
                OnPropertyChanged(nameof(NewPerson));
                Refresh?.Invoke();
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public Person OldPerson {
            get => _oldPerson;
            set {
                _oldPerson = value;
                OnPropertyChanged(nameof(OldPerson));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public Action Refresh { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([NotNull] string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HouseholdPlanEntry : DBBase, IComparable<HouseholdPlanEntry> {
        public enum ColorScheme {
            ForPlanning,
            ForModular
        }

        public const string TableName = "tblHouseholdPlanEntries";
        private readonly int _householdPlanID;
        [ItemNotNull] [NotNull] private readonly ObservableCollection<HouseholdTrait> _traits = new ObservableCollection<HouseholdTrait>();

        [CanBeNull] private ICalcObject _calcObject;

        [NotNull] private string _existingTraits;

        [CanBeNull] private Person _person;

        private ColorScheme _selectedColorScheme = ColorScheme.ForPlanning;

        [CanBeNull] private AffordanceTag _tag;

        private double _timeCount;
        private double _times;
        private TimeType _timeType;

        public HouseholdPlanEntry([NotNull] string name, int householdPlanID, [CanBeNull] AffordanceTag tag, [CanBeNull] Person person, double times, double timeCount, TimeType timeType,
                                  [NotNull] string connectionString, [CanBeNull] int? pID, [CanBeNull] ICalcObject calcObject, [NotNull] StrGuid guid) : base(name, pID, TableName, connectionString,
            guid)
        {
            _householdPlanID = householdPlanID;
            _tag = tag;
            _times = times;
            _timeCount = timeCount;
            _timeType = timeType;
            _person = person;
            _calcObject = calcObject;
            TypeDescription = "Household Plan Entry";
            AffordanceCategory = string.Empty;
            _existingTraits = string.Empty;
            foreach (TimeType type in Enum.GetValues(typeof(TimeType))) {
                TimeTypes.Add(type);
            }
        }

        [NotNull]
        public string AffordanceCategory { get; set; }

        [ItemNotNull]
        [CanBeNull]
        [UsedImplicitly]
        public ObservableCollection<DeviceAction> AllDeviceActions { get; set; }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseholdTrait> AllTraits { get; set; } = new ObservableCollection<HouseholdTrait>();

        [ItemNotNull]
        [CanBeNull]
        [UsedImplicitly]
        public ObservableCollection<PersonAssignments> Assignments { get; set; }

        [CanBeNull]
        [UsedImplicitly]
        public AffordanceTaggingSet Ats { get; set; }

        [NotNull]
        [UsedImplicitly]
        public string AvailableInHousehold {
            get {
                if (Ats == null) {
                    return string.Empty;
                }

                var availableAffordances = new List<Affordance>();
                if (_calcObject is ModularHousehold chh) {
                    foreach (var trait in chh.Traits) {
                        if (trait.DstPerson == _person) {
                            availableAffordances.AddRange(trait.HouseholdTrait.CollectAffordances(true));
                        }
                    }
                }

                foreach (var at in Ats.Entries) {
                    if (at.Tag == _tag) {
                        var aff = at.Affordance;
                        if (availableAffordances.Contains(aff)) {
                            return "x";
                        }
                    }
                }

                return string.Empty;
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string AverageEnergyPerActivation {
            get {
                var energyUses = CollectEnergyUses(AllDeviceActions);
                var builder = new StringBuilder();
                foreach (var energyUse in energyUses) {
                    if (energyUse.Key.Name.ToUpperInvariant() != "NONE") {
                        builder.Append(energyUse.Key.Name + " " + energyUse.Value.ToString("N2", CultureInfo.CurrentCulture) + " " + energyUse.Key.UnitOfSum + "; ");
                    }
                }

                var s = builder.ToString();
                if (s.Length > 0) {
                    s = s.Substring(0, s.Length - 1);
                }

                return s;
            }
        }

        public TimeSpan AverageTimePerActivation {
            get {
                double totalSeconds = 0;
                double count = 0;
                if (Ats == null) {
                    return new TimeSpan(0);
                }

                foreach (var entry in Ats.Entries) {
                    if (entry.Tag == Tag && entry.Affordance?.PersonProfile != null) {
                        totalSeconds += entry.Affordance.PersonProfile.Duration.TotalSeconds;
                        count++;
                    }
                }

                double avgseconds = 0;
                if (count > 0) {
                    avgseconds = totalSeconds / count;
                }

                var ts = TimeSpan.FromSeconds(avgseconds);
                return ts;
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string BackColor {
            get {
                switch (SelectedColorScheme) {
                    case ColorScheme.ForPlanning: {
                        var available = AvailableInHousehold;
                        var desvals = DesireValues.Length;
                        if (available == "x" && desvals > 0) {
                            if (TotalActivations > 0) {
                                return "LightGreen";
                            }

                            return "LightRed";
                        }

                        if (TotalActivations > 0 && desvals == 0) {
                            return "LightBlue";
                        }

                        if (available == "x" && desvals == 0) {
                            return "Yellow";
                        }

                        return string.Empty;
                    }
                    case ColorScheme.ForModular: {
                        var available = AvailableInHousehold;
                        var desvals = DesireValues.Length;
                        if (available == "x" && desvals > 0) {
                            if (ExistingTraits.Length > 0) {
                                return "LightGreen";
                            }

                            return "LightRed";
                        }

                        if (ExistingTraits.Length > 0) {
                            return "LightBlue";
                        }

                        return string.Empty;
                    }
                    default: return string.Empty;
                }
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public ICalcObject CalcObject {
            get => _calcObject;
            set {
                _calcObject = value;
                OnPropertyChangedNoUpdate(nameof(CalcObject));
            }
        }

        [NotNull]
        public string Desires {
            get {
                if (Ats == null) {
                    return string.Empty;
                }

                var s = string.Empty;
                foreach (var at in Ats.Entries) {
                    if (at.Tag == _tag) {
                        var aff = at.Affordance;
                        var builder = new StringBuilder();
                        builder.Append(s);
                        if (aff == null) {
                            throw new LPGException("Affordance was null");
                        }

                        foreach (var desire in aff.AffordanceDesires) {
                            builder.Append(desire.Desire.Name + "; ");
                        }

                        s = builder.ToString();
                    }
                }

                return s;
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string DesireValues {
            get {
                if (Ats == null) {
                    return string.Empty;
                }

                var affdesires = new List<Desire>();

                foreach (var at in Ats.Entries) {
                    if (at.Tag == _tag) {
                        var aff = at.Affordance;
                        if (aff == null) {
                            throw new LPGException("Affordance was null");
                        }

                        foreach (var desire in aff.AffordanceDesires) {
                            affdesires.Add(desire.Desire);
                        }
                    }
                }

                var builder = new StringBuilder();
                if (_calcObject is ModularHousehold chh) {
                    foreach (var trait in chh.Traits) {
                        if (trait.IsValidPerson(Person)) {
                            foreach (var desire in trait.HouseholdTrait.Desires) {
                                if (affdesires.Contains(desire.Desire)) {
                                    var ts = TimeSpan.FromHours((double)desire.DecayTime);
                                    builder.Append(desire.Desire.Name + " " + ts + "; ");
                                }
                            }
                        }
                    }
                }

                return builder.ToString();
            }
        }

        [NotNull]
        [UsedImplicitly]
        public string ExistingTraits {
            get => _existingTraits;
            set {
                _existingTraits = value;
                OnPropertyChangedNoUpdate(nameof(ExistingTraits));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public HouseholdTrait FirstExistingTrait { get; set; }

        public int HouseholdPlanID => _householdPlanID;

        [CanBeNull]
        [UsedImplicitly]
        public Person Person {
            get => _person;
            set => SetValueWithNotify(value, ref _person);
        }

        [UsedImplicitly]
        public ColorScheme SelectedColorScheme {
            get => _selectedColorScheme;
            set {
                _selectedColorScheme = value;
                OnPropertyChangedNoUpdate(nameof(BackColor));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public HouseholdTrait SelectedTrait { get; set; }

        [CanBeNull]
        [UsedImplicitly]
        public AffordanceTag Tag {
            get => _tag;
            set => SetValueWithNotify(value, ref _tag);
        }

        [NotNull]
        [UsedImplicitly]
        public HouseholdPlanEntry ThisEntry => this;

        [UsedImplicitly]
        public double TimeCount {
            get => _timeCount;
            set {
                SetValueWithNotify(value, ref _timeCount, nameof(TimeCount));
                RefreshValues();
            }
        }

        [UsedImplicitly]
        public double Times {
            get => _times;
            set {
                SetValueWithNotify(value, ref _times, nameof(Times));
                RefreshValues();
            }
        }

        [UsedImplicitly]
        public TimeType TimeType {
            get => _timeType;
            set {
                SetValueWithNotify(value, ref _timeType, nameof(TimeType));
                RefreshValues();
            }
        }

        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeType> TimeTypes { get; } = new ObservableCollection<TimeType>();

        [NotNull]
        [UsedImplicitly]
        public string ToolTipText {
            get {
                var s = string.Empty;
                if (_person != null) {
                    s += "Person:" + _person.Name + Environment.NewLine + Environment.NewLine;
                }

                if (_tag != null) {
                    s += "Tag:" + _tag.Name + Environment.NewLine + Environment.NewLine;
                }

                s += "Average time per Activation:" + AverageTimePerActivation + Environment.NewLine + Environment.NewLine;
                s += "Average Energy per Activation:" + AverageEnergyPerActivation + Environment.NewLine + Environment.NewLine;
                s += "Total Time Consumption per Year:" + TotalTime + Environment.NewLine + Environment.NewLine;
                if (AvailableInHousehold == "x") {
                    s += "Available: yes" + Environment.NewLine + Environment.NewLine;
                }
                else {
                    s += "Available: no" + Environment.NewLine + Environment.NewLine;
                }

                s += "Desires filled by this affordance:" + Desires + Environment.NewLine + Environment.NewLine;
                s += "Desires of the person:" + DesireValues + Environment.NewLine + Environment.NewLine;
                return s;
            }
        }

        [UsedImplicitly]
        public int TotalActivations => CalculateTotalActivations();

        public TimeSpan TotalTime {
            get {
                var ts = AverageTimePerActivation;
                double totalActivations = TotalActivations;
                var totalSpan = new TimeSpan(0, 0, (int)(ts.TotalSeconds * totalActivations));
                return totalSpan;
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseholdTrait> Traits => _traits;

        public int CompareTo([CanBeNull] HouseholdPlanEntry other)
        {
            if (_tag == null || other?._tag == null) {
                return 0;
            }

            var result = string.Compare(_tag.Name, other._tag.Name, StringComparison.CurrentCultureIgnoreCase);
            return result;
        }

        [NotNull]
        public Dictionary<VLoadType, double> CollectTotalEnergyUses([NotNull] [ItemNotNull] ObservableCollection<DeviceAction> deviceActions)
        {
            double totalActivations = TotalActivations;
            var energyUses = CollectEnergyUses(deviceActions);
            var totalEnergyUses = new Dictionary<VLoadType, double>();
            foreach (var energyUse in energyUses) {
                if (energyUse.Key.Name.ToUpperInvariant() != "NONE") {
                    totalEnergyUses.Add(energyUse.Key, energyUse.Value * totalActivations);
                }
            }

            return totalEnergyUses;
        }

        public override int CompareTo([CanBeNull] object obj)
        {
            if (!(obj is AffordanceTaggingEntry other)) {
                return 0;
            }

            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<HouseholdPlanEntry> result, [NotNull] string connectionString, bool ignoreMissingTables,
                                            [ItemNotNull] [NotNull] ObservableCollection<AffordanceTag> tagsFromParent, [ItemNotNull] [NotNull] ObservableCollection<Person> persons)
        {
            var aic = new AllItemCollections(tagsFromParent, persons: persons);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        [NotNull]
        public string MakeHash()
        {
            var s = _tag?.Name + "#";
            s += Person?.Name + "#";
            s += Times + "#";
            s += TimeCount + "#";
            s += TimeType + "#";
            return s;
        }

        public void RefreshAllRelevantTraits([NotNull] ModularHousehold chh, bool refreshDropDown)
        {
            if (Ats == null) {
                return;
            }

            if (refreshDropDown) {
                var affdesires = new List<Desire>();
                var affordances = new List<Affordance>();
                // collect desires && affordances
                CollectDesiresAndAffordances(affdesires, affordances);
                // collect appropriate Traits
                foreach (var trait in AllTraits) {
                    var desireFound = FindDesires(affdesires, trait);
                    var affordanceFound = FindAffordances(affordances, trait);
                    if (desireFound && affordanceFound && trait.IsValidForPerson(Person)) {
                        if (trait.IsValidForHousehold(chh)) {
                            if (!_traits.Contains(trait)) {
                                Logger.Get().SafeExecuteWithWait(() => _traits.Add(trait));
                            }
                        }
                    }
                }

                if (SelectedTrait == null) {
                    if (_traits.Count > 0) {
                        SelectedTrait = _traits[0];
                    }
                }
                else {
                    OnPropertyChangedNoUpdate(nameof(SelectedTrait));
                }
            }

            ExistingTraits = string.Empty;
            if (_person != null) {
                var assign = new PersonAssignments(_person, _person);
                if (Assignments != null) {
                    assign = Assignments.FirstOrDefault(x => x.OldPerson == _person);
                }

                var builder = new StringBuilder();

                foreach (var trait in chh.Traits) {
                    if (_traits.Contains(trait.HouseholdTrait) && (trait.DstPerson == _person || trait.DstPerson != null && assign != null && trait.DstPerson == assign.NewPerson)) {
                        builder.Append(trait.HouseholdTrait.PrettyName + "; ");
                        FirstExistingTrait = trait.HouseholdTrait;
                    }

                    OnPropertyChangedNoUpdate(nameof(BackColor));
                }

                ExistingTraits = builder.ToString();
            }
        }

        public override string ToString() => Name;

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            if (_tag == null) {
                message = "Tag not found";
                return false;
            }

            if (_person == null) {
                message = "Person not found";
                return false;
            }

            message = "";
            return true;
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("HouseholdPlanID", _householdPlanID);
            if (_tag != null) {
                cmd.AddParameter("TagID", _tag.IntID);
            }

            if (_person != null) {
                cmd.AddParameter("PersonID", _person.IntID);
            }

            cmd.AddParameter("Times", _times);
            cmd.AddParameter("TimeCount", _timeCount);
            cmd.AddParameter("TimeType", _timeType);
        }

        [NotNull]
        private static HouseholdPlanEntry AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var householdPlanEntryID = dr.GetIntFromLong("ID");
            var householdPlanID = dr.GetIntFromLong("HouseholdPlanID");
            var personID = dr.GetIntFromLong("PersonID");
            var person = aic.Persons.FirstOrDefault(pers => pers.ID == personID);
            var tagID = dr.GetIntFromLong("TagID", false, ignoreMissingFields, -1);
            var tag = aic.AffordanceTags.FirstOrDefault(myTag => myTag.ID == tagID);
            var times = dr.GetDouble("Times", false, 0, ignoreMissingFields);
            var timeCount = dr.GetDouble("TimeCount", false, 0, ignoreMissingFields);
            var timetype = (TimeType)dr.GetIntFromLong("TimeType", false, ignoreMissingFields);
            var name = string.Empty;
            if (tag != null) {
                name = tag.Name;
            }

            var guid = GetGuid(dr, ignoreMissingFields);
            return new HouseholdPlanEntry(name, householdPlanID, tag, person, times, timeCount, timetype, connectionString, householdPlanEntryID, null, guid);
        }

        private int CalculateTotalActivations()
        {
            int unitsPerYear;
            switch (_timeType) {
                case TimeType.Hour:
                    unitsPerYear = 365 * 24;
                    break;
                case TimeType.Day:
                    unitsPerYear = 365;
                    break;
                case TimeType.Week:
                    unitsPerYear = 52;
                    break;
                case TimeType.Month:
                    unitsPerYear = 12;
                    break;
                case TimeType.Year:
                    unitsPerYear = 1;
                    break;
                default: throw new LPGException("Forgotten TimeType. Please report.");
            }

            var result = Times * unitsPerYear / TimeCount;
            return (int)result;
        }

        private void CollectDesiresAndAffordances([ItemNotNull] [NotNull] List<Desire> affdesires, [ItemNotNull] [NotNull] List<Affordance> affordances)
        {
            if (Ats == null) {
                return;
            }

            foreach (var at in Ats.Entries) {
                if (at.Tag == _tag) {
                    var aff = at.Affordance;
                    affordances.Add(aff);
                    if (aff == null) {
                        throw new LPGException("Affordance was null");
                    }

                    foreach (var desire in aff.AffordanceDesires) {
                        affdesires.Add(desire.Desire);
                    }
                }
            }
        }

        [NotNull]
        private Dictionary<VLoadType, double> CollectEnergyUses([ItemNotNull] [CanBeNull] ObservableCollection<DeviceAction> allActions)
        {
            var energyUses = new Dictionary<VLoadType, double>();
            var energycounts = new Dictionary<VLoadType, int>();
            if (Ats == null) {
                return new Dictionary<VLoadType, double>();
            }

            foreach (var entry in Ats.Entries) {
                if (entry.Tag == Tag) {
                    if (entry.Affordance == null) {
                        throw new LPGException("Affordance was null");
                    }

                    var affEnergy = entry.Affordance.CalculateAverageEnergyUse(allActions);
                    foreach (var keyValuePair in affEnergy) {
                        if (!energyUses.ContainsKey(keyValuePair.Key)) {
                            energyUses.Add(keyValuePair.Key, 0);
                            energycounts.Add(keyValuePair.Key, 0);
                        }

                        energyUses[keyValuePair.Key] += keyValuePair.Value;
                        energycounts[keyValuePair.Key] += 1;
                    }
                }
            }

            var avg = new Dictionary<VLoadType, double>();
            foreach (var keyValuePair in energyUses) {
                avg.Add(keyValuePair.Key, energyUses[keyValuePair.Key] / energycounts[keyValuePair.Key]);
            }

            return avg;
        }

        private static bool FindAffordances([ItemNotNull] [NotNull] List<Affordance> affordances, [NotNull] HouseholdTrait trait)
        {
            var affordanceFound = false;
            foreach (var hhtLocation in trait.Locations) {
                foreach (var aff in hhtLocation.AffordanceLocations) {
                    if (affordances.Contains(aff.Affordance)) {
                        affordanceFound = true;
                        break;
                    }
                }
            }

            return affordanceFound;
        }

        private static bool FindDesires([ItemNotNull] [NotNull] List<Desire> affdesires, [NotNull] HouseholdTrait trait)
        {
            var desireFound = false;

            foreach (var desire in trait.Desires) {
                if (affdesires.Contains(desire.Desire)) {
                    desireFound = true;
                    break;
                }
            }

            return desireFound;
        }

        private void RefreshValues()
        {
            OnPropertyChanged(nameof(TotalActivations));
            OnPropertyChanged(nameof(TotalTime));
            OnPropertyChanged(nameof(BackColor));
        }
    }
}
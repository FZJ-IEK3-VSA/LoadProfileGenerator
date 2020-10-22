using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Automation.ResultFiles;
using CalculationController.Integrity;
using Common;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;
using LoadProfileGenerator.Views.Households;

namespace LoadProfileGenerator.Presenters.Households {
    public class HouseholdTemplatePresenter : PresenterBaseDBBase<HouseholdTemplateView> {
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                const int hash = 17;
                // Suitable nullity checks etc, of course :)
                return hash * 23 + TabHeaderPath.GetHashCode();
            }
        }

        internal void ImportFromCHH()
        {
            if (SelectedCHH == null) {
                Logger.Warning("Selected household was null.");
                return;
            }

            ThisTemplate.ImportExistingModularHouseholds(SelectedCHH);
        }

        internal void RefreshFilteredEntries([CanBeNull] string text)
        {
            if (string.IsNullOrEmpty(text)) {
                var entries = ThisTemplate.Entries.ToList();
                _filteredEntries.SynchronizeWithList(entries);
                return;
            }

            var fentries = ThisTemplate.Entries.Where(x => x.PrettyString.ToUpperInvariant().Contains(text.ToUpperInvariant())).ToList();
            _filteredEntries.SynchronizeWithList(fentries);
        }

        internal void RefreshHouseholds()
        {
            var chhs = Sim.ModularHouseholds.Items.Where(x => x.GeneratorID == ThisTemplate.IntID).ToList();
            _generatedHouseholds.SynchronizeWithList(chhs);
        }

        internal void RefreshPersons()
        {
            var templatepersons = _template.Persons.Select(x => x.Person).ToList();
            for (var i = 0; i < _persons.Count; i++) {
                var personEntry = _persons[i];
                if (!templatepersons.Contains(personEntry.Person)) {
                    _persons.RemoveAt(i);
                    i = 0;
                }
            }

            var personEntries = _persons.Select(x => x.Person).ToList();
            foreach (var templateperson in templatepersons) {
                if (!personEntries.Contains(templateperson)) {
                    _persons.Add(new PersonEntry(templateperson, false));
                }
            }
        }

        [ItemNotNull] [NotNull] private readonly ObservableCollection<HHTemplateEntry> _filteredEntries = new ObservableCollection<HHTemplateEntry>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<ModularHousehold> _generatedHouseholds =
            new ObservableCollection<ModularHousehold>();

        [ItemNotNull] [NotNull] private readonly ObservableCollection<PersonEntry> _persons = new ObservableCollection<PersonEntry>();

        [NotNull] private readonly HouseholdTemplate _template;
        [CanBeNull] private string _filterText;
        private int _maxCount;
        private int _maximumTraits;
        private int _minCount;

        [CanBeNull] private ModularHousehold _selectedCHH;

        [CanBeNull] private HHTemplateEntry _selectedEntry;

        [CanBeNull] private HouseholdTag _selectedHouseholdTag;

        [CanBeNull] private Person _selectedPerson;

        [CanBeNull] private TraitTag _selectedTag;

        [CanBeNull] private Vacation _selectedVacation;

        public HouseholdTemplatePresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] HouseholdTemplateView view,
                                          [NotNull] HouseholdTemplate template) : base(view, "ThisTemplate.HeaderString", template,
            applicationPresenter)
        {
            _template = template;
            RefreshHouseholds();
            RefreshPersons();
            RefreshFilteredEntries(_filterText);
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<DateBasedProfile> AllDateBasedProfiles => Sim.DateBasedProfiles.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ModularHousehold> AllModularHouseholds => Sim.ModularHouseholds.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Person> AllPersons => Sim.Persons.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TraitTag> AllTags => Sim.TraitTags.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HouseholdTag> AllTemplateTags => Sim.HouseholdTags.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Vacation> AllVacations => Sim.Vacations.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<LivingPatternTag> LivingPatternTags => Sim.LivingPatternTags.Items;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<HHTemplateEntry> FilteredEntries => _filteredEntries;

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

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<ModularHousehold> GeneratedHouseholds => _generatedHouseholds;

        [UsedImplicitly]
        public int MaxCount {
            get => _maxCount;
            set {
                _maxCount = value;
                OnPropertyChanged(nameof(MaxCount));
            }
        }

        [UsedImplicitly]
        public int MaximumTraits {
            get => _maximumTraits;
            set {
                _maximumTraits = value;
                OnPropertyChanged(nameof(MaximumTraits));
            }
        }

        [UsedImplicitly]
        public int MinCount {
            get => _minCount;
            set {
                _minCount = value;
                OnPropertyChanged(nameof(MinCount));
            }
        }

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<PersonEntry> Persons => _persons;

        [CanBeNull]
        [UsedImplicitly]
        public ModularHousehold SelectedCHH {
            get => _selectedCHH;
            set {
                if (Equals(value, _selectedCHH)) {
                    return;
                }

                _selectedCHH = value;
                OnPropertyChanged(nameof(SelectedCHH));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public HHTemplateEntry SelectedEntry {
            get => _selectedEntry;
            set {
                _selectedEntry = value;
                if (_selectedEntry != null) {
                    MaxCount = _selectedEntry.TraitCountMax;
                    MinCount = _selectedEntry.TraitCountMin;
                    SelectedTag = _selectedEntry.TraitTag;
                    TraitIsMandatory = _selectedEntry.IsMandatory;
                    foreach (var person in _selectedEntry.Persons) {
                        foreach (var entry in Persons) {
                            if (entry.Person == person.Person) {
                                entry.IsChecked = true;
                            }
                        }
                    }
                }
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public HouseholdTag SelectedHouseholdTag {
            get => _selectedHouseholdTag;
            set {
                if (Equals(value, _selectedHouseholdTag)) {
                    return;
                }

                _selectedHouseholdTag = value;
                OnPropertyChanged(nameof(SelectedHouseholdTag));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public Person SelectedPerson {
            get => _selectedPerson;
            set {
                if (Equals(value, _selectedPerson)) {
                    return;
                }

                _selectedPerson = value;
                OnPropertyChanged(nameof(SelectedPerson));
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public TraitTag SelectedTag {
            get => _selectedTag;
            set {
                _selectedTag = value;
                OnPropertyChanged(nameof(SelectedTag));
                if (_selectedTag != null) {
                    MaximumTraits = _selectedTag.CalculateUsedIns(Sim).Count;
                }
                else {
                    MaximumTraits = 0;
                }
            }
        }

        [CanBeNull]
        [UsedImplicitly]
        public Vacation SelectedVacation {
            get => _selectedVacation;
            set {
                if (Equals(value, _selectedVacation)) {
                    return;
                }

                _selectedVacation = value;
                OnPropertyChanged(nameof(SelectedVacation));
            }
        }

        [NotNull]
        [UsedImplicitly]
        public HouseholdTemplate ThisTemplate => _template;

        [UsedImplicitly]
        public bool VacationFromList {
            get {
                if (ThisTemplate.TemplateVacationType == TemplateVacationType.FromList) {
                    return true;
                }

                return false;
            }
            set {
                if (value) {
                    ThisTemplate.TemplateVacationType = TemplateVacationType.FromList;
                }

                //     else
                //       ThisTemplate.TemplateVacationType = (TemplateVacationType) (-1);
            }
        }

        [UsedImplicitly]
        public bool VacationRandomlyGenerated {
            get {
                if (ThisTemplate.TemplateVacationType == TemplateVacationType.RandomlyGenerated) {
                    return true;
                }

                return false;
            }
            set {
                if (value) {
                    ThisTemplate.TemplateVacationType = TemplateVacationType.RandomlyGenerated;
                }

                // else
                //    ThisTemplate.TemplateVacationType = (TemplateVacationType)(-1);
            }
        }

        public bool TraitIsMandatory { get; set; }

        internal void AddEntry([NotNull] TraitTag tag, int min, int max, [ItemNotNull] [NotNull] List<Person> persons, bool isMandatory)
        {
            _template.AddEntry(tag, min, max, persons, isMandatory);
        }

        public override void Close(bool saveToDB, bool removeLast = false)
        {
            if (saveToDB) {
                _template.SaveToDB();
            }

            ApplicationPresenter.CloseTab(this, removeLast);
        }

        internal void Delete()
        {
            Sim.HouseholdTemplates.DeleteItem(_template);
            Close(false);
        }

        internal void DeleteAllEntries()
        {
            while (ThisTemplate.Entries.Count > 0) {
                ThisTemplate.DeleteEntryFromDB(ThisTemplate.Entries[0]);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal void DeleteAllHouseholds()
        {
            RefreshHouseholds();
#pragma warning disable CC0022 // Should dispose object
            var task1 = new Task(() => {
                foreach (var household in _generatedHouseholds) {
                    Sim.ModularHouseholds.DeleteItem(household);
                }

                Logger.Get().SafeExecuteWithWait(RefreshHouseholds);
            });
#pragma warning restore CC0022 // Should dispose object
            task1.Start();
        }

        public override bool Equals(object obj) => obj is HouseholdTemplatePresenter presenter && presenter.ThisTemplate.Equals(_template);

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void GenerateHouseholds(bool generateSettlement)
        {
            if (ThisTemplate.Count == 0) {
                Logger.Error("Number of households to create was 0.");
                return;
            }

            try {
                SimIntegrityChecker.Run(Sim, CheckingOptions.Default());
            }
            catch (DataIntegrityException ex) {
                if (!Config.IsInHeadless) {
                    MessageWindowHandler.Mw.ShowDataIntegrityMessage(ex);
                }

                return;
            }
            catch (LPGException lex) {
                MessageWindowHandler.Mw.ShowDebugMessage(lex);
                return;
            }

            var task1 = new Task(() => {
                try {
                    var previouscount = _generatedHouseholds.Count;
                    ThisTemplate.GenerateHouseholds(Sim, generateSettlement, new List<STTraitLimit>(), new List<TraitTag>());
                    Logger.Get().SafeExecuteWithWait(RefreshHouseholds);
                    var newCount = _generatedHouseholds.Count;
                    var created = newCount - previouscount;
                    MessageWindowHandler.Mw.ShowInfoMessage("Created " + created + " households!", "Success");
                }
                catch (Exception ex) {
                    MessageWindowHandler.Mw.ShowDebugMessage(ex);
                    Logger.Exception(ex);
                }
            });
#pragma warning restore CC0022 // Should dispose object
            task1.Start();
        }

        public void MakeACopy()
        {
            var newTemplate = ThisTemplate.MakeCopy(Sim);
            ApplicationPresenter.OpenItem(newTemplate);
        }
    }
}
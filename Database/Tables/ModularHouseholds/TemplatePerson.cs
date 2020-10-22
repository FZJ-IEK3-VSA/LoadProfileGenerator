/*using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Common;
using Common.Enums;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class TemplatePerson : DBBaseElement {
        public const string TableName = "tblTemplatePerson";

        [ItemNotNull] [NotNull] private readonly ObservableCollection<TemplatePersonTrait> _traits =
            new ObservableCollection<TemplatePersonTrait>();

        private int _age;
        private int _averageSicknessDuration;

        [CanBeNull] private ModularHousehold _baseHousehold;

        [CanBeNull] private Person _basePerson;

        [NotNull] private string _description;
        private PermittedGender _gender;
        private int _sickDays;

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "basePerson")]
        public TemplatePerson([NotNull] string pName,[CanBeNull] int? id,[NotNull]  string description, [NotNull] string connectionString, int age,
            int averageSicknessDuration, PermittedGender gender, int sickDays,
            [CanBeNull] ModularHousehold baseHousehold, [CanBeNull] Person basePerson, StrGuid guid) : base(pName, TableName,
            connectionString, guid)
        {
            ID = id;
            TypeDescription = "Household Template";
            _description = description;
            _age = age;
            _averageSicknessDuration = averageSicknessDuration;
            _gender = gender;
            _sickDays = sickDays;
            _baseHousehold = baseHousehold;
            _basePerson = basePerson;
        }

        public int Age {
            get => _age;
            set => SetValueWithNotify(value, ref _age, nameof(Age));
        }

        [UsedImplicitly]
        public int AverageSicknessDuration {
            get => _averageSicknessDuration;
            set => SetValueWithNotify(value, ref _averageSicknessDuration, nameof(AverageSicknessDuration));
        }

        [CanBeNull]
        public ModularHousehold BaseHousehold {
            get => _baseHousehold;
            set => SetValueWithNotify(value, ref _baseHousehold, true, nameof(BaseHousehold));
        }

        [CanBeNull]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "BasePerson")]
        [UsedImplicitly]
        public Person BasePerson {
            get => _basePerson;
            set => SetValueWithNotify(value, ref _basePerson, true, nameof(BasePerson));
        }

        [NotNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [UsedImplicitly]
        public EnergyIntensityType EnergyIntensityType { get; set; } = EnergyIntensityType.Random;

        public PermittedGender Gender {
            get => _gender;
            set => SetValueWithNotify(value, ref _gender, nameof(Gender));
        }

        public int SickDays {
            get => _sickDays;
            set => SetValueWithNotify(value, ref _sickDays, nameof(SickDays));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TemplatePersonTrait> Traits => _traits;

        public void AddTrait([NotNull] HouseholdTrait trait)
        {
            if (_traits.Any(x => x.Trait == trait)) {
                Logger.Error("This trait was already added.");
                return;
            }
            var entry = new TemplatePersonTrait(null, IntID,
                "newEntry", ConnectionString, trait, System.Guid.NewGuid().ToStrGuid());
            _traits.Add(entry);
            entry.SaveToDB();
            _traits.Sort();
        }

        [NotNull]
        private static TemplatePerson AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var hhid = dr.GetIntFromLong("ID");
            var name = (string) dr["Name"];
            if (name == null) {
                name = "No name";
            }
            var description = dr.GetString("Description", false);
            var age = dr.GetIntFromLong("Age", false);
            var averageSicknessDuration = dr.GetIntFromLong("AverageSicknessDuration", false);
            var gender = (PermittedGender) dr.GetIntFromLong("Gender", false);
            var sickdays = dr.GetIntFromLong("Sickdays", false);
            var chhID = dr.GetNullableIntFromLong("BaseHouseholdID", false, ignoreMissingFields);
            var personID = dr.GetNullableIntFromLong("BasePersonID", false, ignoreMissingFields);
            ModularHousehold baseModularHousehold = null;
            if (chhID != null) {
                baseModularHousehold = aic.ModularHouseholds.FirstOrDefault(x => x.ID == chhID);
            }
            Person basePerson = null;
            if (personID != null) {
                basePerson = aic.Persons.FirstOrDefault(x => x.ID == personID);
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var chh = new TemplatePerson(name, hhid, description, connectionString, age,
                averageSicknessDuration, gender,
                sickdays, baseModularHousehold, basePerson, guid);
            return chh;
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new
            TemplatePerson(FindNewName(isNameTaken, "New  Template Person "), null, "(no description)",
                connectionString, 30, 3, PermittedGender.Male,
                10, null, null, System.Guid.NewGuid().ToStrGuid());

        public override void DeleteFromDB()
        {
            foreach (var entry in _traits) {
                entry.DeleteFromDB();
            }
            base.DeleteFromDB();
        }

        public void DeleteTraitFromDB([NotNull] TemplatePersonTrait entry)
        {
            entry.DeleteFromDB();
            _traits.Remove(entry);
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase ImportFromItem([NotNull] TemplatePerson item,  [NotNull] Simulator dstSim)
        {
            if (item.BaseHousehold != null) {
                GetItemFromListByName(dstSim.ModularHouseholds.Items, item.BaseHousehold.Name);
            }

            Person basePerson = null;
            if (item.BasePerson != null) {
                basePerson = GetItemFromListByName(dstSim.Persons.Items, item.BasePerson.Name);
            }
            var templatePerson = new TemplatePerson(item.Name, null, item.Description,dstSim.ConnectionString,
                item.Age, item.AverageSicknessDuration, item.Gender,
                item.SickDays, null, basePerson, item.Guid);
            templatePerson.SaveToDB();
            foreach (var entry in item.Traits) {
                {
                    var trait = GetItemFromListByName(dstSim.HouseholdTraits.Items, entry.Trait.Name);
                    if (trait == null) {
                        Logger.Error("While importing, could not find trait. Skipping.");
                        continue;
                    }
                    var newEntry =
                        new TemplatePersonTrait(null, templatePerson.IntID,
                            "no name", dstSim.ConnectionString, trait,entry.Guid);
                    newEntry.SaveToDB();
                    templatePerson.Traits.Add(newEntry);
                }
            }

            templatePerson.SaveToDB();
            return templatePerson;
        }

        private static bool IsCorrectTemplatePersonTraitParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hhgEntry = (TemplatePersonTrait) child;

            if (parent.ID == hhgEntry.TemplatePersonID) {
                var chh = (TemplatePerson) parent;
                chh.Traits.Add(hhgEntry);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TemplatePerson> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<HouseholdTrait> householdTraits, bool ignoreMissingTables,
            [ItemNotNull] [NotNull] ObservableCollection<ModularHousehold> modularHouseholds, [ItemNotNull] [NotNull] ObservableCollection<Person> persons)
        {
            var aic = new AllItemCollections(modularHouseholds: modularHouseholds, persons: persons);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);

            var entries = new ObservableCollection<TemplatePersonTrait>();
            TemplatePersonTrait.LoadFromDatabase(entries, connectionString, householdTraits, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(entries), IsCorrectTemplatePersonTraitParent,
                ignoreMissingTables);

            // sort
            foreach (var tp in result) {
                tp.Traits.Sort();
            }
            // cleanup
            result.Sort();
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var entry in _traits) {
                entry.SaveToDB();
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", Name);
            cmd.AddParameter("Age", _age);
            cmd.AddParameter("AverageSicknessDuration", _averageSicknessDuration);
            cmd.AddParameter("Gender", _gender);
            cmd.AddParameter("SickDays", _sickDays);
            cmd.AddParameter("Description", _description);
            if (_baseHousehold != null) {
                cmd.AddParameter("BaseHouseholdID", _baseHousehold.IntID);
            }
            else {
                cmd.AddParameter("BaseHouseholdID", DBNull.Value);
            }
            if (_basePerson != null) {
                cmd.AddParameter("BasePersonID", _basePerson.IntID);
            }
            else {
                cmd.AddParameter("BasePersonID", DBNull.Value);
            }
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim) =>
        ImportFromItem((TemplatePerson)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}*/
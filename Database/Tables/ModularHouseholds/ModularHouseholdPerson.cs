using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class ModularHouseholdPerson : DBBase,
        IComparable<ModularHouseholdPerson>, IJSonSubElement<ModularHouseholdPerson.JsonModularHouseholdPerson>
    {

        public class JsonModularHouseholdPerson : IGuidObject
        {
            public JsonModularHouseholdPerson([NotNull] JsonReference person, [CanBeNull] JsonReference traitTag, StrGuid guid)
            {
                Person = person;
                TraitTag = traitTag;
                Guid = guid;
            }

            /// <summary>
            /// for json only
            /// </summary>
            [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
            public JsonModularHouseholdPerson()
            {
            }

            [NotNull]
            public JsonReference Person { get; set; }

            [CanBeNull]
            public JsonReference TraitTag { get; set; }
            public StrGuid Guid { get; set; }

        }
        public JsonModularHouseholdPerson GetJson()
        {
            var p = new JsonModularHouseholdPerson(Person.GetJsonReference(), TraitTag?.GetJsonReference(), Guid);
            return p;
        }

        public const string ParentIDField = "ParentHouseholdID";
        public const string TableName = "tblCHHPersons";
        [CanBeNull]
        private readonly int? _modularHouseholdID;
        [CanBeNull] private  TraitTag _traitTag;
        [CanBeNull] private  Person _person;

        public ModularHouseholdPerson([CanBeNull]int? pID, [CanBeNull]int? modularHouseholdID, [NotNull] string name, [NotNull] string connectionString,
            [CanBeNull] Person person,[CanBeNull] TraitTag traitTag, StrGuid guid ) : base(name, TableName, connectionString,
            guid) {
            _person = person;
            ID = pID;
            _traitTag = traitTag;
            _modularHouseholdID = modularHouseholdID;
            TypeDescription = "Modular Household Person";
        }
        [CanBeNull]
        public int? ModularHouseholdID => _modularHouseholdID;

        [NotNull]
        [UsedImplicitly]
        public new string Name => ToString();

        [NotNull]
        public Person Person => _person ?? throw new InvalidOperationException();
        [CanBeNull]
        public TraitTag TraitTag => _traitTag;

        public int CompareTo([CanBeNull] ModularHouseholdPerson other) {
            if (other == null) {
                return 0;
            }
            if (other._person == null) {
                return 1;
            }
            if (_person == null) {
                return -1;
            }
            return string.Compare(Person.Name, other.Person.Name, StringComparison.Ordinal);
        }

        [NotNull]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static ModularHouseholdPerson AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var modularHouseholdID = dr.GetIntFromLong("ParentHouseholdID", false, ignoreMissingFields, -1);
            if (modularHouseholdID == -1 && ignoreMissingFields) {
                modularHouseholdID = dr.GetIntFromLong("CombinedHouseholdID", false, ignoreMissingFields, -1);
            }
            var personID = dr.GetIntFromLong("PersonID", false, ignoreMissingFields, -1);
            var p = aic.Persons.FirstOrDefault(myPerson => myPerson.ID == personID);
            var name = "(no name)";
            if (p != null) {
                name = p.PrettyName;
            }
            var traitTagID = dr.GetIntFromLong("TraitTagID", false, ignoreMissingFields, -1);
            var traitTag = aic.TraitTags.FirstOrDefault(x => x.ID == traitTagID);
            var guid = GetGuid(dr, ignoreMissingFields);
            var chht = new ModularHouseholdPerson(id, modularHouseholdID, name, connectionString, p,traitTag,
                guid);
            return chht;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_person == null) {
                message = "Person not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<ModularHouseholdPerson> result,
            [NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [NotNull] ObservableCollection<Person> persons,
            [NotNull] [ItemNotNull] ObservableCollection<TraitTag> traitTags) {
            var aic = new AllItemCollections(persons: persons ,traitTags:traitTags);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_modularHouseholdID != null) {
                cmd.AddParameter("ParentHouseholdID", _modularHouseholdID);
            }
            if (_person != null) {
                cmd.AddParameter("PersonID", _person.IntID);
            }
            if(_traitTag !=null) {
                cmd.AddParameter("TraitTagID",_traitTag.IntID);
            }
        }

        public void SynchronizeDataFromJson(JsonModularHouseholdPerson json, Simulator sim)
        {
           _person = sim.Persons.FindByGuid(json.Person.Guid);
            if (_person == null)
            {
                throw new LPGException("Person with the guid " + json.Person.Guid + " and the name " + json.Person.Name + " could not be found in the database.");
            }

            if (json.TraitTag != null)
            {
                _traitTag = sim.TraitTags.FindByGuid(json.TraitTag.Guid);
            }
        }

        public override string ToString() {
            if (_person != null) {
                return _person.PrettyName;
            }
            return "(no name)";
        }

        public StrGuid RelevantGuid => Guid;
    }
}
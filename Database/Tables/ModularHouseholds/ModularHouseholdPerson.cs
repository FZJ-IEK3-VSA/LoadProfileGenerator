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
            public JsonModularHouseholdPerson([JetBrains.Annotations.NotNull] JsonReference person, [CanBeNull] JsonReference livingPatternTag, StrGuid guid)
            {
                Person = person;
                LivingPatternTag = livingPatternTag;
                Guid = guid;
            }

            /// <summary>
            /// for json only
            /// </summary>
            [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
            public JsonModularHouseholdPerson()
            {
            }

            [JetBrains.Annotations.NotNull]
            public JsonReference Person { get; set; }

            //[CanBeNull]
            //public JsonReference TraitTag { get; set; }
            public StrGuid Guid { get; set; }
            public JsonReference LivingPatternTag { get; set; }
        }
        public JsonModularHouseholdPerson GetJson()
        {
            var p = new JsonModularHouseholdPerson(Person.GetJsonReference(), LivingPatternTag?.GetJsonReference(), Guid);
            return p;
        }

        public const string ParentIDField = "ParentHouseholdID";
        public const string TableName = "tblCHHPersons";
        [CanBeNull]
        private readonly int? _modularHouseholdID;
        //[CanBeNull] private  TraitTag _traitTag;
        [CanBeNull] private LivingPatternTag _livingPatternTag;
        [CanBeNull] private  Person _person;

        public ModularHouseholdPerson([CanBeNull]int? pID, [CanBeNull]int? modularHouseholdID, [JetBrains.Annotations.NotNull] string name, [JetBrains.Annotations.NotNull] string connectionString,
            [CanBeNull] Person person, [CanBeNull] LivingPatternTag livingPatternTag, StrGuid guid ) : base(name, TableName, connectionString,
            guid) {
            _person = person;
            ID = pID;
            //_traitTag = traitTag;
            _livingPatternTag = livingPatternTag;
            _modularHouseholdID = modularHouseholdID;
            TypeDescription = "Modular Household Person";
        }
        [CanBeNull]
        public int? ModularHouseholdID => _modularHouseholdID;

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public new string Name => ToString();

        [JetBrains.Annotations.NotNull]
        public Person Person => _person ?? throw new InvalidOperationException();
        //[CanBeNull]
        //public TraitTag TraitTag => _traitTag;

        [CanBeNull]
        public LivingPatternTag LivingPatternTag {
            get => _livingPatternTag;
            set => SetValueWithNotify(value, ref _livingPatternTag,true,nameof(LivingPatternTag));
        }

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

        [JetBrains.Annotations.NotNull]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static ModularHouseholdPerson AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic) {
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
            //var traitTagID = dr.GetIntFromLong("TraitTagID", false, ignoreMissingFields, -1);
            //var traitTag = aic.TraitTags.FirstOrDefault(x => x.ID == traitTagID);

            var livingPatternTagID = dr.GetIntFromLong("LivingPatternTagID", false, ignoreMissingFields, -1);
            var livingPatternTag = aic.LivingPatternTags.FirstOrDefault(x => x.ID == livingPatternTagID);
            var guid = GetGuid(dr, ignoreMissingFields);
            var chht = new ModularHouseholdPerson(id, modularHouseholdID, name, connectionString, p,
                livingPatternTag, guid);
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

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<ModularHouseholdPerson> result,
            [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Person> persons,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<TraitTag> traitTags, [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<LivingPatternTag> livingPatternTags) {
            var aic = new AllItemCollections(persons: persons ,traitTags:traitTags, livingPatternTags:livingPatternTags);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_modularHouseholdID != null) {
                cmd.AddParameter("ParentHouseholdID", _modularHouseholdID);
            }
            if (_person != null) {
                cmd.AddParameter("PersonID", _person.IntID);
            }
            //if(_traitTag !=null) {
            //    cmd.AddParameter("TraitTagID",_traitTag.IntID);
            //}
            if (_livingPatternTag != null)
            {
                cmd.AddParameter("LivingPatternTagId", _livingPatternTag.IntID);
            }
        }

        public void SynchronizeDataFromJson(JsonModularHouseholdPerson json, Simulator sim)
        {
           _person = sim.Persons.FindByGuid(json.Person.Guid);
            if (_person == null)
            {
                throw new LPGException("Person with the guid " + json.Person.Guid + " and the name " + json.Person.Name + " could not be found in the database.");
            }

            if (json.LivingPatternTag != null)
            {
                _livingPatternTag = sim.LivingPatternTags.FindByGuid(json.LivingPatternTag.Guid);
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
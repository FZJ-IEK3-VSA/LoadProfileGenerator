using System;
using System.Collections.ObjectModel;
using System.Linq;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class HHTemplateEntryPerson : DBBase {
        public const string TableName = "tblHHGEntryPerson";
        private readonly int _hhTemplateEntryID;

        [CanBeNull] private readonly Person _person;

        public HHTemplateEntryPerson([CanBeNull]int? pID, [CanBeNull] Person pPerson, int hhTemplateEntryID, [NotNull] string name,
            [NotNull] string connectionString, [NotNull] string guid)
            : base(name, TableName, connectionString, guid) {
            ID = pID;
            _person = pPerson;
            _hhTemplateEntryID = hhTemplateEntryID;
            TypeDescription = "Household Generator Entry Person";
        }
        [CanBeNull]
        public int? HHTemplateEntryID => _hhTemplateEntryID;

        [NotNull]
        public Person Person => _person ?? throw new InvalidOperationException();

        [NotNull]
        private static HHTemplateEntryPerson AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic) {
            var hhpID = dr.GetIntFromLong("ID");
            var personID = dr.GetIntFromLong("PersonID", ignoreMissingField: ignoreMissingFields);
            var householdID = dr.GetIntFromLong("HHGEntryID");
            var p = aic.Persons.FirstOrDefault(mypers => mypers.ID == personID);
            var name = "(no name)";
            if (p != null) {
                name = p.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);

            var hhp = new HHTemplateEntryPerson(hhpID, p, householdID,
                name, connectionString, guid);
            return hhp;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            if (_person == null) {
                message = "Person not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<HHTemplateEntryPerson> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<Person> persons, bool ignoreMissingTables) {
            var aic = new AllItemCollections(persons: persons);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters([NotNull] Command cmd) {
            if (_person != null) {
                cmd.AddParameter("PersonID", _person.IntID);
            }
            cmd.AddParameter("HHGEntryID", _hhTemplateEntryID);
        }

        public override string ToString() => Person.Name;
    }
}
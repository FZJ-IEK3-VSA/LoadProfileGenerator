using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class HHTemplateEntry : DBBase, IComparable<HHTemplateEntry>, IJSonSubElement<HHTemplateEntry.JsonDto>
    {

        public StrGuid RelevantGuid => Guid;
        public class JsonDto : IGuidObject
        {
            public JsonDto([CanBeNull] JsonReference traitTagReference, StrGuid guid, int traitCountMax, int traitCountMin, string name, bool isMandatory)
            {
                TraitTagReference = traitTagReference;
                Guid = guid;
                IsMandatory = isMandatory;
                TraitCountMax = traitCountMax;
                TraitCountMin = traitCountMin;
                Name = name;
            }

            [CanBeNull]
            public JsonReference TraitTagReference { get; set; }
            public StrGuid Guid { get; set; }

            [JetBrains.Annotations.NotNull]
            [ItemNotNull]
            public List<JsonReference> Persons { get; set; } = new List<JsonReference>();

            public int TraitCountMax { get; set; }

            public int TraitCountMin { get; set; }
            public bool IsMandatory { get; set; }
            public string Name { get; set; }
        }
        public JsonDto GetJson()
        {
            JsonDto hhte = new JsonDto(TraitTag.GetJsonReference(), Guid,
                TraitCountMax, TraitCountMin,Name, IsMandatory) {Persons = new List<JsonReference>()};
            foreach (var person in Persons)
            {
                hhte.Persons.Add(person.Person.GetJsonReference());
            }

            return hhte;
        }

        public const string TableName = "tblHHGEntry";
        [CanBeNull]
        private readonly int? _householdTemplateId;
        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<HHTemplateEntryPerson> _persons;

        [CanBeNull] private TraitTag _tag;

        private int _traitCountMax;
        private int _traitCountMin;
        private bool _isMandatory;

        public HHTemplateEntry([CanBeNull]int? pID, [CanBeNull] int? householdTemplateId, [JetBrains.Annotations.NotNull] string name, [JetBrains.Annotations.NotNull] string connectionString,
            [CanBeNull] TraitTag tag, int traitCountMin, int traitCountMax, [JetBrains.Annotations.NotNull] StrGuid guid, bool isMandatory)
            : base(name, TableName, connectionString, guid)
        {
            _householdTemplateId = householdTemplateId;
            _traitCountMin = traitCountMin;
            _traitCountMax = traitCountMax;
            _persons = new ObservableCollection<HHTemplateEntryPerson>();
            _isMandatory = isMandatory;
            _tag = tag;
            ID = pID;
            TypeDescription = "Household Template Entry";
        }
        [CanBeNull]
        public int? HouseholdTemplateID => _householdTemplateId;

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<HHTemplateEntryPerson> Persons => _persons;

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string PersonString {
            get {
                var builder = new StringBuilder();
                foreach (var person in _persons) {
                    builder.Append(person.Person.PrettyName).Append(", ");
                }
                var persons = builder.ToString();
                if (persons.Length > 2) {
                    persons = persons.Substring(0, persons.Length - 2);
                }
                return persons;
            }
        }

        [JetBrains.Annotations.NotNull]
        public string PrettyString {
            get {
                var persons = new StringBuilder();
                foreach (var person in _persons) {
                    persons.Append(person.Person.PrettyName).Append(", ");
                }
                var personsstr = persons.ToString();
                if (persons.Length > 2) {
                    personsstr = personsstr.Substring(0, personsstr.Length - 2);
                }
                var tag = "(no tag)";
                if (_tag != null) {
                    tag = _tag.Name;
                }
                return "Add to " + personsstr + " between " + _traitCountMin + " and " + _traitCountMax +
                       " traits with the tag " + tag;
            }
        }

        public int TraitCountMax {
            get => _traitCountMax;
            set => SetValueWithNotify(value, ref _traitCountMax);
        }

        public int TraitCountMin {
            get => _traitCountMin;
            set => SetValueWithNotify(value,ref _traitCountMin);
        }

        [JetBrains.Annotations.NotNull]
        public TraitTag TraitTag {
            get => _tag ?? throw new InvalidOperationException();
            set => SetValueWithNotify(value,ref _tag);
        }

        public bool IsMandatory {
            get => _isMandatory;
            set => SetValueWithNotify(value, ref _isMandatory);
        }

        public int CompareTo([CanBeNull] HHTemplateEntry other)
        {
            if (other == null) {
                return 0;
            }
            if (_tag != null && other._tag != null) {
                return string.Compare(_tag.Name, other.TraitTag.Name, StringComparison.Ordinal);
            }

            return string.Compare(PrettyString, other.PrettyString, StringComparison.Ordinal);
        }

        public override void DeleteFromDB()
        {
            foreach (HHTemplateEntryPerson person in Persons) {
                person.DeleteFromDB();
            }
            base.DeleteFromDB();
        }

        public void AddPerson([JetBrains.Annotations.NotNull] Person p)
        {
            var hhgep = new HHTemplateEntryPerson(null, p, IntID, p.Name, ConnectionString, System.Guid.NewGuid().ToStrGuid());
            hhgep.SaveToDB();
            _persons.Add(hhgep);
        }

        [JetBrains.Annotations.NotNull]
        private static HHTemplateEntry AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var hhgID = dr.GetIntFromLong("HouseholdGeneratorID");
            var minCount = dr.GetIntFromLong("TraitCountMin");
            var maxCount = dr.GetIntFromLong("TraitCountMax");
            var tagID = dr.GetNullableIntFromLong("TraitTagID", false, ignoreMissingFields);
            var isMandatory = dr.GetBool("IsMandatory", false, false, ignoreMissingFields);
            var traitTag = aic.TraitTags.FirstOrDefault(mytrait => mytrait.ID == tagID);
            var name = "(no name)" + hhgID + minCount + maxCount + tagID;
            var guid = GetGuid(dr, ignoreMissingFields);

            var chht = new HHTemplateEntry(id, hhgID, name, connectionString,
                traitTag, minCount, maxCount, guid, isMandatory);
            return chht;
        }

        public override int CompareTo(BasicElement other)
        {
            if (other is HHTemplateEntry othr)
            {
                return string.Compare(PrettyString, othr.PrettyString, StringComparison.Ordinal);
            }
            return base.CompareTo(other);
        }

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public override int CompareTo(object obj)
        {
            if (obj is HHTemplateEntry othr)
            {
                return string.Compare(PrettyString, othr.PrettyString, StringComparison.Ordinal);
            }
            return base.CompareTo(obj);
        }

        private static bool IsCorrectHHTemplateEntryParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hhgEntry = (HHTemplateEntryPerson) child;
            if (parent.ID == hhgEntry.HHTemplateEntryID) {
                var chh = (HHTemplateEntry) parent;
                chh._persons.Add(hhgEntry);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_tag == null) {
                message = "Tag not found";
                return false;
            }
            message = "";
            return true;
        }
        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var person in _persons)
            {
                person.SaveToDB();
            }
        }
        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HHTemplateEntry> result, [JetBrains.Annotations.NotNull] string connectionString,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TraitTag> traitTags, bool ignoreMissingTables,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Person> allpersons)
        {
            var aic = new AllItemCollections(traitTags: traitTags);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);

            var persons = new ObservableCollection<HHTemplateEntryPerson>();
            HHTemplateEntryPerson.LoadFromDatabase(persons, connectionString, allpersons, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(persons), IsCorrectHHTemplateEntryParent,
                ignoreMissingTables);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (_tag != null) {
                cmd.AddParameter("TraitTagID", _tag.IntID);
            }
            cmd.AddParameter("TraitCountMin", _traitCountMin);
            cmd.AddParameter("TraitCountMax", _traitCountMax);
            cmd.AddParameter("IsMandatory", _isMandatory);
            if (_householdTemplateId != null) {
                cmd.AddParameter("HouseholdGeneratorID", _householdTemplateId);
            }
        }



        public override string ToString()
        {
            if (_tag != null) {
                return _tag.Name;
            }
            return "(no name)";
        }

        public void SynchronizeDataFromJson(JsonDto jtp, Simulator sim)
        {
            TraitCountMin = jtp.TraitCountMin;
            TraitCountMax = jtp.TraitCountMax;
            var tag = sim.TraitTags.FindByGuid(jtp.TraitTagReference?.Guid) ?? throw new LPGException("Could not find trait tag " + jtp.TraitTagReference);
            TraitTag = tag;
            List<Person> personsToAdd = new List<Person>();
            foreach (JsonReference personRef in jtp.Persons)
            {
                var person = sim.Persons.FindByGuid(personRef.Guid) ?? throw new LPGException("Person not found");
                personsToAdd.Add(person);
            }

            var currentPersonGuids = Persons.Select(x => x.Person.Guid).ToList();
            foreach (Person person in personsToAdd) {
                if (!currentPersonGuids.Contains(person.Guid)) {
                    AddPerson(person);
                }
            }

            var targetPersonGuids = personsToAdd.Select(x => x.Guid).ToList();
            var personsToDelete = new List<HHTemplateEntryPerson>();
            foreach (HHTemplateEntryPerson hhTemplateEntryPerson in Persons) {
                if (!targetPersonGuids.Contains(hhTemplateEntryPerson.Person.Guid)) {
                    personsToDelete.Add(hhTemplateEntryPerson);
                }
            }

            foreach (HHTemplateEntryPerson person in personsToDelete) {
                Persons.Remove(person);
                person.DeleteFromDB();
            }
        }

    }
}
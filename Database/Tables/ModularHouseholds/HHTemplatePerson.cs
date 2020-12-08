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
    public class HHTemplatePerson : DBBase, IJSonSubElement<HHTemplatePerson.JsonDto>
    {
        private const string TableName = "tblHHTemplatePerson";
        private readonly int _hhTemplateID;
        [CanBeNull]
        private readonly TraitTag _livingPattern;
        private LivingPatternTag _livingPatternTag;

        [CanBeNull] private  Person _person;

        public class JsonDto : IGuidObject {
            public JsonDto(StrGuid guid, [CanBeNull] JsonReference personReference,
                           [CanBeNull] JsonReference livingPatternTraitTagReference, string name)
            {
                Guid = guid;
                PersonReference = personReference;
                LivingPatternTraitTagReference = livingPatternTraitTagReference;
                Name = name;
            }

            /// <summary>
            /// for json only
            /// </summary>
            [Obsolete("Only json")]
            [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
            public JsonDto()
            {
            }

            public StrGuid Guid { get; set; }
            [CanBeNull]
            public JsonReference PersonReference { get; set; }
            [CanBeNull]
            public JsonReference LivingPatternTraitTagReference { get; set; }

            public string Name { get; set; }
        }
        public void SynchronizeDataFromJson(JsonDto jtp, Simulator sim)
        {

            if (Person.Guid != jtp.PersonReference?.Guid) {
                Person = sim.Persons.FindByGuid(jtp.PersonReference?.Guid)
                         ?? throw new LPGException("Could not find a database entry for the person " + jtp.PersonReference);
            }

            if (LivingPatternTag?.Guid != jtp.LivingPatternTraitTagReference?.Guid) {
                LivingPatternTag = sim.LivingPatternTags.FindByGuid(jtp.LivingPatternTraitTagReference?.Guid) ??
                                throw new LPGException("Could not find a living pattern trait tag for " + jtp.LivingPatternTraitTagReference);
            }

            Guid = jtp.Guid;
            Name = jtp.Name;
        }

        public JsonDto GetJson()
        {
            JsonDto jtp = new JsonDto(Guid,  Person.GetJsonReference(), LivingPatternTag?.GetJsonReference(), Name) ;
            return jtp;
        }

        public HHTemplatePerson([CanBeNull]int? pID, [CanBeNull] Person pPerson, int hhTemplateID, [JetBrains.Annotations.NotNull] string name,
            [JetBrains.Annotations.NotNull] string connectionString,[CanBeNull] TraitTag livingPattern, LivingPatternTag livingPatternTag, StrGuid guid)
            : base(name, TableName, connectionString, guid) {
            ID = pID;
            _person = pPerson;
            _hhTemplateID = hhTemplateID;
            _livingPattern = livingPattern;
            _livingPatternTag = livingPatternTag;
            TypeDescription = "Household Template Person";
        }
        [CanBeNull]
        public int? HHTemplateID => _hhTemplateID;

        [JetBrains.Annotations.NotNull]
        public Person Person {
            get => _person ?? throw new InvalidOperationException();
            set => SetValueWithNotify(value, ref _person);
        }

        //[CanBeNull]
        //public TraitTag LivingPattern {
        //    get => _livingPattern;
        //    set => SetValueWithNotify(value, ref _livingPattern);
        //}

        [CanBeNull]
        public LivingPatternTag LivingPatternTag
        {
            get => _livingPatternTag;
            set => SetValueWithNotify(value, ref _livingPatternTag);
        }

        [JetBrains.Annotations.NotNull]
        private static HHTemplatePerson AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic) {
            var hhpID = dr.GetIntFromLong("ID");
            var personID = dr.GetIntFromLong("PersonID", ignoreMissingField: ignoreMissingFields);
            var templateID = dr.GetIntFromLong("HHTemplateID");
            var p = aic.Persons.FirstOrDefault(mypers => mypers.ID == personID);
            var name = "(no name)";
            if (p != null) {
                name = p.Name;
            }
            var traitTagID = dr.GetIntFromLong("LivingPatternID", false, ignoreMissingFields,-1);
            var traitTag = aic.TraitTags.FirstOrDefault(x => x.ID == traitTagID);
            var guid = GetGuid(dr, ignoreMissingFields);
            var livingpatternID = dr.GetIntFromLong("LivingPatternTagID", false, ignoreMissingFields);
            var livingPattern = aic.LivingPatternTags.FirstOrDefault(x => x.ID == livingpatternID);
            var hhp = new HHTemplatePerson(hhpID, p, templateID,
                name, connectionString,traitTag,livingPattern, guid);
            return hhp;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_person == null) {
                message = "Person is missing";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HHTemplatePerson> result, [JetBrains.Annotations.NotNull] string connectionString,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Person> persons, bool ignoreMissingTables,[ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TraitTag> traitTags,
                                            [ItemNotNull][JetBrains.Annotations.NotNull] ObservableCollection<LivingPatternTag> livingPatternTags) {
            var aic = new AllItemCollections(persons: persons,traitTags:traitTags, livingPatternTags:livingPatternTags);

            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_person != null) {
                cmd.AddParameter("PersonID", _person.IntID);
            }
            cmd.AddParameter("HHTemplateID", _hhTemplateID);
            if (_livingPattern != null) {
                cmd.AddParameter("LivingPatternID", _livingPattern.IntID);
            }
            if (_livingPatternTag != null)
            {
                cmd.AddParameter("LivingPatternTagID", _livingPatternTag.IntID);
            }
        }

        public override string ToString() => Person.Name;

        public StrGuid RelevantGuid => Guid;

    }
}
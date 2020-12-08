using System;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class HHTTag : DBBase, IJSonSubElement<JsonReference> {
        public const string ParentIDFieldName = "HouseholdTraitID";
        public const string TableName = "tblHHTTags";
        [CanBeNull]
        private readonly int? _householdTraitID;

        [CanBeNull] private readonly TraitTag _tag;

        public HHTTag([CanBeNull]int? pID, [CanBeNull] int? householdTraitID, [CanBeNull] TraitTag tag, [JetBrains.Annotations.NotNull] string connectionString, [JetBrains.Annotations.NotNull] string name, StrGuid guid)
            : base(name, TableName, connectionString,guid) {
            _tag = tag;
            ID = pID;
            _householdTraitID = householdTraitID;
            TypeDescription = "Household Trait Tag";
        }
        [CanBeNull]
        public int? HouseholdTraitID => _householdTraitID;

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
#pragma warning disable S4015 // Inherited member visibility should not be decreased
        public new string Name {
#pragma warning restore S4015 // Inherited member visibility should not be decreased
            get {
                if (_tag == null) {
                    return "(no name)";
                }
                return _tag.Name;
            }
        }

        [JetBrains.Annotations.NotNull]
        public TraitTag Tag => _tag ?? throw new InvalidOperationException();

        [JetBrains.Annotations.NotNull]
        private static HHTTag AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic) {
            var hhtTagID = dr.GetIntFromLong("ID");
            var householdTraitID = dr.GetIntFromLong("HouseholdTraitID");
            var traitTagID = dr.GetNullableIntFromLong("TraitTagID", false, ignoreMissingFields);

            TraitTag traitTag = null;
            if (traitTagID != null) {
                traitTag = aic.TraitTags.FirstOrDefault(x => x.ID == traitTagID);
            }
            var name = "unknown";
            if (traitTag != null) {
                name = traitTag.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var hhad = new HHTTag(hhtTagID, householdTraitID, traitTag, connectionString, name,guid);
            return hhad;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_tag == null) {
                message = "Tag not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<HHTTag> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TraitTag> tags) {
            var aic = new AllItemCollections(traitTags: tags);

            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_tag != null) {
                cmd.AddParameter("TraitTagID", _tag.IntID);
            }
            if (_householdTraitID != null) {
                cmd.AddParameter("HouseholdTraitID", _householdTraitID);
            }
        }

        public JsonReference GetJson() => Tag.GetJsonReference();

        public void SynchronizeDataFromJson(JsonReference json, Simulator sim)
        {
            if (json.Guid != _tag?.Guid) {
                throw new LPGException("Error");
            }
        }

        public override string ToString() {
            if (_tag != null) {
                return _tag.Name;
            }
            return "(no name)";
        }

        public StrGuid RelevantGuid => Tag.Guid;
    }
}
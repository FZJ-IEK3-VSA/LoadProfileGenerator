using System;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class HHTLivingPatternTag : DBBase, IJSonSubElement<JsonReference>
    {
        public const string ParentIDFieldName = "HouseholdTraitID";
        public const string TableName = "tblHHTLivingPatternTags";
        [CanBeNull]
        private readonly int? _householdTraitID;

        [CanBeNull] private readonly LivingPatternTag _tag;

        public HHTLivingPatternTag([CanBeNull] int? pID, [CanBeNull] int? householdTraitID, [CanBeNull] LivingPatternTag tag, [NotNull] string connectionString, [NotNull] string name, StrGuid guid)
            : base(name, TableName, connectionString, guid)
        {
            _tag = tag;
            ID = pID;
            _householdTraitID = householdTraitID;
            TypeDescription = "Household Trait Tag";
        }
        [CanBeNull]
        public int? HouseholdTraitID => _householdTraitID;

        [NotNull]
        [UsedImplicitly]
#pragma warning disable S4015 // Inherited member visibility should not be decreased
        public new string Name
        {
#pragma warning restore S4015 // Inherited member visibility should not be decreased
            get
            {
                if (_tag == null)
                {
                    return "(no name)";
                }
                return _tag.Name;
            }
        }

        [NotNull]
        public LivingPatternTag Tag => _tag ?? throw new InvalidOperationException();

        [NotNull]
        private static HHTLivingPatternTag AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
                                                        [NotNull] AllItemCollections aic)
        {
            var hhtTagID = dr.GetIntFromLong("ID");
            var householdTraitID = dr.GetIntFromLong("HouseholdTraitID");
            var livingpatternID = dr.GetNullableIntFromLong("LivingPatternTagID", false, ignoreMissingFields);

            LivingPatternTag livingPatternTag = null;
            if (livingpatternID != null)
            {
                livingPatternTag = aic.LivingPatternTags.FirstOrDefault(x => x.ID == livingpatternID);
            }
            var name = "unknown";
            if (livingPatternTag != null)
            {
                name = livingPatternTag.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var hhad = new HHTLivingPatternTag(hhtTagID, householdTraitID, livingPatternTag, connectionString, name, guid);
            return hhad;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_tag == null)
            {
                message = "Tag not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull][NotNull] ObservableCollection<HHTLivingPatternTag> result, [NotNull] string connectionString,
                                            bool ignoreMissingTables, [ItemNotNull][NotNull] ObservableCollection<LivingPatternTag> tags)
        {
            var aic = new AllItemCollections(livingPatternTags: tags);

            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (_tag != null)
            {
                cmd.AddParameter("LivingPatternTagID", _tag.IntID);
            }
            if (_householdTraitID != null)
            {
                cmd.AddParameter("HouseholdTraitID", _householdTraitID);
            }
        }

        public JsonReference GetJson() => Tag.GetJsonReference();

        public void SynchronizeDataFromJson(JsonReference json, Simulator sim)
        {
            if (json.Guid != _tag?.Guid)
            {
                throw new LPGException("Error");
            }
        }

        public override string ToString()
        {
            if (_tag != null)
            {
                return _tag.Name;
            }
            return "(no name)";
        }

        public StrGuid RelevantGuid => Tag.Guid;
    }
}
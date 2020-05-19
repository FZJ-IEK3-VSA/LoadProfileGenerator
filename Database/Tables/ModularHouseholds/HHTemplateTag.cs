using System;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class HHTemplateTag : DBBase, IJSonSubElement<JsonReference> {
        public StrGuid RelevantGuid => Tag.Guid;
        public const string TableName = "tblHHTemplateTag";
        private readonly int _hhTemplateID;

        [CanBeNull] private readonly HouseholdTag _householdTag;

        public HHTemplateTag([CanBeNull]int? pID, [CanBeNull] HouseholdTag householdTag, int hhTemplateID, [NotNull] string name,
            [NotNull] string connectionString, StrGuid guid)
            : base(name, TableName, connectionString, guid) {
            ID = pID;
            _householdTag = householdTag;
            _hhTemplateID = hhTemplateID;
            TypeDescription = "Household Template Tag";
        }

        [NotNull]
        public HouseholdTag Tag => _householdTag ?? throw new InvalidOperationException();

        public int TemplateID => _hhTemplateID;

        [NotNull]
        private static HHTemplateTag AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var hhpID = dr.GetIntFromLong("ID");
            var templateTagID = dr.GetIntFromLong("TemplateTagID", ignoreMissingField: ignoreMissingFields);
            var householdID = dr.GetIntFromLong("HHTemplateID");
            var p = aic.HouseholdTags.FirstOrDefault(x => x.ID == templateTagID);
            var name = "(no name)";
            if (p != null) {
                name = p.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);

            var hhp = new HHTemplateTag(hhpID, p, householdID, name, connectionString,guid);
            return hhp;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_householdTag == null) {
                message = "Household Tag is missing";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<HHTemplateTag> result, [NotNull] string connectionString,
            [ItemNotNull] [CanBeNull]ObservableCollection<HouseholdTag> templateTags, bool ignoreMissingTables) {
            var aic = new AllItemCollections(householdTags: templateTags);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_householdTag != null) {
                cmd.AddParameter("TemplateTagID", _householdTag.IntID);
            }
            cmd.AddParameter("HHTemplateID", _hhTemplateID);
        }

        public JsonReference GetJson() => Tag.GetJsonReference();

        public void SynchronizeDataFromJson(JsonReference json, Simulator sim)
        {
            if (_householdTag?.Guid != json.Guid)
            {
                throw new LPGException("This should be impossible");
            }
        }

        public override string ToString() {
            if (_householdTag == null) {
                return "Unknown";
            }
            return _householdTag.Name;
        }
    }
}
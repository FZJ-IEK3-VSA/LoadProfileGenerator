using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class STHouseholdDistributionTag : DBBase {
        private const string TableName = "tblSTHouseholdDistributionTags";
        private readonly int _stHouseholdDistributionID;

        [CanBeNull] private readonly HouseholdTag _tag;

        public STHouseholdDistributionTag([CanBeNull]int? pID, [CanBeNull] HouseholdTag tag, int stHouseholdDistributionID,
            [NotNull] string name,
            [NotNull] string connectionString, [NotNull] StrGuid guid) : base(name, TableName, connectionString, guid) {
            ID = pID;
            _tag = tag;
            _stHouseholdDistributionID = stHouseholdDistributionID;
            TypeDescription = "Settlement Template Distribution Tag";
        }
        [CanBeNull]
        public int? STHouseholdDistributionID => _stHouseholdDistributionID;

        [CanBeNull]
        public HouseholdTag Tag => _tag;

        [NotNull]
        private static STHouseholdDistributionTag AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic) {
            var id =dr.GetIntFromLong("ID");
            var tagID = dr.GetIntFromLong("TagID", ignoreMissingField: ignoreMissingFields);
            var tag = aic.HouseholdTags.FirstOrDefault(x => x.ID == tagID);

            var householdID = dr.GetIntFromLong("STHouseholdDistributionID");
            var name = "(no name)";
            if (tag != null) {
                name = tag.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var hhp = new STHouseholdDistributionTag(id, tag, householdID, name, connectionString, guid);
            return hhp;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            if (_tag == null) {
                message = "Tag not found.";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<STHouseholdDistributionTag> result,
            [NotNull] string connectionString, [ItemNotNull] [NotNull] ObservableCollection<HouseholdTag> tags, bool ignoreMissingTables) {
            var aic = new AllItemCollections(householdTags: tags);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters([NotNull] Command cmd) {
            if (_tag != null) {
                cmd.AddParameter("TagID", _tag.IntID);
            }
            cmd.AddParameter("STHouseholdDistributionID", _stHouseholdDistributionID);
        }

        public override string ToString() {
            if (_tag == null) {
                return "Unknown";
            }
            return _tag.Name;
        }
    }
}
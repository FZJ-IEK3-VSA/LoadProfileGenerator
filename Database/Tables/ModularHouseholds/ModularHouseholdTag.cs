using System;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class ModularHouseholdTag : DBBase, IRelevantGuidProvider {
        public const string ParentIDField = "ModularHouseholdID";
        public const string TableName = "tblModularHouseholdTags";
        private readonly int _chhID;

        [CanBeNull] private readonly HouseholdTag _householdTag;

        public ModularHouseholdTag([CanBeNull]int? pID, [CanBeNull] HouseholdTag householdTag, int chhID, [NotNull] string name,
            [NotNull] string connectionString, StrGuid guid)
            : base(name, TableName, connectionString, guid) {
            ID = pID;
            _householdTag = householdTag;
            _chhID = chhID;
            TypeDescription = "Modular Household Tag";
        }

        public int ModularHouseholdID => _chhID;
        [NotNull]
        public HouseholdTag Tag => _householdTag ?? throw new InvalidOperationException();

        [NotNull]
        private static ModularHouseholdTag AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var hhpID =  dr.GetIntFromLong("ID");
            var modularHouseholdID = dr.GetIntFromLong("TemplateTagID", ignoreMissingField: ignoreMissingFields);
            var householdID = dr.GetIntFromLong("ModularHouseholdID");
            var p = aic.HouseholdTags.FirstOrDefault(x => x.ID == modularHouseholdID);
            var name = "(no name)";
            if (p != null) {
                name = p.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var hhp = new ModularHouseholdTag(hhpID, p, householdID, name, connectionString, guid);
            return hhp;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_householdTag == null) {
                message = "Household tag missing";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<ModularHouseholdTag> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<HouseholdTag> templateTags, bool ignoreMissingTables) {
            var aic = new AllItemCollections(householdTags: templateTags);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_householdTag != null) {
                cmd.AddParameter("TemplateTagID", _householdTag.IntID);
            }
            cmd.AddParameter("ModularHouseholdID", _chhID);
        }

        public override string ToString() {
            if (_householdTag == null) {
                return "Unknown";
            }
            return _householdTag.Name;
        }

        public StrGuid RelevantGuid => Tag.Guid;
    }
}
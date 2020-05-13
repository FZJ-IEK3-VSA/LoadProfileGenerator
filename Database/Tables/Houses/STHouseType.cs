using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class STHouseType : DBBase {
        public const string TableName = "tblSTHouseTypes";

        [CanBeNull] private readonly HouseType _houseType;

        private readonly int _settlementTemplateID;

        public STHouseType([CanBeNull]int? pID, [NotNull] string connectionString, int settlementTemplateID, [NotNull] string name,
            [CanBeNull] HouseType houseType, [NotNull] StrGuid guid)
            : base(name, TableName, connectionString, guid) {
            TypeDescription = "Settlement Template House Type";
            ID = pID;
            _houseType = houseType;
            _settlementTemplateID = settlementTemplateID;
        }

        [CanBeNull]
        public HouseType HouseType => _houseType;

        public int SettlementTemplateID => _settlementTemplateID;

        [NotNull]
        private static STHouseType AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var id =  dr.GetIntFromLong("ID");
            var settlementtemplateID = dr.GetIntFromLong("SettlementTemplateID", false, ignoreMissingFields, -1);
            var householdTemplateID = dr.GetIntFromLong("HouseTypeID", false);
            var ht = aic.HouseTypes.FirstOrDefault(x => x.IntID == householdTemplateID);
            var name = "unknown";
            if (ht != null) {
                name = ht.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);

            var shh = new STHouseType(id, connectionString, settlementtemplateID, name, ht, guid);
            return shh;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            if (_houseType == null) {
                message = "Housetype not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<STHouseType> result, [NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [NotNull] ObservableCollection<HouseType> houseTypes) {
            var aic = new AllItemCollections(houseTypes: houseTypes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters([NotNull] Command cmd) {
            if (_houseType != null) {
                cmd.AddParameter("HouseTypeID", _houseType.IntID);
            }
            cmd.AddParameter("SettlementTemplateID", _settlementTemplateID);
        }

        public override string ToString() {
            if (_houseType != null) {
                return _houseType.Name;
            }

            return "Unknown";
        }
    }
}
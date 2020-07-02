using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class STChargingStationSet : DBBase
    {
        public const string TableName = "tblSTChargingStationSets";

        [CanBeNull] private readonly ChargingStationSet _chargingStationSet;

        private readonly int _settlementTemplateID;

        public STChargingStationSet([CanBeNull] int? pID, [NotNull] string connectionString, int settlementTemplateID, [NotNull] string name,
                                    [CanBeNull] ChargingStationSet chargingStationSet, StrGuid guid) : base(name, TableName, connectionString, guid)
        {
            TypeDescription = "Settlement Template Charging Station Sets";
            ID = pID;
            _chargingStationSet = chargingStationSet;
            _settlementTemplateID = settlementTemplateID;
        }

        [CanBeNull]
        public ChargingStationSet ChargingStationSet => _chargingStationSet;

        public int SettlementTemplateID => _settlementTemplateID;

        [NotNull]
        private static STChargingStationSet AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
                                                         bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var settlementtemplateID = dr.GetIntFromLong("SettlementTemplateID", false, ignoreMissingFields, -1);
            var chargingStationId = dr.GetIntFromLong("ChargingStationSetID", false);
            var css = aic.ChargingStationSets.FirstOrDefault(x => x.IntID == chargingStationId);
            var name = "unknown";
            if (css != null)
            {
                name = css.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);

            var shh = new STChargingStationSet(id, connectionString, settlementtemplateID, name, css, guid);
            return shh;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (ChargingStationSet == null)
            {
                message = "Charging Station Set not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull][NotNull] ObservableCollection<STChargingStationSet> result, [NotNull] string connectionString,
                                            bool ignoreMissingTables, [ItemNotNull][NotNull] ObservableCollection<ChargingStationSet> chargingStationSets)
        {
            var aic = new AllItemCollections(chargingStationSets: chargingStationSets);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (_chargingStationSet != null)
            {
                cmd.AddParameter("ChargingStationSetID", _chargingStationSet.IntID);
            }
            cmd.AddParameter("SettlementTemplateID", _settlementTemplateID);
        }

        public override string ToString()
        {
            if (_chargingStationSet != null)
            {
                return _chargingStationSet.Name;
            }

            return "Unknown";
        }
    }
}
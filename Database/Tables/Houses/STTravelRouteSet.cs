using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class STTravelRouteSet : DBBase
    {
        public const string TableName = "tblSTTravelRouteSets";

        [CanBeNull] private readonly TravelRouteSet _travelRouteSet;

        private readonly int _settlementTemplateID;

        public STTravelRouteSet([CanBeNull] int? pID, [JetBrains.Annotations.NotNull] string connectionString, int settlementTemplateID, [JetBrains.Annotations.NotNull] string name,
                                [CanBeNull] TravelRouteSet travelRouteSet, [NotNull] StrGuid guid) : base(name, TableName, connectionString, guid)
        {
            TypeDescription = "Settlement Template Travel Route Sets";
            ID = pID;
            _travelRouteSet = travelRouteSet;
            _settlementTemplateID = settlementTemplateID;
        }

        [CanBeNull]
        public TravelRouteSet TravelRouteSet => _travelRouteSet;

        public int SettlementTemplateID => _settlementTemplateID;

        [JetBrains.Annotations.NotNull]
        private static STTravelRouteSet AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
                                                     bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var settlementtemplateID = dr.GetIntFromLong("SettlementTemplateID", false, ignoreMissingFields, -1);
            var travelRouteSetID = dr.GetIntFromLong("TravelRouteSetID", false);
            var trs = aic.TravelRouteSets.FirstOrDefault(x => x.IntID == travelRouteSetID);
            var name = "unknown";
            if (trs != null)
            {
                name = trs.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);

            var shh = new STTravelRouteSet(id, connectionString, settlementtemplateID, name, trs, guid);
            return shh;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (TravelRouteSet == null)
            {
                message = "Travel Route Set not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull][JetBrains.Annotations.NotNull] ObservableCollection<STTravelRouteSet> result, [JetBrains.Annotations.NotNull] string connectionString,
                                            bool ignoreMissingTables, [ItemNotNull][JetBrains.Annotations.NotNull] ObservableCollection<TravelRouteSet> travelRouteSets)
        {
            var aic = new AllItemCollections(travelRouteSets: travelRouteSets);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (_travelRouteSet != null)
            {
                cmd.AddParameter("TravelRouteSetID", _travelRouteSet.IntID);
            }
            cmd.AddParameter("SettlementTemplateID", _settlementTemplateID);
        }

        public override string ToString()
        {
            if (_travelRouteSet != null)
            {
                return _travelRouteSet.Name;
            }

            return "Unknown";
        }
    }
}
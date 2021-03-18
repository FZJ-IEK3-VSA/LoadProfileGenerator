using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace Database.Tables.Houses {
    public class STTransportationDeviceSet : DBBase
    {
        public const string TableName = "tblSTTransportationDeviceSets";

        [CanBeNull] private readonly TransportationDeviceSet _transportationDeviceSet;

        private readonly int _settlementTemplateID;

        public STTransportationDeviceSet([CanBeNull] int? pID, [JetBrains.Annotations.NotNull] string connectionString, int settlementTemplateID, [JetBrains.Annotations.NotNull] string name,
                                         [CanBeNull] TransportationDeviceSet transportationDeviceSet, [NotNull] StrGuid guid) : base(name, TableName, connectionString, guid)
        {
            TypeDescription = "Settlement Template Transportation Device Sets";
            ID = pID;
            _transportationDeviceSet = transportationDeviceSet;
            _settlementTemplateID = settlementTemplateID;
        }

        [CanBeNull]
        public TransportationDeviceSet TransportationDeviceSet => _transportationDeviceSet;

        public int SettlementTemplateID => _settlementTemplateID;

        [JetBrains.Annotations.NotNull]
        private static STTransportationDeviceSet AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
                                                              bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var settlementtemplateID = dr.GetIntFromLong("SettlementTemplateID", false, ignoreMissingFields, -1);
            var transportationDeviceSetID = dr.GetIntFromLong("TransportationDeviceSetId", false);
            var trs = aic.TransportationDeviceSets.FirstOrDefault(x => x.IntID == transportationDeviceSetID);
            var name = "unknown";
            if (trs != null)
            {
                name = trs.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);

            var shh = new STTransportationDeviceSet(id, connectionString, settlementtemplateID, name, trs, guid);
            return shh;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (TransportationDeviceSet == null)
            {
                message = "Transportation Device Set not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull][JetBrains.Annotations.NotNull] ObservableCollection<STTransportationDeviceSet> result, [JetBrains.Annotations.NotNull] string connectionString,
                                            bool ignoreMissingTables, [ItemNotNull][JetBrains.Annotations.NotNull] ObservableCollection<TransportationDeviceSet> transportationDeviceSets)
        {
            var aic = new AllItemCollections(transportationDeviceSets:transportationDeviceSets);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (_transportationDeviceSet != null)
            {
                cmd.AddParameter("TransportationDeviceSetID", _transportationDeviceSet.IntID);
            }
            cmd.AddParameter("SettlementTemplateID", _settlementTemplateID);
        }

        public override string ToString()
        {
            if (_transportationDeviceSet != null)
            {
                return _transportationDeviceSet.Name;
            }

            return "Unknown";
        }
    }
}
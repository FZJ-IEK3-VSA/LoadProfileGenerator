using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.Transportation {
    public class TransportationDeviceSetEntry : DBBase {
        private const string TableName = "tblTransportationDeviceSetEntries";

        [CanBeNull] private readonly TransportationDevice _transportationDevice;

        private readonly int _transportationDeviceSetID;

        public TransportationDeviceSetEntry([CanBeNull]int? pID, int transportationDeviceSetID, [NotNull] string connectionString,
            [NotNull] string name, [CanBeNull] TransportationDevice device,
                                            StrGuid guid) : base(name, TableName, connectionString, guid)
        {
            TypeDescription = "Transportation Device Set Entry";
            ID = pID;

            _transportationDevice = device;
            _transportationDeviceSetID = transportationDeviceSetID;
        }

        [UsedImplicitly]
        [CanBeNull]
        public TransportationDevice TransportationDevice => _transportationDevice;

        [UsedImplicitly]
        public int TransportationDeviceSetID => _transportationDeviceSetID;

        [NotNull]
        private static TransportationDeviceSetEntry AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var setid = dr.GetIntFromLong("TransportationDeviceSetID");
            var transportationDeviceCategoryID = dr.GetIntFromLong("TransportationDeviceID");
            var tdevices = aic.TransportationDevices.FirstOrDefault(x => x.ID == transportationDeviceCategoryID);
            var name = "(no name)";
            if (tdevices != null) {
                name = tdevices.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var setentry = new TransportationDeviceSetEntry(id, setid, connectionString, name, tdevices,
                guid);
            return setentry;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_transportationDevice == null) {
                message = "Transportation Device Category not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TransportationDeviceSetEntry> result,
            [NotNull] string connectionString, bool ignoreMissingTables,
            [ItemNotNull] [NotNull] ObservableCollection<TransportationDevice> transportationDevices)
        {
            var aic = new AllItemCollections(transportationDevices: transportationDevices);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("TransportationDeviceSetID", _transportationDeviceSetID);
            if (_transportationDevice != null) {
                cmd.AddParameter("TransportationDeviceID", _transportationDevice.IntID);
            }
        }

        public override string ToString() => Name;
    }
}
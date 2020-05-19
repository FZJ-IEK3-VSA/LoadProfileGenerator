using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.Transportation {
    public class TransportationDeviceSet : DBBaseElement {
        public const string TableName = "tblTransportationDeviceSets";
        [CanBeNull] private string _description;

        [ItemNotNull] [NotNull] private readonly ObservableCollection<TransportationDeviceSetEntry> _transportationDevices =
            new ObservableCollection<TransportationDeviceSetEntry>();
        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var deviceSetEntry in _transportationDevices)
            {
                deviceSetEntry.SaveToDB();
            }
        }
        public TransportationDeviceSet([NotNull] string name, [CanBeNull]int? pID, [NotNull] string connectionString, [CanBeNull] string description,
            StrGuid guid
            )
            : base(name, TableName, connectionString, guid)
        {
            _description = description;
            ID = pID;
            AreNumbersOkInNameForIntegrityCheck = true;
            TypeDescription = "Site";
        }

        [CanBeNull]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<TransportationDeviceSetEntry> TransportationDeviceSetEntries =>
            _transportationDevices;

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new
            TransportationDeviceSet(FindNewName(isNameTaken, "New Transportation Device Set"),
                null, connectionString,"", System.Guid.NewGuid().ToStrGuid());

        [NotNull]
        [UsedImplicitly]
        public static TransportationDeviceSet ImportFromItem([NotNull] TransportationDeviceSet toImport,
            [NotNull] Simulator dstSim)
        {
            var loc = new TransportationDeviceSet(toImport.Name, null, dstSim.ConnectionString,
                toImport.Description, toImport.Guid);
            loc.SaveToDB();
            foreach (TransportationDeviceSetEntry entry in toImport.TransportationDeviceSetEntries) {
                var dstdev = GetItemFromListByName(dstSim.TransportationDevices.It, entry.Name);
                if (dstdev == null)
                {
                    throw new LPGException("Device was null");
                }
                loc.AddDevice(dstdev);
            }
            loc.SaveToDB();
            return loc;
        }

        [UsedImplicitly]
        //TODO: use and remove usedimplicitly
        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TransportationDeviceSet> result,
            [NotNull] string connectionString, bool ignoreMissingTables,
            [ItemNotNull] [NotNull] ObservableCollection<TransportationDevice> transportationDevices)
        {
            var aic = new AllItemCollections(transportationDevices: transportationDevices);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var ld = new ObservableCollection<TransportationDeviceSetEntry>();
            TransportationDeviceSetEntry.LoadFromDatabase(ld, connectionString, ignoreMissingTables, transportationDevices);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(ld), IsCorrectTransportationDeviceSetEntryParent,
                ignoreMissingTables);
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", Name);
            if(_description != null) {
                cmd.AddParameter("Description", _description);
            }
        }

        [NotNull]
        private static TransportationDeviceSet AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name", false, "(no name)", ignoreMissingFields);
            var description = dr.GetString("Description", false, "(no description)", ignoreMissingFields);
            var id = dr.GetIntFromLong("ID", false, ignoreMissingFields, -1);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new TransportationDeviceSet(name, id, connectionString, description, guid);
        }

        private static bool IsCorrectTransportationDeviceSetEntryParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (TransportationDeviceSetEntry) child;
            if (parent.ID == hd.TransportationDeviceSetID) {
                var transportationDeviceSet = (TransportationDeviceSet) parent;
                transportationDeviceSet._transportationDevices.Add(hd);
                return true;
            }

            return false;
        }

        public void AddDevice([NotNull] TransportationDevice tdev)
        {
                TransportationDeviceSetEntry tdse = new TransportationDeviceSetEntry(null, IntID,
                    ConnectionString, tdev.Name, tdev, System.Guid.NewGuid().ToStrGuid());
                tdse.SaveToDB();
            _transportationDevices.Add(tdse);
        }

        public void DeleteEntry([NotNull] TransportationDeviceSetEntry entry)
        {
            entry.DeleteFromDB();
            _transportationDevices.Remove(entry);
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((TransportationDeviceSet)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}
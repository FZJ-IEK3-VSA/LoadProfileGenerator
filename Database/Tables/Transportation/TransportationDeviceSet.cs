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

        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<TransportationDeviceSetEntry> _transportationDevices =
            new ObservableCollection<TransportationDeviceSetEntry>();
        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var deviceSetEntry in _transportationDevices)
            {
                deviceSetEntry.SaveToDB();
            }
        }
        public TransportationDeviceSet([JetBrains.Annotations.NotNull] string name, [CanBeNull]int? pID, [JetBrains.Annotations.NotNull] string connectionString, [CanBeNull] string description,
            [JetBrains.Annotations.NotNull] StrGuid guid
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
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TransportationDeviceSetEntry> TransportationDeviceSetEntries =>
            _transportationDevices;

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString) => new
            TransportationDeviceSet(FindNewName(isNameTaken, "New Transportation Device Set"),
                null, connectionString,"", System.Guid.NewGuid().ToStrGuid());

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static TransportationDeviceSet ImportFromItem([JetBrains.Annotations.NotNull] TransportationDeviceSet toImport,
            [JetBrains.Annotations.NotNull] Simulator dstSim)
        {
            var loc = new TransportationDeviceSet(toImport.Name, null, dstSim.ConnectionString,
                toImport.Description, toImport.Guid);
            loc.SaveToDB();
            foreach (TransportationDeviceSetEntry entry in toImport.TransportationDeviceSetEntries) {
                var dstdev = GetItemFromListByName(dstSim.TransportationDevices.Items, entry.Name);
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
        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TransportationDeviceSet> result,
            [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingTables,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TransportationDevice> transportationDevices)
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

        [JetBrains.Annotations.NotNull]
        private static TransportationDeviceSet AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name", false, "(no name)", ignoreMissingFields);
            var description = dr.GetString("Description", false, "(no description)", ignoreMissingFields);
            var id = dr.GetIntFromLong("ID", false, ignoreMissingFields, -1);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new TransportationDeviceSet(name, id, connectionString, description, guid);
        }

        private static bool IsCorrectTransportationDeviceSetEntryParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (TransportationDeviceSetEntry) child;
            if (parent.ID == hd.TransportationDeviceSetID) {
                var transportationDeviceSet = (TransportationDeviceSet) parent;
                transportationDeviceSet._transportationDevices.Add(hd);
                return true;
            }

            return false;
        }

        public void AddDevice([JetBrains.Annotations.NotNull] TransportationDevice tdev)
        {
                TransportationDeviceSetEntry tdse = new TransportationDeviceSetEntry(null, IntID,
                    ConnectionString, tdev.Name, tdev, System.Guid.NewGuid().ToStrGuid());
                tdse.SaveToDB();
            _transportationDevices.Add(tdse);
        }

        public void DeleteEntry([JetBrains.Annotations.NotNull] TransportationDeviceSetEntry entry)
        {
            entry.DeleteFromDB();
            _transportationDevices.Remove(entry);
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((TransportationDeviceSet)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}
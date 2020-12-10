using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Transportation
{
    public class ChargingStationSet : DBBaseElement
    {

        [NotNull]
        public ChargingStationSet MakeExactCopy([NotNull] Simulator sim)
        {
            var other= sim.ChargingStationSets.CreateNewItem(sim.ConnectionString);
            other.Name = Name + " (copy)";
            other.Description = Description;
            other.SaveToDB();
            foreach (var chargingStationSetEntry in _chargingStations) {
                other.AddChargingStation(chargingStationSetEntry.TransportationDeviceCategory,
                    chargingStationSetEntry.CarChargingLoadType,
                    chargingStationSetEntry.MaxChargingPower,
                    chargingStationSetEntry.Site,chargingStationSetEntry.GridChargingLoadType);
            }
            other.SaveToDB();
            return other;
        }
        public const string TableName = "tblChargingStationSets";
        [ItemNotNull] [NotNull] private readonly ObservableCollection<ChargingStationSetEntry> _chargingStations = new ObservableCollection<ChargingStationSetEntry>();
        [CanBeNull] private string _description;

        public ChargingStationSet([NotNull] string name, [CanBeNull] int? pID, [NotNull] string connectionString,
            [CanBeNull] string description, StrGuid guid) : base(name, TableName,
            connectionString, guid)
        {
            _description = description;
            ID = pID;
            AreNumbersOkInNameForIntegrityCheck = true;
            TypeDescription = "ChargingStationSet";
        }

        [CanBeNull]
        [UsedImplicitly]
        public string Description
        {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        public void AddChargingStation([CanBeNull] TransportationDeviceCategory deviceCategory, [CanBeNull] VLoadType carloadType, double chargingPower,
                                       [CanBeNull] Site site, [CanBeNull] VLoadType gridLoadType)
        {
            ChargingStationSetEntry station = new ChargingStationSetEntry(null,
                carloadType, deviceCategory, IntID,
                chargingPower, ConnectionString, "Charging Station",site, System.Guid.NewGuid().ToStrGuid(), gridLoadType);
            station.SaveToDB();
            _chargingStations.Add(station);
        }

        [ItemNotNull]
        [NotNull]
        public ObservableCollection<ChargingStationSetEntry> ChargingStations => _chargingStations;

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem
            ([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new ChargingStationSet(
            FindNewName(isNameTaken, "New Charging Station Set "),
            null, connectionString, "", System.Guid.NewGuid().ToStrGuid());

        public override void DeleteFromDB()
        {
            foreach (var station in _chargingStations)
            {
                station.DeleteFromDB();
            }

            base.DeleteFromDB();
        }

        [NotNull]
        [UsedImplicitly]
        public static ChargingStationSet ImportFromItem([NotNull] ChargingStationSet toImport, [NotNull] Simulator dstSim)
        {
            var chargingStationSet = new ChargingStationSet(toImport.Name, null, dstSim.ConnectionString, toImport.Description, toImport.Guid);
            chargingStationSet.SaveToDB();

            foreach (var chargingStation in toImport._chargingStations)
            {
                var carloadtype = GetItemFromListByName(dstSim.LoadTypes.Items, chargingStation.CarChargingLoadType?.Name);
                var gridloadtype = GetItemFromListByName(dstSim.LoadTypes.Items, chargingStation.GridChargingLoadType?.Name);
                var transportaitonDeviceCategory = GetItemFromListByName(dstSim.TransportationDeviceCategories.Items,
                    chargingStation.TransportationDeviceCategory?.Name);
                var site = GetItemFromListByName(dstSim.Sites.Items, chargingStation.Site?.Name);
                chargingStationSet.AddChargingStation(transportaitonDeviceCategory, carloadtype, chargingStation.MaxChargingPower,site, gridloadtype);
            }

            return chargingStationSet;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<ChargingStationSet> result, [NotNull] string connectionString,
            bool ignoreMissingTables,
                                            [ItemNotNull] [NotNull] ObservableCollection<VLoadType> loadTypes,
                                            [ItemNotNull] [NotNull] ObservableCollection<TransportationDeviceCategory> categories,
                                            [ItemNotNull] [NotNull] ObservableCollection<Site> sites)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);

            var charging = new ObservableCollection<ChargingStationSetEntry>();
            ChargingStationSetEntry.LoadFromDatabase(charging, connectionString,
                loadTypes, categories,
                sites,ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(charging), IsCorrectSiteChargingParent,
                ignoreMissingTables);
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var chargingStation in _chargingStations)
            {
                chargingStation.SaveToDB();
            }
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", Name);
        }

        [NotNull]
        private static ChargingStationSet AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name", false, "(no name)", ignoreMissingFields);
            var description = dr.GetString("Description", false, "(no description)", ignoreMissingFields);
            var id = dr.GetIntFromLong("ID", false, ignoreMissingFields, -1);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new ChargingStationSet(name, id, connectionString, description, guid);
        }

        private static bool IsCorrectSiteChargingParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (ChargingStationSetEntry)child;

            if (parent.ID == hd.ChargingStationSetID)
            {
                var site = (ChargingStationSet)parent;
                site._chargingStations.Add(hd);
                return true;
            }

            return false;
        }

        public void DeleteChargingStation([NotNull] ChargingStationSetEntry chargingStation)
        {
            chargingStation.DeleteFromDB();
            _chargingStations.Remove(chargingStation);
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((ChargingStationSet)toImport, dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();

    }
}

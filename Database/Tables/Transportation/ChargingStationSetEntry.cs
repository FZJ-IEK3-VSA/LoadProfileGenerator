#region header

// // ProfileGenerator DatabaseIO changed: 2017 03 22 14:33

#endregion

using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Transportation {
    public class ChargingStationSetEntry : DBBase {
        public const string TableName = "tblChargingStationSetEntries";
        [CanBeNull]
        public VLoadType CarChargingLoadType => _carChargingLoadtype;

        [CanBeNull]
        public VLoadType GridChargingLoadType => _gridChargingLoadtype;

        [CanBeNull] private Site _site;
        [CanBeNull]
        public TransportationDeviceCategory TransportationDeviceCategory => _transportationDeviceCategory;

        public double MaxChargingPower => _maxChargingPower;
        [CanBeNull] private readonly VLoadType _carChargingLoadtype;
        [CanBeNull] private readonly VLoadType _gridChargingLoadtype;
        [CanBeNull] private readonly TransportationDeviceCategory _transportationDeviceCategory;
        private readonly double _maxChargingPower;

        public ChargingStationSetEntry([CanBeNull]int? pID, [CanBeNull] VLoadType carChargingLoadtype,  [CanBeNull] TransportationDeviceCategory transportationDeviceCategory,
    int chargingStationSetID, double maxChargingPower, [JetBrains.Annotations.NotNull] string connectionString,
            [JetBrains.Annotations.NotNull] string name, [CanBeNull] Site site, StrGuid guid, [CanBeNull] VLoadType gridChargingLoadtype) : base(name, TableName, connectionString, guid)
        {
            _site = site;
            TypeDescription = "Site Location";
            ID = pID;
            ChargingStationSetID = chargingStationSetID;
            _carChargingLoadtype = carChargingLoadtype;
            _gridChargingLoadtype = gridChargingLoadtype;
            _transportationDeviceCategory = transportationDeviceCategory;
            _maxChargingPower = maxChargingPower;
        }

        [UsedImplicitly]
        public int ChargingStationSetID { get; }

        [CanBeNull]
        public Site Site {
            get => _site;
            set => _site = value;
        }

        [JetBrains.Annotations.NotNull]
        private static ChargingStationSetEntry AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var chargingStationID = dr.GetIntFromLong("ID");
            var chargingStationSetID = dr.GetIntFromLong("chargingStationSetID");
            var carChargingLoadTypeId = dr.GetIntFromLong("CarChargingLoadTypeID",false,ignoreMissingFields);
            var carLoadtype = aic.LoadTypes.FirstOrDefault(x => x.ID == carChargingLoadTypeId);

            var gridChargingLoadTypeId = dr.GetIntFromLong("GridChargingLoadTypeID",false,ignoreMissingFields);
            var gridLoadtype = aic.LoadTypes.FirstOrDefault(x => x.ID == gridChargingLoadTypeId);

            var transportationDeviceCategoryId = dr.GetIntFromLong("TransportationDeviceCategoryID",false,ignoreMissingFields);
            var maxChargingPower = dr.GetDouble("MaxChargingPower",false,0,ignoreMissingFields);
            var siteID = dr.GetIntFromLong("SiteID");
            var site = aic.Sites.FirstOrDefault(x => x.ID == siteID);
            var transportationDeviceCategory =
                aic.TransportationDeviceCategories.FirstOrDefault(x => x.ID == transportationDeviceCategoryId);
            var name = "(no name)";
            if (carLoadtype != null) {
                name = carLoadtype.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var locdev = new ChargingStationSetEntry(chargingStationID,carLoadtype,
                transportationDeviceCategory, chargingStationSetID, maxChargingPower, connectionString, name,
                site, guid, gridLoadtype);
            return locdev;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_carChargingLoadtype == null) {
                message = "Car Charging Loadtype not found";
                return false;
            }
            if (_gridChargingLoadtype == null)
            {
                message = "Grid Charging Loadtype not found";
                return false;
            }
            if (_transportationDeviceCategory == null)
            {
                message = "Transportation Device Category not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<ChargingStationSetEntry> result, [JetBrains.Annotations.NotNull] string connectionString,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<VLoadType> loadTypes, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TransportationDeviceCategory> transportationDeviceCategories,
                                            [JetBrains.Annotations.NotNull] [ItemNotNull] ObservableCollection<Site> sites, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(loadTypes: loadTypes, transportationDeviceCategories:transportationDeviceCategories,sites:sites);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {

            cmd.AddParameter("ChargingStationSetID", ChargingStationSetID);
            if(_carChargingLoadtype != null) {
                cmd.AddParameter("CarChargingLoadtypeID", _carChargingLoadtype.IntID);
            }
            if (_gridChargingLoadtype != null)
            {
                cmd.AddParameter("GridChargingLoadtypeID", _gridChargingLoadtype.IntID);
            }
            if (_transportationDeviceCategory != null)
            {
                cmd.AddParameter("TransportationDeviceCategoryID", _transportationDeviceCategory.IntID);
            }
            cmd.AddParameter("MaxChargingPower",_maxChargingPower);
            if (_site != null) {
                cmd.AddParameter("SiteID", _site.IntID);
            }
        }

        public override string ToString() => Name;
    }
}
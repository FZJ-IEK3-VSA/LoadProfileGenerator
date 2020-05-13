#region header

// // ProfileGenerator DatabaseIO changed: 2017 03 22 14:33

#endregion

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Transportation {
    public class SiteLocation : DBBase {
        public const string TableName = "tblSiteLocations";

        [CanBeNull] private readonly Location _location;

        public SiteLocation([CanBeNull]int? pID, [CanBeNull] Location location, int siteID, [NotNull] string connectionString,
            [NotNull] string name, [NotNull] StrGuid guid) : base(name, TableName, connectionString, guid)
        {
            TypeDescription = "Site Location";
            ID = pID;
            _location = location;
            SiteID = siteID;
        }

        [UsedImplicitly]
        [NotNull]
        public Location Location => _location ?? throw new InvalidOperationException();

        public override string Name {
            get {
                if (_location != null) {
                    return _location.Name;
                }
                return "(no name)";
            }
        }

        [UsedImplicitly]
        public int SiteID { get; }

        [NotNull]
        private static SiteLocation AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var siteLocID = dr.GetIntFromLong("ID");
            var siteID = dr.GetIntFromLong("SiteID");
            var locationID = dr.GetIntFromLong("LocationID");
            var loc = aic.Locations.FirstOrDefault(x => x.ID == locationID);
            var name = "(no name)";
            if (loc != null) {
                name = loc.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var locdev = new SiteLocation(siteLocID, loc, siteID, connectionString, name, guid);
            return locdev;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            if (_location == null) {
                message = "Location not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<SiteLocation> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<Location> locations, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(locations: locations);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("SiteID", SiteID);
            if(_location != null) {
                cmd.AddParameter("LocationID", _location.IntID);
            }
        }

        public override string ToString() => Name;
    }
}
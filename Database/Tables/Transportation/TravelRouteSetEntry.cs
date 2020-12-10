using System;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.Transportation {
    public class TravelRouteSetEntry : DBBase {
        public const string TableName = "tblTravelRouteSetEntry";

        [CanBeNull] private readonly TravelRoute _travelRoute;

        private readonly int _travelRouteSetID;

        public TravelRouteSetEntry([CanBeNull]int? pID, int travelRouteSetID, [NotNull] string connectionString, [NotNull] string name,
            [CanBeNull] TravelRoute travelRoute, StrGuid guid)
            : base(name, TableName, connectionString, guid)
        {
            TypeDescription = "Travel Route Step";
            ID = pID;

            _travelRoute = travelRoute;
            _travelRouteSetID = travelRouteSetID;
        }

        [UsedImplicitly]
        [NotNull]
        public TravelRoute TravelRoute => _travelRoute ?? throw new InvalidOperationException();

        [UsedImplicitly]
        public int TravelRouteSetID => _travelRouteSetID;

        [NotNull]
        private static TravelRouteSetEntry AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var setid = dr.GetIntFromLong("TravelRouteSetID");
            var routeID = dr.GetIntFromLong("TravelRouteID");
            var route = aic.TravelRoutes.FirstOrDefault(x => x.ID == routeID);
            //var name = dr.GetString("Name",false,"",ignoreMissingFields);
            const string name = "no name";
            var guid = GetGuid(dr, ignoreMissingFields);
            var step = new TravelRouteSetEntry(id, setid, connectionString, name, route, guid);
            return step;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_travelRoute == null) {
                message = "Travel Route not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TravelRouteSetEntry> result, [NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [NotNull] ObservableCollection<TravelRoute> travelRoutes)
        {
            var aic = new AllItemCollections(travelRoutes: travelRoutes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("TravelRouteSetID", _travelRouteSetID);
            //cmd.AddParameter("Name",Name);
            if (_travelRoute != null) {
                cmd.AddParameter("TravelRouteID", _travelRoute.IntID);
            }
        }

        public override string ToString() => Name;

        public double CalculateTotalDistance() => TravelRoute.CalculateTotalDistance();
    }
}
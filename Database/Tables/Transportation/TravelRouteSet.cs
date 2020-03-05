using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.Transportation {
    public class TravelRouteSet : DBBaseElement {
        public const string TableName = "tblTravelRouteSet";

        [ItemNotNull] [NotNull] private readonly ObservableCollection<TravelRouteSetEntry> _routes =
            new ObservableCollection<TravelRouteSetEntry>();

        [CanBeNull] private string _description;

        public TravelRouteSet([NotNull] string name,
                              [CanBeNull]int? pID,
                              [NotNull] string connectionString,
                              [CanBeNull] string description, [NotNull] string guid) : base(name,
            TableName, connectionString, guid)
        {
            _description = description;
            ID = pID;
            AreNumbersOkInNameForIntegrityCheck = true;
            TypeDescription = "Distance Set";
        }

        [CanBeNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [ItemNotNull]
        [UsedImplicitly]
        [NotNull]
        public ObservableCollection<TravelRouteSetEntry> TravelRoutes => _routes;

        public void AddRoute([NotNull] TravelRoute route,bool savetodb = true)
        {
            if (route == null) {
                throw new LPGException("Can't add a null route.");
            }

            if (_routes.Any(x => x.TravelRoute == route)) {
                Logger.Error("Route " + route.PrettyName+  " was already added");
                return;
            }
            if (route.ConnectionString != ConnectionString) {
                throw new LPGException("A location from another DB was just added!");
            }
            var entry = new TravelRouteSetEntry(null, IntID, ConnectionString, route.Name, route, System.Guid.NewGuid().ToString());
            _routes.Add(entry);
            if(savetodb) {
                entry.SaveToDB();
            }

            _routes.Sort();
        }

        [NotNull]
        private static TravelRouteSet AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name", false, "(no name)", ignoreMissingFields);
            var description = dr.GetString("Description", false, "(no description)", ignoreMissingFields);
            var id = dr.GetIntFromLong("ID", false, ignoreMissingFields, -1);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new TravelRouteSet(name, id, connectionString, description, guid);
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new
            TravelRouteSet(FindNewName(isNameTaken, "New Travel Route Set "), null, connectionString, "", System.Guid.NewGuid().ToString());

        public void DeleteEntry([NotNull] TravelRouteSetEntry ld)
        {
            ld.DeleteFromDB();
            _routes.Remove(ld);
        }

        public void DeleteEntry([NotNull] TravelRoute ld)
        {
            var trse = TravelRoutes.Where(x => x.TravelRoute == ld).ToList();
            if (trse.Count == 0) {
                return;
            }
            foreach (TravelRouteSetEntry entry in trse) {
                entry.DeleteFromDB();
                _routes.Remove(entry);
            }
        }

        [NotNull]
        [UsedImplicitly]
        public static TravelRouteSet ImportFromItem([NotNull] TravelRouteSet toImport,
            [NotNull] Simulator dstSim)
        {
            var loc = new TravelRouteSet(toImport.Name, null,dstSim.ConnectionString, toImport.Description, toImport.Guid);
            loc.SaveToDB();
            foreach (var routeEntry in toImport.TravelRoutes) {
                var dstroute = GetItemFromListByName(dstSim.TravelRoutes.MyItems,
                    routeEntry.TravelRoute.Name);
                if (dstroute == null) {
                    Logger.Error("Travel Route not found, skipping.");
                    continue;
                }
                loc.AddRoute(dstroute);
            }
            return loc;
        }

        private static bool IsCorrectTravelRouteSetParent([NotNull] DBBase parent, [NotNull] DBBase child)
        {
            var hd = (TravelRouteSetEntry) child;
            if (parent.ID == hd.TravelRouteSetID) {
                var site = (TravelRouteSet) parent;
                site.TravelRoutes.Add(hd);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TravelRouteSet> result, [NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [NotNull] ObservableCollection<TravelRoute> travelRoutes)
        {
            var aic = new AllItemCollections(travelRoutes: travelRoutes);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
            var ld = new ObservableCollection<TravelRouteSetEntry>();
            TravelRouteSetEntry.LoadFromDatabase(ld, connectionString, ignoreMissingTables, travelRoutes);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(ld), IsCorrectTravelRouteSetParent,
                ignoreMissingTables);
        }

        public override void SaveToDB()
        {
            base.SaveToDB();
            foreach (var routeEntry in _routes) {
                routeEntry.SaveToDB();
            }
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Name", Name);
            if(_description!=null) {
                cmd.AddParameter("Description", _description);
            }
        }

        [NotNull]
        public override DBBase ImportFromGenericItem([NotNull] DBBase toImport, [NotNull] Simulator dstSim)
            => ImportFromItem((TravelRouteSet)toImport,dstSim);

        [NotNull]
        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}
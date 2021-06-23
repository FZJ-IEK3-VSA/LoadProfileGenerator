using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.Transportation {
    public class TravelRouteSet : DBBaseElement {
        public const string TableName = "tblTravelRouteSet";

        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<TravelRouteSetEntry> _routes =
            new ObservableCollection<TravelRouteSetEntry>();

        [CanBeNull] private string _description;
        [CanBeNull] private AffordanceTaggingSet _affordanceTaggingSet;


        public TravelRouteSet([JetBrains.Annotations.NotNull] string name,
                              [CanBeNull]int? pID,
                              [JetBrains.Annotations.NotNull] string connectionString,
                              [CanBeNull] string description, [NotNull] StrGuid guid, AffordanceTaggingSet affordanceTaggingSet) : base(name,
            TableName, connectionString, guid)
        {
            _description = description;
            ID = pID;
            AreNumbersOkInNameForIntegrityCheck = true;
            TypeDescription = "Distance Set";
            _affordanceTaggingSet = affordanceTaggingSet;
        }

        [CanBeNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [CanBeNull]
        [UsedImplicitly]
        public AffordanceTaggingSet AffordanceTaggingSet
        {
            get => _affordanceTaggingSet;
            set => SetValueWithNotify(value, ref _affordanceTaggingSet, false, nameof(AffordanceTaggingSet));
        }

        [ItemNotNull]
        [UsedImplicitly]
        [JetBrains.Annotations.NotNull]
        public ObservableCollection<TravelRouteSetEntry> TravelRoutes => _routes;

        public void AddRoute([JetBrains.Annotations.NotNull] TravelRoute route, int minimumAge, int maximumAge, PermittedGender gender, int affordanceTaggingSetEntryID, double weight, bool savetodb = true)
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
            var entry = new TravelRouteSetEntry(null, IntID, ConnectionString, route.Name, route, minimumAge, maximumAge, gender, affordanceTaggingSetEntryID, weight, System.Guid.NewGuid().ToStrGuid());
            _routes.Add(entry);
            if(savetodb) {
                entry.SaveToDB();
            }

            _routes.Sort();
        }

        [JetBrains.Annotations.NotNull]
        private static TravelRouteSet AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name", false, "(no name)", ignoreMissingFields);
            var description = dr.GetString("Description", false, "(no description)", ignoreMissingFields);
            var id = dr.GetIntFromLong("ID", false, ignoreMissingFields, -1);
            var affordanceTaggingSetID = dr.GetIntFromLong("AffordanceTaggingSetID", false, ignoreMissingFields, -1);
            var affordanceTaggingSet = aic.AffordanceTaggingSets.FirstOrDefault(x => x.IntID == affordanceTaggingSetID);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new TravelRouteSet(name, id, connectionString, description, guid, affordanceTaggingSet);
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString) => new
            TravelRouteSet(FindNewName(isNameTaken, "New Travel Route Set "), null, connectionString, "", System.Guid.NewGuid().ToStrGuid(), null);

        public void DeleteEntry([JetBrains.Annotations.NotNull] TravelRouteSetEntry ld)
        {
            ld.DeleteFromDB();
            _routes.Remove(ld);
        }

        public void DeleteEntry([JetBrains.Annotations.NotNull] TravelRoute ld)
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

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static TravelRouteSet ImportFromItem([JetBrains.Annotations.NotNull] TravelRouteSet toImport,
            [JetBrains.Annotations.NotNull] Simulator dstSim)
        {
            var loc = new TravelRouteSet(toImport.Name, null,dstSim.ConnectionString, toImport.Description, toImport.Guid, toImport.AffordanceTaggingSet);
            dstSim.TravelRouteSets.Items.Add(loc);
            loc.SaveToDB();
            foreach (var routeEntry in toImport.TravelRoutes) {
                var dstroute = GetItemFromListByName(dstSim.TravelRoutes.Items,
                    routeEntry.TravelRoute.Name);
                if (dstroute == null) {
                    Logger.Error("Travel Route not found, skipping.");
                    continue;
                }
                int affordanceTaggingSetEntryID = -1; // Todo: lookup correct ID
                loc.AddRoute(dstroute, routeEntry.MinimumAge, routeEntry.MaximumAge, routeEntry.Gender, affordanceTaggingSetEntryID, routeEntry.Weight);
            }
            return loc;
        }

        private static bool IsCorrectTravelRouteSetParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (TravelRouteSetEntry) child;
            if (parent.ID == hd.TravelRouteSetID) {
                var site = (TravelRouteSet) parent;
                site.TravelRoutes.Add(hd);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TravelRouteSet> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TravelRoute> travelRoutes, ObservableCollection<AffordanceTaggingSet> affordanceTaggingSets)
        {
            var aic = new AllItemCollections(travelRoutes: travelRoutes, affordanceTaggingSets: affordanceTaggingSets);
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

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("Name", Name);
            if(_description!=null) {
                cmd.AddParameter("Description", _description);
            }
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((TravelRouteSet)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}
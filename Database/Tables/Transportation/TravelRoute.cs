#region header

// // ProfileGenerator DatabaseIO changed: 2017 03 22 16:39

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.Transportation {
    public class TravelRoute : DBBaseElement {
        public const string TableName = "tblTravelRoutes";

        [ItemNotNull] [JetBrains.Annotations.NotNull] private readonly ObservableCollection<TravelRouteStep> _steps = new ObservableCollection<TravelRouteStep>();
        [CanBeNull] private string _description;

        [CanBeNull] private Site _siteA;

        [CanBeNull] private Site _siteB;
        [CanBeNull] private string _routeKey;

        [CanBeNull]
        public string RouteKey {
            get => _routeKey;
            set => SetValueWithNotify(value, ref _routeKey, nameof(RouteKey));
        }

        public override string PrettyName {
            get {
                    return Name + " (" + _steps.Count + " steps, " + _steps.Select(x => x.Distance).Sum() + " m)";
            }
        }
        public override void SaveToDB()
        {
            base.SaveToDB();
            using (Connection con = new Connection(ConnectionString)) {
                con.Open();
                using (var tr = con.BeginTransaction()) {
                    foreach (var travelRouteStep in _steps) {
                        travelRouteStep.SaveToDB(con);
                    }

                    tr.Commit();
                }
            }
        }
        public TravelRoute([CanBeNull]int? pID, [JetBrains.Annotations.NotNull] string connectionString, [JetBrains.Annotations.NotNull] string name, [CanBeNull] string description, [CanBeNull] Site siteA,
            [CanBeNull] Site siteB, [NotNull] StrGuid guid, [CanBeNull] string routeKey)
            : base(name, TableName, connectionString, guid)
        {
            TypeDescription = "Travel Route";
            ID = pID;
            _siteA = siteA;
            _siteB = siteB;
            _routeKey = routeKey;
            _description = description;
        }

        [UsedImplicitly]
        [CanBeNull]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        [CanBeNull]
        [UsedImplicitly]
        public Site SiteA {
            get => _siteA;
            set => SetValueWithNotify(value, ref _siteA, false, nameof(SiteA));
        }

        [CanBeNull]
        [UsedImplicitly]
        public Site SiteB {
            get => _siteB;
            set => SetValueWithNotify(value, ref _siteB, false, nameof(SiteB));
        }
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public ObservableCollection<TravelRouteStep> Steps => _steps;

        [UsedImplicitly]
        //TODO: use and remove used implictily
        public void AddStep([JetBrains.Annotations.NotNull] TravelRouteStep step)
        {
            if (step == null) {
                throw new LPGException("Can't add a null step.");
            }
            if (step.ConnectionString != ConnectionString) {
                throw new LPGException("A step from another DB was just added!");
            }

            _steps.Add(step);
            step.SaveToDB();
            _steps.Sort();
            OnPropertyChanged(nameof(PrettyName));
        }

        public void AddStep([JetBrains.Annotations.NotNull] string name, [JetBrains.Annotations.NotNull] TransportationDeviceCategory category, double distance, int stepNumber, [CanBeNull] string stepKey, bool save = true)
        {
            var step = new TravelRouteStep(null, IntID, ConnectionString,
                name, category, distance, stepNumber, System.Guid.NewGuid().ToStrGuid(), stepKey);
            _steps.Add(step);
            if (save) {
                step.SaveToDB();
                _steps.Sort();
            }
            OnPropertyChanged(nameof(PrettyName));
        }


        [JetBrains.Annotations.NotNull]
        private static TravelRoute AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var name = dr.GetString("Name","(no name)");
            var description = dr.GetString("Description","");
            var siteAID = dr.GetIntFromLong("SiteAID");
            var siteBID = dr.GetIntFromLong("SiteBID");
            var routeKey = dr.GetString("RouteKey", false, "", ignoreMissingFields);
            var siteA = aic.Sites.FirstOrDefault(x => x.ID == siteAID);
            var siteB = aic.Sites.FirstOrDefault(x => x.ID == siteBID);
            var guid = GetGuid(dr, ignoreMissingFields);
            var locdev = new TravelRoute(id, connectionString, name,
                description, siteA, siteB, guid,routeKey);
            return locdev;
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([JetBrains.Annotations.NotNull] Func<string, bool> isNameTaken, [JetBrains.Annotations.NotNull] string connectionString) => new TravelRoute(
            null, connectionString, FindNewName(isNameTaken, "New Travel Route "),
            "", null, null, System.Guid.NewGuid().ToStrGuid(),"");

        public void DeleteStep([JetBrains.Annotations.NotNull] TravelRouteStep step)
        {
            step.DeleteFromDB();
            _steps.Remove(step);
            OnPropertyChanged(nameof(PrettyName));
        }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public static TravelRoute ImportFromItem([JetBrains.Annotations.NotNull] TravelRoute toImport, [JetBrains.Annotations.NotNull] Simulator dstSim)
        {
            Site a = GetItemFromListByName(dstSim.Sites.Items, toImport.SiteA?.Name);
            Site b = GetItemFromListByName(dstSim.Sites.Items, toImport.SiteB?.Name);

            var route = new TravelRoute(null, dstSim.ConnectionString,
                toImport.Name, toImport.Description, a, b, toImport.Guid, toImport.RouteKey);
            route.SaveToDB();
            foreach (var step in toImport._steps) {
                var td = GetItemFromListByName(dstSim.TransportationDeviceCategories.Items,
                    step.TransportationDeviceCategory.Name);
                if (td == null) {
                    Logger.Error("Could not find a transportation device category while importing. Skipping.");
                    continue;
                }
                route.AddStep(step.Name, td, step.Distance, step.StepNumber, step.StepKey);
            }
            return route;
        }

        private static bool IsCorrectTravelRouteParent([JetBrains.Annotations.NotNull] DBBase parent, [JetBrains.Annotations.NotNull] DBBase child)
        {
            var hd = (TravelRouteStep) child;
            if (parent.ID == hd.RouteID) {
                var route = (TravelRoute) parent;
                route._steps.Add(hd);
                return true;
            }
            return false;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            /*      if (_siteA == null)
                  {
                      message = "Site A not found";
                      return false;
                  }
                  if (_siteA == null)
                  {
                      message = "Site B not found";
                      return false;
                  }*/
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TravelRoute> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<TransportationDeviceCategory> transportationDeviceCategories,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Site> sites)
        {
            var aic = new AllItemCollections(sites: sites);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
            var ld = new ObservableCollection<TravelRouteStep>();
            TravelRouteStep.LoadFromDatabase(ld, connectionString, transportationDeviceCategories, ignoreMissingTables);
            SetSubitems(new List<DBBase>(result), new List<DBBase>(ld), IsCorrectTravelRouteParent,
                ignoreMissingTables);
            foreach (TravelRoute route in result) {
                route.Steps.Sort();
            }
        }

        protected override void SetSqlParameters(Command cmd)
        {
            if (_siteA != null) {
                cmd.AddParameter("SiteAID", _siteA.IntID);
            }
            if (_siteB != null) {
                cmd.AddParameter("SiteBID", _siteB.IntID);
            }
            cmd.AddParameter("Name", Name);
            if(_description!=null) {
                cmd.AddParameter("Description", _description);
            }

            if (_routeKey != null) {
                cmd.AddParameter("RouteKey", _routeKey);
            }
        }

        public override string ToString() => Name;

        public double CalculateTotalDistance()
        {
            double sum = 0;
            foreach (TravelRouteStep travelRouteStep in _steps) {
                sum += travelRouteStep.Distance;
            }

            return sum;
        }

        public override DBBase ImportFromGenericItem(DBBase toImport, Simulator dstSim)
            => ImportFromItem((TravelRoute)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();

        [JetBrains.Annotations.NotNull]
        public TravelRoute MakeACopy([JetBrains.Annotations.NotNull] Simulator sim)
        {
            var newRoute = sim.TravelRoutes.CreateNewItem(sim.ConnectionString);
            newRoute.Name = Name;
            newRoute.Description = Description;
            newRoute.SiteA = SiteA;
            newRoute.SiteB = SiteB;
            newRoute.RouteKey = _routeKey;
            foreach (var step in _steps)
            {
                newRoute.AddStep(step.Name, step.TransportationDeviceCategory, step.Distance, step.StepNumber, step.StepKey);
            }

            return newRoute;
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.Transportation {
    public class TravelRouteStep : DBBase, IComparable<TravelRouteStep> {
        [CanBeNull]
        public string StepKey { get; }
        public const string TableName = "tblTravelRouteSteps";

        [CanBeNull] private readonly TransportationDeviceCategory _deviceCategory;

        private readonly double _distance;
        private readonly int _routeID;

        private readonly int _stepNumber;

        public TravelRouteStep([CanBeNull]int? pID, int routeID, [NotNull] string connectionString, [NotNull] string name,
            [CanBeNull] TransportationDeviceCategory deviceCategory, double distance, int stepNumber, StrGuid guid, [CanBeNull] string stepKey) : base(name, TableName,
            connectionString, guid)
        {
            StepKey = stepKey;
            TypeDescription = "Travel Route Step";
            ID = pID;
            _deviceCategory = deviceCategory;
            _distance = distance;
            _routeID = routeID;
            _stepNumber = stepNumber;
        }

        [UsedImplicitly]
        public double Distance => _distance;

        [UsedImplicitly]
        public int RouteID => _routeID;

        [UsedImplicitly]
        public int StepNumber => _stepNumber;

        [NotNull]
        [UsedImplicitly]
        public TransportationDeviceCategory TransportationDeviceCategory => _deviceCategory ?? throw new InvalidOperationException();

        public int CompareTo([CanBeNull] TravelRouteStep other)
        {
            if (ReferenceEquals(this, other)) {
                return 0;
            }
            if (other is null) {
                return 1;
            }

            var stepComparison = _stepNumber.CompareTo(other._stepNumber);
            if (stepComparison != 0) {
                return stepComparison;
            }
            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        [NotNull]
        private static TravelRouteStep AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var routeid = dr.GetIntFromLong("TravelRouteID");
            var transportationDeviceID = dr.GetIntFromLong("TravelDeviceID");
            var stepNumber = dr.GetIntFromLong("StepNumber");
            var distance = dr.GetDouble("Distance");
            var transportationDeviceCategory = aic.TransportationDeviceCategories.FirstOrDefault(x => x.ID == transportationDeviceID);
            var name = dr.GetString("Name",false,"(no name)",ignoreMissingFields);
            var stepKey = dr.GetString("StepKey", false, "", ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            var step = new TravelRouteStep(id, routeid, connectionString, name, transportationDeviceCategory, distance,
                stepNumber, guid, stepKey);
            return step;
        }

        protected override bool IsItemLoadedCorrectly(out string message)
        {
            if (_deviceCategory == null) {
                message = "Transportation Device not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TravelRouteStep> result, [NotNull] string connectionString,
            [ItemNotNull] [NotNull] ObservableCollection<TransportationDeviceCategory> transportationDeviceCategories, bool ignoreMissingTables)
        {
            var aic = new AllItemCollections(transportationDeviceCategories: transportationDeviceCategories);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd)
        {
            cmd.AddParameter("TravelRouteID", _routeID);
            if (_deviceCategory != null) {
                cmd.AddParameter("TravelDeviceID", _deviceCategory.IntID);
            }
            cmd.AddParameter("Distance", _distance);
            cmd.AddParameter("StepNumber", _stepNumber);
            cmd.AddParameter("Name", Name);
            if (StepKey != null) {
                cmd.AddParameter("StepKey", StepKey);
            }
        }

        public override string ToString() => Name;
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.Transportation {
    public class TransportationDeviceCategory : DBBaseElement {
        public const string TableName = "tblTransportationDeviceCategories";
        [CanBeNull] private string _description;
        private bool _isLimitedToSingleLocation;

        public TransportationDeviceCategory([NotNull] string name, [CanBeNull] int? pID,
                                            [NotNull] string connectionString, [CanBeNull] string description,
            bool isLimitedToSingleLocation, [NotNull] string guid)
            : base(name, TableName, connectionString, guid)
        {
            _isLimitedToSingleLocation = isLimitedToSingleLocation;
            _description = description;
            ID = pID;
            AreNumbersOkInNameForIntegrityCheck = true;
            TypeDescription = "Site";
        }

        [CanBeNull]
        [UsedImplicitly]
        public string Description {
            get => _description;
            set => SetValueWithNotify(value, ref _description, nameof(Description));
        }

        public bool IsLimitedToSingleLocation {
            get => _isLimitedToSingleLocation;
            set => SetValueWithNotify(value, ref _isLimitedToSingleLocation, nameof(IsLimitedToSingleLocation));
        }

        [NotNull]
        [UsedImplicitly]
        public static DBBase CreateNewItem([NotNull] Func<string, bool> isNameTaken, [NotNull] string connectionString) => new
            TransportationDeviceCategory(FindNewName(isNameTaken, "New Transportation Device Category "), null,
                connectionString, "", false, System.Guid.NewGuid().ToString());

        [NotNull]
        [UsedImplicitly]
        public static TransportationDeviceCategory ImportFromItem([NotNull] TransportationDeviceCategory toImport,
            [NotNull] Simulator dstSim)
        {
            var loc = new TransportationDeviceCategory(toImport.Name, null,dstSim.ConnectionString,
                toImport.Description, toImport.IsLimitedToSingleLocation,toImport.Guid);
            loc.SaveToDB();
            return loc;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TransportationDeviceCategory> result,
            [NotNull] string connectionString,
            bool ignoreMissingTables)
        {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, true);
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            message = "";
            return true;
        }

        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("Name", Name);
            if(_description!=null) {
                cmd.AddParameter("Description", _description);
            }

            cmd.AddParameter("IsLimitedToSingleLocation", _isLimitedToSingleLocation);
        }

        [NotNull]
        private static TransportationDeviceCategory AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var name = dr.GetString("Name", false, "(no name)", ignoreMissingFields);
            var description = dr.GetString("Description", false, "(no description)", ignoreMissingFields);
            var id = dr.GetIntFromLong("ID", false, ignoreMissingFields, -1);
            var isLimitedToSingleLocation = dr.GetBool("IsLimitedToSingleLocation", false, ignoreMissingFields);
            var guid = GetGuid(dr, ignoreMissingFields);
            return new TransportationDeviceCategory(name, id, connectionString, description, isLimitedToSingleLocation, guid);
        }

        [NotNull]
        public override DBBase ImportFromGenericItem([NotNull] DBBase toImport, [NotNull] Simulator dstSim)
            => ImportFromItem((TransportationDeviceCategory)toImport,dstSim);

        public override List<UsedIn> CalculateUsedIns(Simulator sim) => throw new NotImplementedException();
    }
}
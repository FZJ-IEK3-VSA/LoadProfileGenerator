using System.Collections.ObjectModel;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.Validation {
    public class LoadtypeOutcome : DBBase {
        public const string TableName = "tblLoadTypeOutcomes";
        private readonly int _calculationOutcomeID;

        public LoadtypeOutcome([NotNull] string name, int calculationOutcomeID, [NotNull] string loadTypeName, double value,
            [NotNull] string connectionString,[NotNull] StrGuid guid, [CanBeNull]int? pID = null) : base(name, pID, TableName, connectionString, guid) {
            _calculationOutcomeID = calculationOutcomeID;
            LoadTypeName = loadTypeName;
            Value = value;
            TypeDescription = "Loadtype Outcome";
        }

        public int CalculationOutcomeID => _calculationOutcomeID;

        [CanBeNull]
        public string LoadTypeName { get; }

        public double Value { get; }

        [NotNull]
        private static LoadtypeOutcome AssignFields([NotNull] DataReader dr, [NotNull] string connectionString, bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var calcoutcomeID = dr.GetIntFromLong("CalculationOutcomeID", false, ignoreMissingFields, -1);
            var loadTypeName = dr.GetString("LoadTypeName");
            var value = dr.GetDouble("Value");
            var name = loadTypeName + " " + value;
            var guid = GetGuid(dr, ignoreMissingFields);
            return new LoadtypeOutcome(name, calcoutcomeID, loadTypeName, value, connectionString,guid, id);
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            if (_calculationOutcomeID < 0) {
                message = "Calculation outcome not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<LoadtypeOutcome> result, [NotNull] string connectionString,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters([NotNull] Command cmd) {
            cmd.AddParameter("CalculationOutcomeID", _calculationOutcomeID);
            if(LoadTypeName != null) {
                cmd.AddParameter("LoadTypeName", LoadTypeName);
            }

            cmd.AddParameter("Value", Value);
        }
    }
}
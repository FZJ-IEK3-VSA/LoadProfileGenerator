using System.Collections.ObjectModel;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.Validation {
    public class LoadtypeOutcome : DBBase {
        public const string TableName = "tblLoadTypeOutcomes";
        private readonly int _calculationOutcomeID;

        public LoadtypeOutcome([JetBrains.Annotations.NotNull] string name, int calculationOutcomeID, [JetBrains.Annotations.NotNull] string loadTypeName, double value,
            [JetBrains.Annotations.NotNull] string connectionString,StrGuid guid, [CanBeNull]int? pID = null) : base(name, pID, TableName, connectionString, guid) {
            _calculationOutcomeID = calculationOutcomeID;
            LoadTypeName = loadTypeName;
            Value = value;
            TypeDescription = "Loadtype Outcome";
        }

        public int CalculationOutcomeID => _calculationOutcomeID;

        [CanBeNull]
        public string LoadTypeName { get; }

        public double Value { get; }

        [JetBrains.Annotations.NotNull]
        private static LoadtypeOutcome AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var calcoutcomeID = dr.GetIntFromLong("CalculationOutcomeID", false, ignoreMissingFields, -1);
            var loadTypeName = dr.GetString("LoadTypeName");
            var value = dr.GetDouble("Value");
            var name = loadTypeName + " " + value;
            var guid = GetGuid(dr, ignoreMissingFields);
            return new LoadtypeOutcome(name, calcoutcomeID, loadTypeName, value, connectionString,guid, id);
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_calculationOutcomeID < 0) {
                message = "Calculation outcome not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<LoadtypeOutcome> result, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            cmd.AddParameter("CalculationOutcomeID", _calculationOutcomeID);
            if(LoadTypeName != null) {
                cmd.AddParameter("LoadTypeName", LoadTypeName);
            }

            cmd.AddParameter("Value", Value);
        }
    }
}
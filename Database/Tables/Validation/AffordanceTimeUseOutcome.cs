using System;
using System.Collections.ObjectModel;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.Validation {
    public class AffordanceTimeUseOutcome : DBBase {
        public const string TableName = "tblAffordanceOutcomes";
        private readonly int _calculationOutcomeID;

        public AffordanceTimeUseOutcome([NotNull] string name, int calculationOutcomeID, [NotNull] string affordanceName,
            double timeInMinutes,
            [NotNull] string personName, int executions,
            [NotNull] string connectionString,[NotNull] string guid, [CanBeNull] int? pID = null)
            : base(name, pID, TableName, connectionString, guid) {
            _calculationOutcomeID = calculationOutcomeID;
            AffordanceName = affordanceName;
            PersonName = personName;
            TimeInMinutes = timeInMinutes;
            Executions = executions;
            TypeDescription = "Loadtype Outcome";
        }

        [NotNull]
        public string AffordanceName { get; }

        public int CalculationOutcomeID => _calculationOutcomeID;
        public int Executions { get; }
        [NotNull]
        public string PersonName { get; }

        public double TimeInMinutes { get; }

        [NotNull]
        [UsedImplicitly]
        public string TimePerExecution {
            get {
                if (Executions == 0) {
                    return "00:00:00";
                }
                var ts = TimeSpan.FromMinutes(TimeInMinutes / Executions);
                return ts.ToString();
            }
        }

        [NotNull]
        private static AffordanceTimeUseOutcome AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields,
            [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID");
            var calcoutcomeID = dr.GetIntFromLong("CalculationOutcomeID", false, ignoreMissingFields, -1);
            var affordanceName = dr.GetString("AffordanceName");
            var personName = dr.GetString("PersonName");
            var timeInMinutes = dr.GetDouble("TimeInMinutes");
            var executions = dr.GetIntFromLong("Executions");
            var name = personName + " " + affordanceName;
            var guid = GetGuid(dr, ignoreMissingFields);
            return new AffordanceTimeUseOutcome(name, calcoutcomeID, affordanceName, timeInMinutes, personName,
                executions, connectionString,guid, id);
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            if (_calculationOutcomeID < 0) {
                message = "Calculation outcome not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<AffordanceTimeUseOutcome> result,
            [NotNull] string connectionString,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters([NotNull] Command cmd) {
            cmd.AddParameter("CalculationOutcomeID", _calculationOutcomeID);
            cmd.AddParameter("PersonName", PersonName);
            cmd.AddParameter("AffordanceName", AffordanceName);
            cmd.AddParameter("TimeInMinutes", TimeInMinutes);
            cmd.AddParameter("Executions", Executions);
        }
    }
}
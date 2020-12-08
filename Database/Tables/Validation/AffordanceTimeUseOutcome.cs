using System;
using System.Collections.ObjectModel;
using Automation;
using Database.Database;
using JetBrains.Annotations;

namespace Database.Tables.Validation {
    public class AffordanceTimeUseOutcome : DBBase {
        public const string TableName = "tblAffordanceOutcomes";
        private readonly int _calculationOutcomeID;

        public AffordanceTimeUseOutcome([JetBrains.Annotations.NotNull] string name, int calculationOutcomeID, [JetBrains.Annotations.NotNull] string affordanceName,
            double timeInMinutes,
            [JetBrains.Annotations.NotNull] string personName, int executions,
            [JetBrains.Annotations.NotNull] string connectionString,StrGuid guid, [CanBeNull] int? pID = null)
            : base(name, pID, TableName, connectionString, guid) {
            _calculationOutcomeID = calculationOutcomeID;
            AffordanceName = affordanceName;
            PersonName = personName;
            TimeInMinutes = timeInMinutes;
            Executions = executions;
            TypeDescription = "Loadtype Outcome";
        }

        [JetBrains.Annotations.NotNull]
        public string AffordanceName { get; }

        public int CalculationOutcomeID => _calculationOutcomeID;
        public int Executions { get; }
        [JetBrains.Annotations.NotNull]
        public string PersonName { get; }

        public double TimeInMinutes { get; }

        [JetBrains.Annotations.NotNull]
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

        [JetBrains.Annotations.NotNull]
        private static AffordanceTimeUseOutcome AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingFields,
            [JetBrains.Annotations.NotNull] AllItemCollections aic) {
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

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_calculationOutcomeID < 0) {
                message = "Calculation outcome not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<AffordanceTimeUseOutcome> result,
            [JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables) {
            var aic = new AllItemCollections();
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            cmd.AddParameter("CalculationOutcomeID", _calculationOutcomeID);
            cmd.AddParameter("PersonName", PersonName);
            cmd.AddParameter("AffordanceName", AffordanceName);
            cmd.AddParameter("TimeInMinutes", TimeInMinutes);
            cmd.AddParameter("Executions", Executions);
        }
    }
}
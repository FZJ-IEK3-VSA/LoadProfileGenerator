using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Automation;
using Common.Enums;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.BasicHouseholds {
    public class AffVariableOperation : DBBase {
        public const string TableName = "tblAffordanceVariableOps";
        [CanBeNull]
        private readonly int? _affordanceID;
        [CanBeNull]
        private readonly string _description;

        [CanBeNull] private readonly Location _location;

        [CanBeNull] private readonly Variable _variable;

        private readonly VariableAction _variableAction;
        private readonly VariableExecutionTime _variableExecutionTime;
        private readonly VariableLocationMode _variableLocationMode;
        private readonly double _variableValue;

        public AffVariableOperation(double variableValue,
                                    [CanBeNull]int? id,
                                    [CanBeNull] int? affordanceID,
                                    [JetBrains.Annotations.NotNull] string connectionString,
            VariableLocationMode variableLocationMode, [CanBeNull] Location location, VariableAction variableAction,
            [CanBeNull] Variable variable, [CanBeNull] string description, VariableExecutionTime variableExecutionTime,[JetBrains.Annotations.NotNull] string name, StrGuid guid)
            : base(name, TableName, connectionString, guid) {
            _variableValue = variableValue;
            ID = id;
            _affordanceID = affordanceID;
            TypeDescription = "Affordance Variable Operation";
            _variableLocationMode = variableLocationMode;
            _location = location;
            _variableAction = variableAction;
            _variable = variable;
            _description = description;
            _variableExecutionTime = variableExecutionTime;
        }

        public VariableAction Action => _variableAction;
        [CanBeNull]
        public int? AffordanceID => _affordanceID;

        [CanBeNull]
        public string Description => _description;

        public VariableExecutionTime ExecutionTime => _variableExecutionTime;
        [JetBrains.Annotations.NotNull]
        public string ExecutionTimeStr
            => VariableExecutionTimeHelper.ConvertToVariableDescription(_variableExecutionTime);
        [CanBeNull]
        public Location Location => _location;

        public VariableLocationMode LocationMode => _variableLocationMode;

        public double Value => _variableValue;
        [JetBrains.Annotations.NotNull]
        public Variable Variable => _variable ?? throw new InvalidOperationException();

        [JetBrains.Annotations.NotNull]
        private static AffVariableOperation AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull]string connectionString,
            bool ignoreMissingFields, [JetBrains.Annotations.NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID", true, ignoreMissingFields, -1);
            var affordanceID = dr.GetIntFromLong("AffordanceID", false, ignoreMissingFields, -1);

            var value = dr.GetDouble("Value", false, 0, ignoreMissingFields);
            var locmode =
                (VariableLocationMode) dr.GetIntFromLong("LocationMode", false, ignoreMissingFields);
            var locationID = dr.GetIntFromLong("LocationID", false, ignoreMissingFields, -1);
            var loc = aic.Locations.FirstOrDefault(x => x.IntID == locationID);
            var ta = (VariableAction) dr.GetIntFromLong("Action", false, ignoreMissingFields);
            var variableID = dr.GetIntFromLong("VariableID", false, ignoreMissingFields, -1);
            var va = aic.Variables.FirstOrDefault(x => x.ID == variableID);
            var description = dr.GetString("Description", false, string.Empty, ignoreMissingFields);
            var executionTime =
                (VariableExecutionTime) dr.GetIntFromLong("ExecutionTime", false, ignoreMissingFields);
            var name = "(no name)";
            if (va != null) {
                name = va.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var tup = new AffVariableOperation(value, id, affordanceID, connectionString, locmode, loc,
                ta, va, description, executionTime, name, guid);
            return tup;
        }

        protected override bool IsItemLoadedCorrectly(out string message) {
            if (_variable == null) {
                message = "Variable not found";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<AffVariableOperation> result,[JetBrains.Annotations.NotNull] string connectionString,
            bool ignoreMissingTables, [JetBrains.Annotations.NotNull][ItemNotNull] ObservableCollection<Location> locations,
            [JetBrains.Annotations.NotNull][ItemNotNull]    ObservableCollection<Variable> variables) {
            var aic = new AllItemCollections(locations: locations, variables: variables);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            if (_affordanceID != null) {
                cmd.AddParameter("AffordanceID", _affordanceID);
            }

            cmd.AddParameter("Value", _variableValue);
            cmd.AddParameter("LocationMode", (int) _variableLocationMode);
            if (_location != null) {
                cmd.AddParameter("LocationID", _location.IntID);
            }
            cmd.AddParameter("Action", _variableAction);
            if (_variable != null) {
                cmd.AddParameter("VariableID", _variable.IntID);
            }
            if (_description != null) {
                cmd.AddParameter("Description", _description);
            }
            cmd.AddParameter("ExecutionTime", _variableExecutionTime);
        }

        public override string ToString() {
            var s = string.Empty;
            if (_variable != null) {
                s += "Variable " + _variable.PrettyName + ", ";
            }
            s += VariableActionHelper.ConvertToVariableDescription(_variableAction) + ", ";
            s += "Value " + _variableValue.ToString(CultureInfo.CurrentCulture);
            return s;
        }
    }
}
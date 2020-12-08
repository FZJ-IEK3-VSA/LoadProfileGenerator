using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Automation;
using Common.Enums;
using Database.Database;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace Database.Tables.BasicHouseholds {
    public class SubAffordanceVariableOp : DBBase {
        public const string TableName = "tblSubAffordanceVariableOps";

        private readonly int _affordanceID;
        private readonly VariableExecutionTime _executionTime;

        [CanBeNull] private readonly Location _location;

        private readonly double _value;

        [CanBeNull] private readonly Variable _variable;

        private readonly VariableAction _variableAction;
        private readonly VariableLocationMode _variableLocationMode;

        public SubAffordanceVariableOp(double value,[CanBeNull] int? id, int affordanceID, [JetBrains.Annotations.NotNull] string connectionString,
            VariableLocationMode variableLocationMode, [CanBeNull] Location location, VariableAction variableAction,
            [CanBeNull] Variable variable, VariableExecutionTime executionTime, [JetBrains.Annotations.NotNull] string name, StrGuid guid) : base(name, TableName,
            connectionString, guid) {
            _value = value;
            ID = id;
            _affordanceID = affordanceID;
            TypeDescription = "SubAffordance Variable Operation";
            _variableLocationMode = variableLocationMode;
            _location = location;
            _variableAction = variableAction;
            _variable = variable;
            _executionTime = executionTime;
        }

        public int AffordanceID => _affordanceID;

        public VariableExecutionTime ExecutionTime => _executionTime;

        [JetBrains.Annotations.NotNull]
        public string ExecutionTimeStr => VariableExecutionTimeHelper.ConvertToVariableDescription(_executionTime);

        [CanBeNull]
        public Location Location => _location;

        public VariableLocationMode LocationMode => _variableLocationMode;

        [JetBrains.Annotations.NotNull]
        public string LocationModeStr
            => VariableLocationModeHelper.ConvertToVariableActionDescription(_variableLocationMode);

        public double Value => _value;

        [CanBeNull]
        public Variable Variable => _variable;

        public VariableAction VariableAction => _variableAction;

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string VariableActionStr => VariableActionHelper.ConvertToVariableDescription(_variableAction);

        [JetBrains.Annotations.NotNull]
        private static SubAffordanceVariableOp AssignFields([JetBrains.Annotations.NotNull] DataReader dr, [JetBrains.Annotations.NotNull] string connectionString,
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
            var variable = aic.Variables.FirstOrDefault(x => x.ID == variableID);
            var executionTime =
                (VariableExecutionTime) dr.GetIntFromLong("ExecutionTime", false, ignoreMissingFields);
            var name = "(no name)";
            if (variable != null) {
                name = variable.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);
            var tup = new SubAffordanceVariableOp(value, id, affordanceID, connectionString, locmode,
                loc, ta, variable, executionTime, name, guid);
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

        public static void LoadFromDatabase([ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<SubAffordanceVariableOp> result,
            [JetBrains.Annotations.NotNull] string connectionString, bool ignoreMissingTables, [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Location> locations,
            [ItemNotNull] [JetBrains.Annotations.NotNull] ObservableCollection<Variable> variables) {
            var aic = new AllItemCollections(locations: locations, variables: variables);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters(Command cmd) {
            cmd.AddParameter("AffordanceID", _affordanceID);

            cmd.AddParameter("Value", _value);
            cmd.AddParameter("LocationMode", (int) _variableLocationMode);
            if (_location != null) {
                cmd.AddParameter("LocationID", _location.IntID);
            }
            cmd.AddParameter("Action", _variableAction);
            if (_variable != null) {
                cmd.AddParameter("VariableID", _variable.IntID);
            }
            cmd.AddParameter("ExecutionTime", _executionTime);
        }

        public override string ToString() {
            var s = string.Empty;
            if (_variable != null) {
                s += "Variable " + _variable.PrettyName + ", ";
            }
            s += VariableActionHelper.ConvertToVariableDescription(_variableAction) + ", ";
            s += "Value " + _value.ToString(CultureInfo.CurrentCulture);
            return s;
        }
    }
}
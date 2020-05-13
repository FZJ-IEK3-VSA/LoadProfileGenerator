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
    public class AffVariableRequirement : DBBase {
        public const string TableName = "tblAffordanceVariableReqs";

        [CanBeNull]
        private readonly int? _affordanceID;
        private readonly VariableCondition _condition;
        [NotNull] private readonly string _description;

        [CanBeNull] private readonly Location _location;

        private readonly double _value;

        [CanBeNull] private readonly Variable _variable;

        private readonly VariableLocationMode _variableLocationMode;

        public AffVariableRequirement(double value, [CanBeNull]int? id, [CanBeNull]int? affordanceID, [NotNull] string connectionString,
            VariableLocationMode variableLocationMode, [CanBeNull] Location location, VariableCondition condition,
            [CanBeNull] Variable variable,
            [NotNull] string description, [NotNull] string name, [NotNull] StrGuid guid) : base(name, TableName, connectionString, guid) {
            _value = value;
            ID = id;
            _affordanceID = affordanceID;
            TypeDescription = "Affordance Variable Requirement";
            _variableLocationMode = variableLocationMode;
            _location = location;
            _condition = condition;
            _variable = variable;
            _description = description;
        }
        [CanBeNull]
        public int? AffordanceID => _affordanceID;

        public VariableCondition Condition => _condition;

        [NotNull]
        public string ConditionStr => VariableConditionHelper.ConvertToVariableDescription(_condition);

        [NotNull]
        public string Description => _description;

        [CanBeNull]
        public Location Location => _location;

        public VariableLocationMode LocationMode => _variableLocationMode;

        public double Value => _value;
        [NotNull]
        public Variable Variable => _variable ?? throw new InvalidOperationException();

        [NotNull]
        private static AffVariableRequirement AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic) {
            var id = dr.GetIntFromLong("ID", true, ignoreMissingFields, -1);
            var affordanceID = dr.GetIntFromLong("AffordanceID", false, ignoreMissingFields, -1);
            var variableValue = dr.GetDouble("Value", false, 0, ignoreMissingFields);
            var locmode =
                (VariableLocationMode) dr.GetIntFromLong("LocationMode", false, ignoreMissingFields);
            var locationID = dr.GetIntFromLong("LocationID", false, ignoreMissingFields, -1);
            var loc = aic.Locations.FirstOrDefault(x => x.IntID == locationID);
            var ta = (VariableCondition) dr.GetIntFromLong("Condition", false, ignoreMissingFields);
            var variableID = dr.GetIntFromLong("VariableID", false, ignoreMissingFields, -1);
            var va = aic.Variables.FirstOrDefault(x => x.ID == variableID);
            var description = dr.GetString("Description", false, string.Empty, ignoreMissingFields);
            var name = "(no name)";
            if (va != null) {
                name = va.Name;
            }
            var guid = GetGuid(dr, ignoreMissingFields);


            var tup = new AffVariableRequirement(variableValue, id, affordanceID, connectionString,
                locmode, loc, ta, va, description, name, guid);
            return tup;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message) {
            if (_variable == null) {
                message = "Variable not found.";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<AffVariableRequirement> result,
            [NotNull] string connectionString,
            bool ignoreMissingTables, [ItemNotNull] [NotNull] ObservableCollection<Location> locations,
            [ItemNotNull] [NotNull] ObservableCollection<Variable> variables) {
            var aic = new AllItemCollections(locations: locations, variables: variables);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }

        protected override void SetSqlParameters([NotNull] Command cmd) {
            if (_affordanceID != null) {
                cmd.AddParameter("AffordanceID", _affordanceID);
            }
            cmd.AddParameter("Value", _value);
            cmd.AddParameter("LocationMode", (int) _variableLocationMode);
            if (_location != null) {
                cmd.AddParameter("LocationID", _location.IntID);
            }
            cmd.AddParameter("Condition", _condition);
            if (_variable != null) {
                cmd.AddParameter("VariableID", _variable.IntID);
            }
            cmd.AddParameter("Description", _description);
        }

        public override string ToString() {
            var s = string.Empty;
            if (_variable != null) {
                s += "Variable " + _variable.PrettyName + ", ";
            }
            s += VariableConditionHelper.ConvertToVariableDescription(_condition) + ", ";
            s += "Value " + _value.ToString(CultureInfo.CurrentCulture);
            return s;
        }
    }
}
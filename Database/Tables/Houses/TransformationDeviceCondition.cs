using System.Collections.ObjectModel;
using System.Linq;
using Database.Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace Database.Tables.Houses {

    public class TransformationDeviceCondition : DBBase {
        public const string TableName = "tblTransformationDeviceConditions";


        private readonly double _maxValue;
        private readonly double _minValue;
        private Variable _variable;

        public TransformationDeviceCondition([CanBeNull]int? pID,
             double minValue, double maxValue,
             int transformationDeviceID, [NotNull] string connectionString, [NotNull] string name,
                                             [NotNull] string guid, Variable variable) : base(
            name, TableName, connectionString, guid)
        {
            ID = pID;
            _minValue = minValue;
            _maxValue = maxValue;
            _variable = variable;
            TransformationDeviceID = transformationDeviceID;
            TypeDescription = "Transformation Device Load Type";
        }


        public double MaxValue => _maxValue;

        public double MinValue => _minValue;

        public Variable Variable => _variable;

        [NotNull]
        [UsedImplicitly]
        public string TextDescription => GetTextDescription();

        public int TransformationDeviceID { get; }

        [NotNull]
        private static TransformationDeviceCondition AssignFields([NotNull] DataReader dr, [NotNull] string connectionString,
            bool ignoreMissingFields, [NotNull] AllItemCollections aic)
        {
            var id = dr.GetIntFromLong("ID");
            var transformationDeviceID = dr.GetIntFromLong("TransformationDeviceID");
            var variableID = dr.GetIntFromLong("ConditionLoadTypeID", false, ignoreMissingFields, -1);
            var variable = aic.Variables.FirstOrDefault(mylt => mylt.ID == variableID);
            var minValue = dr.GetDouble("MinValue", false, 0, ignoreMissingFields);
            var maxValue = dr.GetDouble("MaxValue", false, 10000, ignoreMissingFields);
            var name = "no name";
            var guid = GetGuid(dr, ignoreMissingFields);

            var tdlt = new TransformationDeviceCondition(id, minValue, maxValue,  transformationDeviceID,
                connectionString, name, guid, variable);
            return tdlt;
        }

        [NotNull]
        private string GetTextDescription()
        {
            var variableName = "(no load type set)";
            if (Variable != null) {
                variableName = Variable.Name;
            }
            return "Value for " + variableName + " between " + _minValue + " and " + _maxValue;
        }

        protected override bool IsItemLoadedCorrectly([NotNull] out string message)
        {
            if (_variable == null) {
                message = "Variable not found.";
                return false;
            }
            message = "";
            return true;
        }

        public static void LoadFromDatabase([ItemNotNull] [NotNull] ObservableCollection<TransformationDeviceCondition> result,
            [NotNull] string connectionString, [ItemNotNull] [NotNull] ObservableCollection<VLoadType> loadTypes,
             bool ignoreMissingTables, ObservableCollection<Variable> variables)
        {
            var aic = new AllItemCollections(loadTypes: loadTypes, variables:variables);
            LoadAllFromDatabase(result, connectionString, TableName, AssignFields, aic, ignoreMissingTables, false);
        }


        protected override void SetSqlParameters([NotNull] Command cmd)
        {
            cmd.AddParameter("TransformationDeviceID", TransformationDeviceID);
            if (_variable != null) {
                cmd.AddParameter("ConditionLoadTypeID", _variable.IntID);
            }
            cmd.AddParameter("MinValue", _minValue);
            cmd.AddParameter("MaxValue", _maxValue);
        }

        public override string ToString() => Name;
    }
}
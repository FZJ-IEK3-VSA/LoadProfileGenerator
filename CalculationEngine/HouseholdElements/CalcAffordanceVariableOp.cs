using Common.Enums;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements {
    public class CalcAffordanceVariableOp {
        public CalcAffordanceVariableOp([NotNull] string name, double value, [NotNull] CalcLocation location, VariableAction variableAction,
            VariableExecutionTime executionTime, [NotNull] string variableGuid)
        {
            Name = name;
            Value = value;
            CalcLocation = location;
            VariableAction = variableAction;
            ExecutionTime = executionTime;
            VariableGuid = variableGuid;
        }

        [NotNull]
        public CalcLocation CalcLocation { get; }
        public VariableExecutionTime ExecutionTime { get; }
        [NotNull]
        public string VariableGuid { get; }

        [NotNull]
        public string Name { get; }
        public double Value { get; }
        public VariableAction VariableAction { get; }
    }
}
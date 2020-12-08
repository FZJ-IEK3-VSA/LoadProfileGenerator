using Automation;
using Common.Enums;

namespace CalculationEngine.HouseholdElements {
    public class CalcAffordanceVariableOp {
        public CalcAffordanceVariableOp([JetBrains.Annotations.NotNull] string name, double value, [JetBrains.Annotations.NotNull] CalcLocation location, VariableAction variableAction,
            VariableExecutionTime executionTime, StrGuid variableGuid)
        {
            Name = name;
            Value = value;
            CalcLocation = location;
            VariableAction = variableAction;
            ExecutionTime = executionTime;
            VariableGuid = variableGuid;
        }

        [JetBrains.Annotations.NotNull]
        public CalcLocation CalcLocation { get; }
        public VariableExecutionTime ExecutionTime { get; }
        public StrGuid VariableGuid { get; }

        [JetBrains.Annotations.NotNull]
        public string Name { get; }
        public double Value { get; }
        public VariableAction VariableAction { get; }
    }
}
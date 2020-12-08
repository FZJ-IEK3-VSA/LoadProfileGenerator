using Automation;
using Common.Enums;

namespace Common.CalcDto {
    public class CalcAffordanceVariableOpDto
    {
        public CalcAffordanceVariableOpDto([JetBrains.Annotations.NotNull]string name, double value, [JetBrains.Annotations.NotNull] string locationName,
                                           StrGuid locationGuid, VariableAction variableAction,
                                           VariableExecutionTime executionTime, StrGuid variableGuid)
        {
            Name = name;
            Value = value;
            LocationName = locationName;
            LocationGuid = locationGuid;
            VariableAction = variableAction;
            ExecutionTime = executionTime;
            VariableGuid = variableGuid;
        }

        public VariableExecutionTime ExecutionTime { get; }
        public StrGuid VariableGuid { get; }

        [JetBrains.Annotations.NotNull] public string Name { get; }
        public double Value { get; }
        [JetBrains.Annotations.NotNull] public string LocationName { get; }
        public StrGuid LocationGuid { get; }
        public VariableAction VariableAction { get; }
    }
}
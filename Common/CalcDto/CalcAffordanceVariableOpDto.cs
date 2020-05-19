using Automation;
using Common.Enums;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcAffordanceVariableOpDto
    {
        public CalcAffordanceVariableOpDto([NotNull]string name, double value, [NotNull] string locationName,
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

        [NotNull] public string Name { get; }
        public double Value { get; }
        [NotNull] public string LocationName { get; }
        public StrGuid LocationGuid { get; }
        public VariableAction VariableAction { get; }
    }
}
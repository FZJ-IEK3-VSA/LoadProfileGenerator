using Automation;
using Common.Enums;

namespace Common.CalcDto {
    public class VariableRequirementDto
    {
        public StrGuid VariableGuid { get; }

        public VariableRequirementDto([JetBrains.Annotations.NotNull]string name, double value, [JetBrains.Annotations.NotNull] string location, StrGuid locationGuid,
                                      VariableCondition variableCondition,
                                      StrGuid variableGuid)
        {
            VariableGuid = variableGuid;
            Name = name;
            Value = value;
            CalcLocationName = location;
            LocationGuid = locationGuid;
            VariableCondition = variableCondition;
        }
        [JetBrains.Annotations.NotNull]
        public string CalcLocationName { get; }
        public StrGuid LocationGuid { get; }
        [JetBrains.Annotations.NotNull]
        public string Name { get; }
        public double Value { get; }
        public VariableCondition VariableCondition { get; }
    }
}
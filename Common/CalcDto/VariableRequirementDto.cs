using Automation;
using Common.Enums;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class VariableRequirementDto
    {
        [NotNull]
        public StrGuid VariableGuid { get; }

        public VariableRequirementDto([NotNull]string name, double value, [NotNull] string location, [NotNull] StrGuid locationGuid,
                                      VariableCondition variableCondition,
                                      [NotNull]StrGuid variableGuid)
        {
            VariableGuid = variableGuid;
            Name = name;
            Value = value;
            CalcLocationName = location;
            LocationGuid = locationGuid;
            VariableCondition = variableCondition;
        }
        [NotNull]
        public string CalcLocationName { get; }
        [NotNull]
        public StrGuid LocationGuid { get; }
        [NotNull]
        public string Name { get; }
        public double Value { get; }
        public VariableCondition VariableCondition { get; }
    }
}
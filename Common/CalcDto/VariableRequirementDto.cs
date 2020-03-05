using Common.Enums;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class VariableRequirementDto
    {
        [NotNull]
        public string VariableGuid { get; }

        public VariableRequirementDto([NotNull]string name, double value, [NotNull] string location, [NotNull] string locationGuid,
                                      VariableCondition variableCondition,
                                      [NotNull]string variableGuid)
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
        public string LocationGuid { get; }
        [NotNull]
        public string Name { get; }
        public double Value { get; }
        public VariableCondition VariableCondition { get; }
    }
}
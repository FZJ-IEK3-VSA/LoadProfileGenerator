using Automation;
using Common.Enums;

namespace CalculationEngine.HouseholdElements {
    public class VariableRequirement {
        [JetBrains.Annotations.NotNull]
        private readonly CalcVariableRepository _repository;
        private readonly StrGuid _variableGuid;

        public VariableRequirement([JetBrains.Annotations.NotNull] string name, double value, [JetBrains.Annotations.NotNull] string location, StrGuid locationGuid,
                                                 VariableCondition variableCondition, [JetBrains.Annotations.NotNull] CalcVariableRepository repository,
                                                 StrGuid variableGuid)
        {
            _repository = repository;
            _variableGuid = variableGuid;
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

        public bool IsMet()
        {
            var value = _repository.GetValueByGuid(_variableGuid);
            if (VariableConditionHelper.CheckCondition(value, VariableCondition,
                Value)) {
                return true;
            }

            return false;
        }
    }
}
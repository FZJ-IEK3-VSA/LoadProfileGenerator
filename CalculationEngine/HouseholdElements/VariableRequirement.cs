using Automation;
using Common.Enums;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements {
    public class VariableRequirement {
        [NotNull]
        private readonly CalcVariableRepository _repository;
        [NotNull]
        private readonly StrGuid _variableGuid;

        public VariableRequirement([NotNull] string name, double value, [NotNull] string location, [NotNull] StrGuid locationGuid,
                                                 VariableCondition variableCondition, [NotNull] CalcVariableRepository repository,
                                                 [NotNull] StrGuid variableGuid)
        {
            _repository = repository;
            _variableGuid = variableGuid;
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
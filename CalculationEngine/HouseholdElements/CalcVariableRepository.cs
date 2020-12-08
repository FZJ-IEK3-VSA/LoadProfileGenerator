using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements
{
    public class CalcVariable {
        public CalcVariable([JetBrains.Annotations.NotNull] string name, StrGuid guid, double value,
                            [JetBrains.Annotations.NotNull] string locationName, StrGuid locationGuid, [JetBrains.Annotations.NotNull] HouseholdKey householdKey)
        {
            Name = name;
            Guid = guid;
            Value = value;
            LocationName = locationName;
            LocationGuid = locationGuid;
            HouseholdKey = householdKey;
            SqlName = (householdKey+ "_"+ locationName + "_" + Name).Replace(" ", "");
        }

        [JetBrains.Annotations.NotNull]
        public string Name { get; }
        public StrGuid Guid { get; }
        public double Value { get; set; }
        [JetBrains.Annotations.NotNull]
        public string LocationName { get; }
        public StrGuid LocationGuid { get; }
        [JetBrains.Annotations.NotNull]
        public HouseholdKey HouseholdKey { get; }
        [JetBrains.Annotations.NotNull]
        public string SqlName { get; }
    }

    public class CalcVariableRepository
    {
        [JetBrains.Annotations.NotNull]
        private readonly Dictionary<StrGuid, CalcVariable> _variablesByGuid = new Dictionary<StrGuid, CalcVariable>();
        [JetBrains.Annotations.NotNull]
        private readonly VariableOperator _variableOperator;

        public CalcVariableRepository()
        {
            _variableOperator = new VariableOperator(this);
        }

        public void AddExecutionEntry([JetBrains.Annotations.NotNull] string name, double value, [JetBrains.Annotations.NotNull] CalcLocation location, VariableAction variableAction,
                                      [JetBrains.Annotations.NotNull] TimeStep timeStep, StrGuid variableGuid)
        {
            _variableOperator.AddEntry(name,value,location,variableAction,
                timeStep,variableGuid);
        }

        public void RegisterVariable([JetBrains.Annotations.NotNull] CalcVariable variable)
        {
            _variablesByGuid.Add(variable.Guid,variable);
        }

        public double GetValueByGuid(StrGuid variableGuid)
        {
            if (!_variablesByGuid.ContainsKey(variableGuid)) {
                throw new LPGException("Could not find the variable with the guid:" + variableGuid);
            }
            return _variablesByGuid[variableGuid].Value;
        }

        public CalcVariable GetVariableByGuid(StrGuid variableGuid)
        {
            if (!_variablesByGuid.ContainsKey(variableGuid))
            {
                throw new LPGException("Could not find the variable with the guid:" + variableGuid);
            }
            return _variablesByGuid[variableGuid];
        }

        public void SetValueByGuid(StrGuid variableGuid, double value)
        {
            _variablesByGuid[variableGuid].Value = value;
        }

        public void AddToValueByGuid(StrGuid variableGuid, double value)
        {
            _variablesByGuid[variableGuid].Value += value;
        }

        public void SubstractFromValueByGuid(StrGuid variableGuid, double value)
        {
            _variablesByGuid[variableGuid].Value -= value;
        }

        public void Execute([JetBrains.Annotations.NotNull] TimeStep timestep)
        {
            _variableOperator.Execute(timestep);
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<CalcVariable> GetAllVariables()
        {
            return _variablesByGuid.Values.ToList();
        }

        public bool IsVariableRegistered(StrGuid variableGuid)
        {
            return _variablesByGuid.ContainsKey(variableGuid);
        }
    }
}

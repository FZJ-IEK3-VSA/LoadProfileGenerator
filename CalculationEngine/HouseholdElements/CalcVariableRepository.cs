﻿using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements
{
    public class CalcVariable {
        public CalcVariable([NotNull] string name, [NotNull] string guid, double value, [NotNull] string locationName, [NotNull] string locationGuid, [NotNull] HouseholdKey householdKey)
        {
            Name = name;
            Guid = guid;
            Value = value;
            LocationName = locationName;
            LocationGuid = locationGuid;
            HouseholdKey = householdKey;
            SqlName = (householdKey+ "_"+ locationName + "_" + Name).Replace(" ", "");
        }

        [NotNull]
        public string Name { get; }
        [NotNull]
        public string Guid { get; }
        public double Value { get; set; }
        [NotNull]
        public string LocationName { get; }
        [NotNull]
        public string LocationGuid { get; }
        [NotNull]
        public HouseholdKey HouseholdKey { get; }
        [NotNull]
        public string SqlName { get; }
    }

    public class CalcVariableRepository
    {
        [NotNull]
        private readonly Dictionary<string, CalcVariable> _variablesByGuid = new Dictionary<string, CalcVariable>();
        [NotNull]
        private readonly VariableOperator _variableOperator;

        public CalcVariableRepository()
        {
            _variableOperator = new VariableOperator(this);
        }

        public void AddExecutionEntry([NotNull] string name, double value, [NotNull] CalcLocation location, VariableAction variableAction,
                                      [NotNull] TimeStep timeStep, [NotNull] string variableGuid)
        {
            _variableOperator.AddEntry(name,value,location,variableAction,
                timeStep,variableGuid);
        }

        public void RegisterVariable([NotNull] CalcVariable variable)
        {
            _variablesByGuid.Add(variable.Guid,variable);
        }

        public double GetValueByGuid([NotNull] string variableGuid)
        {
            if (!_variablesByGuid.ContainsKey(variableGuid)) {
                throw new LPGException("Could not find the variable with the guid:" + variableGuid);
            }
            return _variablesByGuid[variableGuid].Value;
        }

        public CalcVariable GetVariableByGuid([NotNull] string variableGuid)
        {
            if (!_variablesByGuid.ContainsKey(variableGuid))
            {
                throw new LPGException("Could not find the variable with the guid:" + variableGuid);
            }
            return _variablesByGuid[variableGuid];
        }

        public void SetValueByGuid([NotNull] string variableGuid, double value)
        {
            _variablesByGuid[variableGuid].Value = value;
        }

        public void AddToValueByGuid([NotNull] string variableGuid, double value)
        {
            _variablesByGuid[variableGuid].Value += value;
        }

        public void SubstractFromValueByGuid([NotNull] string variableGuid, double value)
        {
            _variablesByGuid[variableGuid].Value -= value;
        }

        public void Execute([NotNull] TimeStep timestep)
        {
            _variableOperator.Execute(timestep);
        }

        [NotNull]
        [ItemNotNull]
        public List<CalcVariable> GetAllVariables()
        {
            return _variablesByGuid.Values.ToList();
        }

        public bool IsVariableRegistered([NotNull] string variableGuid)
        {
            return _variablesByGuid.ContainsKey(variableGuid);
        }
    }
}

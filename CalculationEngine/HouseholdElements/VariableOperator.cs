using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using JetBrains.Annotations;

namespace CalculationEngine.HouseholdElements {
    public class VariableOperator {
        [JetBrains.Annotations.NotNull]
        private CalcVariableRepository Repository { get; }

        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        private readonly List<ExecutionEntry> _entries = new List<ExecutionEntry>();

        public VariableOperator([JetBrains.Annotations.NotNull] CalcVariableRepository repository)
        {
            Repository = repository;
        }

        public void AddEntry([JetBrains.Annotations.NotNull] string name, double value, [JetBrains.Annotations.NotNull] CalcLocation location, VariableAction variableAction,
            [JetBrains.Annotations.NotNull] TimeStep timeStep, StrGuid variableGuid)
        {
            var ee = new ExecutionEntry(name, value, location, variableAction,
                timeStep,variableGuid);
            _entries.Add(ee);
        }

        public void Execute([JetBrains.Annotations.NotNull] TimeStep timestep)
        {
            var items2Delete = new List<ExecutionEntry>();
            foreach (var entry in _entries) {
                if (entry.TimeStep == timestep) {
                    entry.Execute(Repository);
                    items2Delete.Add(entry);
                }
            }
            foreach (var entry in items2Delete) {
                _entries.Remove(entry);
            }
        }

        private class ExecutionEntry {
            private readonly StrGuid _variableGuid;

            public ExecutionEntry([JetBrains.Annotations.NotNull] string name, double value, [JetBrains.Annotations.NotNull] CalcLocation location, VariableAction variableAction,
                                  [JetBrains.Annotations.NotNull] TimeStep timeStep, StrGuid variableGuid)
            {
                _variableGuid = variableGuid;
                Name = name;
                Value = value;
                CalcLocation = location;
                VariableAction = variableAction;
                TimeStep = timeStep;
            }

            [JetBrains.Annotations.NotNull]
#pragma warning disable IDE0052 // Remove unread private members
            //for keeping track of the location
            private CalcLocation CalcLocation { [UsedImplicitly] get; }
#pragma warning restore IDE0052 // Remove unread private members
            [JetBrains.Annotations.NotNull]
#pragma warning disable IDE0052 // Remove unread private members
            //for keeping track of the name
            private string Name { [UsedImplicitly] get; }
#pragma warning restore IDE0052 // Remove unread private members

            [JetBrains.Annotations.NotNull]
            public TimeStep TimeStep { get; }

            private double Value { get; }
            private VariableAction VariableAction { get; }

            public void Execute([JetBrains.Annotations.NotNull] CalcVariableRepository repository)
            {
                switch (VariableAction) {
                    case VariableAction.SetTo:
                        repository.SetValueByGuid(_variableGuid,  Value);
                        break;
                    case VariableAction.Add:
                        repository.AddToValueByGuid(_variableGuid, Value);
                        break;
                    case VariableAction.Subtract:
                        repository.SubstractFromValueByGuid(_variableGuid, Value);
                        break;
                    default:
                        throw new LPGException("Forgotten Variable Action!");
                }
            }
        }
    }
}
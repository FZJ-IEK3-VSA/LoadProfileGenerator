using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Automation.ResultFiles;

namespace Common.Enums {
    public enum VariableAction {
        SetTo,
        Add,
        Subtract
    }

    public enum VariableExecutionTime {
        Beginning,
        EndOfPerson,
        EndofDevices
    }

    public static class VariableExecutionTimeHelper {
        [NotNull]
        [ItemNotNull]
        public static List<string> CollectAllStrings() => MakeAllEntries().Select(x => x.Description).ToList();

        public static VariableExecutionTime ConvertToEnum([NotNull] string description) {
            var entries = MakeAllEntries();
            return entries.First(x => x.Description == description).Action;
        }

        [NotNull]
        public static string ConvertToVariableDescription(VariableExecutionTime tc) {
            var entries = MakeAllEntries();
            return entries.First(x => x.Action == tc).Description;
        }

        [NotNull]
        [ItemNotNull]
        private static List<VariableExecutionTimeHelperEntry> MakeAllEntries() {
            var strings = new List<VariableExecutionTimeHelperEntry>
            {
                new VariableExecutionTimeHelperEntry(VariableExecutionTime.Beginning,
                "Beginning of the Affordance"),
                new VariableExecutionTimeHelperEntry(VariableExecutionTime.EndOfPerson,
                "When the person profile ends"),
                new VariableExecutionTimeHelperEntry(VariableExecutionTime.EndofDevices,
                "When the last device ends")
            };
            return strings;
        }

        private class VariableExecutionTimeHelperEntry {
            public VariableExecutionTimeHelperEntry(VariableExecutionTime action, [NotNull] string description) {
                Action = action;
                Description = description;
            }

            public VariableExecutionTime Action { get; }

            [NotNull]
            public string Description { get; }
        }
    }

    public static class VariableActionHelper {
        [NotNull]
        [ItemNotNull]
        public static List<string> CollectAllStrings() => MakeAllEntries().Select(x => x.Description).ToList();

        public static VariableAction ConvertToVariableAction([NotNull] string description) {
            var entries = MakeAllEntries();
            return entries.First(x => x.Description == description).Action;
        }

        [NotNull]
        public static string ConvertToVariableDescription(VariableAction tc) {
            var entries = MakeAllEntries();
            return entries.First(x => x.Action == tc).Description;
        }

        [NotNull]
        [ItemNotNull]
        private static List<VariableActionHelperEntry> MakeAllEntries() {
            var strings = new List<VariableActionHelperEntry>
            {
                new VariableActionHelperEntry(VariableAction.SetTo, "Set to"),
                new VariableActionHelperEntry(VariableAction.Add, "Add"),
                new VariableActionHelperEntry(VariableAction.Subtract, "Subtract")
            };
            return strings;
        }

        private class VariableActionHelperEntry {
            public VariableActionHelperEntry(VariableAction action, [NotNull] string description) {
                Action = action;
                Description = description;
            }

            public VariableAction Action { get; }

            [NotNull]
            public string Description { get; }
        }
    }

    public enum VariableLocationMode {
        CurrentLocation,
        OtherLocation
    }

    public static class VariableLocationModeHelper {
        [NotNull]
        [ItemNotNull]
        public static List<string> CollectAllStrings() => MakeAllEntries().Select(x => x.Description).ToList();

        public static VariableLocationMode ConvertToVariableAction([NotNull] string description) {
            var entries = MakeAllEntries();
            return entries.First(x => x.Description == description).LocationMode;
        }

        [NotNull]
        public static string ConvertToVariableActionDescription(VariableLocationMode tc) {
            var entries = MakeAllEntries();
            return entries.First(x => x.LocationMode == tc).Description;
        }

        [NotNull]
        [ItemNotNull]
        private static List<VariableLocationModeHelperHelperEntry> MakeAllEntries() {
            var strings = new List<VariableLocationModeHelperHelperEntry>
            {
                new VariableLocationModeHelperHelperEntry(VariableLocationMode.CurrentLocation,
                "Current Location (that is whereever this affordance is located)"),
                new VariableLocationModeHelperHelperEntry(VariableLocationMode.OtherLocation,
                "Other Location (for example always the kitchen)")
            };

            return strings;
        }

        private class VariableLocationModeHelperHelperEntry {
            public VariableLocationModeHelperHelperEntry(VariableLocationMode locationMode, [NotNull] string description) {
                LocationMode = locationMode;
                Description = description;
            }

            [NotNull]
            public string Description { get; }

            public VariableLocationMode LocationMode { get; }
        }
    }

    public enum VariableCondition {
        Equal,
        EqualOrGreater,
        EqualOrLess,
        Greater,
        Less
    }

    public static class VariableConditionHelper {
        public static bool CheckCondition(double currentVariableValue, VariableCondition variableCondition,
            double setValue) {
            var execute = true;
            switch (variableCondition) {
                case VariableCondition.Equal:
                    if (Math.Abs(currentVariableValue - setValue) > 0.000001) {
                        execute = false;
                    }
                    break;
                case VariableCondition.EqualOrGreater:
                    if (currentVariableValue < setValue) // this is correct because >= inverted is <
                    {
                        execute = false;
                    }
                    break;
                case VariableCondition.EqualOrLess:
                    if (currentVariableValue > setValue) {
                        execute = false;
                    }
                    break;
                case VariableCondition.Greater:
                    if (currentVariableValue <= setValue) {
                        execute = false;
                    }
                    break;
                case VariableCondition.Less:
                    if (currentVariableValue >= setValue) {
                        execute = false;
                    }
                    break;
                default:
                    throw new LPGException("Forgotten VariableCondition");
            }

            return execute;
        }

        [NotNull]
        [ItemNotNull]
        public static List<string> CollectAllStrings() => MakeAllEntries().Select(x => x.Description).ToList();

        public static VariableCondition ConvertToVariableCondition([NotNull] string description) {
            var entries = MakeAllEntries();
            return entries.First(x => x.Description == description).VariableCondition;
        }

        [NotNull]
        public static string ConvertToVariableDescription(VariableCondition tc) {
            var entries = MakeAllEntries();
            return entries.First(x => x.VariableCondition == tc).Description;
        }

        [NotNull]
        [ItemNotNull]
        private static List<TiggerConditionEntry> MakeAllEntries() {
            var strings = new List<TiggerConditionEntry>
            {
                new TiggerConditionEntry(VariableCondition.Equal, "Equal to"),
                new TiggerConditionEntry(VariableCondition.EqualOrGreater, "Equal or greater than"),
                new TiggerConditionEntry(VariableCondition.EqualOrLess, "Equal or less than"),
                new TiggerConditionEntry(VariableCondition.Greater, "Greater than"),
                new TiggerConditionEntry(VariableCondition.Less, "Less than")
            };
            return strings;
        }

        private class TiggerConditionEntry {
            public TiggerConditionEntry(VariableCondition variableCondition, [NotNull] string description) {
                VariableCondition = variableCondition;
                Description = description;
            }

            [NotNull]
            public string Description { get; }

            public VariableCondition VariableCondition { get; }
        }
    }
}
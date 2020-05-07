using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace Common.Enums {
    public enum BodilyActivityLevel
    {
        Unknown,
        Outside,
        Low,
        High
    }

    public static class BodilyActivityLevelHelper
    {
        [NotNull]
        public static Dictionary<BodilyActivityLevel, string> BodilyActivityLevelEnumDictionary { get; } =
            new Dictionary<BodilyActivityLevel, string> {
                {BodilyActivityLevel.Outside, "No internal heat gain"},
                {BodilyActivityLevel.Low, "Low activity level (100W-120W)"},
                {BodilyActivityLevel.High, "High activity level (>120W)"},
            };
    }

    public enum ActionAfterInterruption {
        GoBackToOld,
        LookForNew
    }

    public static class ActionAfterInterruptionHelper {
        [NotNull]
        [ItemNotNull]
        public static List<string> CollectAllStrings() {
            return MakeAllEntries().Select(x => x.Description).ToList();
        }

        [NotNull]
        public static string ConvertToDescription(ActionAfterInterruption tc) {
            var entries = MakeAllEntries();
            return entries.First(x => x.Action == tc).Description;
        }

        public static ActionAfterInterruption ConvertToEnum([NotNull] string description) {
            var entries = MakeAllEntries();
            return entries.First(x => x.Description == description).Action;
        }

        [NotNull]
        [ItemNotNull]
        private static List<InterruptionHandlerEntry> MakeAllEntries() {
            var strings = new List<InterruptionHandlerEntry>
            {
                new InterruptionHandlerEntry(ActionAfterInterruption.GoBackToOld,
                "Back to the old affordance"),
                new InterruptionHandlerEntry(ActionAfterInterruption.LookForNew, "Find a new affordance")
            };
            return strings;
        }

        private class InterruptionHandlerEntry {
            public InterruptionHandlerEntry(ActionAfterInterruption action, [NotNull] string description) {
                Action = action;
                Description = description;
            }

            public ActionAfterInterruption Action { get; }

            [NotNull]
            public string Description { get; }
        }
    }
}
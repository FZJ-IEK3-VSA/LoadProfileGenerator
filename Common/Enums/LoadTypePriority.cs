using System.Collections.Generic;
using Automation;

namespace Common.Enums {
    public static class LoadTypePriorityHelper {
        [JetBrains.Annotations.NotNull]
        public static Dictionary<LoadTypePriority, string> LoadTypePriorityDictionaryAll { get; } =
            new Dictionary<LoadTypePriority, string> {
                {LoadTypePriority.Mandatory, "Mandatory"},
                {LoadTypePriority.RecommendedForHouseholds, "Recommended for households"},
                {LoadTypePriority.RecommendedForHouses, "Recommended for households and houses"},
                {LoadTypePriority.OptionalLoadtypes, "Optional"},
                {LoadTypePriority.All, "All"}
            };

        [JetBrains.Annotations.NotNull]
        public static Dictionary<LoadTypePriority, string> LoadTypePriorityDictionarySelection { get; } =
            new Dictionary<LoadTypePriority, string> {
                {LoadTypePriority.Mandatory, "Mandatory"},
                {LoadTypePriority.RecommendedForHouseholds, "Recommended for households"},
                {LoadTypePriority.RecommendedForHouses, "Recommended for households and houses"},
                {LoadTypePriority.OptionalLoadtypes, "Optional"}
            };
    }
}

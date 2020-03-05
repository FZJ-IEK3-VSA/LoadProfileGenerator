using JetBrains.Annotations;
using System.Collections.Generic;
using Automation;

namespace Common.Enums {
    public static class LoadTypePriorityHelper {
        [NotNull]
        public static Dictionary<LoadTypePriority, string> LoadTypePriorityDictionaryAll { get; } =
            new Dictionary<LoadTypePriority, string> {
                {LoadTypePriority.Mandatory, "Mandatory"},
                {LoadTypePriority.RecommendedForHouseholds, "Recommended for households"},
                {LoadTypePriority.RecommendedForHouses, "Recommended for households and houses"},
                {LoadTypePriority.Optional, "Optional"},
                {LoadTypePriority.All, "All"}
            };

        [NotNull]
        public static Dictionary<LoadTypePriority, string> LoadTypePriorityDictionarySelection { get; } =
            new Dictionary<LoadTypePriority, string> {
                {LoadTypePriority.Mandatory, "Mandatory"},
                {LoadTypePriority.RecommendedForHouseholds, "Recommended for households"},
                {LoadTypePriority.RecommendedForHouses, "Recommended for households and houses"},
                {LoadTypePriority.Optional, "Optional"}
            };
    }
}

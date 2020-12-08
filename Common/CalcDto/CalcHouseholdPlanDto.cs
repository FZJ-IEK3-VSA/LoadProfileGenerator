using System;
using System.Collections.Generic;

namespace Common.CalcDto {
    public class CalcHouseholdPlanDto {
        public CalcHouseholdPlanDto([JetBrains.Annotations.NotNull] Dictionary<string, string> affordanceTags,
            [JetBrains.Annotations.NotNull] Dictionary<string, Dictionary<string, TimeSpan>> personTagTimeUse,
            [JetBrains.Annotations.NotNull] Dictionary<CalcLoadTypeDto, Dictionary<string, double>> tagEnergyUse,
            [JetBrains.Annotations.NotNull] string householdName,
            [JetBrains.Annotations.NotNull] string taggingSetName,
            [JetBrains.Annotations.NotNull] Dictionary<string, Dictionary<string, int>> personExecutionCount, int id, [JetBrains.Annotations.NotNull] string name) {
            PersonExecutionCount = personExecutionCount;
            TaggingSetName = taggingSetName;
            AffordanceTags = affordanceTags;
            PersonTagTimeUsePlan = personTagTimeUse;
            TagEnergyUse = tagEnergyUse;
            HouseholdName = householdName;
            ID = id;
            Name = name;
        }

        [JetBrains.Annotations.NotNull]
        public Dictionary<string, string> AffordanceTags { get; }

        [JetBrains.Annotations.NotNull]
        public string HouseholdName { get; }

        public int ID { get; }

        [JetBrains.Annotations.NotNull]
        public string Name { get; }

        [JetBrains.Annotations.NotNull]
        public Dictionary<string, Dictionary<string, int>> PersonExecutionCount { get; }

        [JetBrains.Annotations.NotNull]
        public Dictionary<string, Dictionary<string, TimeSpan>> PersonTagTimeUsePlan { get; }

        [JetBrains.Annotations.NotNull]
        public Dictionary<CalcLoadTypeDto, Dictionary<string, double>> TagEnergyUse { get; }

        [JetBrains.Annotations.NotNull]
        public string TaggingSetName { get; }
    }
}
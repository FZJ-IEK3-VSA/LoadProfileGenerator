using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcHouseholdPlanDto {
        public CalcHouseholdPlanDto([NotNull] Dictionary<string, string> affordanceTags,
            [NotNull] Dictionary<string, Dictionary<string, TimeSpan>> personTagTimeUse,
            [NotNull] Dictionary<CalcLoadTypeDto, Dictionary<string, double>> tagEnergyUse,
            [NotNull] string householdName,
            [NotNull] string taggingSetName,
            [NotNull] Dictionary<string, Dictionary<string, int>> personExecutionCount, int id, [NotNull] string name) {
            PersonExecutionCount = personExecutionCount;
            TaggingSetName = taggingSetName;
            AffordanceTags = affordanceTags;
            PersonTagTimeUsePlan = personTagTimeUse;
            TagEnergyUse = tagEnergyUse;
            HouseholdName = householdName;
            ID = id;
            Name = name;
        }

        [NotNull]
        public Dictionary<string, string> AffordanceTags { get; }

        [NotNull]
        public string HouseholdName { get; }

        public int ID { get; }

        [NotNull]
        public string Name { get; }

        [NotNull]
        public Dictionary<string, Dictionary<string, int>> PersonExecutionCount { get; }

        [NotNull]
        public Dictionary<string, Dictionary<string, TimeSpan>> PersonTagTimeUsePlan { get; }

        [NotNull]
        public Dictionary<CalcLoadTypeDto, Dictionary<string, double>> TagEnergyUse { get; }

        [NotNull]
        public string TaggingSetName { get; }
    }
}
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class PersonTimeEstimate {
        public PersonTimeEstimate([JetBrains.Annotations.NotNull] string personName, double estimatedTimeUseInH, double vacationEstimateInH,
            int traitsWithoutEstimate, [JetBrains.Annotations.NotNull] string notConsideredTraits)
        {
            PersonName = personName;
            EstimatedTimeUseInH = estimatedTimeUseInH;
            VacationEstimateInH = vacationEstimateInH;
            TraitsWithoutEstimate = traitsWithoutEstimate;
            NotConsideredTraits = notConsideredTraits;
        }

        [UsedImplicitly]
        public double EstimatedTimeUseInH { get; }

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string NotConsideredTraits { get; }

        [UsedImplicitly]
        public double Percentage => (EstimatedTimeUseInH + VacationEstimateInH) / 8760 * 100;

        [JetBrains.Annotations.NotNull]
        [UsedImplicitly]
        public string PersonName { get; }

        [UsedImplicitly]
        public int TraitsWithoutEstimate { get; }

        [UsedImplicitly]
        public double VacationEstimateInH { get; }
    }
}
using JetBrains.Annotations;

namespace Database.Tables.ModularHouseholds {
    public class PersonTimeEstimate {
        public PersonTimeEstimate([NotNull] string personName, double estimatedTimeUseInH, double vacationEstimateInH,
            int traitsWithoutEstimate, [NotNull] string notConsideredTraits)
        {
            PersonName = personName;
            EstimatedTimeUseInH = estimatedTimeUseInH;
            VacationEstimateInH = vacationEstimateInH;
            TraitsWithoutEstimate = traitsWithoutEstimate;
            NotConsideredTraits = notConsideredTraits;
        }

        [UsedImplicitly]
        public double EstimatedTimeUseInH { get; }

        [NotNull]
        [UsedImplicitly]
        public string NotConsideredTraits { get; }

        [UsedImplicitly]
        public double Percentage => (EstimatedTimeUseInH + VacationEstimateInH) / 8760 * 100;

        [NotNull]
        [UsedImplicitly]
        public string PersonName { get; }

        [UsedImplicitly]
        public int TraitsWithoutEstimate { get; }

        [UsedImplicitly]
        public double VacationEstimateInH { get; }
    }
}
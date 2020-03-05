using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcDesireDto {
        [NotNull]
        public string Name { get; }
        public int DesireID { get; }
        public decimal Threshold { get; }
        public decimal DecayTime { get; }
        public decimal Value { get; }
        public decimal Weight { get; }
        public int TimestepsPerHour { get; }
        public decimal CriticalThreshold { get; }
        [CanBeNull]
        public string SharedDesireGuid { get; }
        [NotNull]
        public string SourceTrait { get; }
        [NotNull]
        public string DesireCategory { get; }

        public CalcDesireDto([NotNull]string name, int desireID, decimal threshold, decimal decayTime, decimal value,
                             decimal weight,
                             int timestepsPerHour, decimal criticalThreshold,
                             [CanBeNull] string sharedDesireGuid,
                             [NotNull]string sourceTrait, [NotNull]string desireCategory)
        {
            Name = name;
            DesireID = desireID;
            Threshold = threshold;
            DecayTime = decayTime;
            Value = value;
            Weight = weight;
            TimestepsPerHour = timestepsPerHour;
            CriticalThreshold = criticalThreshold;
            SharedDesireGuid = sharedDesireGuid;
            SourceTrait = sourceTrait;
            DesireCategory = desireCategory;
        }
    }
}
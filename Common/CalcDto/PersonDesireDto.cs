using Common.Enums;

namespace Common.CalcDto {
    public class PersonDesireDto {
        public PersonDesireDto([JetBrains.Annotations.NotNull] string name, int desireID, decimal threshold, decimal decayTime, decimal weight,
                               decimal criticalThreshold, [JetBrains.Annotations.NotNull] string sourceTrait, [JetBrains.Annotations.NotNull] string desireCategory,
                               HealthStatus healthStatus, bool isSharedDesire)
        {
            Name = name;
            DesireID = desireID;
            Threshold = threshold;
            DecayTime = decayTime;
            Weight = weight;
            CriticalThreshold = criticalThreshold;
            SourceTrait = sourceTrait;
            DesireCategory = desireCategory;
            HealthStatus = healthStatus;
            IsSharedDesire = isSharedDesire;
        }

        [JetBrains.Annotations.NotNull]
        public string Name { get; }
        public int DesireID { get; }
        public decimal Threshold { get; }
        public decimal DecayTime { get; }
        public decimal Weight { get; }

        public decimal CriticalThreshold { get; }

        //[CanBeNull] SharedDesireValue sharedDesireValue,
        [JetBrains.Annotations.NotNull]
        public string SourceTrait { get; }
        [JetBrains.Annotations.NotNull]
        public string DesireCategory { get; }
        public HealthStatus HealthStatus { get; }
        public bool IsSharedDesire { get; }
    }
}
using Common.Enums;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class PersonDesireDto {
        public PersonDesireDto([NotNull] string name, int desireID, decimal threshold, decimal decayTime, decimal weight,
                               decimal criticalThreshold, [NotNull] string sourceTrait, [NotNull] string desireCategory,
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

        [NotNull]
        public string Name { get; }
        public int DesireID { get; }
        public decimal Threshold { get; }
        public decimal DecayTime { get; }
        public decimal Weight { get; }

        public decimal CriticalThreshold { get; }

        //[CanBeNull] SharedDesireValue sharedDesireValue,
        [NotNull]
        public string SourceTrait { get; }
        [NotNull]
        public string DesireCategory { get; }
        public HealthStatus HealthStatus { get; }
        public bool IsSharedDesire { get; }
    }
}
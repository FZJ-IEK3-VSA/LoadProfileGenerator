using JetBrains.Annotations;

namespace Common.CalcDto {
    public class AvailabilityDataReferenceDto {
        public AvailabilityDataReferenceDto([NotNull]string name, [NotNull]string guid)
        {
            Name = name;
            Guid = guid;
        }
        [NotNull]
        public string Name { get; }
        [NotNull]
        public string Guid { get; }
    }
}
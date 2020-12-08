using Automation;

namespace Common.CalcDto {
    public class AvailabilityDataReferenceDto {
        public AvailabilityDataReferenceDto([JetBrains.Annotations.NotNull]string name, StrGuid guid)
        {
            Name = name;
            Guid = guid;
        }
        [JetBrains.Annotations.NotNull]
        public string Name { get; }
        public StrGuid Guid { get; }
    }
}
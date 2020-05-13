using Automation;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class AvailabilityDataReferenceDto {
        public AvailabilityDataReferenceDto([NotNull]string name, [NotNull]StrGuid guid)
        {
            Name = name;
            Guid = guid;
        }
        [NotNull]
        public string Name { get; }
        [NotNull]
        public StrGuid Guid { get; }
    }
}
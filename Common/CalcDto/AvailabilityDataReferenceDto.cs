using Automation;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class AvailabilityDataReferenceDto {
        public AvailabilityDataReferenceDto([NotNull]string name, StrGuid guid)
        {
            Name = name;
            Guid = guid;
        }
        [NotNull]
        public string Name { get; }
        public StrGuid Guid { get; }
    }
}
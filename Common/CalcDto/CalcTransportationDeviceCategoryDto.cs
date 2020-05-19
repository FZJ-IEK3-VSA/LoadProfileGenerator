using Automation;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcTransportationDeviceCategoryDto
    {
        public CalcTransportationDeviceCategoryDto([NotNull] string name, int id, bool isLimitedToSingleLocation,
                                                   StrGuid guid)
        {
            Name = name;
            ID = id;
            IsLimitedToSingleLocation = isLimitedToSingleLocation;
            Guid = guid;
        }
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        public bool IsLimitedToSingleLocation { get; }
        public StrGuid Guid { get; }
    }
}
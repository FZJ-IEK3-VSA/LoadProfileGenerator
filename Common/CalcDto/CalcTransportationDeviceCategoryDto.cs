using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcTransportationDeviceCategoryDto
    {
        public CalcTransportationDeviceCategoryDto([NotNull] string name, int id, bool isLimitedToSingleLocation,
                                                   [NotNull] string guid)
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
        [NotNull]
        public string Guid { get; }
    }
}
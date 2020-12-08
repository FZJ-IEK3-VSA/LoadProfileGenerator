
namespace Common.CalcDto {
    public class CalcChargingStationDto
    {
        public CalcChargingStationDto([JetBrains.Annotations.NotNull] CalcTransportationDeviceCategoryDto deviceCategory, [JetBrains.Annotations.NotNull]CalcLoadTypeDto gridchargingLoadType, double maxChargingPower,
                                      [JetBrains.Annotations.NotNull] CalcLoadTypeDto carChargingLoadType)
        {
            DeviceCategory = deviceCategory;
            GridChargingLoadType = gridchargingLoadType;
            CarChargingLoadType = carChargingLoadType;
            MaxChargingPower = maxChargingPower;
        }
        [JetBrains.Annotations.NotNull]
        public CalcTransportationDeviceCategoryDto DeviceCategory { get; }
        [JetBrains.Annotations.NotNull]
        public CalcLoadTypeDto GridChargingLoadType { get; }
        [JetBrains.Annotations.NotNull]
        public CalcLoadTypeDto CarChargingLoadType { get; }
        public double MaxChargingPower { get; }
    }
}
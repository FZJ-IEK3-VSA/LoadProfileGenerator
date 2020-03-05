
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcChargingStationDto
    {
        public CalcChargingStationDto([NotNull] CalcTransportationDeviceCategoryDto deviceCategory, [NotNull]CalcLoadTypeDto gridchargingLoadType, double maxChargingPower,
                                      [NotNull] CalcLoadTypeDto carChargingLoadType)
        {
            DeviceCategory = deviceCategory;
            GridChargingLoadType = gridchargingLoadType;
            CarChargingLoadType = carChargingLoadType;
            MaxChargingPower = maxChargingPower;
        }
        [NotNull]
        public CalcTransportationDeviceCategoryDto DeviceCategory { get; }
        [NotNull]
        public CalcLoadTypeDto GridChargingLoadType { get; }
        [NotNull]
        public CalcLoadTypeDto CarChargingLoadType { get; }
        public double MaxChargingPower { get; }
    }
}
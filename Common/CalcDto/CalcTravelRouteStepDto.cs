using Automation;

namespace Common.CalcDto {
    public class CalcTravelRouteStepDto  {
        [JetBrains.Annotations.NotNull]
        public string Name { get; }
        public int ID { get; }
        [JetBrains.Annotations.NotNull]
        public CalcTransportationDeviceCategoryDto TransportationDeviceCategory { get; }
        public int StepNumber { get; }
        public double DistanceInM { get; }
        public StrGuid Guid { get; }

        public CalcTravelRouteStepDto([JetBrains.Annotations.NotNull]string name, int id,
                                      [JetBrains.Annotations.NotNull] CalcTransportationDeviceCategoryDto transportationDeviceCategory, int stepNumber,
                                      double distanceInM,
                                      StrGuid guid)
        {
            Name = name;
            ID = id;
            TransportationDeviceCategory = transportationDeviceCategory;
            StepNumber = stepNumber;
            DistanceInM = distanceInM;
            Guid = guid;
        }
    }
}
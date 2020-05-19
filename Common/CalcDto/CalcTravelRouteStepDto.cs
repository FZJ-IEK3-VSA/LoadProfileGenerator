using Automation;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcTravelRouteStepDto  {
        [NotNull]
        public string Name { get; }
        public int ID { get; }
        [NotNull]
        public CalcTransportationDeviceCategoryDto TransportationDeviceCategory { get; }
        public int StepNumber { get; }
        public double DistanceInM { get; }
        public StrGuid Guid { get; }

        public CalcTravelRouteStepDto([NotNull]string name, int id,
                                      [NotNull] CalcTransportationDeviceCategoryDto transportationDeviceCategory, int stepNumber,
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
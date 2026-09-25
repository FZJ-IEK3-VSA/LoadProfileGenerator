using CalculationEngine.CitySimulation;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using System.Collections.Generic;

namespace CalculationEngine.Transportation
{
    public interface ICalcSite
    {
        string Name { get; }

        bool DeviceChangeAllowed { get; }

        CalcSite SiteCategory { get; }

        IReadOnlyCollection<CalcLocation> Locations { get; }

        List<CalcChargingStation> ChargingDevices { get; }

        PointOfInterestId? PointOfInterest { get; }

        List<CalcChargingStation> CollectChargingDevicesFor(CalcTransportationDeviceCategory category, CalcLoadType carLoadType);

        List<CalcTravelRoute> GetAllRoutesTo(TimeStep timeStep, ICalcSite dstSite, List<CalcTransportationDevice> devicesAtSrc,
            CalcPersonDto person);
    }
}
using CalculationEngine.HouseholdElements;
using Common.CalcDto;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CalculationEngine.Transportation
{
    public interface ICalcSite
    {
        string Name { get; }

        bool DeviceChangeAllowed { get; }

        CalcSite SiteCategory { get; }

        IReadOnlyCollection<CalcLocation> Locations { get; }

        List<CalcChargingStation> ChargingDevices { get; }


        bool AreCategoriesAvailable(List<CalcTransportationDeviceCategory> neededDeviceCategories,
            List<CalcTransportationDevice> vehiclepool, List<CalcTransportationDevice> devicesAtLoc,
            CalcPersonDto person, DeviceOwnershipMapping<string, CalcTransportationDevice> deviceOwnerships);

        List<CalcChargingStation> CollectChargingDevicesFor(CalcTransportationDeviceCategory category, CalcLoadType carLoadType);

        List<CalcTravelRoute> GetAllRoutesTo(ICalcSite dstSite, List<CalcTransportationDevice> devicesAtSrc,
            CalcPersonDto person, DeviceOwnershipMapping<string, CalcTransportationDevice> travelDeviceOwnerships);

        bool IsSameCategory(ICalcSite other);
    }
}
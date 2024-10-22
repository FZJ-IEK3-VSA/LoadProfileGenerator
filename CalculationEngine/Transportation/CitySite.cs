using CalculationEngine.CitySimulation;
using CalculationEngine.HouseholdElements;
using Common.CalcDto;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;

namespace CalculationEngine.Transportation
{
    /// <summary>
    /// The specific site where an affordance takes place. Combines a CalcSite object, which is used for generic
    /// transport handling, with a specific point of interest to distinguish different sites of the same type.
    /// </summary>
    /// <param name="pointOfInterest">ID of the point of interest this site refers to</param>
    /// <param name="siteCategory">the shared CalcSite object defining the site category of this CitySite object</param>
    public class CitySite(PointOfInterestId? pointOfInterest, CalcSite siteCategory) : ICalcSite, IEquatable<CitySite>
    {
        public PointOfInterestId? PointOfInterest { get; } = pointOfInterest;

        public CalcSite SiteCategory { get; } = siteCategory;

        public string Name => SiteCategory.Name + $" (POI {PointOfInterest})";

        public bool DeviceChangeAllowed => SiteCategory.DeviceChangeAllowed;

        public IReadOnlyCollection<CalcLocation> Locations => SiteCategory.Locations;

        public List<CalcChargingStation> ChargingDevices => SiteCategory.ChargingDevices;


        public bool AreCategoriesAvailable(List<CalcTransportationDeviceCategory> neededDeviceCategories, List<CalcTransportationDevice> vehiclepool, List<CalcTransportationDevice> devicesAtLoc, CalcPersonDto person, DeviceOwnershipMapping<string, CalcTransportationDevice> deviceOwnerships)
            => SiteCategory.AreCategoriesAvailable(neededDeviceCategories, vehiclepool, devicesAtLoc, person, deviceOwnerships);

        public List<CalcChargingStation> CollectChargingDevicesFor(CalcTransportationDeviceCategory category, CalcLoadType carLoadType)
            => SiteCategory.CollectChargingDevicesFor(category, carLoadType);

        public List<CalcTravelRoute> GetAllRoutesTo(ICalcSite dstSite, List<CalcTransportationDevice> devicesAtSrc, CalcPersonDto person, DeviceOwnershipMapping<string, CalcTransportationDevice> travelDeviceOwnerships)
            => SiteCategory.GetAllRoutesTo(dstSite, devicesAtSrc, person, travelDeviceOwnerships);

        public bool IsSameCategory(ICalcSite other) => SiteCategory.IsSameCategory(other);
    }
}

using Automation.ResultFiles;
using CalculationEngine.Transportation;
using Common;
using Common.CalcDto;
using System.Collections.Generic;

namespace CalculationEngine.Activities
{
    /// <summary>
    /// Stores information on a travel activity.
    /// </summary>
    /// <param name="route">the route to the destination site</param>
    public class TravelInformation(CalcTravelRoute route)
    {
        /// <summary>
        /// The route object, if this activation is a dynamic travel affordance.
        /// </summary>
        public CalcTravelRoute Route { get; } = route;

        /// <summary>
        /// List of transportation device usages for this travel activity
        /// </summary>
        public List<CalcTravelRoute.CalcTravelDeviceUseEvent>? TravelDeviceUseEvents { get; set; }

        /// <summary>
        /// The name of the travel activity
        /// </summary>
        public string TravelName => CalcAffordanceTaggingSetDto.GetTravelActivityName(Route.Name);

        public void StartTravel(TimeStep timestep, string personName, AffordanceBaseTransportDecorator affordance)
        {
            // get the route which was already determined in IsBusy and activate it
            int travelDuration = Route.Activate(timestep, personName, out var usedDeviceEvents);
            TravelDeviceUseEvents = usedDeviceEvents;

            // log transportation info
            affordance.LogTransportationStatus(timestep, Route.SiteA, travelDuration);
        }

        public void FinishTravel(TimeStep startTime, string personName, int duration, AffordanceBaseTransportDecorator affordance)
        {
            if (TravelDeviceUseEvents is null)
                throw new LPGException("Did not store the travel device use events in a travel activity.");

            // finish usage for all devices
            foreach (var deviceUse in TravelDeviceUseEvents)
            {
                deviceUse.Device.FinishTravel(startTime, affordance.Site);
            }

            int sourceAffordanceDuration = -1; // dummy value - is currently not used in transportation logging
            affordance.LogTransportationEvent(TravelDeviceUseEvents, personName, startTime!, Route.SiteA, Route, duration, sourceAffordanceDuration);
        }
    }
}

using Automation.ResultFiles;
using CalculationEngine.Transportation;
using Common;
using Common.CalcDto;
using Common.SQLResultLogging.Loggers;
using System.Collections.Generic;

namespace CalculationEngine.Activities
{
    /// <summary>
    /// Stores information on a travel activity.
    /// </summary>
    /// <param name="route">the route to the destination site</param>
    public class TravelInformation(CalcTravelRoute route, TransportationDeviceChoice deviceChoice)
    {
        /// <summary>
        /// The route object, if this activation is a dynamic travel affordance.
        /// </summary>
        public CalcTravelRoute Route { get; } = route;

        /// <summary>
        /// A collection of all alternative transportation devices the person could use for traveling.
        /// </summary>
        public TransportationDeviceChoice DeviceChoice { get; } = deviceChoice;

        /// <summary>
        /// List of transportation device usages for this travel activity
        /// </summary>
        public List<CalcTravelRoute.CalcTravelDeviceUseEvent>? TravelDeviceUseEvents { get; set; }

        /// <summary>
        /// The name of the travel activity
        /// </summary>
        public string TravelName => CalcAffordanceTaggingSetDto.GetTravelActivityName(Route.Name);

        /// <summary>
        /// Starts the travel activity by activating the route and transportation devices and logging the transportation status.
        /// </summary>
        /// <param name="timestep">current timestep and start of the travel activity</param>
        /// <param name="personName">name of the traveler</param>
        /// <param name="affordance">affordance transport decorator for the travel</param>
        public void StartTravel(TimeStep timestep, string personName, AffordanceBaseTransportDecorator affordance)
        {
            // get the route which was already determined in IsBusy and activate it
            int travelDuration = Route.Activate(timestep, personName, out var usedDeviceEvents);
            TravelDeviceUseEvents = usedDeviceEvents;

            // log transportation info
            affordance.LogTransportationStatus(timestep, Route.SiteA, travelDuration, DeviceChoice);
        }

        /// <summary>
        /// Finishes the travel activity, frees the transportation devices, and logs the transportation event.
        /// </summary>
        /// <param name="startTime">start time of the travel activity</param>
        /// <param name="endTime">end time of the travel activity</param>
        /// <param name="personName">name of the traveler</param>
        /// <param name="duration">duration of the travel activity</param>
        /// <param name="affordance">affordance transport decorator for the travel</param>
        /// <exception cref="LPGException">if the travel device use events were not correctly set</exception>
        public void FinishTravel(TimeStep startTime, TimeStep endTime, string personName, int duration, AffordanceBaseTransportDecorator affordance)
        {
            if (TravelDeviceUseEvents is null)
                throw new LPGException("Did not store the travel device use events in a travel activity.");

            // finish usage for all devices
            foreach (var deviceUse in TravelDeviceUseEvents)
            {
                deviceUse.Device.FinishTravel(endTime, affordance.Site);
            }

            // if devices may be left at the destination site, release the person's device ownership
            Route.FinishTravel(personName);

            int sourceAffordanceDuration = -1; // dummy value - is currently not used in transportation logging
            affordance.LogTransportationEvent(TravelDeviceUseEvents, personName, startTime!, Route.SiteA, Route, duration, sourceAffordanceDuration);
        }
    }
}

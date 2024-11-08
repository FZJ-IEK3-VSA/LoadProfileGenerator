using Automation.ResultFiles;
using CalculationEngine.Transportation;
using Common;
using System.Collections.Generic;

namespace CalculationEngine.Activities
{
    /// <summary>
    /// Activation of a traveling affordance
    /// </summary>
    /// <param name="route">the route to the destination site</param>
    /// <param name="sourceSite">the site from which the person set off when activating the affordance</param>
    public class TravelInformation(CalcTravelRoute route, ICalcSite sourceSite)
    {
        /// <summary>
        /// The route object, if this activation is a dynamic travel affordance.
        /// </summary>
        public CalcTravelRoute Route { get; } = route;

        /// <summary>
        /// The site the activating person was at before the affordance
        /// </summary>
        public ICalcSite SourceSite { get; } = sourceSite;

        /// <summary>
        /// List of transportation device usages for this travel activity
        /// </summary>
        public List<CalcTravelRoute.CalcTravelDeviceUseEvent>? TravelDeviceUseEvents { get; set; }

        public void StartTravel(TimeStep timestep, string personName, ICalcSite? currentSite, AffordanceBaseTransportDecorator affordance)
        {
            if (currentSite != SourceSite)
                throw new LPGException("Error in transport configuration: person is at another site than planned");

            // get the route which was already determined in IsBusy and activate it
            int travelDuration = Route.Activate(timestep, personName, out var usedDeviceEvents);
            TravelDeviceUseEvents = usedDeviceEvents;

            // log transportation info
            affordance.LogTransportationStatus(timestep, SourceSite, travelDuration);
        }

        public void FinishTravel(TimeStep startTime, string personName, int duration, AffordanceBaseTransportDecorator affordance)
        {
            if (TravelDeviceUseEvents is null)
                throw new LPGException("Did not store the travel device use events in a travel activity.");

            int sourceAffordanceDuration = -1; // dummy value - is currently not used in transportation logging
            affordance.LogTransportationEvent(TravelDeviceUseEvents, personName, startTime!, SourceSite, Route, duration, sourceAffordanceDuration);
        }
    }
}

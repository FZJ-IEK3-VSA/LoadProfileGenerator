using CalculationEngine.CitySimulation;
using CalculationEngine.Transportation;
using Common;
using System.Collections.Generic;
using static CalculationEngine.Transportation.CalcTravelRoute;

namespace CalculationEngine.HouseholdElements
{
    /// <summary>
    /// Stores information about a single activation of a remote affordance, with unknown duration.
    /// </summary>
    /// <param name="name">affordance name</param>
    /// <param name="dataSource">source of the affordance activation</param>
    /// <param name="start">timestep in which the affordance starts</param>
    /// <param name="route">the route to the destination site</param>
    /// <param name="sourceSite">the site from which the person set off when activating the affordance</param>
    public class RemoteAffordanceActivation(string name, string dataSource, TimeStep start, PointOfInterestId? destination,
        CalcTravelRoute? route, CalcSite? sourceSite, ICalcAffordanceBase affordance,
        List<CalcTravelDeviceUseEvent>? travelDeviceUseEvents = null) : IAffordanceActivation
    {
        public bool IsDetermined => false;

        public string Name { get; } = name;

        public string DataSource { get; } = dataSource;

        /// <summary>
        /// The timestep in which the activity started
        /// </summary>
        public TimeStep Start { get; } = start;

        /// <summary>
        /// ID of the point of interest where the affordance will be carried out, or where
        /// the destination of the travel is. null means the destination is at home.
        /// </summary>
        public PointOfInterestId? Destination { get; } = destination;

        /// <summary>
        /// The route object, if this activation is a dynamic travel affordance.
        /// </summary>
        public CalcTravelRoute? Route { get; } = route;

        /// <summary>
        /// The site the activating person was at before the affordance
        /// </summary>
        public CalcSite? SourceSite { get; } = sourceSite;

        public ICalcAffordanceBase Affordance { get; } = affordance;

        // TODO: only temporary solution; can be removed when travel steps are activated individually as they start
        public List<CalcTravelDeviceUseEvent> TravelDeviceUseEvents { get; } = travelDeviceUseEvents ?? [];
    }
}

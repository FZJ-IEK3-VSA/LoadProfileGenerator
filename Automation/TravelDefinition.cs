using System.Collections.Generic;

namespace Automation
{
    /// <summary>
    /// Contains information about all routes in a scenario
    /// </summary>
    public class TravelDefinition
    {
        /// <summary>
        /// The routes that are available to the persons in the house of this house job
        /// </summary>
        public List<RoutesForTimeSlot> TimeSlotRouteLists { get; set; } = [];

        /// <summary>
        /// Optionally maps all POI IDs to the corresponding Cluster IDs used in the route
        /// data objects
        /// </summary>
        public Dictionary<string, string>? PoiClusterMapping { get; set; } = null;

        /// <summary>
        /// Whether each route should be duplicated for the inverse direction
        /// </summary>
        public bool MirrorRoutes { get; set; } = false;

        /// <summary>
        /// Optional minimum age to be able to drive cars
        /// </summary>
        public int MinimumDrivingAge { get; set; } = -1;
    }
}

using System.Collections.Generic;

namespace Automation
{
    /// <summary>
    /// Stores all route data objects that can be used within the same specified time slot.
    /// </summary>
    /// <param name="timeSlot">the time slot during which the routes are available</param>
    /// <param name="routesList">the routes for the time slot</param>
    public class RoutesForTimeSlot(TimeSlot timeSlot, List<RouteData> routesList)
    {
        /// <summary>
        /// The time slot for which the routes are valid
        /// </summary>
        public TimeSlot TimeSlot { get; set; } = timeSlot;

        /// <summary>
        /// The routes that are available to the persons during the specified time slot
        /// </summary>
        public List<RouteData> Routes { get; set; } = routesList;
    }
}

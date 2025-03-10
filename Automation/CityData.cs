using System.Collections.Generic;

namespace Automation
{
    /// <summary>
    /// Specifies all relevant information about the city and the environment of the simulation targets,
    /// including all specific points of interest.
    /// </summary>
    public class CityData
    {
        /// <summary>
        /// Specifies all points of interest in the city. Each of them will be represented by
        /// a dedicated new location and site.
        /// </summary>
        public Dictionary<string, PointOfInterestData> PointsOfInterest { get; set; } = [];

        /// <summary>
        /// The routes that are available to the persons in the house of this house job
        /// </summary>
        public List<RouteData> Routes { get; set; } = [];

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
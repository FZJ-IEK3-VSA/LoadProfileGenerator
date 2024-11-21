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
    }
}
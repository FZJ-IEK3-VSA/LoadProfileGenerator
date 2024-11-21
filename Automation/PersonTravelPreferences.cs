using System.Collections.Generic;

namespace Automation
{
    /// <summary>
    /// Specifies the traveling preferences of a single person, including the points of interest they visit and
    /// the routes they take to them.
    /// </summary>
    /// <param name="PoiWeights">maps the ID of each point of interest this person visits to the corresponding weight</param>
    /// <param name="Routes">the routes this person takes for remote activities</param>
    /// <param name="MirrorRoutes">whether each route should be duplicated for the inverse direction</param>
    public record PersonTravelPreferences(Dictionary<string, int> PoiWeights, List<RouteData> Routes, bool MirrorRoutes = true);
}
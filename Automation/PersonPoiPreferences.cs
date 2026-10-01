using System.Collections.Generic;

namespace Automation
{
    /// <summary>
    /// Specifies the traveling preferences of a single person, including the points of interest they visit and
    /// the routes they take to them.
    /// </summary>
    /// <param name="PoiWeights">maps the ID of each point of interest this person visits to the corresponding weight</param>
    public record PersonPoiPreferences(Dictionary<string, double> PoiWeights);
}
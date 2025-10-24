namespace Automation
{
    /// <summary>
    /// Defines a single point of interest that will be turned into a location for traits and affordances.
    /// </summary>
    /// <param name="LocationType">the location whose role this POI will assume, replacing it in traits</param>
    /// <param name="Coordinates">coordinates of the POI</param>
    /// <param name="TimeLimit">an optional timelimit that will be imposed on all affordances at this POI</param>
    public record PointOfInterestData(JsonReference LocationType, Coordinates Coordinates, JsonReference? TimeLimit = null, int QueueCapacity = -1);

    //public record QueuePOIData(JsonReference LocationType, Coordinates Coordinates, JsonReference? TimeLimit = null, int QueueCapacity = 1)
    //    : PointOfInterestData(LocationType, Coordinates, TimeLimit);
}
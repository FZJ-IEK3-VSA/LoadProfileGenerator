namespace Automation
{
    /// <summary>
    /// Stores latitude and longitude of a location.
    /// </summary>
    /// <param name="Latitude">the latitude as number in decimal format</param>
    /// <param name="Longitude">the longitude as number in decimal format</param>
    public record Coordinates(double Latitude, double Longitude);
}

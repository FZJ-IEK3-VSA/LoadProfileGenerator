namespace Automation
{
    // TODO: I could also make RouteCollection objects that contain all route variants between one Start and End point
    // TODO: do I need both distance and delay? How do I incorporate delay from the transport model?
    public record RouteData(string Start, string Destination, int Distance, double Delay, JsonReference TransportationDeviceCategory, double Weight);
}
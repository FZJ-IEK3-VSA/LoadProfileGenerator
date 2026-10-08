using Automation.ResultFiles;
using System.Collections.Generic;

namespace Common.SQLResultLogging.Loggers
{
    /// <summary>
    /// Stores all movable devices that a person can choose from for a single travel. Only includes devices at the
    /// persons current site that the person could actually use. Does not include non-movable devices such as public
    /// transport, so a person might choose neither of these devices, or the list might be empty.
    /// </summary>
    /// <param name="HouseholdKey">the household key</param>
    /// <param name="Timestep">the starting timestep of the travel</param>
    /// <param name="PersonName">the traveling person</param>
    /// <param name="TransportationDevices">the list of available devices</param>
    /// <param name="SourceSite">the current site of the person, where the travel starts</param>
    /// <param name="DestinationSite">the destination of the travel</param>
    /// <param name="OwnedDevice">the movable device that the person took ownership of, if any</param>
    public record TransportationDeviceChoice(HouseholdKey HouseholdKey, TimeStep Timestep, string PersonName,
        List<string> TransportationDevices, string SourceSite, string DestinationSite, string OwnedDevice) : IHouseholdKey
    {
        public string OwnedDevice { get; set; } = OwnedDevice;
    }
}

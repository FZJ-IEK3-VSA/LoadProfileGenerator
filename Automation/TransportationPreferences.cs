using Automation.ResultFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automation
{
    /// <summary>
    /// A class that stores the transportation preference of a single person to a single destination site
    /// </summary>
    public class TransportationPreference

    {
        public TransportationPreference(JsonReference destinationSite, List<JsonReference> transportationDeviceCategories, List<double> weights)
        {
            DestinationSite = destinationSite;
            TransportationDeviceCategories = transportationDeviceCategories;
            Weights = weights;
            if (transportationDeviceCategories.Count != Weights.Count)
            {
                throw new LPGException("There must be exactly one weight for each transportation device category.");
            }
        }

        public JsonReference DestinationSite { get; set; }

        /// <summary>
        /// The TransportationDeviceCategories a person can choose from to reach the destination
        /// </summary>
        public List<JsonReference> TransportationDeviceCategories { get; set; }

        /// <summary>
        /// The weights associated with the TransportationDeviceCategories
        /// </summary>
        public List<double> Weights { get; set; }
    }
}

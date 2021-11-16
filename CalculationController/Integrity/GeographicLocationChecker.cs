using Common;
using Database;

namespace CalculationController.Integrity {
    internal class GeographicLocationChecker : BasicChecker {
        public GeographicLocationChecker(bool performCleanupChecks) :
            base("Geographic Locations", performCleanupChecks) {
        }

        protected override void Run(Simulator sim, CheckingOptions options) {
            foreach (var geoloc in sim.GeographicLocations.Items) {
                if (geoloc.LightTimeLimit == null) {
                    throw new DataIntegrityException("Geographic Location " + geoloc.Name +" has no definition for the time when people turn on lights at home.",
                        geoloc);
                }
                if (geoloc.LightTimeLimit.RootEntry == null)
                {
                    throw new DataIntegrityException("The TimeLimit of the Geographic Location " + geoloc.Name + " has no root entry.");
                }
                if (geoloc.SolarRadiationProfile == null)
                {
                    throw new DataIntegrityException("The Geographic Location " + geoloc.Name + " has no solar radiation profile assigned, which is " +
                        "necessary to simulate daylight-dependend activities.", geoloc);
                }
            }
        }
    }
}
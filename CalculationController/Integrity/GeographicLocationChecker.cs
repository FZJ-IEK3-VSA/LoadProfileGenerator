using Common;
using Database;

namespace CalculationController.Integrity {
    internal class GeographicLocationChecker : BasicChecker {
        public GeographicLocationChecker(bool performCleanupChecks) :
            base("Geographic Locations", performCleanupChecks) {
        }

        protected override void Run(Simulator sim) {
            foreach (var geoloc in sim.GeographicLocations.It) {
                if (geoloc.LightTimeLimit == null) {
                    throw new DataIntegrityException("Geographic Location " + geoloc.Name +" has no definition for the time when people turn on lights at home.",
                        geoloc);
                }
            }
        }
    }
}
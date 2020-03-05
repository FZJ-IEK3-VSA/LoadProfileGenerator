using System.Collections.Generic;
using Common;
using Database;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace CalculationController.Integrity
{
    public class TravelRouteChecks : BasicChecker
    {
        public TravelRouteChecks(bool performCleanupChecks) : base("Travel Route Checks", performCleanupChecks)
        {
        }

        protected override void Run([NotNull] Simulator sim)
        {
            foreach (var travelroute in sim.TravelRoutes.It) {
                if(travelroute.Steps.Count == 0) {
                    throw new DataIntegrityException("The travel route " + travelroute.PrettyName + " has no steps. Please fix.",travelroute);
                }

                if (PerformCleanupChecks) {
                    if (string.IsNullOrWhiteSpace(travelroute.RouteKey)) {
                        throw new DataIntegrityException("The travel route " + travelroute.PrettyName + " has no route key. Please fix.", travelroute);
                    }
                    bool atLeastOneKey = false;
                    foreach (var step in travelroute.Steps) {
                        if (!string.IsNullOrWhiteSpace(step.StepKey)) {
                            atLeastOneKey = true;
                        }
                    }
                    if (!atLeastOneKey) {
                        throw new DataIntegrityException("The travel route " + travelroute.PrettyName + " has not a single step key. Please fix.", travelroute);
                    }
                }
            }

            foreach (TravelRoute one in sim.TravelRoutes.It) {
                foreach (TravelRoute two in sim.TravelRoutes.It) {
                    if (one == two) {
                        continue;
                    }

                    if (one.SiteA == two.SiteB && one.SiteB == two.SiteA) {
                        if (one.RouteKey != two.RouteKey) {
                            List<BasicElement> routes = new List<BasicElement>();
                            routes.Add(one);
                            routes.Add(two);
                            throw new DataIntegrityException("The travel route keys on the matching routes " + one.PrettyName + " and " + two.PrettyName + " don't match. Please fix.", routes);

                        }
                    }
                }
            }
        }
    }
}

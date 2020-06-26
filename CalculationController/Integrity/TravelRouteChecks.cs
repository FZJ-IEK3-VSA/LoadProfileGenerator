using System.Collections.Generic;
using System.Linq;
using Common;
using Database;
using Database.Tables.Transportation;

namespace CalculationController.Integrity
{
    public class TravelRouteSetChecks : BasicChecker {
        public TravelRouteSetChecks(bool performCleanupChecks) : base("Travel Route Set Checks", performCleanupChecks)
        {
        }

        protected override void Run(Simulator sim)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            foreach (var routeSet in sim.TravelRouteSets.It) {
                var arr = routeSet.Name.Split(' ');
                var kmstr = arr.FirstOrDefault(x => x.EndsWith("km"));
                if (kmstr == null) {
                    throw new DataIntegrityException("No distance declaration in the name of the route set " + routeSet.Name, routeSet);
                }
                //var kmstr2 = kmstr.Replace("km", "");
                //bool success = int.TryParse(kmstr2, out int km);
                //if(!success) {
                    //continue;
                //}

                foreach (var route in routeSet.TravelRoutes) {
                    if (!route.TravelRoute.Name.ToLower().Contains("workplace")) {
                        continue;
                    }

                    if (!route.TravelRoute.Name.Contains(" " +kmstr)) {
                        throw new DataIntegrityException("Workplace route " + route.TravelRoute.PrettyName + " in the route set " + routeSet.Name + " does not match the distance from the name which should be " + kmstr, routeSet  );
                    }
                }
            }
        }
    }

    public class TravelRouteChecks : BasicChecker
    {
        public TravelRouteChecks(bool performCleanupChecks) : base("Travel Route Checks", performCleanupChecks)
        {
        }

        protected override void Run(Simulator sim)
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

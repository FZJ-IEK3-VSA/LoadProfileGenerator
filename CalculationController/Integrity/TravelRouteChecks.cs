using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Database;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace CalculationController.Integrity
{
    public class TravelRouteSetChecks : BasicChecker {
        public TravelRouteSetChecks(bool performCleanupChecks) : base("Travel Route Set Checks", performCleanupChecks)
        {
        }

        protected override void Run(Simulator sim, CheckingOptions options)
        {
            if (!PerformCleanupChecks) {
                return;
            }
            foreach (var routeSet in sim.TravelRouteSets.Items) {
                // determine the reference distance for the travel route set
                var arr = routeSet.Name.Split(' ');
                const string km = "km";
                var kmstr = arr.FirstOrDefault(x => x.EndsWith(km));
                if (kmstr is null)
                {
                    // this travel route set has no distance declaration
                    return;
                }
                int routeSetDistance = int.Parse(kmstr.RemoveSuffix(km)) * 1000;


                // check if each workplace route in the set fits to this distance (with a tolerance)
                const double tolerance = 1000;
                foreach (var route in routeSet.TravelRoutes) {
                    if (!route.TravelRoute.Name.ToLower().Contains("workplace")) {
                        continue;
                    }

                    double routeDistance = route.TravelRoute.CalculateTotalDistance();
                    if (routeDistance < routeSetDistance - tolerance|| routeDistance > routeSetDistance + tolerance)
                    {
                        throw new DataIntegrityException($"Workplace route {route.TravelRoute.PrettyName} in the route set {routeSet.Name} has a distance of {routeDistance/1000}km. " +
                            $"This does not match the distance specified for the travel route set, which should be {kmstr}", routeSet);
                    }
                }

                var sites = routeSet.TravelRoutes.Select(x => x.TravelRoute.SiteA).ToList();
                var sitesB = routeSet.TravelRoutes.Select(x => x.TravelRoute.SiteB).ToList();
                sites.AddRange(sitesB);
                sites = sites.Distinct().ToList();
                var hh = sim.ModularHouseholds[0];
                var sitelocs = sites.SelectMany(x => x.Locations).ToList();
                var locsAtSites = sitelocs.Select(x => x.Location).Distinct().ToList();
                foreach (var loc in hh.CollectLocations()) {
                    if (!locsAtSites.Contains(loc)) {
                        throw new DataIntegrityException("The location " + loc.PrettyName  + " in the household " + hh.PrettyName + " at travel route set  " +  routeSet.Name + " is not covered by any site. Add a route with this site.", routeSet );
                    }
                }
                CheckRouteCompleteness(routeSet,sites);
            }

        }

        public void CheckRouteCompleteness([NotNull] TravelRouteSet travelRouteSet, [NotNull] [ItemNotNull] List<Site> sites)
        {
            //figure out if every site is connected to every other site
            foreach (Site siteA in sites) {
                foreach (Site siteB in sites) {
                    if (siteB == siteA) {
                        continue;
                    }

                    var tr = travelRouteSet.TravelRoutes.FirstOrDefault(x =>
                        x.TravelRoute.SiteA == siteA && x.TravelRoute.SiteB == siteB || x.TravelRoute.SiteA == siteB && x.TravelRoute.SiteB == siteA);
                    if (tr == null) {
                        throw new DataIntegrityException("There seems to be no route from " + siteA.PrettyName + " to " + siteB.PrettyName +
                                                         " in the travel route set " + travelRouteSet.PrettyName +
                                                         ". Every site needs to be connected to every other site, since the LPG has no routing functionality yet. Please fix.");
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

        protected override void Run(Simulator sim, CheckingOptions options)
        {
            foreach (var travelroute in sim.TravelRoutes.Items) {
                if(travelroute.Steps.Count == 0) {
                    throw new DataIntegrityException("The travel route " + travelroute.PrettyName + " has no steps. Please fix.",travelroute);
                }

                // check that e.g. not two cars are used in a single route
                if (travelroute.Steps.Where(step => step.TransportationDeviceCategory.IsLimitedToSingleLocation).Count() > 1)
                {
                    throw new DataIntegrityException("The travel route " + travelroute.PrettyName + " has multiple steps with a transportation device category " +
                        "for devices that are limited to a single location (e.g. multiple \"Car Category\" steps). Only one such step is allowed at most. Please " +
                        "remove the excess steps or change their category.");
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

            // skip the following checks for the generated routes in a city simulation
            if (!options.CitySimulationEnabled)
            {
                // check if travel route keys match for routes in the opposite direction
                foreach (TravelRoute one in sim.TravelRoutes.Items)
                {
                    foreach (TravelRoute two in sim.TravelRoutes.Items)
                    {
                        if (one == two)
                        {
                            continue;
                        }

                        if (one.SiteA == two.SiteB && one.SiteB == two.SiteA)
                        {
                            if (one.RouteKey != two.RouteKey)
                            {
                                List<BasicElement> routes = [one, two];
                                throw new DataIntegrityException($"The travel route keys on the matching routes {one.PrettyName} and {two.PrettyName} don't match. Please fix.", routes);

                            }
                        }
                    }
                }
            }
            
        }
    }
}

using CalculationEngine.HouseholdElements;
using CalculationEngine.Transportation;
using Common;
using Common.JSON;
using Database;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using Common.CalcDto;
using JetBrains.Annotations;

namespace CalculationController.CalcFactories {
    public class CalcTransportationDtoFactory {
        [JetBrains.Annotations.NotNull]
        private readonly CalcLoadTypeDtoDictionary _loadTypeDict;

        public CalcTransportationDtoFactory([JetBrains.Annotations.NotNull] CalcLoadTypeDtoDictionary loadTypeDict)
        {
            _loadTypeDict = loadTypeDict;
        }

        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        public void MakeTransportationDtos([JetBrains.Annotations.NotNull] Simulator sim, [JetBrains.Annotations.NotNull] ModularHousehold mhh,
                                           [CanBeNull] TransportationDeviceSet transportationDeviceSet,
                                           [CanBeNull] TravelRouteSet travelRouteSet,
                                           [CanBeNull] ChargingStationSet chargingStationSet,
                                           [JetBrains.Annotations.NotNull][ItemNotNull] out List<CalcSiteDto> sites,
                                           [JetBrains.Annotations.NotNull][ItemNotNull] out List<CalcTransportationDeviceDto> transportationDevices,
                                           [JetBrains.Annotations.NotNull][ItemNotNull] out List<CalcTravelRouteDto> routes,
                                           [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcLocationDto> locations, [JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            if (transportationDeviceSet == null) {
                throw new LPGException("Transportationdeviceset was null");
            }
            if (travelRouteSet == null)
            {
                throw new LPGException("travelRouteSet was null");
            }
            if (chargingStationSet == null)
            {
                throw new LPGException("chargingStationSet was null");
            }

            if (travelRouteSet == null) {
                throw new LPGException("Travel Route Set was null");
            }

            var sitesFromAllTravelRoutes = MakeTravelRouteSites(mhh, travelRouteSet, out var householdSites);

            //check if all locations have sites
            CheckReachabilityofLocations(mhh.CollectLocations(), sitesFromAllTravelRoutes, mhh.Name,
                travelRouteSet.Name);
            //check if all sites are reachable from all other sites
            CheckRouteCompleteness(travelRouteSet, householdSites);
            // check if at least one route from each site to each other site is doable with the given transport
            CheckRouteTransportationDeviceCompleteness(travelRouteSet, householdSites, transportationDeviceSet);

            var categoriesDict = MakeCalcTransportationDeviceCategoryDtos(sim);

            sites = MakeCalcSiteDtos( householdSites, locations,  categoriesDict, key,chargingStationSet);

            // figure out the transportation devices
            var selectedDevices = SelectTransportationDevices(transportationDeviceSet);

            //create the calc transportation devices
            //TODO: introduce load types
            transportationDevices = MakeTransportationDevices(selectedDevices, categoriesDict,key);

            routes  = MakeTravelRoutes(travelRouteSet, householdSites,categoriesDict, sites,key);
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<CalcTransportationDeviceDto> MakeTransportationDevices([JetBrains.Annotations.NotNull][ItemNotNull] List<TransportationDevice> transportationDevices,
                                              [JetBrains.Annotations.NotNull] Dictionary<TransportationDeviceCategory, CalcTransportationDeviceCategoryDto> categoriesDict, [JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            List< CalcTransportationDeviceDto > transportationDeviceDtos = new List<CalcTransportationDeviceDto>();
            foreach (var transportationDevice in transportationDevices) {
                var loads = new List<CalcDeviceLoadDto>();
                foreach (TransportationDeviceLoad load in transportationDevice.Loads) {
                    VLoadType lt = load.LoadType;
                    if (lt == null) {
                        throw new LPGException("Loadtype was null. This should never happen. Please report");
                    }

                    if (_loadTypeDict.SimulateLoadtype(lt)) {
                        CalcLoadTypeDto loadType = _loadTypeDict.GetLoadtypeDtoByLoadType(lt);
                        loads.Add(new CalcDeviceLoadDto(transportationDevice.Name + " - " + lt.Name,
                            transportationDevice.IntID, loadType.Name, loadType.Guid,
                            0, 0, Guid.NewGuid().ToStrGuid(), load.MaxPower));
                    }
                }

                double distanceToEnergyFactor = 0;
                CalcLoadTypeDto chargingLoadType = null;
                if (transportationDevice.ChargingDistanceAmount > 0 && transportationDevice.ChargingLoadType!=null) {
                    distanceToEnergyFactor = DistanceToPowerFactor(transportationDevice.ChargingDistanceAmount, transportationDevice.ChargingEnergyAmount, transportationDevice.ChargingLoadType.ConversionFaktorPowerToSum);
                    chargingLoadType = _loadTypeDict.Ltdtodict[transportationDevice.ChargingLoadType];
                }

                if (transportationDevice.TransportationDeviceCategory == null) {
                    throw new LPGException("Transportation device category was null");
                }
                CalcTransportationDeviceDto cd = new CalcTransportationDeviceDto(transportationDevice.Name,
                    transportationDevice.IntID, categoriesDict[transportationDevice.TransportationDeviceCategory],
                    transportationDevice.SpeedInMPerSecond, loads,key, transportationDevice.TotalRangeInMeters,
                    distanceToEnergyFactor, transportationDevice.ChargingPower, chargingLoadType?.Name,chargingLoadType?.Guid, Guid.NewGuid().ToStrGuid(),
                    transportationDevice.TransportationDeviceCategory.IsLimitedToSingleLocation);
                transportationDeviceDtos.Add(cd);
            }

            return transportationDeviceDtos;
        }

        public static double DistanceToPowerFactor(double chargingDistanceAmount, double chargingEnergyAmount,
                                                   double powerToEnergyFactor)
        {
            var distanceToEnergyFactor = chargingDistanceAmount /
                                            chargingEnergyAmount * powerToEnergyFactor;
            return distanceToEnergyFactor;
        }

        [JetBrains.Annotations.NotNull]
        private static Dictionary<TransportationDeviceCategory, CalcTransportationDeviceCategoryDto> MakeCalcTransportationDeviceCategoryDtos([JetBrains.Annotations.NotNull] Simulator sim)
        {
            Dictionary<TransportationDeviceCategory, CalcTransportationDeviceCategoryDto> categoriesDict =
                new Dictionary<TransportationDeviceCategory, CalcTransportationDeviceCategoryDto>();
            foreach (TransportationDeviceCategory category in sim.TransportationDeviceCategories.Items) {
                CalcTransportationDeviceCategoryDto ctdc = new CalcTransportationDeviceCategoryDto(category.Name,
                    category.IntID, category.IsLimitedToSingleLocation, Guid.NewGuid().ToStrGuid());
                categoriesDict.Add(category, ctdc);
            }

            return categoriesDict;
        }

        public static void CheckReachabilityofLocations([JetBrains.Annotations.NotNull][ItemNotNull] List<Location> locations, [JetBrains.Annotations.NotNull][ItemNotNull] List<Site> sites,
                                                        [JetBrains.Annotations.NotNull] string calcHouseholdName, [JetBrains.Annotations.NotNull] string travelRouteSetName)
        {
            List<Location> siteLocations = sites.SelectMany(x => x.Locations.Select(y => y.Location)).ToList();

            List<Location> missingLocations = new List<Location>();
            foreach (Location hhloc in locations) {
                if (!siteLocations.Contains(hhloc)) {
                    missingLocations.Add(hhloc);
                }
            }

            if (missingLocations.Count > 0) {
                StringBuilder sb = new StringBuilder();
                foreach (Location missingLocation in missingLocations) {
                    sb.AppendLine(missingLocation.PrettyName);
                }

                throw new DataIntegrityException("The locations: " + sb + " are in the household " +
                                                 calcHouseholdName
                                                 + " but they are not represented in the travel route set " +
                                                 travelRouteSetName +
                                                 ". Every location in the household must be reachable. Please fix.");
            }
        }

        public static void CheckRouteCompleteness([JetBrains.Annotations.NotNull] TravelRouteSet travelRouteSet, [JetBrains.Annotations.NotNull][ItemNotNull] List<Site> sites)
        {
            //figure out if every site is connected to every other site
            foreach (Site siteA in sites) {
                foreach (Site siteB in sites) {
                    if (siteB == siteA) {
                        continue;
                    }

                    var tr = travelRouteSet.TravelRoutes.FirstOrDefault(x =>
                        x.TravelRoute.SiteA == siteA && x.TravelRoute.SiteB == siteB ||
                        x.TravelRoute.SiteA == siteB && x.TravelRoute.SiteB == siteA);
                    if (tr == null) {
                        throw new DataIntegrityException("There seems to be no route from " + siteA.PrettyName +
                                                         " to " + siteB.PrettyName +
                                                         " in the travel route set " + travelRouteSet.PrettyName +
                                                         ". Every site needs to be connected to every other site, since the LPG has no routing functionality yet. Please fix.");
                    }
                }
            }
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<CalcSiteDto> MakeCalcSiteDtos([JetBrains.Annotations.NotNull][ItemNotNull] List<Site> householdSites,
                                                [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcLocationDto> calcLocations,
                                                [JetBrains.Annotations.NotNull] Dictionary<TransportationDeviceCategory,
                                                CalcTransportationDeviceCategoryDto> categoriesDict,
                                                   [JetBrains.Annotations.NotNull] HouseholdKey householdKey,
                                                   [CanBeNull] ChargingStationSet chargingStationSet)
        {
            List<CalcSiteDto> calcSites = new List<CalcSiteDto>();
            //create the calcsites
            foreach (Site site in householdSites) {
                CalcSiteDto calcSite = new CalcSiteDto(site.Name, site.DeviceChangeAllowed, site.IntID, Guid.NewGuid().ToStrGuid(),householdKey);
                if (chargingStationSet != null) {
                    var chargingStationsAtSite =
                        chargingStationSet.ChargingStations.Where(x => x.Site == site).ToList();
                    foreach (var chargingStationSetEntry in chargingStationsAtSite) {
                        if (chargingStationSetEntry.CarChargingLoadType == null) {
                            throw new LPGException("Car charging load type was null");
                        }
                        if (chargingStationSetEntry.GridChargingLoadType == null)
                        {
                            throw new LPGException("Grid charging load type was null");
                        }

                        CalcLoadTypeDto carlt = _loadTypeDict.Ltdtodict[chargingStationSetEntry.CarChargingLoadType];
                        CalcLoadTypeDto gridlt = _loadTypeDict.Ltdtodict[chargingStationSetEntry.GridChargingLoadType];
                        if (chargingStationSetEntry.TransportationDeviceCategory == null) {
                            throw new LPGException("charging device category was null");
                        }

                        CalcTransportationDeviceCategoryDto cat =
                            categoriesDict[chargingStationSetEntry.TransportationDeviceCategory];
                        calcSite.AddChargingStation(gridlt, cat, chargingStationSetEntry.MaxChargingPower,carlt);
                    }
                }

                foreach (var calcloc in calcLocations) {
                    foreach (SiteLocation location in site.Locations) {
                        if (calcloc.ID == location.Location.IntID) {
                            calcSite.AddLocation(calcloc);
                        }
                    }
                }
                calcSites.Add(calcSite);
            }
            return calcSites;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private static List<CalcTravelRouteDto> MakeTravelRoutes([JetBrains.Annotations.NotNull] TravelRouteSet travelRouteSet, [JetBrains.Annotations.NotNull][ItemNotNull] List<Site> householdSites,
                                                                 [JetBrains.Annotations.NotNull] Dictionary<TransportationDeviceCategory, CalcTransportationDeviceCategoryDto> categoriesDict,
                                                                 [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcSiteDto> calcSites, [JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            List<CalcTravelRouteDto> routes = new List<CalcTravelRouteDto>();
            //make travel routes
            var neededRoutes = travelRouteSet.TravelRoutes.Where(x =>
                householdSites.Contains(x.TravelRoute.SiteA) && householdSites.Contains(x.TravelRoute.SiteB));
            foreach (TravelRouteSetEntry entry in neededRoutes) {
                if (entry.TravelRoute.SiteA == null)
                {
                    throw new LPGException("site A was null");
                }
                if (entry.TravelRoute.SiteB == null)
                {
                    throw new LPGException("site B was null");
                }
                CalcSiteDto siteA = calcSites.Single(x => x.ID == entry.TravelRoute.SiteA.IntID);
                CalcSiteDto siteB = calcSites.Single(x => x.ID == entry.TravelRoute.SiteB.IntID);
                CalcTravelRouteDto ctr = new CalcTravelRouteDto(entry.TravelRoute.Name, entry.MinimumAge, entry.MaximumAge, entry.Gender, travelRouteSet.AffordanceTaggingSet?.Name, entry.AffordanceTag?.Name, entry.Weight,
                    entry.TravelRoute.IntID, key, Guid.NewGuid().ToStrGuid(), siteA.Name, siteA.Guid,siteB.Name,siteB.Guid);
                foreach (TravelRouteStep step in entry.TravelRoute.Steps) {
                    CalcTransportationDeviceCategoryDto cat = categoriesDict[step.TransportationDeviceCategory];
                    ctr.AddTravelRouteStep(step.Name, step.IntID, cat, step.StepNumber, step.Distance,
                        Guid.NewGuid().ToStrGuid());
                }
                routes.Add(ctr);
            }

            foreach (var site in calcSites) {
                CalcTravelRouteDto ctr = new CalcTravelRouteDto("Travel Route inside the site " + site.Name, -1, -1, Common.Enums.PermittedGender.All, null, null, 1.0,
                    -1,key, Guid.NewGuid().ToStrGuid(), site.Name, site.Guid,site.Name,site.Guid);
                routes.Add(ctr);
            }
            return routes;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private static List<Site> MakeTravelRouteSites([JetBrains.Annotations.NotNull] ModularHousehold mhh, [JetBrains.Annotations.NotNull] TravelRouteSet travelRouteSet,
                                                       [JetBrains.Annotations.NotNull][ItemNotNull] out List<Site> householdSites)
        {
            //first figure out the sites in the travel route set
            List<Site> travelRouteSites = new List<Site>();
            foreach (var route in travelRouteSet.TravelRoutes) {
                travelRouteSites.Add(route.TravelRoute.SiteA);
                travelRouteSites.Add(route.TravelRoute.SiteB);
            }

            travelRouteSites = travelRouteSites.Distinct().ToList();
            //then look for all the sites in the household and make sure that they are covered by the travel route
            householdSites = new List<Site>();
            foreach (Location location in mhh.CollectLocations()) {
                Site site = travelRouteSites.FirstOrDefault(x => x.Locations.Any(y => y.Location == location));
                if (site == null) {
                    throw new LPGException("Could not find a site for the location " + location.PrettyName +
                                           " in the travel route set " + travelRouteSet.PrettyName);
                }

                if (!householdSites.Contains(site)) {
                    householdSites.Add(site);
                }
            }

            return travelRouteSites;
        }

        private static bool IsAtLeastOneRouteOk([JetBrains.Annotations.NotNull] TravelRouteSet travelRouteSet,
                                                [JetBrains.Annotations.NotNull][ItemNotNull] List<TransportationDeviceCategory> categories, [JetBrains.Annotations.NotNull] Site siteA, [JetBrains.Annotations.NotNull] Site siteB)
        {
            var tr = travelRouteSet.TravelRoutes.Where(x =>
                x.TravelRoute.SiteA == siteA && x.TravelRoute.SiteB == siteB ||
                x.TravelRoute.SiteA == siteB && x.TravelRoute.SiteB == siteA).ToList();
            bool atLeastOneRouteIsOk = false;
            foreach (TravelRouteSetEntry routeSetEntry in tr) {
                bool allstepsareok = true;
                foreach (TravelRouteStep step in routeSetEntry.TravelRoute.Steps) {
                    if (!categories.Contains(step.TransportationDeviceCategory)) {
                        allstepsareok = false;
                    }
                }

                if (allstepsareok) {
                    atLeastOneRouteIsOk = true;
                }
            }

            return atLeastOneRouteIsOk;
        }

        private static void CheckRouteTransportationDeviceCompleteness([JetBrains.Annotations.NotNull] TravelRouteSet travelRouteSet,
                                                                       [JetBrains.Annotations.NotNull][ItemNotNull] List<Site> householdSites,
                                                                       [JetBrains.Annotations.NotNull] TransportationDeviceSet transportationDeviceSet)
        {
            var devices = transportationDeviceSet.TransportationDeviceSetEntries.Select(x => x.TransportationDevice)
                .ToList();
            var categories = devices.Select(x => x.TransportationDeviceCategory).Distinct().ToList();
            //figure out if every site is connected to every other site
            foreach (Site siteA in householdSites) {
                foreach (Site siteB in householdSites) {
                    if (siteB == siteA) {
                        continue;
                    }

                    bool atLeastOneRouteIsOk = IsAtLeastOneRouteOk(travelRouteSet, categories, siteA, siteB);

                    if (!atLeastOneRouteIsOk) {
                        throw new DataIntegrityException("There seems to be no route from " + siteA.PrettyName +
                                                         " to " + siteB.PrettyName +
                                                         " that is usable by the given transportation device set. Please fix.");
                    }
                }
            }
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private static List<TransportationDevice> SelectTransportationDevices([JetBrains.Annotations.NotNull] TransportationDeviceSet transportationDeviceSet)
        {
            List<TransportationDevice> selectedDevices = new List<TransportationDevice>();
            foreach (var device in transportationDeviceSet.TransportationDeviceSetEntries)
            {
                if (!selectedDevices.Contains(device.TransportationDevice))
                {
                    selectedDevices.Add(device.TransportationDevice);
                }
            }

            return selectedDevices;
        }
    }

    public class CalcTransportationFactory {
        [JetBrains.Annotations.NotNull]
        private readonly CalcLoadTypeDictionary _loadTypeDict;

        private readonly CalcRepo _calcRepo;

        public CalcTransportationFactory( [JetBrains.Annotations.NotNull] CalcLoadTypeDictionary loadTypeDict,
                                         CalcRepo calcRepo)
        {
            _loadTypeDict = loadTypeDict;
            _calcRepo = calcRepo;
        }

        public void MakeTransportation([JetBrains.Annotations.NotNull] CalcHouseholdDto household,
                                       [JetBrains.Annotations.NotNull] DtoCalcLocationDict locDict,
                                       [JetBrains.Annotations.NotNull] CalcHousehold chh,
                                       [JetBrains.Annotations.NotNull] List<CalcAffordanceTaggingSetDto> affordanceTaggingSets)
        {
            if (household.CalcTravelRoutes == null || household.CalcSites == null ||
                household.CalcTransportationDevices == null) {
                Logger.Info("Missing something for the transportation");
                if(household.CalcTravelRoutes == null) {
                    Logger.Info("No travel routes are set");
                }

                if (household.CalcSites == null) {
                    Logger.Info("No sites are set");
                }

                if (household.CalcTransportationDevices == null) {
                    Logger.Info("No Transportation devices are set");
                }

                return;
            }

            TransportationHandler th = new TransportationHandler();
            chh.TransportationHandler = th;
            th.AddAffordanceTaggingSets(affordanceTaggingSets);
            Logger.Info("Making Calc Sites");
            var sites = MakeCalcSites(household, locDict, _loadTypeDict, th);
            foreach (var site in sites) {
                th.AddSite(site);
            }
            Logger.Info("Making travel routes");
            MakeTravelRoutes(household.CalcTravelRoutes, chh, sites, th);
            Logger.Info("Making Transportation devices");
            MakeTransportationDevice(chh, household.CalcTransportationDevices);
            Logger.Info("Setting affordances");
            SetAffordances(chh);
            Logger.Info("Finished initalizing the transportation");
        }

        private void MakeTravelRoutes([JetBrains.Annotations.NotNull][ItemNotNull] List<CalcTravelRouteDto> travelRouteDtos,
                                      [JetBrains.Annotations.NotNull] CalcHousehold chh, [JetBrains.Annotations.NotNull][ItemNotNull] List<CalcSite> sites, [JetBrains.Annotations.NotNull] TransportationHandler th)
        {
            foreach (CalcTravelRouteDto travelRouteDto in travelRouteDtos) {
                CalcSite siteA = sites.Single(x => x.Guid == travelRouteDto.SiteAGuid);
                CalcSite siteB = sites.Single(x => x.Guid == travelRouteDto.SiteBGuid);

                //if (siteA != null && siteB != null) {
                    //if either site is null, the travel route is not usable for this household
                    CalcTravelRoute travelRoute = new CalcTravelRoute(travelRouteDto.Name, travelRouteDto.MinimumAge, travelRouteDto.MaximumAge, travelRouteDto.Gender, travelRouteDto.AffordanceTaggingSetName,
                        travelRouteDto.AffordanceTagName, travelRouteDto.Weight, siteA, siteB, th.VehicleDepot, th.LocationUnlimitedDevices,  chh.HouseholdKey, travelRouteDto.Guid,_calcRepo);
                    foreach (var step in travelRouteDto.Steps) {
                        CalcTransportationDeviceCategory category = th.GetCategory(step.TransportationDeviceCategory);
                        travelRoute.AddTravelRouteStep(step.Name,  category, step.StepNumber, step.DistanceInM,
                            step.Guid);
                    }
                if (siteA != siteB) {
                    th.TravelRoutes.Add(travelRoute);
                }else {
                    th.SameSiteRoutes.Add(siteA,travelRoute);
                }

                //}
            }
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        private List<CalcSite> MakeCalcSites([JetBrains.Annotations.NotNull] CalcHouseholdDto household, [JetBrains.Annotations.NotNull]
                                                    DtoCalcLocationDict locDict, [JetBrains.Annotations.NotNull] CalcLoadTypeDictionary ltDict,
                                                    [JetBrains.Annotations.NotNull] TransportationHandler th
                                                    )
        {
            List<CalcSite> sites = new List<CalcSite>();
            //Dictionary<string, CalcSite> siteDictByGuid = new Dictionary<string, CalcSite>();
            if(household.CalcSites == null) {
                throw new LPGException("CalcSites was null"); //for the bug in the null checking
            }

            foreach (var siteDto in household.CalcSites) {
                var calcSite = new CalcSite(siteDto.Name, siteDto.DeviceChangeAllowed,  siteDto.Guid,household.HouseholdKey);
                sites.Add(calcSite);
                //siteDictByGuid.Add(siteDto.Guid, calcSite);
                foreach (var locGuid in siteDto.LocationGuid) {
                    CalcLocation calcLoc = locDict.GetCalcLocationByGuid(locGuid);
                    calcLoc.CalcSite = calcSite;
                    calcSite.Locations.Add(calcLoc);
                }

                foreach (var chargingStation in siteDto.ChargingStations) {
                    var gridLt = ltDict.GetLoadtypeByGuid(chargingStation.GridChargingLoadType.Guid);
                    var carLt = ltDict.GetLoadtypeByGuid(chargingStation.CarChargingLoadType.Guid);
                    var cat = th.GetCategory(chargingStation.DeviceCategory);
                    BitArray isBusy = new BitArray(_calcRepo.CalcParameters.InternalTimesteps);
                    //todo: implement this fully
                    calcSite.AddChargingStation(gridLt,cat,chargingStation.MaxChargingPower,
                        carLt, _calcRepo, isBusy);
                }
            }
            return sites;
        }

        private void SetAffordances([JetBrains.Annotations.NotNull] CalcHousehold chh)
        {
            if (chh.TransportationHandler == null)
            {
                throw new LPGException("no transportation handler");
            }

            foreach (CalcLocation location in chh.Locations) {
                foreach (var aff in location.PureAffordances) {
                    //replace with affordance decorator
                    var sites = chh.TransportationHandler.CalcSites.Where(x => x.Locations.Contains(location)).ToList();
                    if (sites.Count == 0)
                    {
                        throw new DataIntegrityException("No calc site has the location " + location.Name + ". To make the transportation work, every site needs one location.");
                    }

                    if (sites.Count > 1) {
                        throw new DataIntegrityException("More than one calc site has the location " + location.Name);
                    }

                    AffordanceBaseTransportDecorator abtd = new AffordanceBaseTransportDecorator(
                        aff, sites[0], chh.TransportationHandler, aff.Name,
                        chh.HouseholdKey, Guid.NewGuid().ToStrGuid(), _calcRepo);
                    location.AddTransportationAffordance(abtd);
                }

                var persons = location.IdleAffs.Keys.ToList();
                foreach (var person in persons) {
                    var sites = chh.TransportationHandler.CalcSites.Where(x => x.Locations.Contains(location)).ToList();
                    if (sites.Count == 0)
                    {
                        throw new DataIntegrityException("No calc site has the location " + location.Name + ". To make the transportation work, every site needs one location.");
                    }

                    if (sites.Count > 1)
                    {
                        throw new DataIntegrityException("More than one calc site has the location " + location.Name);
                    }

                    var aff = location.IdleAffs[person];
                    AffordanceBaseTransportDecorator abtd = new AffordanceBaseTransportDecorator(
                        aff, sites[0], chh.TransportationHandler, aff.Name,
                        chh.HouseholdKey, Guid.NewGuid().ToStrGuid(), _calcRepo);
                    location.IdleAffs[person] = abtd;
                }
            }
        }

        private void MakeTransportationDevice([JetBrains.Annotations.NotNull] CalcHousehold chh,
                                              [JetBrains.Annotations.NotNull][ItemNotNull]
                                              List<CalcTransportationDeviceDto> transportationDevices)
        {
            foreach (var transportationDevice in transportationDevices) {
                var loads = new List<CalcDeviceLoad>();
                foreach (var load in transportationDevice.Loads) {
                    CalcLoadType clt = _loadTypeDict.GetLoadtypeByGuid(load.LoadTypeGuid);
                    loads.Add(new CalcDeviceLoad(load.Name, load.MaxPower, clt, 0, 0));
                }

                double distanceToEnergyFactor = transportationDevice.EnergyToDistanceFactor;

                CalcLoadType chargingLoadType = null;
                if (transportationDevice.ChargingCalcLoadTypeGuid!=null) {
                    chargingLoadType= _loadTypeDict.GetLoadtypeByGuid(transportationDevice.ChargingCalcLoadTypeGuid);
                }
                if (chh.TransportationHandler == null)
                {
                    throw new LPGException("no transportation handler");
                }

                CalcDeviceDto cdd = new CalcDeviceDto(transportationDevice.Name,
                    transportationDevice.Category.Guid,
                    chh.HouseholdKey,
                    OefcDeviceType.Transportation,transportationDevice.Category.Name,
                    string.Empty,transportationDevice.Guid, StrGuid.Empty, string.Empty, FlexibilityType.EnergyBudget, 14400);
                var category = chh.TransportationHandler.GetCategory(transportationDevice.Category);
                CalcTransportationDevice cd = new CalcTransportationDevice(
                    category,
                    transportationDevice.AverageSpeedInMPerS, loads,
                    transportationDevice.FullRangeInMeters,
                    distanceToEnergyFactor, transportationDevice.MaxChargingPower, chargingLoadType,
                    chh.TransportationHandler.CalcSites,
                      cdd, _calcRepo);
                if (category.IsLimitedToSingleLocation) {
                    chh.TransportationHandler.AddVehicleDepotDevice(cd);
                }
                else {
                    chh.TransportationHandler.LocationUnlimitedDevices.Add(cd);
                }
            }
        }
    }
}
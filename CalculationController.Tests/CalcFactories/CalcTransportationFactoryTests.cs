using System;
using System.Collections.Generic;
using Automation;
using CalculationController.CalcFactories;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using Database.Tables.Transportation;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace CalculationController.Tests.CalcFactories {
    public class CalcTransportationFactoryTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CheckReachabilityofLocationsTest()
        {
            List<Location> locations = new List<Location>();
            List<Site> sites = new List<Site>();
            CalcTransportationDtoFactory.CheckReachabilityofLocations(locations, sites, "calchouseholdname",
                "travelroutesetname");
            locations.Add(new Location("loc1", 1, "", Guid.NewGuid().ToStrGuid()));
            //not sites, one loc
            Action crashFunction1 = ()=>
                CalcTransportationDtoFactory.CheckReachabilityofLocations(locations, sites, "calchouseholdname", "travelroutesetname");

            crashFunction1.Should().Throw<DataIntegrityException>();
            //two locations in the same site
            locations.Add(new Location("loc2", 2, "", Guid.NewGuid().ToStrGuid()));
            sites.Add(new Site("site1", 1, "", "bla", true, Guid.NewGuid().ToStrGuid()));
            sites[0].AddLocation(locations[0], false);
            sites[0].AddLocation(locations[1], false);
            CalcTransportationDtoFactory.CheckReachabilityofLocations(locations, sites, "calchouseholdname",
                "travelroutesetname");
            sites[0].Locations.Clear();
            //two locations in two sites
            sites.Add(new Site("site1", 1, "", "bla", true, Guid.NewGuid().ToStrGuid()));
            sites.Add(new Site("site2", 2, "", "bla", true, Guid.NewGuid().ToStrGuid()));
            sites[0].AddLocation(locations[0], false);
            sites[1].AddLocation(locations[1], false);
            CalcTransportationDtoFactory.CheckReachabilityofLocations(locations, sites, "calchouseholdname",
                "travelroutesetname");
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CheckRouteCompletenessTest()
        {
            TravelRouteSet trs = new TravelRouteSet("trs", 1, "", "", Guid.NewGuid().ToStrGuid(), null);
            List<Site> sites = new List<Site>();
            //test with empty
            CalcTransportationDtoFactory.CheckRouteCompleteness(trs,sites);

            //test with one route
            Site sitea = new Site("sitea",1,"","", true, Guid.NewGuid().ToStrGuid());
            Site siteb = new Site("siteb", 2, "", "", true, Guid.NewGuid().ToStrGuid());
            sites.Add(sitea);
            sites.Add(siteb);
            trs.AddRoute(new TravelRoute(1, "", "Route1", "desc", sitea, siteb, Guid.NewGuid().ToStrGuid(), null), 0, 80, Common.Enums.PermittedGender.All, null, null, 1.0, false);
            CalcTransportationDtoFactory.CheckRouteCompleteness(trs, sites);

            //one route missing
            Site sitec = new Site("sitec", 2, "", "", true, Guid.NewGuid().ToStrGuid());
            sites.Add(sitec);
            trs.AddRoute(new TravelRoute(1, "", "Route2", "desc", siteb, sitec, Guid.NewGuid().ToStrGuid(), null), 0, 80, Common.Enums.PermittedGender.All, null, null, 1.0, false);
            Action  crashfunction1 = () => CalcTransportationDtoFactory.CheckRouteCompleteness(trs, sites);
            crashfunction1.Should().Throw<DataIntegrityException>();
            //add missing route
            trs.AddRoute(new TravelRoute(1, "", "Route3", "desc", sitea, sitec, Guid.NewGuid().ToStrGuid(), null), 0, 80, Common.Enums.PermittedGender.All, null, null, 1.0, false);
            CalcTransportationDtoFactory.CheckRouteCompleteness(trs, sites);
        }

        public CalcTransportationFactoryTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
using System;
using System.Collections.Generic;
using Automation;
using CalculationController.CalcFactories;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace CalculationController.Tests.CalcFactories {
    [TestFixture]
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
            void Crashfunction1() => CalcTransportationDtoFactory.CheckReachabilityofLocations(locations, sites, "calchouseholdname", "travelroutesetname");
            Assert.That(Crashfunction1, Throws.TypeOf<DataIntegrityException>());
            //two locations in the same site
            locations.Add(new Location("loc2", 2, "", Guid.NewGuid().ToStrGuid()));
            sites.Add(new Site("site1", 1, "", "bla", Guid.NewGuid().ToStrGuid()));
            sites[0].AddLocation(locations[0], false);
            sites[0].AddLocation(locations[1], false);
            CalcTransportationDtoFactory.CheckReachabilityofLocations(locations, sites, "calchouseholdname",
                "travelroutesetname");
            sites[0].Locations.Clear();
            //two locations in two sites
            sites.Add(new Site("site1", 1, "", "bla", Guid.NewGuid().ToStrGuid()));
            sites.Add(new Site("site2", 2, "", "bla", Guid.NewGuid().ToStrGuid()));
            sites[0].AddLocation(locations[0], false);
            sites[1].AddLocation(locations[1], false);
            CalcTransportationDtoFactory.CheckReachabilityofLocations(locations, sites, "calchouseholdname",
                "travelroutesetname");
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CheckRouteCompletenessTest()
        {
            TravelRouteSet trs = new TravelRouteSet("trs", 1, "", "", Guid.NewGuid().ToStrGuid());
            List<Site> sites = new List<Site>();
            //test with empty
            CalcTransportationDtoFactory.CheckRouteCompleteness(trs,sites);

            //test with one route
            Site sitea = new Site("sitea",1,"","", Guid.NewGuid().ToStrGuid());
            Site siteb = new Site("siteb", 2, "", "", Guid.NewGuid().ToStrGuid());
            sites.Add(sitea);
            sites.Add(siteb);
            trs.AddRoute(new TravelRoute(1,"","Route1","desc",sitea,siteb, Guid.NewGuid().ToStrGuid(),null),false);
            CalcTransportationDtoFactory.CheckRouteCompleteness(trs, sites);

            //one route missing
            Site sitec = new Site("sitec", 2, "", "", Guid.NewGuid().ToStrGuid());
            sites.Add(sitec);
            trs.AddRoute(new TravelRoute(1, "", "Route2", "desc", siteb, sitec, Guid.NewGuid().ToStrGuid(),null), false);
            void Crashfunction1() => CalcTransportationDtoFactory.CheckRouteCompleteness(trs, sites);
            Assert.That(Crashfunction1, Throws.TypeOf<DataIntegrityException>());
            //add missing route
            trs.AddRoute(new TravelRoute(1, "", "Route3", "desc", sitea, sitec, Guid.NewGuid().ToStrGuid(),null), false);
            CalcTransportationDtoFactory.CheckRouteCompleteness(trs, sites);
        }

        public CalcTransportationFactoryTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
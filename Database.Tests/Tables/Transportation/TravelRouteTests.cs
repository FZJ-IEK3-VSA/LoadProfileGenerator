using System;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using Database.Tables.Transportation;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.Transportation
{

    public class TravelRouteTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TravelRouteTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(TravelRoute.TableName);
                db.ClearTable(TravelRouteStep.TableName);
                Location loc = new Location("loc1", null, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                loc.SaveToDB();
                Site siteA = new Site("site1", null, db.ConnectionString, "desc", Guid.NewGuid().ToStrGuid());
                siteA.SaveToDB();

                Site siteB = new Site("site2", null, db.ConnectionString, "desc", Guid.NewGuid().ToStrGuid());
                siteB.SaveToDB();

                TransportationDeviceCategory td = new
                    TransportationDeviceCategory("transportationdevicecategory", null, db.ConnectionString, "desc", true, Guid.NewGuid().ToStrGuid());
                td.SaveToDB();
                TravelRoute tr = new TravelRoute(null, db.ConnectionString, "route", "desc", siteA, siteB,
                    Guid.NewGuid().ToStrGuid(), null);
                tr.SaveToDB();
                tr.AddStep("name1", td, 100, 1, "key1");
                tr.AddStep("name3", td, 100, 10, "key3");
                tr.AddStep("name2", td, 100, 2, "key2");
                //test the  sorting of steps while adding based on step number
                ("name1").Should().Be(tr.Steps[0].Name);
                ("name2").Should().Be(tr.Steps[1].Name);
                ("name3").Should().Be(tr.Steps[2].Name);
                ("key1").Should().Be(tr.Steps[0].StepKey);
                ("key2").Should().Be(tr.Steps[1].StepKey);
                ("key3").Should().Be(tr.Steps[2].StepKey);
                foreach (TravelRouteStep step in tr.Steps)
                {
                    Logger.Info(step.Name);
                }
                //loading
                ObservableCollection<Site> sites = new ObservableCollection<Site>
            {
                siteA,
                siteB
            };
                ObservableCollection<TransportationDeviceCategory> transportationDeviceCategories = new ObservableCollection<TransportationDeviceCategory>
            {
                td
            };
                ObservableCollection<TravelRoute> routes = new ObservableCollection<TravelRoute>();
                TravelRoute.LoadFromDatabase(routes, db.ConnectionString, false, transportationDeviceCategories, sites);
                (routes.Count).Should().Be(1);
                tr = routes[0];
                ("name1").Should().Be(tr.Steps[0].Name);
                ("name2").Should().Be(tr.Steps[1].Name);
                ("name3").Should().Be(tr.Steps[2].Name);
                ("key1").Should().Be(tr.Steps[0].StepKey);
                ("key2").Should().Be(tr.Steps[1].StepKey);
                ("key3").Should().Be(tr.Steps[2].StepKey);
                db.Cleanup();
            }
        }

        public TravelRouteTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
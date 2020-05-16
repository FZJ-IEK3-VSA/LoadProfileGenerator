using System;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using Database.Tables.Transportation;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.Transportation
{

    public class SiteTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SiteLocationTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(Site.TableName);
                db.ClearTable(SiteLocation.TableName);
                Location loc = new Location("loc1", null, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                loc.SaveToDB();
                Site site = new Site("site1", null, db.ConnectionString, "desc", Guid.NewGuid().ToStrGuid());
                TransportationDeviceCategory cat = new TransportationDeviceCategory("bla", 1, db.ConnectionString, "desc", true, Guid.NewGuid().ToStrGuid());
                VLoadType lt = new VLoadType("mylt", "desc", "w", "kwh", 1, 1, new TimeSpan(0, 1, 0), 1, db.ConnectionString,
                    LoadTypePriority.All, false, Guid.NewGuid().ToStrGuid());
                lt.SaveToDB();
                cat.SaveToDB();
                site.SaveToDB();
                //site.AddChargingStation(cat, lt, 1);
                site.AddLocation(loc);
                //loading
                ObservableCollection<Site> slocs = new ObservableCollection<Site>();
                ObservableCollection<Location> locs = new ObservableCollection<Location>
            {
                loc
            };
                Site.LoadFromDatabase(slocs, db.ConnectionString,
                    false, locs);
                //Site mysite = slocs[0];
                //(mysite.ChargingStations.Count).Should().Be(1);
                db.Cleanup();
                (slocs.Count).Should().Be(1);
            }
        }

        public SiteTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}

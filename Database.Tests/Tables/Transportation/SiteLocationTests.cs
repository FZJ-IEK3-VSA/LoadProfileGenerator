using System;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Database.Tests.Tables.Transportation
{
    [TestFixture]
    public class SiteLocationTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SiteLocationTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Location loc = new Location("loc1", null, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                loc.SaveToDB();
                SiteLocation sl = new SiteLocation(null, loc, -1, db.ConnectionString, "name", Guid.NewGuid().ToStrGuid());
                ObservableCollection<SiteLocation> slocs = new ObservableCollection<SiteLocation>();
                ObservableCollection<Location> locs = new ObservableCollection<Location>();
                sl.SaveToDB();
                locs.Add(loc);
                SiteLocation.LoadFromDatabase(slocs, db.ConnectionString, locs, false);
                db.Cleanup();
                Assert.AreEqual(1, slocs.Count);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SiteWithLocationTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(Site.TableName);
                db.ClearTable(SiteLocation.TableName);
                Location loc = new Location("loc1", null, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                loc.SaveToDB();
                Site site = new Site("site1", null, db.ConnectionString, "desc", Guid.NewGuid().ToStrGuid());
                site.SaveToDB();
                site.AddLocation(loc);
                //loading
                ObservableCollection<Site> slocs = new ObservableCollection<Site>();
                ObservableCollection<Location> locs = new ObservableCollection<Location>
            {
                loc
            };
                Site.LoadFromDatabase(slocs, db.ConnectionString,
                    false, locs);
                db.Cleanup();
                Assert.AreEqual(1, slocs.Count);
            }
        }

        public SiteLocationTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
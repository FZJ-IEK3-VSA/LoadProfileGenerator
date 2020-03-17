using System;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using Database.Tables.Transportation;
using NUnit.Framework;

namespace Database.Tests.Tables.Transportation
{
    [TestFixture]
    public class SiteLocationTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void SiteLocationTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            Location loc = new Location("loc1",null,db.ConnectionString, Guid.NewGuid().ToString());
            loc.SaveToDB();
            SiteLocation sl = new SiteLocation(null,loc,-1,db.ConnectionString,"name", Guid.NewGuid().ToString());
            ObservableCollection<SiteLocation> slocs = new ObservableCollection<SiteLocation>();
            ObservableCollection<Location> locs = new ObservableCollection<Location>();
            sl.SaveToDB();
            locs.Add(loc);
            SiteLocation.LoadFromDatabase(slocs,db.ConnectionString,locs,false);
            db.Cleanup();
            Assert.AreEqual(1,slocs.Count);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void SiteWithLocationTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            db.ClearTable(Site.TableName);
            db.ClearTable(SiteLocation.TableName);
            Location loc = new Location("loc1", null, db.ConnectionString, Guid.NewGuid().ToString());
            loc.SaveToDB();
            Site site = new Site("site1", null, db.ConnectionString, "desc",Guid.NewGuid().ToString());
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
}
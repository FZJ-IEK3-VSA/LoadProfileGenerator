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
    public class SiteTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void SiteLocationTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());

            db.ClearTable(Site.TableName);
            db.ClearTable(SiteLocation.TableName);
            Location loc = new Location("loc1", null, db.ConnectionString, Guid.NewGuid().ToString());
            loc.SaveToDB();
            Site site = new Site("site1",null,db.ConnectionString,"desc", Guid.NewGuid().ToString());
            TransportationDeviceCategory cat = new TransportationDeviceCategory("bla",1,db.ConnectionString,"desc",true, Guid.NewGuid().ToString());
            VLoadType lt = new VLoadType("mylt","desc","w","kwh",1,1,new TimeSpan(0,1,0),1,db.ConnectionString,
                LoadTypePriority.All,false, Guid.NewGuid().ToString());
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
                false,locs);
            //Site mysite = slocs[0];
            //Assert.AreEqual(1, mysite.ChargingStations.Count);
            db.Cleanup();
            Assert.AreEqual(1, slocs.Count);
        }
    }
}

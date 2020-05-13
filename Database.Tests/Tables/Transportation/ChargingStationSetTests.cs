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
    public class ChargingStationSetTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RunChargingStationSetTests()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(ChargingStationSet.TableName);
                Location loc = new Location("loc1", null, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                loc.SaveToDB();
                ChargingStationSet sl = new ChargingStationSet("blub", null, db.ConnectionString, "desc", Guid.NewGuid().ToStrGuid());
                sl.SaveToDB();
                TransportationDeviceCategory tdc = new TransportationDeviceCategory("tdc", null, db.ConnectionString, "desc", false, Guid.NewGuid().ToStrGuid());
                tdc.SaveToDB();
                VLoadType vlt = (VLoadType)VLoadType.CreateNewItem(s => false, db.ConnectionString);
                vlt.SaveToDB();
                Site site = (Site)Site.CreateNewItem(_ => false, db.ConnectionString);
                site.SaveToDB();
                sl.AddChargingStation(tdc, vlt, 10, site, vlt);

                ObservableCollection<VLoadType> lts = new ObservableCollection<VLoadType>
            {
                vlt
            };
                ObservableCollection<TransportationDeviceCategory> cats = new ObservableCollection<TransportationDeviceCategory>
            {
                tdc
            };
                ObservableCollection<ChargingStationSet> css = new ObservableCollection<ChargingStationSet>();
                ObservableCollection<Site> sites = new ObservableCollection<Site>
            {
                site
            };
                ChargingStationSet.LoadFromDatabase(css,
                    db.ConnectionString, false, lts, cats, sites);
                db.Cleanup();
                Assert.AreEqual(1, css.Count);
                Assert.AreEqual(1, css[0].ChargingStations.Count);
            }
        }
    }
}
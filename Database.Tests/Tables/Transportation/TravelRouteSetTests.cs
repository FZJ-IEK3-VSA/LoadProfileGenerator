using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.Transportation;
using NUnit.Framework;

namespace Database.Tests.Tables.Transportation
{
    [TestFixture]
    public class TravelRouteSetTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TravelRouteSetTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            db.ClearTable(TravelRouteSet.TableName);
            db.ClearTable(TravelRouteSetEntry.TableName);

            TravelRouteSet set = new TravelRouteSet("set1",null,db.ConnectionString,"desc", System.Guid.NewGuid().ToString());
            set.SaveToDB();
            Site a = new Site("a",null,db.ConnectionString,"desc", System.Guid.NewGuid().ToString());
            a.SaveToDB();
            Site b = new Site("b", null, db.ConnectionString, "desc", System.Guid.NewGuid().ToString());
            b.SaveToDB();
            TravelRoute route = new TravelRoute(null,db.ConnectionString,"routename","routedesc",a,b, System.Guid.NewGuid().ToString(),null);
            route.SaveToDB();
            set.AddRoute(route);
            //loading
            ObservableCollection<TravelRoute> routes = new ObservableCollection<TravelRoute>
            {
                route
            };
            ObservableCollection<TravelRouteSet> sets = new ObservableCollection<TravelRouteSet>();
            TravelRouteSet.LoadFromDatabase(sets, db.ConnectionString,false,routes);
            db.Cleanup();
            Assert.AreEqual(1, sets.Count);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TravelRouteSetTestImportExport()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim =new Simulator(db.ConnectionString);
            var travelrouteset = sim.TravelRouteSets[0];
            //var copy =
                TravelRouteSet.ImportFromItem(travelrouteset, sim);
            //var jsonOriginal = JsonConvert.SerializeObject(travelrouteset, Formatting.Indented,
            //    new JsonSerializerSettings()
            //    {
            //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            //    });
            //var jsonCopy = JsonConvert.SerializeObject(copy, Formatting.Indented,
            //    new JsonSerializerSettings()
            //    {
            //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            //    });
            db.Cleanup();
            //Assert.AreEqual(jsonOriginal, jsonCopy);
            }
    }
}
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.Transportation;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.Transportation
{

    public class TravelRouteSetTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TravelRouteSetTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(TravelRouteSet.TableName);
                db.ClearTable(TravelRouteSetEntry.TableName);

                TravelRouteSet set = new TravelRouteSet("set1", null, db.ConnectionString, "desc", System.Guid.NewGuid().ToStrGuid());
                set.SaveToDB();
                Site a = new Site("a", null, db.ConnectionString, "desc", System.Guid.NewGuid().ToStrGuid());
                a.SaveToDB();
                Site b = new Site("b", null, db.ConnectionString, "desc", System.Guid.NewGuid().ToStrGuid());
                b.SaveToDB();
                TravelRoute route = new TravelRoute(null, db.ConnectionString, "routename", "routedesc", a, b, System.Guid.NewGuid().ToStrGuid(), null);
                route.SaveToDB();
                set.AddRoute(route);
                //loading
                ObservableCollection<TravelRoute> routes = new ObservableCollection<TravelRoute>
            {
                route
            };
                ObservableCollection<TravelRouteSet> sets = new ObservableCollection<TravelRouteSet>();
                TravelRouteSet.LoadFromDatabase(sets, db.ConnectionString, false, routes);
                db.Cleanup();
                (sets.Count).Should().Be(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TravelRouteSetTestImportExport()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
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
            }
            //(jsonCopy).Should().Be(jsonOriginal);
        }

        public TravelRouteSetTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
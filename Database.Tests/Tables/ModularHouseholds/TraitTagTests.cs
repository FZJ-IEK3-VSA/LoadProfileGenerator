using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Helpers;
using Database.Tables.ModularHouseholds;
using NUnit.Framework;

namespace Database.Tests.Tables.ModularHouseholds {
    [TestFixture]
    public class TraitTagTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TraitTagTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            db.ClearTable(TraitTag.TableName);
            var cat = new CategoryDBBase<TraitTag>("Household Trait Tags");
            TraitTag.LoadFromDatabase(cat.It, db.ConnectionString, false);
            Assert.AreEqual(0, cat.MyItems.Count);
            cat.CreateNewItem(db.ConnectionString);
            cat.SaveToDB();
            var tags = new ObservableCollection<TraitTag>();
            TraitTag.LoadFromDatabase(tags, db.ConnectionString, false);
            Assert.AreEqual(1, tags.Count);
            db.Cleanup();
        }
    }
}
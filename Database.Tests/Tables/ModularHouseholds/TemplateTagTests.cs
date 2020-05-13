using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Helpers;
using Database.Tables.ModularHouseholds;
using NUnit.Framework;

namespace Database.Tests.Tables.ModularHouseholds {
    [TestFixture]
    public class TemplateTagTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TemplateTagTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(HouseholdTag.TableName);
                var cat = new CategoryDBBase<HouseholdTag>("Template Tags");
                HouseholdTag.LoadFromDatabase(cat.It, db.ConnectionString, false);
                Assert.AreEqual(0, cat.MyItems.Count);
                cat.CreateNewItem(db.ConnectionString);
                cat.SaveToDB();
                var tags = new ObservableCollection<HouseholdTag>();
                HouseholdTag.LoadFromDatabase(tags, db.ConnectionString, false);
                Assert.AreEqual(1, tags.Count);
                db.Cleanup();
            }
        }
    }
}
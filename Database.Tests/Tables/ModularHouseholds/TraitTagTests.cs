using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Helpers;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Database.Tests.Tables.ModularHouseholds {
    [TestFixture]
    public class TraitTagTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void TraitTagTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
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

        public TraitTagTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
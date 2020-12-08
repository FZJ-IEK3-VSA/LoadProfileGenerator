using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Helpers;
using Database.Tables.ModularHouseholds;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.ModularHouseholds {

    public class TraitTagTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TraitTagTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(TraitTag.TableName);
                var cat = new CategoryDBBase<TraitTag>("Household Trait Tags");
                TraitTag.LoadFromDatabase(cat.Items, db.ConnectionString, false);
                (cat.Items.Count).Should().Be(0);
                cat.CreateNewItem(db.ConnectionString);
                cat.SaveToDB();
                var tags = new ObservableCollection<TraitTag>();
                TraitTag.LoadFromDatabase(tags, db.ConnectionString, false);
                (tags.Count).Should().Be(1);
                db.Cleanup();
            }
        }

        public TraitTagTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
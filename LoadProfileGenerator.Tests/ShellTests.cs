using Automation;
using Common;
using Common.Tests;
using Database;
using Database.Tests;
using Xunit;
using Xunit.Abstractions;

namespace LoadProfileGenerator.Tests
{
    public class ShellTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void UpdateVacationsInHouseholdTemplates1Test()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                Shell.UpdateVacationsInHouseholdTemplates1(sim);
                db.Cleanup();
            }
        }
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void UpdateLivingPattern()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass())) {
                Simulator sim = new Simulator(db.ConnectionString);
                Shell.ConvertAllTraitTagsToLivingPatternTags(sim);
            }
        }

        //[Fact]
        //[Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        //public void UpdatePersonTags()
        //{
        //    using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
        //    {
        //        Simulator sim = new Simulator(db.ConnectionString);
        //        Shell.ConvertAllPersonTagsToLivingPatternTags(sim);
        //    }
        //}
        public ShellTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
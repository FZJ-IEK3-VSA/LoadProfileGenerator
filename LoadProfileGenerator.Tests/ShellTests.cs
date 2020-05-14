using Automation;
using Common;
using Common.Tests;
using Database;
using Database.Tests;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;

namespace LoadProfileGenerator.Tests
{
    [TestFixture]
    public class ShellTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void UpdateVacationsInHouseholdTemplates1Test()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                Shell.UpdateVacationsInHouseholdTemplates1(sim);
                db.Cleanup();
            }
        }

        public ShellTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
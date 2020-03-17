using Automation;
using Common;
using Common.Tests;
using Database;
using Database.Tests;
using NUnit.Framework;

namespace LoadProfileGenerator.Tests
{
    [TestFixture]
    public class ShellTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void UpdateVacationsInHouseholdTemplates1Test()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.LoadProfileGenerator);
            Simulator sim = new Simulator(db.ConnectionString);
            Shell.UpdateVacationsInHouseholdTemplates1(sim);
            db.Cleanup();
        }
    }
}
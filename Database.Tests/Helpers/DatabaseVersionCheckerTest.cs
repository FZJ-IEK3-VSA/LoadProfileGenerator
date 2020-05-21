using Automation;
using Automation.ResultFiles;
using Common;
using Common.Tests;
using Database.Helpers;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Helpers
{

    public class DatabaseVersionCheckerTest: UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunatabaseVersionCheckerTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                DatabaseVersionChecker.CheckVersion(db.ConnectionString);
                string previousVersion = DatabaseVersionChecker.DstVersion;
                DatabaseVersionChecker.DstVersion = "asdf";
                Assert.Throws(typeof(LPGException), () => DatabaseVersionChecker.CheckVersion(db.ConnectionString));
                DatabaseVersionChecker.DstVersion = previousVersion;
            }
        }

        public DatabaseVersionCheckerTest([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}

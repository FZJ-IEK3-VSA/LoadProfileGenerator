using Automation;
using Automation.ResultFiles;
using Common;
using Database.Helpers;

using Xunit;


namespace Database.Tests.Helpers
{

    public class DatabaseVersionCheckerTest
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void Run()
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
    }
}

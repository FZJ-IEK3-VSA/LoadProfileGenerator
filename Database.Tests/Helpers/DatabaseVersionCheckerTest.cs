using Automation;
using Automation.ResultFiles;
using Common;
using Database.Helpers;
using NUnit.Framework;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace Database.Tests.Helpers
{
    [TestFixture]
    public class DatabaseVersionCheckerTest
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
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

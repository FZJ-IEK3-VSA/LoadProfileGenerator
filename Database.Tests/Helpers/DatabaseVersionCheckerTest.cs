using Automation.ResultFiles;
using Common;
using Database.Helpers;
using NUnit.Framework;

namespace Database.Tests.Helpers
{
    [TestFixture]
    public class DatabaseVersionCheckerTest
    {
        [Test]
        [Category("BasicTest")]
        public void Run()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            DatabaseVersionChecker.CheckVersion(db.ConnectionString);
            string previousVersion = DatabaseVersionChecker.DstVersion;
            DatabaseVersionChecker.DstVersion = "asdf";
            Assert.Throws(typeof(LPGException), () => DatabaseVersionChecker.CheckVersion(db.ConnectionString));
            DatabaseVersionChecker.DstVersion = previousVersion;
        }
    }
}

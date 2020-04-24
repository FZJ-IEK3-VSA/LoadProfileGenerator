using NUnit.Framework;

namespace Common.Tests
{
    public class UnitTestBaseClass
    {
        [SetUp]
        public void BaseSetUp()
        {
            Logger.Info("Base Setup performed");
            string basePath = WorkingDir.DetermineBaseWorkingDir(true);
            CleanTestBase.CleanFolder(false,basePath,false);
            Logger.Info("Base Test Path is " + basePath);
        }

        public bool SkipEndCleaning { get; set; }
        [TearDown]
        public void BaseTearDown()
        {
            if (SkipEndCleaning) {
                return;
            }

            string basePath = WorkingDir.DetermineBaseWorkingDir(true);
            CleanTestBase.CleanFolder(false, basePath,false);
            Logger.Info("Base Teardown Performed");
        }
    }
}

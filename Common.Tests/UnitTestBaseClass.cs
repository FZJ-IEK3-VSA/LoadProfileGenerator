using JetBrains.Annotations;
using NUnit.Framework;

namespace Common.Tests
{
    public class UnitTestBaseClass
    {
        private bool _skipEndCleaning;

        [SetUp]
        public void BaseSetUp()
        {
            Logger.Info("Base Setup performed");
            string basePath = WorkingDir.DetermineBaseWorkingDir(true);
            CleanTestBase.CleanFolder(false,basePath,false);
            Logger.Info("Base Test Path is " + basePath);
        }

        public void SkipEndCleaning([CanBeNull] WorkingDir wd)
        {
            _skipEndCleaning = true;
            if (wd != null) {
                wd.SkipCleaning = true;
            }
        }

        [TearDown]
        public void BaseTearDown()
        {
            if (_skipEndCleaning) {
                return;
            }

            string basePath = WorkingDir.DetermineBaseWorkingDir(true);
            CleanTestBase.CleanFolder(false, basePath,false);
            Logger.Info("Base Teardown Performed");
        }
    }
}

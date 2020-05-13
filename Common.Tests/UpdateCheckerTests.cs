using Automation;
using NUnit.Framework;

namespace Common.Tests
{
    [TestFixture()]
    public class UpdateCheckerTests : UnitTestBaseClass
    {
        [Test()]
        [Category(UnitTestCategories.BasicTest)]
        public void GetLatestVersionTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                UpdateChecker uc = new UpdateChecker();
                string s = uc.GetLatestVersion(out _);
                Logger.Info(s);
                wd.CleanUp();
            }
        }
    }
}
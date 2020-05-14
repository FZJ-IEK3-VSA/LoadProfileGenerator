using Automation;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;

namespace Common.Tests
{
    [TestFixture()]
    public class UpdateCheckerTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
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

        public UpdateCheckerTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
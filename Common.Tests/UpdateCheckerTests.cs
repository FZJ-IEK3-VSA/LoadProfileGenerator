using Automation;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;

namespace Common.Tests
{
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
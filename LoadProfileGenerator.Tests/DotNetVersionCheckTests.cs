using Automation;
using Common.Tests;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;
using Logger = Common.Logger;

namespace LoadProfileGenerator.Tests {
    public class DotNetVersionCheckTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void GetReleaseKeyTest()
        {
            var version = DotNetVersionCheck.GetReleaseKey();
            Logger.Info("Version: " + version);
        }

        public DotNetVersionCheckTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
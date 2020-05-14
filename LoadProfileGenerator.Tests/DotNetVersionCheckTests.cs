using Automation;
using Common.Tests;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Logger = Common.Logger;

namespace LoadProfileGenerator.Tests {
    [TestFixture]
    public class DotNetVersionCheckTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
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
using Automation;
using Common.Tests;
using NUnit.Framework;
using Logger = Common.Logger;

namespace LoadProfileGenerator.Tests {
    [TestFixture]
    public class DotNetVersionCheckTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void GetReleaseKeyTest()
        {
            var version = DotNetVersionCheck.GetReleaseKey();
            Logger.Info("Version: " + version);
        }
    }
}
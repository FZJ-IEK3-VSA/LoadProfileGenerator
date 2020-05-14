using Automation;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;

namespace Common.Tests
{
    [TestFixture]
    public class UtiliTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void GetCurrentMethodAndClassTest()
        {
            string s = Utili.GetCurrentMethodAndClass();
             Logger.Debug("Current Method:" + s);
        }

        public UtiliTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
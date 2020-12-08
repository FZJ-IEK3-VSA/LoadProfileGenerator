using Automation;
using Xunit;
using Xunit.Abstractions;

namespace Common.Tests
{
    public class UtiliTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void GetCurrentMethodAndClassTest()
        {
            string s = Utili.GetCurrentMethodAndClass();
             Logger.Debug("Current Method:" + s);
        }

        public UtiliTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
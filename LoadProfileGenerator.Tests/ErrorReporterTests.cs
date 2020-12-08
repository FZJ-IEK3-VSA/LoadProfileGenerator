using Automation;
using Common;
using Common.Tests;
using Xunit;
using Xunit.Abstractions;

namespace LoadProfileGenerator.Tests {
    public class ErrorReporterTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunTest()
        {
            var er = new ErrorReporter();
            er.Run("testmessage", "teststack");
        }

        public ErrorReporterTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
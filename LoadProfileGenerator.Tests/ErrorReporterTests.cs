using Automation;
using Common;
using Common.Tests;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;

namespace LoadProfileGenerator.Tests {
    [TestFixture]
    public class ErrorReporterTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void RunTest()
        {
            var er = new ErrorReporter();
            er.Run("testmessage", "teststack");
        }

        public ErrorReporterTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
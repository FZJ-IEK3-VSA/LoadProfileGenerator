using Common;
using Common.Tests;
using NUnit.Framework;

namespace LoadProfileGenerator.Tests {
    [TestFixture]
    public class ErrorReporterTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void RunTest()
        {
            var er = new ErrorReporter();
            er.Run("testmessage", "teststack");
        }
    }
}
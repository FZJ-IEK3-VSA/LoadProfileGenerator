using Automation;
using NUnit.Framework;

namespace Common.Tests
{
    [TestFixture]
    public class UtiliTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void GetCurrentMethodAndClassTest()
        {
            string s = Utili.GetCurrentMethodAndClass();
             Logger.Debug("Current Method:" + s);
        }
    }
}
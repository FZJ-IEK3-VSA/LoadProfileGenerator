using System;
using NUnit.Framework;

namespace Common.Tests
{
    [TestFixture()]
    public class UpdateCheckerTests : UnitTestBaseClass
    {
        [Test()]
        [Category("BasicTest")]
        public void GetLatestVersionTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            UpdateChecker uc = new UpdateChecker();
            string s = uc.GetLatestVersion(out _);
            Console.WriteLine(s);
            wd.CleanUp();
        }
    }
}
using System;
using NUnit.Framework;

namespace Common.Tests
{
    public class UnitTestBaseClass
    {
        [SetUp]
        public void BaseSetUp()
        {
            Console.WriteLine("Base Setup performed");
            string basePath = WorkingDir.DetermineBaseWorkingDir(true);
            CleanTestBase.CleanFolder(false,basePath,false);
            Console.WriteLine("Base Test Path is " + basePath);
        }

        public bool SkipEndCleaning { get; set; }
        [TearDown]
        public void BaseTearDown()
        {
            if (SkipEndCleaning) {
                return;
            }

            string basePath = WorkingDir.DetermineBaseWorkingDir(true);
            CleanTestBase.CleanFolder(false, basePath,false);
            Console.WriteLine("Base Teardown Performed");
        }
    }
}

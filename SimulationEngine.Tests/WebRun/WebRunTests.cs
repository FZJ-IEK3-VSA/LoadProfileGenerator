using System;
using System.IO;
using Common;
using Common.Tests;
using Database.Tests;
using NUnit.Framework;

namespace SimulationEngine.Tests.WebRun
{
    [TestFixture]
    public class WebRunTests : UnitTestBaseClass
    {
        [Test]
        [Category("QuickChart")]
        public void RunTest()
        {
            Program.CatchErrors = false;
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
            File.Copy(@"c:\work\webrun\calcjob.txt", Path.Combine(wd.WorkingDirectory, "calcjob.txt"), true);
            File.Copy(db.FileName, Path.Combine(wd.WorkingDirectory, "profilegenerator.db3"), true);
            Environment.CurrentDirectory = wd.WorkingDirectory;
            WebRunner.WebRun.WebRunOptions wo = new WebRunner.WebRun.WebRunOptions
            {
                Directory = wd.WorkingDirectory
            };
            WebRunner.WebRun.Run(wo);
        }
    }
}
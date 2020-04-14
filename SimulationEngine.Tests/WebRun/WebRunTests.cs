using System;
using System.IO;
using Automation;
using Common;
using Common.Tests;
using Database.Tests;
using NUnit.Framework;
using SimulationEngineLib;

namespace SimulationEngine.Tests.WebRun
{
    [TestFixture]
    public class WebRunTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            SimulationEngineConfig.CatchErrors = false;
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            File.Copy(@"c:\work\webrun\calcjob.txt", Path.Combine(wd.WorkingDirectory, "calcjob.txt"), true);
            File.Copy(db.FileName, Path.Combine(wd.WorkingDirectory, "profilegenerator.db3"), true);
            Environment.CurrentDirectory = wd.WorkingDirectory;
            SimulationEngineLib.WebRunner.WebRun.WebRunOptions wo = new SimulationEngineLib.WebRunner.WebRun.WebRunOptions
            {
                Directory = wd.WorkingDirectory
            };
            SimulationEngineLib.WebRunner.WebRun.Run(wo);
        }
    }
}
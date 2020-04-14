using System;
using System.IO;
using Automation;
using Common;
using Common.Tests;
using Database.Tests;
using NUnit.Framework;
using SimulationEngineLib;

//using SimulationEngine.WebRunner;

namespace SimulationEngine.Tests.WebRun
{
    [TestFixture]
    public class SqlLiteExporterTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void Run()
        {
            SimulationEngineConfig.CatchErrors = false;
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            File.Copy(db.FileName, Path.Combine(wd.WorkingDirectory, "profilegenerator.db3"));

            Directory.SetCurrentDirectory(wd.WorkingDirectory);

         //   SqliteExporter.RunFullExport("Data Source=profilegenerator.db3");
            GC.WaitForPendingFinalizers();
            GC.Collect();
            wd.CleanUp(1);
            db.Cleanup();
        }
    }
}
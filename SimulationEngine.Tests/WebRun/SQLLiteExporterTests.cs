using System;
using System.IO;
using Common;
using Common.Tests;
using Database.Tests;
using NUnit.Framework;
//using SimulationEngine.WebRunner;

namespace SimulationEngine.Tests.WebRun
{
    [TestFixture]
    public class SqlLiteExporterTests : UnitTestBaseClass
    {
        [Test]
        [Category("QuickChart")]
        public void Run()
        {
            Program.CatchErrors = false;
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(),DatabaseSetup.TestPackage.SimulationEngine);
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
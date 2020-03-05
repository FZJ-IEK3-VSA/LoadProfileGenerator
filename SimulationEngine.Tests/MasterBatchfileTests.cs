using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Common;
using Common.JSON;
using Common.Tests;
using Database;
using Database.Tables.Houses;
using Database.Tests;
using NUnit.Framework;
using SimulationEngine.Calculation;
using SimulationEngine.SettlementCalculation;

namespace SimulationEngine.Tests
{
    [TestFixture]
    public class MasterBatchfileTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void TestSettlementInformation()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(),DatabaseSetup.TestPackage.SimulationEngine);
            Simulator sim = new Simulator(db.ConnectionString);
            Settlement set = sim.Settlements[0];
            BatchOptions bo = new BatchOptions();
            BatchfileFromSettlement.MakeSettlementJson(set,sim,bo,wd.WorkingDirectory);
            SettlementInformation si = SettlementInformation.Read(wd.WorkingDirectory);
            Assert.NotNull(si);
            db.Cleanup();
            //wd.CleanUp();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [Test]
        [Category("QuickChart")]
        public void RunNaturalLightBatch()
        {
            Program.IsUnitTest = true;
            Program.CatchErrors = false;
            SimulationEngineTestPreparer se = new SimulationEngineTestPreparer("RunNaturalLight");
            List<string> arguments = new List<string>
            {
                "--MakeLightBatch"
            };
            Program.Main(arguments.ToArray());
            se.Clean();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [Test]
        [Category("QuickChart")]
        public void RunTest()
        {
            SimulationEngineTestPreparer se = new SimulationEngineTestPreparer("MasterBatch");
            List<string> arguments = new List<string>
            {
                "--MakeMasterBatch"
            };
            Program.Main(arguments.ToArray());
            DateTime d = DateTime.Now;
            string dstDir = @"x:\Calc\" + d.Year + "." + d.Month + "." + d.Day + ".." + d.Hour + "." + d.Minute;
            Process p = new Process {StartInfo = {FileName = "robocopy.exe", Arguments = se.WorkingDirectory + " " + dstDir + " /E /MIR", UseShellExecute = true}};
            p.Start();
            p.WaitForExit();
            p.Dispose();
            p = new Process {StartInfo = {FileName = "robocopy.exe"}};
            dstDir = @"e:\masterbatchForImport";
            p.StartInfo.Arguments = se.WorkingDirectory + " " + dstDir + " /E /MIR";
            p.StartInfo.UseShellExecute = true;
            p.Start();
            p.WaitForExit();
            p.Dispose();
            se.Clean();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [Test]
        [Category("ManualOnly")]
        public void PdfReplacerTest()
        {
            SimulationEngineTestPreparer se = new SimulationEngineTestPreparer("MasterBatch");
            List<string> arguments = new List<string>
            {
                "--Batch-ModularHouseholds"
            };
            Program.IsUnitTest = true;
            Program.Main(arguments.ToArray());
            DateTime d = DateTime.Now;
            string dstDir = @"x:\Calc\" + d.Year + "." + d.Month + "." + d.Day + ".." + d.Hour + "." + d.Minute;
            using (Process p = new Process {StartInfo = {FileName = "robocopy.exe", Arguments = se.WorkingDirectory + " " + dstDir + " /E /MIR", UseShellExecute = true}}) {
                p.Start();
            }

            using (var p = new Process { StartInfo = { FileName = "robocopy.exe" } }) {
                dstDir = @"e:\masterbatchForImport";
                p.StartInfo.Arguments = se.WorkingDirectory + " " + dstDir + " /E /MIR";
                p.StartInfo.UseShellExecute = true;
                p.Start();
                p.WaitForExit();
                p.Dispose();
            }

            se.Clean();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Common;
using Common.JSON;
using Common.Tests;
using Database;
using Database.Tables.Houses;
using Database.Tests;
using NUnit.Framework;
using SimulationEngineLib;
using SimulationEngineLib.Calculation;
using SimulationEngineLib.SettlementCalculation;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace SimulationEngine.Tests
{
    [TestFixture]
    public class MasterBatchfileTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TestSettlementInformation()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    Simulator sim = new Simulator(db.ConnectionString);
                    Settlement set = sim.Settlements[0];
                    BatchOptions bo = new BatchOptions();
                    BatchfileFromSettlement.MakeSettlementJson(set, sim, bo, wd.WorkingDirectory);
                    SettlementInformation si = SettlementInformation.Read(wd.WorkingDirectory);
                    Assert.NotNull(si);
                    db.Cleanup();
                }
            }
            //wd.CleanUp();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunNaturalLightBatch()
        {
            SimulationEngineConfig.IsUnitTest = true;
            SimulationEngineConfig.CatchErrors = false;
            SimulationEngineTestPreparer se = new SimulationEngineTestPreparer("RunNaturalLight");
            List<string> arguments = new List<string>
            {
                "--MakeLightBatch"
            };
            MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
            se.Clean();
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            SimulationEngineTestPreparer se = new SimulationEngineTestPreparer("MasterBatch");
            List<string> arguments = new List<string>
            {
                "--MakeMasterBatch"
            };
            MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
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
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void PdfReplacerTest()
        {
            SimulationEngineTestPreparer se = new SimulationEngineTestPreparer("MasterBatch");
            List<string> arguments = new List<string>
            {
                "--Batch-ModularHouseholds"
            };
            SimulationEngineConfig.IsUnitTest = true;
            MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
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

        public MasterBatchfileTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
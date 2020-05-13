using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Automation;
using Common;
using Common.Tests;
using NUnit.Framework;
using SimulationEngineLib;

namespace SimulationEngine.Tests {
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SettlementProcessorTests : UnitTestBaseClass
    {
        private static void MakeOneCalculation([JetBrains.Annotations.NotNull] string srcPath, int idx) {
            using (var p = new Process()) {
                p.StartInfo.WorkingDirectory = srcPath;
                p.StartInfo.FileName = "SimulationEngine.exe";
                p.StartInfo.Arguments = "--Calculate --ModularHousehold --CalcObjectNumber " + idx +
                                        " --StartDate 01.01.2015 --EndDate 03.01.2015";
                p.Start();
                p.WaitForExit();
            }
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RunTest() {
            using var se = new SimulationEngineTestPreparer(Utili.GetCurrentMethodAndClass());
            var srcPath = se.WorkingDirectory;
            Directory.SetCurrentDirectory(srcPath);
            for (var idx = 0; idx < 5; idx++) {
                MakeOneCalculation(srcPath, idx);
            }
            //SettlementProcessor sp = new SettlementProcessor();
            //string connectionString = "Data Source=ProfileGenerator.db3";
            //   sp.RunCollection(connectionString);
            se.Clean();
        }

        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void RunQuickTest()
        {
            if(Directory.Exists(@"Z:\IEEEv7\SettlementProcessing")) {
                Directory.Delete(@"Z:\IEEEv7\SettlementProcessing",true);
                Thread.Sleep(500);
            }
            const string path = @"Z:\IEEEv7";
            Directory.SetCurrentDirectory(path);
            List<string> arguments = new List<string>
            {
                "ProcessSettlement",
                "-Directory",
                ".",
                "-SettlementResultPDF"
            };
            MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
        }
    }
}
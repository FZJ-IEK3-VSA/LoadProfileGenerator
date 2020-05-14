#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.JSON;
using Common.Tests;
using Database;
using Database.DatabaseMerger;
using Database.Helpers;
using Database.Tests;
using NUnit.Framework;
using SimulationEngineLib;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace SimulationEngine.Tests {
    [TestFixture]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class ProgramTests : UnitTestBaseClass
    {
        [JetBrains.Annotations.NotNull]
        public static WorkingDir SetupDB3([JetBrains.Annotations.NotNull] string name, bool clearTemplatedFirst = false) {
            var srcPath = DatabaseSetup.GetSourcepath(null);
            SimulationEngineConfig.CatchErrors = false;
            var wd = new WorkingDir(name);
            var dstfullName = Path.Combine(wd.WorkingDirectory, "profilegenerator.db3");
            SimulationEngineConfig.IsUnitTest = true;
            if (File.Exists("profilegenerator.db3")) {
                File.Delete("profilegenerator.db3");
            }
            File.Copy(srcPath, dstfullName);

            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            Logger.Info("Using directory:" + Directory.GetCurrentDirectory());
            var connectionString = "Data Source=" + dstfullName;
            // ReSharper disable once UseObjectOrCollectionInitializer
            Simulator sim = new Simulator(connectionString);
            sim.MyGeneralConfig.DeviceProfileHeaderMode = DeviceProfileHeaderMode.Standard;
            if (clearTemplatedFirst) {
                sim.FindAndDeleteAllTemplated();
            }
            DatabaseVersionChecker.CheckVersion(connectionString);
            Logger.Info("Checked database version. It is ok.");
            return wd;
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void CSVImportTest()
        {
            using (var db1 = new DatabaseSetup(Utili.GetCurrentMethodAndClass() + "_export"))
            {
                //export
                using (var wd = SetupDB3(Utili.GetCurrentMethodAndClass()))
                {
                    var sim = new Simulator(db1.ConnectionString);
                    ModularHouseholdSerializer.ExportAsCSV(sim.ModularHouseholds[0], sim,
                        Path.Combine(wd.WorkingDirectory, "testexportfile.csv"));
                    //import

                    var arguments = new List<string>
            {
                "--ImportHouseholdDefinition",
                "testexportfile.csv"
            };
                    MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                    db1.Cleanup();
                    wd.CleanUp(1);
                }
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void CSVImportTest2()
        {
            using (var db1 = new DatabaseSetup(Utili.GetCurrentMethodAndClass() + "_export"))
            {
                //export
                using (var wd = SetupDB3(Utili.GetCurrentMethodAndClass()))
                {
                    const string srcfile = @"v:\work\CHR15a_Sc1.csv";
                    File.Copy(srcfile, Path.Combine(wd.WorkingDirectory, "hh.csv"));
                    var sim = new Simulator("Data Source=profilegenerator.db3") { MyGeneralConfig = { CSVCharacter = ";" } };
                    sim.MyGeneralConfig.SaveToDB();
                    var dbm = new DatabaseMerger(sim);
                    const string importPath = @"v:\work\profilegenerator_hennings.db3";
                    dbm.RunFindItems(importPath, null);
                    dbm.RunImport(null);
                    ModularHouseholdSerializer.ExportAsCSV(sim.ModularHouseholds[0], sim,
                        Path.Combine(wd.WorkingDirectory, "testexportfile.csv"));
                    //import

                    var arguments = new List<string>
            {
                "--ImportHouseholdDefinition",
                "hh.csv"
            };
                    MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                    db1.Cleanup();
                    wd.CleanUp(1);
                }
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void ListEnviromentalVariables() {
            SimulationEngineConfig.CatchErrors = false;
            var dict = Environment.GetEnvironmentVariables();
#pragma warning disable CS8605 // Unboxing a possibly null value.
            foreach (DictionaryEntry env in dict) {
#pragma warning restore CS8605 // Unboxing a possibly null value.
                var name = (string) env.Key;
                var value =( (string?) env.Value)??throw new LPGException("value was null");
                Logger.Info(name + " = " + value);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MainListLoadTypePriorities()
        {
            using (var wd = SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var arguments = new List<string>
            {
                "List",
                "-LoadtypePriorities"
            };
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void MainTestBatchCommandlineModularHouseholds()
        {
            using (var wd = SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var arguments = new List<string>();
                var dstpath = Path.Combine(wd.WorkingDirectory, "calc");
                var args =
                    "Calculate -CalcObjectType ModularHousehold -CalcObjectNumber 0 -OutputFileDefault ReasonableWithChartsAndPDF " +
                    "-StartDate 01.01.2015 -EndDate 10.01.2015 -SkipExisting -OutputDirectory " +
                    dstpath;
                arguments.AddRange(args.Split(' '));
                arguments = arguments.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                wd.CleanUp(1);
            }
        }

        /*
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MainTestBatchModularHouseholds() {
            var wd = SetupDB3(Utili.GetCurrentMethodAndClass());
            const string filename = "Start-ModularHousehold.cmd";
            if (File.Exists(filename)) {
                File.Delete(filename);
            }
            var arguments = new List<string>
            {
                "TestBatch",
                "-ModularHouseholds"
            };
            Program.Main(arguments.ToArray());
            Assert.IsTrue(File.Exists(filename));
            wd.CleanUp(1);
        }*/
        /*
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MainTestBatchSettlements() {
            const string filename = "Start-Settlement.cmd";
            if (File.Exists(filename)) {
                File.Delete(filename);
            }
            var wd = SetupDB3(nameof(MainTestBatchSettlements));
            var arguments = new List<string>
            {
                "TestBatch",
                "-Settlements"
            };
            Program.Main(arguments.ToArray());
            Assert.IsTrue(File.Exists(filename));
            wd.CleanUp(1);
        }*/

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MainTestForHelp()
        {
            using (var wd = SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var args = Array.Empty<string>();
                Assert.Throws<LPGException>(() => MainSimEngine.Run(args.ToArray(), "simulationengine.exe"));
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void MainTestLaunchParallelModularHouseholds()
        {
            SimulationEngineConfig.CatchErrors = false;
            var di = new DirectoryInfo(".");
            var fis = di.GetFiles("*.*");
            var oldDir = Directory.GetCurrentDirectory();
            var db3Path = Path.Combine(oldDir, "profilegenerator-latest.db3");
            using (var wd = new WorkingDir("ModularBatchPar"))
            {
                Thread.Sleep(1000);
                var prevDir = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(wd.WorkingDirectory);
                foreach (var fi in fis)
                {
                    var dstpath = Path.Combine(wd.WorkingDirectory, fi.Name);
                    fi.CopyTo(dstpath);
                }
                if (File.Exists("profilegenerator.db3"))
                {
                    File.Delete("profilegenerator.db3");
                }
                File.Copy(db3Path, "profilegenerator.db3");
                const string filename = "Start-ModularHousehold.cmd";
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
                var arguments = new List<string>
            {
                "--Batch-ModularHouseholds"
            };
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                Assert.IsTrue(File.Exists(filename));
                arguments.Clear();
                arguments.Add("--LaunchParallel");
                arguments.Add("--NumberCores");
                arguments.Add("4");
                arguments.Add("--Batchfile");
                arguments.Add("Start-ModularHousehold.cmd");
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                Directory.SetCurrentDirectory(prevDir);
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MainTestListGeoLocs()
        {
            using (var wd = SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var arguments = new List<string>
            {
                "List",
                "-GeographicLocations"
            };
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void ProfilesGeoLocs()
        {
            using (var wd = SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var arguments = new List<string>
            {
                "Calculate",
                "-CalcObjectType",
                "ModularHousehold",
                "-CalcObjectNumber",
                "1",
                "-OutputDirectory",
                Path.Combine(wd.WorkingDirectory,"Results"),
                "-LoadtypePriority",
                "Mandatory",
                "-SkipExisting",
                "-MeasureCalculationTimes",
                "-TemperatureProfileIndex",
                "0",
                "-GeographicLocationIndex",
                "10",
                "-StartDate",
                new DateTime(2018,1,1).ToString(CultureInfo.CurrentCulture),
                "-EndDate",
                new DateTime(2018,12,31).ToString(CultureInfo.CurrentCulture),
                "-EnergyIntensityType",
                "Random",
                "-ExternalTimeResolution",
                "00:15",
                "-OutputFileDefault",
                "OnlySums",
                "-CalcOption",
                "LocationsFile"
            };
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MainTestListHouses()
        {
            using (var wd = SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var arguments = new List<string>
            {
                "List",
                "-Houses"
            };
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MainTestListModularHouseholds()
        {
            using (var wd = SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var arguments = new List<string>
            {
                "List",
                "-ModularHouseholds"
            };
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MainTestListSettlements()
        {
            SimulationEngineConfig.CatchErrors = false;
            using (var wd = SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var arguments = new List<string>
            {
                "List",
                "-Settlements"
            };
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MainTestListTemperatureProfiles()
        {
            using (var wd = SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var arguments = new List<string>
            {
                "List",
                "-TemperatureProfiles"
            };
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MainTestNoDB3() {
            SimulationEngineConfig.CatchErrors = false;
            if (File.Exists("profilegenerator.db3")) {
                File.Delete("profilegenerator.db3");
            }
            SimulationEngineConfig.IsUnitTest = true;
            var args = Array.Empty<string>();
            Assert.Throws<LPGException>(() => MainSimEngine.Run(args.ToArray(), "simulationengine.exe"));
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest5)]
        public void SettlementToBatchTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                var settlementname = sim.Settlements.It[0].Name.Substring(0, 20);
                db.Cleanup();
                SimulationEngineConfig.CatchErrors = false;
                SimulationEngineConfig.IsUnitTest = true;
                using var setp = new SimulationEngineTestPreparer(nameof(SettlementToBatchTest));
                var arguments = new List<string>
            {
                "Batch",
                "-SettlementIndex",
                "0",
                "-Suffix",
                "Test",
                "-OutputFileDefault",
                "OnlyOverallSum"
            };
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                var di = new DirectoryInfo(setp.WorkingDirectory);
                var fis = di.GetFiles("*.cmd", SearchOption.AllDirectories);
                string targetname = "Start-" + settlementname + ".Test.cmd";
                Logger.Info("Targetname: " + targetname);
                foreach (FileInfo info in fis)
                {
                    Logger.Info("File: " + info.Name);
                }
                var fi = fis.First(x => x.Name == targetname);
                string? line;
                using (var sr = new StreamReader(fi.FullName))
                {
                    line = sr.ReadLine();
                }
                if (line == null)
                {
                    throw new LPGException("Readline failed.");
                }
                var arr = line.Split(' ');
                var args2 = new List<string>();
                Logger.Info("Line: " + line);
                for (var i = 1; i < arr.Length; i++)
                {
                    args2.Add(arr[i]);
                }
                foreach (var arg in args2)
                {
                    Logger.Info(arg);
                }
                MainSimEngine.Run(args2.ToArray(), "simulationengine.exe");
                setp.Clean();
            }
        }

        public ProgramTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Automation.ResultFiles;
using Common;
using Common.JSON;
using Common.Tests;
using Database;
using Database.DatabaseMerger;
using Database.Helpers;
using Database.Tests;
using JetBrains.Annotations;
using NUnit.Framework;

namespace SimulationEngine.Tests {
    [TestFixture]
    public class ProgramTests : UnitTestBaseClass
    {
        [NotNull]
        public static WorkingDir SetupDB3([NotNull] string name, bool clearTemplatedFirst = false) {
            var srcPath = DatabaseSetup.GetSourcepath(null, DatabaseSetup.TestPackage.SimulationEngine);
            Program.CatchErrors = false;
            var wd = new WorkingDir(name);
            var dstfullName = Path.Combine(wd.WorkingDirectory, "profilegenerator.db3");
            Program.IsUnitTest = true;
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

        [Test]
        [Category("QuickChart")]
        public void CSVImportTest() {
            var db1 = new DatabaseSetup(Utili.GetCurrentMethodAndClass() + "_export", DatabaseSetup.TestPackage.SimulationEngine);
            //export
            var wd = SetupDB3(Utili.GetCurrentMethodAndClass());

            var sim = new Simulator(db1.ConnectionString);
            ModularHouseholdSerializer.ExportAsCSV(sim.ModularHouseholds[0], sim,
                Path.Combine(wd.WorkingDirectory, "testexportfile.csv"));
            //import

            var arguments = new List<string>
            {
                "--ImportHouseholdDefinition",
                "testexportfile.csv"
            };
            Program.Main(arguments.ToArray());
            db1.Cleanup();
            wd.CleanUp(1);
        }

        [Test]
        [Category("QuickChart")]
        public void CSVImportTest2() {
            var db1 = new DatabaseSetup(Utili.GetCurrentMethodAndClass() + "_export", DatabaseSetup.TestPackage.SimulationEngine);
            //export
            var wd = SetupDB3(Utili.GetCurrentMethodAndClass());
            const string srcfile = @"v:\work\CHR15a_Sc1.csv";
            File.Copy(srcfile, Path.Combine(wd.WorkingDirectory, "hh.csv"));
            var sim = new Simulator("Data Source=profilegenerator.db3") {MyGeneralConfig = {CSVCharacter = ";"}};
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
            Program.Main(arguments.ToArray());
            db1.Cleanup();
            wd.CleanUp(1);
        }

        [Test]
        [Category("BasicTest")]
        public void ListEnviromentalVariables() {
            Program.CatchErrors = false;
            var dict = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry env in dict) {
                var name = (string) env.Key;
                var value = (string) env.Value;
                Logger.Info(name + " = " + value);
            }
        }

        [Test]
        [Category("BasicTest")]
        public void MainListLoadTypePriorities() {
            var wd = SetupDB3(Utili.GetCurrentMethodAndClass());
            var arguments = new List<string>
            {
                "List",
                "-LoadtypePriorities"
            };
            Program.Main(arguments.ToArray());
            wd.CleanUp(1);
        }

        [Test]
        [Category("QuickChart")]
        public void MainTestBatchCommandlineModularHouseholds() {
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            SetupDB3(Utili.GetCurrentMethodAndClass());
            var arguments = new List<string>();
            var dstpath = Path.Combine(wd.WorkingDirectory, "calc");
            var args =
                "Calculate -CalcObjectType ModularHousehold -CalcObjectNumber 0 -OutputFileDefault ReasonableWithChartsAndPDF " +
                "-StartDate 01.01.2015 -EndDate 10.01.2015 -SkipExisting -OutputDirectory " +
                dstpath;
            arguments.AddRange(args.Split(' '));
            arguments = arguments.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            Program.Main(arguments.ToArray());
            wd.CleanUp(1);
        }

        [Test]
        [Category("BasicTest")]
        public void MainTestBatchHouses() {
            const string filename = "Start-House.cmd";
            if (File.Exists(filename)) {
                File.Delete(filename);
            }
            var wd = SetupDB3(Utili.GetCurrentMethodAndClass());
            var arguments = new List<string>
            {
                "TestBatch",
                "-Houses"
            };
            Program.Main(arguments.ToArray());
            Assert.IsTrue(File.Exists(filename));
            wd.CleanUp(1);
        }

        [Test]
        [Category("BasicTest")]
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
        }

        [Test]
        [Category("BasicTest")]
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
        }

        [Test]
        [Category("BasicTest")]
        public void MainTestForHelp() {
            var wd = SetupDB3(Utili.GetCurrentMethodAndClass());
            var args = Array.Empty<string>();
            Assert.Throws<LPGException>(()=> Program.Main(args));
            wd.CleanUp(1);
        }

        [Test]
        [Category("QuickChart")]
        public void MainTestLaunchParallelModularHouseholds() {
            Program.CatchErrors = false;
            var di = new DirectoryInfo(".");
            var fis = di.GetFiles("*.*");
            var oldDir = Directory.GetCurrentDirectory();
            var db3Path = Path.Combine(oldDir, "profilegenerator-latest.db3");
            var wd = new WorkingDir("ModularBatchPar");
            Thread.Sleep(1000);
            var prevDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            foreach (var fi in fis) {
                var dstpath = Path.Combine(wd.WorkingDirectory, fi.Name);
                fi.CopyTo(dstpath);
            }
            if (File.Exists("profilegenerator.db3")) {
                File.Delete("profilegenerator.db3");
            }
            File.Copy(db3Path, "profilegenerator.db3");
            const string filename = "Start-ModularHousehold.cmd";
            if (File.Exists(filename)) {
                File.Delete(filename);
            }
            var arguments = new List<string>
            {
                "--Batch-ModularHouseholds"
            };
            Program.Main(arguments.ToArray());
            Assert.IsTrue(File.Exists(filename));
            arguments.Clear();
            arguments.Add("--LaunchParallel");
            arguments.Add("--NumberCores");
            arguments.Add("4");
            arguments.Add("--Batchfile");
            arguments.Add("Start-ModularHousehold.cmd");
            Program.Main(arguments.ToArray());
            Directory.SetCurrentDirectory(prevDir);
            wd.CleanUp(1);
        }

        [Test]
        [Category("BasicTest")]
        public void MainTestListGeoLocs() {
            var wd = SetupDB3(Utili.GetCurrentMethodAndClass());
            var arguments = new List<string>
            {
                "List",
                "-GeographicLocations"
            };
            Program.Main(arguments.ToArray());
            wd.CleanUp(1);
        }

        [Test]
        [Category("BasicTest")]
        public void ProfilesGeoLocs()
        {
            var wd = SetupDB3(Utili.GetCurrentMethodAndClass());
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
            Program.Main(arguments.ToArray());
            wd.CleanUp(1);
        }

        [Test]
        [Category("BasicTest")]
        public void MainTestListHouses() {
            var wd = SetupDB3(Utili.GetCurrentMethodAndClass());
            var arguments = new List<string>
            {
                "List",
                "-Houses"
            };
            Program.Main(arguments.ToArray());
            wd.CleanUp(1);
        }

        [Test]
        [Category("BasicTest")]
        public void MainTestListModularHouseholds() {
            var wd = SetupDB3(Utili.GetCurrentMethodAndClass());
            var arguments = new List<string>
            {
                "List",
                "-ModularHouseholds"
            };
            Program.Main(arguments.ToArray());
            wd.CleanUp(1);
        }

        [Test]
        [Category("BasicTest")]
        public void MainTestListSettlements() {
            Program.CatchErrors = false;
            var wd = SetupDB3(Utili.GetCurrentMethodAndClass());
            var arguments = new List<string>
            {
                "List",
                "-Settlements"
            };
            Program.Main(arguments.ToArray());
            wd.CleanUp(1);
        }

        [Test]
        [Category("BasicTest")]
        public void MainTestListTemperatureProfiles() {
            var wd = SetupDB3(Utili.GetCurrentMethodAndClass());
            var arguments = new List<string>
            {
                "List",
                "-TemperatureProfiles"
            };
            Program.Main(arguments.ToArray());
            wd.CleanUp(1);
        }

        [Test]
        [Category("BasicTest")]
        public void MainTestNoDB3() {
            Program.CatchErrors = false;
            if (File.Exists("profilegenerator.db3")) {
                File.Delete("profilegenerator.db3");
            }
            Program.IsUnitTest = true;
            var args = Array.Empty<string>();
            Assert.Throws<LPGException>(() => Program.Main(args));
        }

        [Test]
        [Category("LongTest5")]
        public void SettlementToBatchTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
            Simulator sim = new Simulator(db.ConnectionString);
            var settlementname = sim.Settlements.It[0].Name.Substring(0,20);
            db.Cleanup();
            Program.CatchErrors = false;
            Program.IsUnitTest = true;
            var setp = new SimulationEngineTestPreparer(nameof(SettlementToBatchTest));
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
            Program.Main(arguments.ToArray());
            var di = new DirectoryInfo(setp.WorkingDirectory);
            var fis = di.GetFiles("*.cmd", SearchOption.AllDirectories);
            string targetname = "Start-" + settlementname + ".Test.cmd";
            Logger.Info("Targetname: " + targetname);
            foreach (FileInfo info in fis) {
                Logger.Info("File: " + info.Name);
            }
            var fi = fis.First(x => x.Name == targetname);
            string line;
            using (var sr = new StreamReader(fi.FullName)) {
                line = sr.ReadLine();
            }
            if (line == null) {
                throw new LPGException("Readline failed.");
            }
            var arr = line.Split(' ');
            var args2 = new List<string>();
            Logger.Info("Line: " + line);
            for (var i = 1; i < arr.Length; i++) {
                args2.Add(arr[i]);
            }
            foreach (var arg in args2) {
                Logger.Info(arg);
            }
            Program.Main(args2.ToArray());
            setp.Clean();
        }
    }
}
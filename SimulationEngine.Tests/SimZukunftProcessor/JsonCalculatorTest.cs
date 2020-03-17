using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.JSON;
using Database;
using Database.Tests;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using SimulationEngine.SimZukunftProcessor;

namespace SimulationEngine.Tests.SimZukunftProcessor
{
    public class CSVProfileReader
    {
        public enum FileTypeEnum {
            Overall,
            SumProfile
        }
        public CSVProfileReader([NotNull] string filename, [NotNull] string fullFilename)
        {
            Filename = filename;
            FullFilename = fullFilename;
        }

        [NotNull]
        public string Filename { get; set; }
        [NotNull]
        public string FullFilename { get; set; }
        [CanBeNull]
        public string Loadtype { get; set; }
        [CanBeNull]
        public string Household { get; set; }
        public FileTypeEnum FileType { get; set; }
        [NotNull]
        public List<int> TimeSteps { get; set; } = new List<int>();
        [NotNull]
        [ItemNotNull]
        public List<string> TimeStamps { get; set; } = new List<string>();
        [ItemNotNull]
        [NotNull]
        public List<List<double>> RowValues { get; set; } = new List<List<double>>();
        [NotNull]
        [ItemNotNull]
        [SuppressMessage("ReSharper", "CollectionNeverQueried.Local")]
        private List<string> Headers { get; } = new List<string>();
        public double SumAllValues { get; set; }
        [NotNull]
        public static CSVProfileReader ReadFile([NotNull] string filename)
        {
            FileInfo fi = new FileInfo(filename);
            Console.WriteLine("Reading " + fi.Name);
            var filearr = fi.Name.Split('.');
            CSVProfileReader cp = new CSVProfileReader(fi.Name,fi.FullName);

            if (filearr[0] == "Overall" && filearr[1] == "SumProfiles") {
                cp.Loadtype = filearr[2];
                cp.FileType = FileTypeEnum.Overall;
            }
            else if(filearr[0] == "SumProfiles" && filearr.Length == 4) {
                cp.Loadtype = filearr[2];
                cp.FileType = FileTypeEnum.SumProfile;
                cp.Household = filearr[1];
            }
            else {
                throw new LPGException("Unknown file type: " + fi.Name);
            }
            StreamReader sr = new StreamReader(filename);
            string header = sr.ReadLine();
            if (header == null) {
                throw new LPGException("Header was null");
            }
            var headerarr = header.Split(';');
            cp.Headers.AddRange(headerarr);
            while (!sr.EndOfStream) {
                string line = sr.ReadLine();
                if (line == null) {
                    throw new LPGException("Line was null");
                }
                var arr = line.Split(';');
                if (arr.Length > 2) {
                    cp.TimeSteps.Add(Convert.ToInt32(arr[0]));
                    cp.TimeStamps.Add(arr[1]);
                    var vals = new List<double>();
                    for (int i = 2; i < arr.Length; i++) {
                        if (!string.IsNullOrWhiteSpace(arr[i])) {
                            double d = Convert.ToDouble(arr[i]);
                            vals.Add(d);
                        }
                    }
                    cp.RowValues.Add(vals);
                }
            }
            sr.Close();
            double sum = 0;
            foreach (var row in cp.RowValues) {
                sum += row.Sum();
            }

            cp.SumAllValues = sum;
            return cp;
        }
    }


    public class JsonCalculatorTest
    {
        [Test]
        [Category("ManualOnly")]
        public void SumChecker()
        {
            const string startpath = @"D:\LPGUnitTest\JsonCalculatorTest.EndToEndTest\results";
            var di = new DirectoryInfo(startpath);
            var subdirs = di.GetDirectories();
            foreach (DirectoryInfo subdir in subdirs) {
                var resultDirs = subdir.GetDirectories("Results");
                if (resultDirs.Length != 1) {
                    Logger.Info("No Results found in " + subdir.FullName);
                    continue;
                }

                var resultFiles = resultDirs[0].GetFiles("*.csv");
                List<CSVProfileReader> csvs = new List<CSVProfileReader>();
                foreach (FileInfo info in resultFiles) {
                    csvs.Add( CSVProfileReader .ReadFile(info.FullName));
                }

                var loadTypes = csvs.Select(x => x.Loadtype).Distinct().ToList();
                foreach (string loadType in loadTypes) {
                    var filtered = csvs.Where(x => x.Loadtype == loadType).ToList();
                    foreach (var reader in filtered) {
                        Console.WriteLine(reader.Filename);
                    }
                }
            }

        }
        [Test]
        [Category("ManualOnly")]
        public void EndToEndTest()
        {
            Console.WriteLine(Assembly.GetCallingAssembly().FullName);
            Config.SkipFreeSpaceCheckForCalculation = true;
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DirectoryInfo simengine = new DirectoryInfo(@"V:\Dropbox\LPG\SimulationEngine\bin\x64\Debug");
            foreach (FileInfo srcFile in simengine.GetFiles("*.*",SearchOption.AllDirectories)) {
                string relative = srcFile.FullName.Substring(simengine.FullName.Length);
                while (relative.StartsWith("\\")) {
                    relative = relative.Substring(1);
                }

                string dstPath = Path.Combine(wd.WorkingDirectory, relative);
                FileInfo dstFile = new FileInfo(dstPath);
                if (dstFile.Directory != null && !(dstFile.Directory.Exists)) {
                    dstFile.Directory.Create();
                }
                srcFile.CopyTo(dstPath);
            }
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(),
                DatabaseSetup.TestPackage.SimulationEngine);
            HouseGenerator.CreateDistrictDefinition(db.ConnectionString);
            DistrictDefinitionProcessingOptions ddpo = new DistrictDefinitionProcessingOptions
            {
                DstPath = wd.Combine("Results"),
                JsonPath = wd.Combine("Example")
            };
            HouseGenerator.ProcessDistrictDefinitionFiles(ddpo,db.ConnectionString);
            var lo = new ParallelJsonLauncher.ParallelJsonLauncherOptions
            {
                JsonDirectory = ddpo.DstPath
            };
            ParallelJsonLauncher.LaunchParallel(lo);
           // db.Cleanup();
        }

        [Test]
        [Category("ManualOnly")]
        public void OnlyExternalTest()
        {
            Logger.Get().StartCollectingAllMessages();
            Console.WriteLine(Assembly.GetCallingAssembly().FullName);
            Config.SkipFreeSpaceCheckForCalculation = true;
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DirectoryInfo simengine = new DirectoryInfo(@"V:\Dropbox\LPG\SimulationEngine\bin\Debug\net472");
            foreach (FileInfo srcFile in simengine.GetFiles("*.*", SearchOption.AllDirectories))
            {
                string relative = srcFile.FullName.Substring(simengine.FullName.Length);
                while (relative.StartsWith("\\"))
                {
                    relative = relative.Substring(1);
                }

                string dstPath = Path.Combine(wd.WorkingDirectory, relative);
                FileInfo dstFile = new FileInfo(dstPath);
                if (dstFile.Directory != null && !(dstFile.Directory.Exists))
                {
                    dstFile.Directory.Create();
                }
                srcFile.CopyTo(dstPath);
            }
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(),DatabaseSetup.TestPackage.SimulationEngine);
            Simulator sim = new Simulator(db.ConnectionString);
            var house =sim.Houses.FindByName("CHR07 in HT04 with Car, 05 km to work, 3.7 kW Charging at home");
            if (house == null) {
                throw new LPGException("house not found");
            }

            JsonCalcSpecification jcs = JsonCalcSpecification.MakeDefaultsForTesting();
            jcs.CalcObject = house.GetJsonReference();
            jcs.OutputDirectory = Path.Combine(wd.WorkingDirectory, "Results");
            jcs.PathToDatabase = db.FileName;
            jcs.DeleteSqlite = true;
            jcs.StartDate = new DateTime(2019,1,1);
            jcs.EndDate = new DateTime(2019,1,7);
            if (jcs.LoadtypesForPostprocessing == null) {
                jcs.LoadtypesForPostprocessing = new List<string>();
            }
            jcs.LoadtypesForPostprocessing.Add("Electricity");

            jcs.DefaultForOutputFiles = OutputFileDefault.None;
            if (jcs.CalcOptions == null) {
                jcs.CalcOptions = new List<CalcOption>();
            }
            jcs.CalcOptions.Add(CalcOption.SumProfileExternalEntireHouse);
            JsonCalcSpecSerializer jg = new JsonCalcSpecSerializer();
            string jsonfileName = wd.Combine("calcspec.json");
            jg.WriteJsonToFile(jsonfileName, jcs);

            JsonCalculator jc = new JsonCalculator();
            var options = new JsonDirectoryOptions(jsonfileName);
            jc.Calculate(options);
            const string sqliteanalyizer = @"v:\dropbox\lpg\sqlite3_analyzer.exe";
            if (File.Exists(sqliteanalyizer)) {
                StreamWriter sw = new StreamWriter(Path.Combine(jcs.OutputDirectory, "analyzesqlite.cmd"));
                DirectoryInfo resultDi = new DirectoryInfo(jcs.OutputDirectory);
                var fis = resultDi.GetFiles("*.sqlite");
                foreach (var fi in fis) {
                    sw.WriteLine(sqliteanalyizer + " \"" + fi.FullName + "\" > " + fi.Name + ".analysis.txt");
                }
                sw.Close();
            }
            else {
                throw new LPGException("analyzer not found");
            }
            // db.Cleanup();
        }

        [Test]
        [Category("BasicTest")]
        public void TestDistrictDefinition()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(),
                DatabaseSetup.TestPackage.SimulationEngine);
            HouseGenerator.CreateDistrictDefinition(db.ConnectionString);
            // db.Cleanup();
        }


        [Test]
        [Category("BasicTest")]
        public void RunJsonCalculatorTest()
        {
            Logger.Get().StartCollectingAllMessages();
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(),
                DatabaseSetup.TestPackage.SimulationEngine);
            string dbPath = wd.Combine("my.db3");
            File.Copy(db.FileName,dbPath );
            Console.WriteLine("DB File Path: " + dbPath);
            JsonCalcSpecification jcs = new JsonCalcSpecification();
            if (jcs.CalcOptions == null) {
                throw new LPGException("Calcoptions was null");
            }
            jcs.CalcOptions.Add(CalcOption.DeviceProfiles);
            jcs.CalcOptions.Add(CalcOption.DeviceProfiles);
            jcs.PathToDatabase = dbPath;
            jcs.StartDate = new DateTime(2019,1,1);
            jcs.EndDate = new DateTime(2019, 1, 3);
            jcs.DefaultForOutputFiles = OutputFileDefault.OnlySums;
            Simulator sim = new Simulator(db.ConnectionString);
            jcs.CalcObject = sim.Houses[0].GetJsonReference();
            JsonCalcSpecSerializer jg = new JsonCalcSpecSerializer();
            string jsonfilename = wd.Combine("Example.json");
            jg.WriteJsonToFile(jsonfilename, jcs);
            JsonCalculator jc = new JsonCalculator();
            JsonDirectoryOptions jo = new JsonDirectoryOptions
            {
                Input = jsonfilename
            };
            jc.Calculate(jo);
            Directory.SetCurrentDirectory(wd.PreviousCurrentDir);
            //wd.CleanUp();
        }


        [Test]
        [Category("BasicTest")]
        public void RunJsonCalculatorTestForExternalTimeResolutionJsonFile()
        {
            Logger.Get().StartCollectingAllMessages();
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(),
                DatabaseSetup.TestPackage.SimulationEngine);
            string dbPath = wd.Combine("my.db3");
            File.Copy(db.FileName, dbPath);
            Console.WriteLine("DB File Path: " + dbPath);
            JsonCalcSpecification jcs = new JsonCalcSpecification();
            if (jcs.CalcOptions == null)
            {
                throw new LPGException("Calcoptions was null");
            }
            jcs.DefaultForOutputFiles = OutputFileDefault.None;
            jcs.CalcOptions.Add(CalcOption.SumProfileExternalIndividualHouseholdsAsJson);
            jcs.PathToDatabase = dbPath;
            jcs.StartDate = new DateTime(2019, 1, 1);
            jcs.EndDate = new DateTime(2019, 1, 3);
            jcs.ExternalTimeResolution = "00:15:00";
            jcs.LoadTypePriority = LoadTypePriority.RecommendedForHouses;
            Simulator sim = new Simulator(db.ConnectionString);
            jcs.CalcObject = sim.Houses[0].GetJsonReference();
            JsonCalcSpecSerializer jg = new JsonCalcSpecSerializer();
            string jsonfilename = wd.Combine("Example.json");
            jg.WriteJsonToFile(jsonfilename, jcs);
            JsonCalculator jc = new JsonCalculator();
            JsonDirectoryOptions jo = new JsonDirectoryOptions
            {
                Input = jsonfilename
            };
            jc.Calculate(jo);
            DirectoryInfo dstDir = new DirectoryInfo(wd.WorkingDirectory);
            var files = dstDir.GetFiles("*.json", SearchOption.AllDirectories);
            foreach (FileInfo info in files) {
                Logger.Info(info.Name);
            }
            Assert.That(files.Length,Is.GreaterThan(1));
            Directory.SetCurrentDirectory(wd.PreviousCurrentDir);
            //wd.CleanUp();
        }
        [Test]
        [Category("ManualOnly")]
        public void RunJsonCalculatorExportTest()
        {
            Config.IsInHeadless = true;
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            //DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
            JsonCalculator jc = new JsonCalculator();
            const string jsonfilename = @"C:\work\LPGUnitTest\SettlementTests.JsonCalcSpecTest\CHR07 in HT04 with Car, 30 km to work, 22kW Charging at work 4.json";
            JsonDirectoryOptions jo = new JsonDirectoryOptions
            {
                Input = jsonfilename

            };
            Logger.Get().StartCollectingAllMessages();
            jc.Calculate(jo);
            Directory.SetCurrentDirectory(wd.PreviousCurrentDir);
            //wd.CleanUp();
        }

        [Test]
        [Category("ManualOnly")]
        public void RunBrokenHouseholdCalcTest()
        {
            Config.IsInHeadless = true;
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var db = new FileInfo(@"V:\Dropbox\LPG\ImportFiles\profilegenerator900.db3");
            string fullDstPath = Path.Combine(wd.WorkingDirectory, "profilegenerator.db3");
            db.CopyTo(fullDstPath);
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            Simulator sim = new Simulator("Data Source="+fullDstPath);
            sim.Should().NotBeNull();
            wd.CleanUp();
        }

        [Test]
        [Category("ManualOnly")]
        public void RunFailingStuffTest()
        {
            Config.IsInHeadless = true;
            Config.IsInUnitTesting = true;
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            //DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
            JsonCalculator jc = new JsonCalculator();
            const string jsonfilename = @"V:\Dropbox\LPG\failingTest.json";
            JsonDirectoryOptions jo = new JsonDirectoryOptions
            {
                Input = jsonfilename

            };
            Logger.Get().StartCollectingAllMessages();
            jc.Calculate(jo);
            Directory.SetCurrentDirectory(wd.PreviousCurrentDir);
            //wd.CleanUp();
        }

    }
}

#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Tests;
using Database;
using Database.Tests;
using FluentAssertions;
using JetBrains.Annotations;
using SimulationEngineLib.HouseJobProcessor;
using Xunit;
using Xunit.Abstractions;

namespace SimulationEngine.Tests.SimZukunftProcessor
{
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class CSVProfileReader
    {
        public enum FileTypeEnum {
            Overall,
            SumProfile
        }
        public CSVProfileReader([JetBrains.Annotations.NotNull] string filename, [JetBrains.Annotations.NotNull] string fullFilename)
        {
            Filename = filename;
            FullFilename = fullFilename;
        }

        [JetBrains.Annotations.NotNull]
        public string Filename { get; set; }
        [JetBrains.Annotations.NotNull]
        public string FullFilename { get; set; }
        public string? Loadtype { get; set; }
        public string? Household { get; set; }
        public FileTypeEnum FileType { get; set; }
        [JetBrains.Annotations.NotNull]
        public List<int> TimeSteps { get; set; } = new List<int>();
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public List<string> TimeStamps { get; set; } = new List<string>();
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<List<double>> RowValues { get; set; } = new List<List<double>>();
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        [SuppressMessage("ReSharper", "CollectionNeverQueried.Local")]
        private List<string> Headers { get; } = new List<string>();
        public double SumAllValues { get; set; }
        [JetBrains.Annotations.NotNull]
        public static CSVProfileReader ReadFile([JetBrains.Annotations.NotNull] string filename)
        {
            FileInfo fi = new FileInfo(filename);
            Logger.Info("Reading " + fi.Name);
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
            string? header = sr.ReadLine();
            if (header == null) {
                throw new LPGException("Header was null");
            }
            var headerarr = header.Split(';');
            cp.Headers.AddRange(headerarr);
            while (!sr.EndOfStream) {
                string? line = sr.ReadLine();
                if (line == null) {
                    throw new LPGException("Line was null");
                }
                var arr = line.Split(';');
                if (arr.Length > 2) {
                    cp.TimeSteps.Add(Convert.ToInt32(arr[0], CultureInfo.InvariantCulture));
                    cp.TimeStamps.Add(arr[1]);
                    var vals = new List<double>();
                    for (int i = 2; i < arr.Length; i++) {
                        if (!string.IsNullOrWhiteSpace(arr[i])) {
                            double d = Convert.ToDouble(arr[i], CultureInfo.InvariantCulture);
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


    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class JsonCalculatorTest :UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
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

                var loadTypes = csvs.Select(x => x.Loadtype??throw new LPGException("lt was null")).Distinct().ToList();
                foreach (string loadType in loadTypes) {
                    var filtered = csvs.Where(x => x.Loadtype == loadType).ToList();
                    foreach (var reader in filtered) {
                        Logger.Info(reader.Filename);
                    }
                }
            }

        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void OnlyExternalTest()
        {
            Logger.Get().StartCollectingAllMessages();
            Logger.Info(Utili.GetCurrentAssemblyVersion());
            Config.SkipFreeSpaceCheckForCalculation = true;
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
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
                using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    Simulator sim = new Simulator(db.ConnectionString);
                    var house = sim.Houses.FindFirstByName("CHR07 in HT04 with Car, 05 km to work, 3.7 kW Charging at home");
                    if (house == null)
                    {
                        throw new LPGException("house not found");
                    }

                    JsonCalcSpecification jcs = JsonCalcSpecification.MakeDefaultsForTesting();
                    jcs.OutputDirectory = Path.Combine(wd.WorkingDirectory, "Results");
                    jcs.DeleteSqlite = true;
                    jcs.StartDate = new DateTime(2019, 1, 1);
                    jcs.EndDate = new DateTime(2019, 1, 7);
                    if (jcs.LoadtypesForPostprocessing == null)
                    {
                        jcs.LoadtypesForPostprocessing = new List<string>();
                    }
                    jcs.LoadtypesForPostprocessing.Add("Electricity");

                    jcs.DefaultForOutputFiles = OutputFileDefault.NoFiles;
                    if (jcs.CalcOptions == null)
                    {
                        jcs.CalcOptions = new List<CalcOption>();
                    }
                    jcs.CalcOptions.Add(CalcOption.SumProfileExternalEntireHouse);

                    JsonCalculator jc = new JsonCalculator();
                    jc.StartHousehold(sim, jcs, house.GetJsonReference());
                    const string sqliteanalyizer = @"v:\dropbox\lpg\sqlite3_analyzer.exe";
                    if (File.Exists(sqliteanalyizer))
                    {
                        StreamWriter sw = new StreamWriter(Path.Combine(jcs.OutputDirectory, "analyzesqlite.cmd"));
                        DirectoryInfo resultDi = new DirectoryInfo(jcs.OutputDirectory);
                        var fis = resultDi.GetFiles("*.sqlite");
                        foreach (var fi in fis)
                        {
                            sw.WriteLine(sqliteanalyizer + " \"" + fi.FullName + "\" > " + fi.Name + ".analysis.txt");
                        }
                        sw.Close();
                    }
                    else
                    {
                        throw new LPGException("analyzer not found");
                    }
                }
            }
            // db.Cleanup();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TestDistrictDefinition()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                Directory.SetCurrentDirectory(wd.WorkingDirectory);
            }
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                HouseGenerator.CreateExampleHouseJob(db.ConnectionString);
            }
            // db.Cleanup();
        }


        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunJsonCalculatorTest()
        {
            Logger.Get().StartCollectingAllMessages();
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                Directory.SetCurrentDirectory(wd.WorkingDirectory);
                using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    string dbPath = wd.Combine("my.db3");
                    File.Copy(db.FileName, dbPath);
                    Logger.Info("DB File Path: " + dbPath);
                    JsonCalcSpecification jcs = new JsonCalcSpecification();
                    if (jcs.CalcOptions == null)
                    {
                        throw new LPGException("Calcoptions was null");
                    }
                    jcs.CalcOptions.Add(CalcOption.DeviceProfilesIndividualHouseholds);
                    jcs.CalcOptions.Add(CalcOption.DeviceProfilesIndividualHouseholds);
                    jcs.StartDate = new DateTime(2019, 1, 1);
                    jcs.EndDate = new DateTime(2019, 1, 3);
                    jcs.DefaultForOutputFiles = OutputFileDefault.OnlySums;
                    Simulator sim = new Simulator(db.ConnectionString);
                    JsonCalculator jc = new JsonCalculator();
                    jc.StartHousehold(sim, jcs, sim.Houses[0].GetJsonReference());
                }
                Directory.SetCurrentDirectory(wd.PreviousCurrentDir);
                wd.CleanUp();
            }
        }


        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunJsonCalculatorTestForExternalTimeResolutionJsonFile()
        {
            Logger.Get().StartCollectingAllMessages();
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                Directory.SetCurrentDirectory(wd.WorkingDirectory);
                using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    string dbPath = wd.Combine("my.db3");
                    File.Copy(db.FileName, dbPath);
                    Logger.Info("DB File Path: " + dbPath);
                    JsonCalcSpecification jcs = new JsonCalcSpecification();
                    if (jcs.CalcOptions == null)
                    {
                        throw new LPGException("Calcoptions was null");
                    }
                    jcs.DefaultForOutputFiles = OutputFileDefault.NoFiles;
                    jcs.CalcOptions.Add(CalcOption.SumProfileExternalIndividualHouseholdsAsJson);
                    jcs.StartDate = new DateTime(2019, 1, 1);
                    jcs.EndDate = new DateTime(2019, 1, 3);
                    jcs.ExternalTimeResolution = "00:15:00";
                    jcs.LoadTypePriority = LoadTypePriority.RecommendedForHouses;
                    Simulator sim = new Simulator(db.ConnectionString);
                    JsonCalculator jc = new JsonCalculator();
                    jc.StartHousehold(sim, jcs, sim.Houses[0].GetJsonReference());
                }
                Directory.SetCurrentDirectory(wd.PreviousCurrentDir);
                wd.CleanUp();
            }
        }
        //[Fact]
        //[Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        //public void RunJsonCalculatorExportTest()
        //{
        //    Config.IsInHeadless = true;
        //    WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
        //    Directory.SetCurrentDirectory(wd.WorkingDirectory);
        //    //DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
        //    JsonCalculator jc = new JsonCalculator();
        //    const string jsonfilename = @"C:\work\LPGUnitTest\SettlementTests.JsonCalcSpecTest\CHR07 in HT04 with Car, 30 km to work, 22kW Charging at work 4.json";
        //    JsonDirectoryOptions jo = new JsonDirectoryOptions
        //    {
        //        Input = jsonfilename

        //    };
        //    Logger.Get().StartCollectingAllMessages();
        //    jc.Calculate(jo);
        //    Directory.SetCurrentDirectory(wd.PreviousCurrentDir);
        //    //wd.CleanUp();
        //}

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunBrokenHouseholdCalcTest()
        {
            Config.IsInHeadless = true;
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                var db = new FileInfo(@"V:\Dropbox\LPG\ImportFiles\profilegenerator900.db3");
                string fullDstPath = Path.Combine(wd.WorkingDirectory, "profilegenerator.db3");
                db.CopyTo(fullDstPath);
                Directory.SetCurrentDirectory(wd.WorkingDirectory);
                Simulator sim = new Simulator("Data Source=" + fullDstPath);
                sim.Should().NotBeNull();
                wd.CleanUp();
            }
        }

        //[Fact]
        //[Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        //public void RunFailingStuffTest()
        //{
        //    Config.IsInHeadless = true;
        //    Config.IsInUnitTesting = true;
        //    WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
        //    Directory.SetCurrentDirectory(wd.WorkingDirectory);
        //    //DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.SimulationEngine);
        //    JsonCalculator jc = new JsonCalculator();
        //    const string jsonfilename = @"V:\Dropbox\LPG\failingTest.json";
        //    JsonDirectoryOptions jo = new JsonDirectoryOptions
        //    {
        //        Input = jsonfilename

        //    };
        //    Logger.Get().StartCollectingAllMessages();
        //    jc.Calculate(jo);
        //    Directory.SetCurrentDirectory(wd.PreviousCurrentDir);
        //    //wd.CleanUp();
        //}

        public JsonCalculatorTest([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}

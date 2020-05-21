using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.Tests;
using Database;
using Database.Helpers;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using Database.Tests;
using SimulationEngineLib;
using SimulationEngineLib.SimZukunftProcessor;
using Xunit;
using Xunit.Abstractions; //using iTextSharp.text.pdf;


namespace SimulationEngine.Tests {
    public static class HouseJobCalcPreparer {
        public static HouseCreationAndCalculationJob PrepareExistingHouseForTesting(Simulator sim, House house)
        {
            var hj = new HouseCreationAndCalculationJob();
            hj.CalcSpec = JsonCalcSpecification.MakeDefaultsForTesting();
            hj.HouseDefinitionType = HouseDefinitionType.HouseName;
            hj.HouseReference = new HouseReference(house.GetJsonReference());
            return hj;
        }

        public static HouseCreationAndCalculationJob PrepareNewHouseForOutputFileTesting(Simulator sim)
        {
            var hj = new HouseCreationAndCalculationJob();
            hj.CalcSpec = JsonCalcSpecification.MakeDefaultsForTesting();
            hj.CalcSpec.StartDate = new DateTime(2020, 1, 1);
            hj.CalcSpec.EndDate = new DateTime(2020, 1, 3);
            hj.CalcSpec.DeleteDAT = false;
            hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.NoFiles;
            hj.CalcSpec.DeleteSqlite = false;
            hj.CalcSpec.ExternalTimeResolution = "00:15:00";
            hj.House = new HouseData(StrGuid.FromString("houseguid"), "HT01", 1000, 100, "housename");
            hj.House.Households = new List<HouseholdData>();
            var hhd = new HouseholdData("householdid", false, "householdname", null, null, null, null,
                HouseholdDataSpecificationType.ByHouseholdName);
            var hh = sim.ModularHouseholds.FindFirstByName("CHR01", FindMode.StartsWith);
            hhd.HouseholdNameSpecification = new HouseholdNameSpecification(hh.GetJsonReference());
            hj.House.Households.Add(hhd);
            return hj;
        }

        public static HouseCreationAndCalculationJob PrepareNewHouseForHouseholdTesting(Simulator sim, ModularHousehold hh)
        {
            var hj = new HouseCreationAndCalculationJob();
            hj.CalcSpec = JsonCalcSpecification.MakeDefaultsForTesting();
            hj.CalcSpec.StartDate = new DateTime(2020, 1, 1);
            hj.CalcSpec.EndDate = new DateTime(2020, 1, 3);
            hj.CalcSpec.DeleteDAT = false;
            hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.NoFiles;
            hj.CalcSpec.DeleteSqlite = false;
            hj.CalcSpec.ExternalTimeResolution = "00:15:00";
            hj.House = new HouseData(StrGuid.FromString("houseguid"), "HT01", 1000, 100, "housename");
            hj.House.Households = new List<HouseholdData>();
            var hhd = new HouseholdData("householdid", false, "householdname", null, null, null, null,
                HouseholdDataSpecificationType.ByHouseholdName);
            hhd.HouseholdNameSpecification = new HouseholdNameSpecification(hh.GetJsonReference());
            hj.House.Households.Add(hhd);
            return hj;
        }
    }

    public class ResultList {
        public CalcOption Option { get; set; }
        public List<string> ResultKeys { get; } = new List<string>();

        public static ResultList Make(CalcOption option, params string[] resultfileKeys)
        {
            var rl = new ResultList();
            rl.Option = option;
            rl.ResultKeys.AddRange(resultfileKeys);
            return rl;
        }
    }

    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class HouseJobTestHelper {
        // ReSharper disable once CollectionNeverUpdated.Local
        private static readonly Dictionary<CalcOption, ResultList> _resultFileIdsByCalcOption = new Dictionary<CalcOption, ResultList>();

        static HouseJobTestHelper()
        {
        }

        public static void CheckForResultfile(string wd, CalcOption option)
        {
            var srls = new SqlResultLoggingService(wd);
            var rfel = new ResultFileEntryLogger(srls);
            var rfes = rfel.Load();
            var foundKeys = new List<string>();
            //collect all the file keys

            foreach (var rfe in rfes) {
                if (!File.Exists(rfe.FullFileName)) {
                    throw new LPGException("File " + rfe.FullFileName + " was registered, but is not actually present.");
                }

                var rfeKey = "ResultFileID." + rfe.ResultFileID;
                foundKeys.Add(rfeKey);
            }

            List<string> filesthatdontneedtoregister = new List<string>();
            filesthatdontneedtoregister.Add("finished.flag");
            filesthatdontneedtoregister.Add("log.commandlinecalculation.txt");
            filesthatdontneedtoregister.Add("results.general.sqlite");
            filesthatdontneedtoregister.Add("results.general.sqlite-wal");
            filesthatdontneedtoregister.Add("results.general.sqlite-shm");
            filesthatdontneedtoregister.Add("results.hh1.sqlite");
            filesthatdontneedtoregister.Add("results.house.sqlite");
            //check if all files are registered
            DirectoryInfo di = new DirectoryInfo(wd);
            var files = di.GetFiles("*.*", SearchOption.AllDirectories);
            var registeredFiles = rfes.Select(x => x.FullFileName).ToList();
            foreach (var file in files) {
                if (filesthatdontneedtoregister.Contains(file.Name.ToLower(CultureInfo.InvariantCulture))) {
                    continue;
                }
                if (!registeredFiles.Contains(file.FullName)) {
                    throw new LPGException("Found unregistered file: " + file.FullName);
                }
            }


            var hhKeyLogger = new HouseholdKeyLogger(srls);
            var keys = hhKeyLogger.Load();
            foreach (var key in keys) {
                var tables = srls.LoadTables(key.HouseholdKey);
                foreach (var table in tables) {
                    var tblkey = key.KeyType + "???" + table.TableName;
                    foundKeys.Add(tblkey);
                }
            }

            foundKeys = foundKeys.Distinct().ToList();
            if (_resultFileIdsByCalcOption.ContainsKey(option)) {
                var rl = _resultFileIdsByCalcOption[option];
                foreach (var key in rl.ResultKeys) {
                    if (!foundKeys.Contains(key)) {
                        throw new LPGException("in the found keys the file " + key + " is missing.");
                    }
                }

                foreach (var key in foundKeys) {
                    if (!rl.ResultKeys.Contains(key)) {
                        throw new LPGException("in the result list keys the file " + key + " is missing.");
                    }
                }
            }

            else {
                //todo: this needs to be done differently. need to disable as much as possible except the given calc option first.
                var sb = new StringBuilder();
                sb.Append("--------------");
                sb.AppendLine();
                sb.Append("_resultFileIdsByCalcOption.Add(CalcOption." + option + ",  ResultList.Make(CalcOption.");
                sb.Append(option).Append(", ");
                foreach (var key in foundKeys) {
                    sb.AppendLine();
                    sb.Append("\"").Append(key).Append("\", ");
                }

                sb.Remove(sb.Length - 2, 2);
                sb.Append("));");
                sb.AppendLine();
                sb.Append("--------------");
                Logger.Info(sb.ToString());
                //throw new LPGException(sb.ToString());
            }
        }

        public static void RunSingleHouse(Func<Simulator, HouseCreationAndCalculationJob> makeHj, Action<string> checkResults)
        {
            Logger.Get().StartCollectingAllMessages();
            Logger.Threshold = Severity.Debug;
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass())) {
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass())) {
                    wd.SkipCleaning = true;
                    var targetdb = wd.Combine("profilegenerator.db3");
                    File.Copy(db.FileName, targetdb, true);
                    Directory.SetCurrentDirectory(wd.WorkingDirectory);
                    var sim = new Simulator(db.ConnectionString);
                    var houseGenerator = new HouseGenerator();
                    var hj = makeHj(sim);
                    houseGenerator.ProcessSingleHouseJob(hj, null, sim);
                    if (hj.CalcSpec == null) {
                        throw new LPGException("calcspec was null");
                    }

                    checkResults(wd.Combine(hj.CalcSpec.OutputDirectory));
                }
            }
        }
    }

    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class CalculationTests : UnitTestBaseClass {
        public CalculationTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        private static void WriteOutputFileFunction(StreamWriter sw, CalcOption option)
        {
            sw.WriteLine("");
            sw.WriteLine("[Fact]");
            sw.WriteLine("[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]");

            sw.WriteLine("public void TestHouseJobs" + option + "(){");
            sw.WriteLine("      const CalcOption co = CalcOption." + option + ";");
            sw.WriteLine("      HouseJobTestHelper.RunSingleHouse((sim) => {");
            sw.WriteLine("      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);");
            sw.WriteLine("      hj.CalcSpec.CalcOptions.Add(co); return hj;");
            sw.WriteLine("      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));");
            sw.WriteLine("}");
            sw.WriteLine("");
        }


        private static void RunDateTimeOnAllFiles([JetBrains.Annotations.NotNull] string firstTimestep,
                                                  [JetBrains.Annotations.NotNull] string firstTimestamp, int targetLineCount)
        {
            Config.CatchErrors = false;
            Config.IsInUnitTesting = true;
            var di = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "CHH3.RunTimeAxisTest"));
            var reportsDir = new DirectoryInfo(Path.Combine(di.FullName, "Reports"));
            var fis1 = reportsDir.GetFiles("*.csv");
            var resultsDir = new DirectoryInfo(Path.Combine(di.FullName, "Results"));
            var fis2 = resultsDir.GetFiles("*.csv");
            var debugDir = new DirectoryInfo(Path.Combine(di.FullName, "Debugging"));
            var fis3 = debugDir.GetFiles("*.csv");
            var fis = new List<FileInfo>();
            fis.AddRange(fis1);
            fis.AddRange(fis2);
            fis.AddRange(fis3);
            fis.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
            var sumFiles = new List<string> {
                "ActivationsPerHour",
                "ActivityFrequenciesPerMinute",
                "ActivityPercentage",
                "AffordanceEnergyUse",
                "AffordanceTaggingSet",
                "AffordanceTimeUse",
                "DeviceDurationCurves",
                "DeviceSums",
                "DeviceTaggingSet",
                "DurationCurve",
                "ExecutedActionsOverviewCount",
                "HouseholdPlan",
                "LocationStatistics",
                "ImportProfile",
                "TimeOfUseEnergyProfiles",
                "TimeOfUseProfiles",
                "TotalsPerLoadtype",
                "WeekdayProfiles"
            };
            var filesWithVaryingLineCount = new List<string> {
                "Actions.",
                "Locations.",
                "Thoughts."
            };
            var filesToCheck = new List<FileInfo>();
            foreach (var fi in fis) {
                var contains = false;
                foreach (var sumFile in sumFiles) {
                    if (fi.Name.ToUpperInvariant().StartsWith(sumFile.ToUpperInvariant(), StringComparison.Ordinal)) {
                        contains = true;
                    }
                }

                if (!contains) {
                    filesToCheck.Add(fi);
                }
            }

            var filesWithLineCounts = new List<Tuple<FileInfo, int>>();
            foreach (var fileInfo in filesToCheck) {
                var linecount = 1;
                using (var sr = new StreamReader(fileInfo.FullName)) {
                    sr.ReadLine(); // header
                    var firstLine = sr.ReadLine();

                    if (firstLine == null) {
                        throw new LPGException("Readline failed");
                    }

                    var arr = firstLine.Split(';');
                    if (arr[0] != firstTimestep) {
                        throw new LPGException("File: " + fileInfo.Name + ": First timestep was: " + arr[0] + " instead of " + firstTimestep);
                    }

                    if (arr[1] != firstTimestamp) {
                        throw new LPGException("File: " + fileInfo.Name + ": First timestamp was: " + arr[1] + " instead of " + firstTimestamp);
                    }

                    while (!sr.EndOfStream) {
                        sr.ReadLine();
                        linecount++;
                    }

                    if (!filesWithVaryingLineCount.Any(x => fileInfo.Name.StartsWith(x, StringComparison.Ordinal))) {
                        if (linecount != targetLineCount) {
                            throw new LPGException("The file " + fileInfo.Name + ": has " + linecount + " lines instead of " + targetLineCount);
                        }
                    }
                }

                Logger.Info(fileInfo.Name + ": " + linecount);
                filesWithLineCounts.Add(new Tuple<FileInfo, int>(fileInfo, linecount));
            }

            foreach (var pair in filesWithLineCounts) {
                Logger.Info(pair.Item1.Name + " " + pair.Item2);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void GenerateCalcoptionTests()
        {
            var sw = new StreamWriter(@"C:\Work\LPGDev\SimulationEngine.Tests\SystematicCalcOptionTests.cs");
            sw.WriteLine("using Automation;");
            sw.WriteLine("using Xunit;");
            sw.WriteLine("using Common.Tests;");
            sw.WriteLine("using Xunit.Abstractions;");
            sw.WriteLine("using JetBrains.Annotations;");
            sw.WriteLine("#pragma warning disable 8602");
            sw.WriteLine("namespace SimulationEngine.Tests {");
            sw.WriteLine("public class SystematicCalcOptionTests :UnitTestBaseClass {");
            foreach (CalcOption opt in Enum.GetValues(typeof(CalcOption))) {
                WriteOutputFileFunction(sw, opt);
            }

            sw.WriteLine("public SystematicCalcOptionTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }");
            sw.WriteLine("}}");
            sw.Close();
        }



        [Fact]
        public void TestHouseJobs()
        {
            HouseJobTestHelper.RunSingleHouse(sim => {
                var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
                //hj.CalcSpec.CalcOptions.Add(CalcOption.ActionsLogfile);
                return hj;
            }, x => HouseJobTestHelper.CheckForResultfile(x, CalcOption.BasicOverview));
        }

        [Fact]
        public void TestHouseJobs1()
        {
            const CalcOption co = CalcOption.ActivationFrequencies;
            HouseJobTestHelper.RunSingleHouse(sim => {
                var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
                if (hj.CalcSpec?.CalcOptions == null) {
                    throw new LPGException();
                }

                hj.CalcSpec.CalcOptions.Add(co);
                return hj;
            }, x => HouseJobTestHelper.CheckForResultfile(x, co));
        }

        [Fact]
        public void TestHouseHouseholdVariation()
        {
            const int hhnumer = 0;
            HouseJobTestHelper.RunSingleHouse(sim => {
                var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
                if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
                hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
                return hj; }, x => {});
        }

        private static void WriteHouseholdTestFunction(StreamWriter sw, int idx)
        {
            sw.WriteLine("");
            sw.WriteLine("[Fact]");
            sw.WriteLine("[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]");

            sw.WriteLine("public void TestHouseholdTest" + idx + "(){");
            sw.WriteLine("      const int hhnumer = "+idx+";");
            sw.WriteLine("      HouseJobTestHelper.RunSingleHouse(sim => {");
            sw.WriteLine("      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);");
            sw.WriteLine("      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }");
            sw.WriteLine("      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;");
            sw.WriteLine("return hj; }, x => {});");
            sw.WriteLine("}");
            sw.WriteLine("");
        }
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void GenerateHouseholdTests()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            var sw = new StreamWriter(@"C:\Work\LPGDev\SimulationEngine.Tests\SystematicHouseholdTests.cs");
            sw.WriteLine("using Automation;");
            sw.WriteLine("using Automation.ResultFiles;");
            sw.WriteLine("using Xunit;");
            sw.WriteLine("using Common.Tests;");
            sw.WriteLine("using Xunit.Abstractions;");
            sw.WriteLine("using JetBrains.Annotations;");
            sw.WriteLine("#pragma warning disable 8602");
            sw.WriteLine("namespace SimulationEngine.Tests {");
            sw.WriteLine("public class SystematicHouseholdTests :UnitTestBaseClass {");
            for (int i = 0; i < sim.ModularHouseholds.It.Count; i++) {
                WriteHouseholdTestFunction(sw, i);
            }

            sw.WriteLine(
                    "public SystematicHouseholdTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }");
                sw.WriteLine("}}");
                sw.Close();
        }
    }
}
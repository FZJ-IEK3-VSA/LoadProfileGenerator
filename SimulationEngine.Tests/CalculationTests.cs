using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Autofac;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor;
using ChartCreator2;
using Common;
using Common.Enums;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.Tests;
using Database;
using Database.Helpers;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using Database.Tests;
using FluentAssertions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SimulationEngineLib.HouseJobProcessor;
using Xunit;
using Xunit.Abstractions;

//using iTextSharp.text.pdf;


namespace SimulationEngine.Tests {
    public enum TestDuration {
        OneMonth,
        ThreeMonths,
        TwelveMonths
    }
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public static class HouseJobCalcPreparer {
        public static HouseCreationAndCalculationJob PrepareExistingHouseForTesting([JetBrains.Annotations.NotNull] House house)
        {
            var hj = new HouseCreationAndCalculationJob
            {
                CalcSpec = JsonCalcSpecification.MakeDefaultsForTesting(),
                HouseDefinitionType = HouseDefinitionType.HouseName,
                HouseRef = new HouseReference(house.GetJsonReference())
            };
            return hj;
        }

        public static HouseCreationAndCalculationJob PrepareNewHouseForHouseholdTesting(Simulator sim, string guid, TestDuration duration)
        {
            var hj = new HouseCreationAndCalculationJob
            {
                CalcSpec = JsonCalcSpecification.MakeDefaultsForTesting()
            };
            hj.CalcSpec.StartDate = new DateTime(2020, 1, 1);
            SetEndDate(duration, hj);

    hj.CalcSpec.DeleteDAT = false;
            hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.NoFiles;
            hj.CalcSpec.DeleteSqlite = false;
            hj.CalcSpec.ExternalTimeResolution = "00:15:00";
            hj.CalcSpec.EnableTransportation = false;
            hj.House = new HouseData(StrGuid.FromString("houseguid"), "HT01", 1000, 100, "housename")
            {
                Households = new List<HouseholdData>()
            };
            var hhd = new HouseholdData("householdid",
                "householdname", sim.ChargingStationSets[0].GetJsonReference(),
                sim.TransportationDeviceSets[0].GetJsonReference(),
                sim.TravelRouteSets[0].GetJsonReference(), null,
                HouseholdDataSpecificationType.ByHouseholdName);
            var hh = sim.ModularHouseholds.FindByGuid(guid.ToStrGuid());
            if (hh == null) {
                throw new LPGException("hh was null");
            }
            hhd.HouseholdNameSpec = new HouseholdNameSpecification(hh.GetJsonReference());
            hj.House.Households.Add(hhd);
            return hj;
        }

        private static void SetEndDate(TestDuration duration, HouseCreationAndCalculationJob hj)
        {
            if (hj.CalcSpec == null) {
                throw new LPGException("was null");
            }
            if (duration == TestDuration.OneMonth) {
                hj.CalcSpec.EndDate = new DateTime(2020, 2, 1);
            }
            else if (duration == TestDuration.ThreeMonths) {
                hj.CalcSpec.EndDate = new DateTime(2020, 4, 1);
            }
            else if (duration == TestDuration.TwelveMonths) {
                hj.CalcSpec.EndDate = new DateTime(2020, 12, 31);
            }
            else {
                throw new LPGException("Unkown duration");
            }
        }

        public static HouseCreationAndCalculationJob PrepareNewHouseForHouseholdTestingWithTransport(
            Simulator sim, string guid, TestDuration duration)
        {
            var hj = new HouseCreationAndCalculationJob
            {
                CalcSpec = JsonCalcSpecification.MakeDefaultsForTesting()
            };
            hj.CalcSpec.StartDate = new DateTime(2020, 1, 1);
            SetEndDate(duration, hj);
            hj.CalcSpec.DeleteDAT = false;
            hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.NoFiles;
            hj.CalcSpec.DeleteSqlite = false;
            hj.CalcSpec.ExternalTimeResolution = "00:15:00";
            hj.CalcSpec.EnableTransportation = true;
            hj.House = new HouseData(StrGuid.FromString("houseguid"), "HT01", 1000, 100, "housename")
            {
                Households = new List<HouseholdData>()
            };
            var hhd = new HouseholdData("householdid",
                "householdname", sim.ChargingStationSets[0].GetJsonReference(),
                sim.TransportationDeviceSets[0].GetJsonReference(),
                sim.TravelRouteSets[0].GetJsonReference(), null,
                HouseholdDataSpecificationType.ByHouseholdName);
            var hh = sim.ModularHouseholds.FindByGuid(guid.ToStrGuid());
            if (hh == null) {
                throw new LPGException("No household found");
            }
            hhd.HouseholdNameSpec = new HouseholdNameSpecification(hh.GetJsonReference());
            hj.House.Households.Add(hhd);
            return hj;
        }


        public static HouseCreationAndCalculationJob PrepareNewHouseForHousetypeTesting(Simulator sim, string guid)
        {
            var hj = new HouseCreationAndCalculationJob
            {
                CalcSpec = JsonCalcSpecification.MakeDefaultsForTesting()
            };
            hj.CalcSpec.StartDate = new DateTime(2020, 1, 1);
            hj.CalcSpec.EndDate = new DateTime(2020, 1, 3);
            hj.CalcSpec.DeleteDAT = false;
            hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.NoFiles;
            hj.CalcSpec.DeleteSqlite = false;
            hj.CalcSpec.ExternalTimeResolution = "00:15:00";
            hj.CalcSpec.EnableTransportation = false;
            var ht = sim.HouseTypes.FindByGuid(guid.ToStrGuid());
            if (ht == null) {
                throw new LPGException("Housetype not found");
            }
            hj.House = new HouseData(StrGuid.FromString("houseguid"), ht.HouseTypeCode, 1000, 100, "housename")
            {
                Households = new List<HouseholdData>()
            };
            var hhd = new HouseholdData("householdid",
                "householdname", sim.ChargingStationSets[0].GetJsonReference(),
                sim.TransportationDeviceSets[0].GetJsonReference(),
                sim.TravelRouteSets[0].GetJsonReference(), null,
                HouseholdDataSpecificationType.ByHouseholdName);
            var hh = sim.ModularHouseholds[0];
            hhd.HouseholdNameSpec = new HouseholdNameSpecification(hh.GetJsonReference());
            hj.House.Households.Add(hhd);
            return hj;
        }

        public static HouseCreationAndCalculationJob PrepareNewHouseForOutputFileTesting(Simulator sim)
        {
            var hj = new HouseCreationAndCalculationJob
            {
                CalcSpec = JsonCalcSpecification.MakeDefaultsForTesting()
            };
            hj.CalcSpec.StartDate = new DateTime(2020, 1, 1);
            hj.CalcSpec.EndDate = new DateTime(2020, 1, 3);
            hj.CalcSpec.DeleteDAT = false;
            hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.NoFiles;
            hj.CalcSpec.DeleteSqlite = false;
            hj.CalcSpec.ExternalTimeResolution = "00:15:00";
            hj.House = new HouseData(StrGuid.FromString("houseguid"), "HT01", 1000, 100, "housename")
            {
                Households = new List<HouseholdData>()
            };
            var hhd = new HouseholdData("householdid",
                "householdname", null, null, null, null,
                HouseholdDataSpecificationType.ByHouseholdName);
            var hh = sim.ModularHouseholds.FindFirstByName("CHR01", FindMode.StartsWith);
            if (hh == null) {
                throw new LPGException("Was null");
            }
            hhd.HouseholdNameSpec = new HouseholdNameSpecification(hh.GetJsonReference());
            hj.House.Households.Add(hhd);
            return hj;
        }
    }

    public class ResultList {
        public CalcOption Option { get; set; }
        public List<string> ResultKeys { get; } = new List<string>();

        public static ResultList Make(CalcOption option, params string[] resultfileKeys)
        {
            var rl = new ResultList
            {
                Option = option
            };
            rl.ResultKeys.AddRange(resultfileKeys);
            return rl;
        }
    }

    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public static class HouseJobTestHelper {
        // ReSharper disable once CollectionNeverUpdated.Local


        public static void CheckForResultfile(string wd, CalcOption option)
        {

             var peakWorkingSet = Process.GetCurrentProcess().PeakWorkingSet64;
             const long memoryCap = 1024 * 1024 * 2000;
            peakWorkingSet.Should().BeLessThan(memoryCap);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            var srls = new SqlResultLoggingService(wd);
            var rfel = new ResultFileEntryLogger(srls);
            var rfes = rfel.Load();
            var foundOptions = new List<CalcOption>();
            //collect all the file keys

            foreach (var rfe in rfes) {
                if (!File.Exists(rfe.FullFileName)) {
                    throw new LPGException("File " + rfe.FullFileName +
                                           " was registered, but is not actually present.");
                }

                foundOptions.Add(rfe.EnablingCalcOption);
            }

            // ReSharper disable once CollectionNeverQueried.Local
            var allTables = new List<ResultTableDefinition>();
            var hhKeyLogger = new HouseholdKeyLogger(srls);
            var keys = hhKeyLogger.Load();
            foreach (var key in keys) {
                if (!srls.FilenameByHouseholdKey.ContainsKey(key.HHKey)) {
                    continue;
                }

                var fn = srls.FilenameByHouseholdKey[key.HHKey];
                if (!File.Exists(fn.Filename)) {
                    continue;
                }

                var tables = srls.LoadTables(key.HHKey);
                allTables.AddRange(tables);
                foreach (var table in tables) {
                    foundOptions.Add(table.EnablingOption);
                }
            }

            foundOptions = foundOptions.Distinct().ToList();

            var optionsThatDontResultInFiles = new List<CalcOption>
            {
                CalcOption.MakePDF,
                CalcOption.ActionCarpetPlot,
                CalcOption.HouseholdPlan,
                CalcOption.CalculationFlameChart,
                CalcOption.MakeGraphics,
                CalcOption.LogErrorMessages,
                CalcOption.EnergyCarpetPlot,
                CalcOption.DurationCurve,
                CalcOption.TransportationDeviceCarpetPlot,
                CalcOption.LocationCarpetPlot
            };
            if (!optionsThatDontResultInFiles.Contains(option)) {
                if (!foundOptions.Contains(option)) {
                    throw new LPGException("Option found that doesn't result in any files");
                }
            }

            var fftd = new FileFactoryAndTrackerDummy();
            var cp = new CalculationProfiler();
            var container = PostProcessingManager.RegisterEverything(wd, cp, fftd);
            HashSet<CalcOption> enabledOptions = new HashSet<CalcOption>();
            enabledOptions.Add(option);
            ChartProcessorManager.ChartingFunctionDependencySetter(wd,cp,fftd,enabledOptions,false);
            using (var scope = container.BeginLifetimeScope()) {
                var odm = scope.Resolve<OptionDependencyManager>();
                odm.EnableRequiredOptions(enabledOptions);
            }

            foreach (var enabledOption in enabledOptions) {
                foundOptions.Remove(enabledOption);
            }

            if (foundOptions.Contains(CalcOption.BasicOverview)) {
                foundOptions.Remove(CalcOption.BasicOverview);
            }

            if (foundOptions.Count > 0) {
                var s = string.Join("\n", foundOptions.Select(x => x.ToString()));
                throw new LPGException("found stuff that was not requested:" + s);
            }

            var filesthatdontneedtoregister = new List<string>
            {
                "finished.flag",
                "log.commandlinecalculation.txt",
                "results.general.sqlite",
                "results.general.sqlite-wal",
                "results.general.sqlite-shm",
                "results.hh1.sqlite",
                "results.hh1.sqlite-shm",
                "results.hh1.sqlite-wal",
                "results.house.sqlite",
                "results.house.sqlite-shm",
                "results.house.sqlite-wal",
                "calculationprofiler.json",
                "calculationdurationflamechart.calcmanager.png"
            };
            //check if all files are registered
            var di = new DirectoryInfo(wd);
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


            //foundKeys = foundKeys.Distinct().ToList();
            //if (_resultFileIdsByCalcOption.ContainsKey(option)) {
            //    var rl = _resultFileIdsByCalcOption[option];
            //    foreach (var key in rl.ResultKeys) {
            //        if (!foundKeys.Contains(key)) {
            //            throw new LPGException("in the found keys the file " + key + " is missing.");
            //        }
            //    }

            //    foreach (var key in foundKeys) {
            //        if (!rl.ResultKeys.Contains(key)) {
            //            throw new LPGException("in the result list keys the file " + key + " is missing.");
            //        }
            //    }
            //}

            //else {
            //    //todo: this needs to be done differently. need to disable as much as possible except the given calc option first.
            //    var sb = new StringBuilder();
            //    sb.Append("--------------");
            //    sb.AppendLine();
            //    sb.Append("_resultFileIdsByCalcOption.Add(CalcOption." + option + ",  ResultList.Make(CalcOption.");
            //    sb.Append(option).Append(", ");
            //    foreach (var key in foundKeys) {
            //        sb.AppendLine();
            //        sb.Append("\"").Append(key).Append("\", ");
            //    }

            //    sb.Remove(sb.Length - 2, 2);
            //    sb.Append("));");
            //    sb.AppendLine();
            //    sb.Append("--------------");
            //    Logger.Info(sb.ToString());
            //    //throw new LPGException(sb.ToString());
            //}
        }

        public static void RunSingleHouse(Func<Simulator, HouseCreationAndCalculationJob> makeHj, Action<string> checkResults,
                                          bool skipcleaning = false)
        {
            Logger.Get().StartCollectingAllMessages();
            //Logger.Threshold = Severity.Debug;
            using var wd = new WorkingDir(Utili.GetCallingMethodAndClass());
            wd.SkipCleaning = skipcleaning;
            using var db = new DatabaseSetup(Utili.GetCallingMethodAndClass());
            var targetdb = wd.Combine("profilegenerator.db3");
            File.Copy(db.FileName, targetdb, true);
            Directory.SetCurrentDirectory(wd.WorkingDirectory);
            var sim = new Simulator(db.ConnectionString);
            var houseGenerator = new HouseGenerator();
            var hj = makeHj(sim);
            houseGenerator.ProcessSingleHouseJob(hj, sim);
            if (hj.CalcSpec?.OutputDirectory == null)
            {
                throw new LPGException("calcspec was null");
            }

            checkResults(wd.Combine(hj.CalcSpec.OutputDirectory));
        }
    }


    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class CalculationTests : UnitTestBaseClass {
        public CalculationTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestIfAllAutonomousDevicesAreInTheResultFiles()
        {
            static void CheckResults(string path)
            {

                var srls = new SqlResultLoggingService(path);
                var keyLogger = new HouseholdKeyLogger(srls);
                var keys = keyLogger.Load();
                var hhkey = keys.Single(x => x.KeyType == HouseholdKeyType.Household).HHKey;
                var hhdtoLogger = new HouseholdDtoLogger(srls);
                var hhdto = hhdtoLogger.Load(hhkey);
                Logger.Warning("found " + hhdto.AutoDevices.Count + " autonomous devices");
                //check device dtos
                var deviceDefinitionLogger = new CalcDeviceDtoLogger(srls);
                var deviceDtos = deviceDefinitionLogger.Load(hhkey);
                var deviceNames = deviceDtos.Select(x => x.Name).ToList();
                foreach (var hhdevdto in hhdto.DeviceDtos) {
                    if(!deviceNames.Contains(hhdevdto.Name))
                    {
                        throw new LPGException("Missing device");
                    }
                }
                //autodevs
                var autodevlogger = new CalcAutoDevDtoLogger(srls);
                var autoDevDtos = autodevlogger.Load(hhkey);
                var autoDeviceNames = autoDevDtos.Select(x => x.Name).ToList();
                foreach (var autodevdto in hhdto.AutoDevices)
                {
                    if (!autoDeviceNames.Contains(autodevdto.Name))
                    {
                        throw new LPGException("Missing auto device");
                    }
                }
                //device activations

                //var deviceActivaitonLogger = new DeviceActivationEntryLogger(srls);
                //var deviceActivations = deviceActivaitonLogger.Read(hhkey);
                var deviceArchiveLogger = new CalcDeviceArchiveDtoLogger(srls);
                var devices = deviceArchiveLogger.Load(hhkey);
                var activatedDevices = devices.Select(x => x.Device.Name).Distinct().ToList();
                //var activatedDevices = deviceActivations.Select(x => x.DeviceName).Distinct();
                foreach (var deviceName in deviceNames) {
                    if (!activatedDevices.Contains(deviceName)) {
                        Logger.Warning("Device " + deviceName+ " was never activated");
                    }
                }

                int neverActivatedAutoDevs = 0;
                foreach (var deviceName in autoDeviceNames)
                {
                    if (!activatedDevices.Contains(deviceName))
                    {
                        Logger.Warning("Auto Device " + deviceName + " was never activated");
                        neverActivatedAutoDevs++;
                    }
                }

                if (neverActivatedAutoDevs == autoDeviceNames.Count) {
                    throw new LPGException("Not a single autodev was ever activated");
                }
            }
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.JsonDeviceProfilesIndividualHouseholds;
            Logger.Threshold = Severity.Warning;
            HouseJobTestHelper.RunSingleHouse((sim) => {
                var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid, TestDuration.OneMonth);
                //hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.All;
                if(hj.CalcSpec == null) {
                    throw new LPGException("was null");
                }

                hj.CalcSpec.StartDate = new DateTime(2020, 1, 1);
                hj.CalcSpec.EndDate = new DateTime(2020,3,31);
                if (hj.CalcSpec.CalcOptions == null) {
                    throw new LPGException("was null");
                }

                hj.CalcSpec.CalcOptions.Add(co);
                hj.CalcSpec.CalcOptions.Add(CalcOption.HouseholdContents);
                hj.CalcSpec.CalcOptions.Add(CalcOption.DeviceActivations);

                return hj;
            }, CheckResults);
        }

        private static void WriteCalcOptionFunction(StreamWriter sw, CalcOption option, ModularHousehold hh)
        {
            sw.WriteLine("");
            sw.WriteLine("[Fact]");
            sw.WriteLine("[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]");

            sw.WriteLine("public void TestHouseJobs" + option + "(){");
            sw.WriteLine("      const string hhguid = \"" + hh.Guid.StrVal + "\";");
            sw.WriteLine("      const CalcOption co = CalcOption." + option + ";");
            sw.WriteLine("      HouseJobTestHelper.RunSingleHouse((sim) => {");
            sw.WriteLine(
                "      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);");
            sw.WriteLine("      hj.CalcSpec.CalcOptions.Add(co); return hj;");
            sw.WriteLine("      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));");
            sw.WriteLine("}");
            sw.WriteLine("");
        }

        private static void WriteHousetypeFunction(StreamWriter sw, HouseType ht, int idx)
        {
            sw.WriteLine("");
            sw.WriteLine("[Fact]");
            sw.WriteLine("[Trait(UnitTestCategories.Category, UnitTestCategories.HousetypeTests)]");

            sw.WriteLine("public void TestHousetype" + idx + ht.HouseTypeCode + "(){");
            sw.WriteLine("      const string htguid = \"" + ht.Guid.StrVal + "\";");
            sw.WriteLine("      HouseJobTestHelper.RunSingleHouse((sim) =>");
            sw.WriteLine("      HouseJobCalcPreparer.PrepareNewHouseForHousetypeTesting(sim, htguid)");
            sw.WriteLine("      , (x) => {});");
            sw.WriteLine("}");
            sw.WriteLine("");
        }

        private static void WriteHouseholdTestFunction(StreamWriter sw, ModularHousehold hh, int idx)
        {
            sw.WriteLine("");
            sw.WriteLine("[Fact]");
            sw.WriteLine("[Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdTest)]");

            sw.WriteLine("public void TestBasicHousehold" + idx + "(){");
            sw.WriteLine("      const string hhguid = \"" + hh.Guid.StrVal + "\";");
            sw.WriteLine("      HouseJobTestHelper.RunSingleHouse(sim => {");
            sw.WriteLine("      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,hhguid, TestDuration.ThreeMonths);");
            sw.WriteLine("      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }");
            sw.WriteLine("      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;");
            sw.WriteLine("return hj; }, x => {});");
            sw.WriteLine("}");
            sw.WriteLine("");
        }

        private static void WriteHouseholdTestFunctionWithTransport(StreamWriter sw, ModularHousehold hh, int idx)
        {
            sw.WriteLine("");
            sw.WriteLine("[Fact]");
            sw.WriteLine("[Trait(UnitTestCategories.Category, UnitTestCategories.HouseholdsWithTransportation)]");

            sw.WriteLine("public void TestHouseholdWithTransport" + idx + "(){");
            sw.WriteLine("      const string hhguid = \"" + hh.Guid.StrVal + "\";");
            sw.WriteLine("      HouseJobTestHelper.RunSingleHouse(sim => {");
            sw.WriteLine(
                "      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim,hhguid,TestDuration.ThreeMonths);");
            sw.WriteLine("      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }");
            sw.WriteLine("      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;");
            sw.WriteLine("return hj; }, x => {});");
            sw.WriteLine("}");
            sw.WriteLine("");
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void CheckForCarElectrictyFiles()
        {
            static HouseCreationAndCalculationJob PrepareHousejob(Simulator sim)
            {
                HouseCreationAndCalculationJob houseCreationAndCalculationJob = new HouseCreationAndCalculationJob();
                var hj = houseCreationAndCalculationJob;
                hj.CalcSpec = JsonCalcSpecification.MakeDefaultsForTesting();
                hj.CalcSpec.StartDate = new DateTime(2020, 1, 1);
                hj.CalcSpec.EndDate = new DateTime(2020, 1, 3);
                hj.CalcSpec.DeleteDAT = false;
                hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
                hj.CalcSpec.DeleteSqlite = false;
                hj.CalcSpec.ExternalTimeResolution = "00:15:00";
                hj.CalcSpec.EnableTransportation = true;
                if (hj.CalcSpec.CalcOptions == null) {
                    throw new LPGException("calcoptions was null");
                }

                hj.CalcSpec.CalcOptions.Add(CalcOption.TotalsPerLoadtype);
                hj.House = new HouseData(StrGuid.FromString("houseguid"), "HT01", 1000, 100, "housename")
                {
                    Households = new List<HouseholdData>()
                };
                var chargingstationSet = sim.ChargingStationSets.FindFirstByNameNotNull("car electricity", FindMode.Partial);
                var hhd = new HouseholdData("householdid", "householdname", chargingstationSet.GetJsonReference(),
                    sim.TransportationDeviceSets[0].GetJsonReference(), sim.TravelRouteSets[0].GetJsonReference(), null,
                    HouseholdDataSpecificationType.ByHouseholdName);
                var hh = sim.ModularHouseholds.FindFirstByNameNotNull("CHR01", FindMode.StartsWith);

                hhd.HouseholdNameSpec = new HouseholdNameSpecification(hh.GetJsonReference());
                hj.House.Households.Add(hhd);
                return hj;
            }

            HouseJobTestHelper.RunSingleHouse(sim => {
                var hj = PrepareHousejob(sim);
                return hj;
            }, x => CheckForResultfile(x));
        }

        private static void CheckForResultfile(string wd)
        {
            var srls = new SqlResultLoggingService(wd);
            var rfel = new ResultFileEntryLogger(srls);
            var rfes = rfel.Load();
            var foundcar = false;
            foreach (var entry in rfes) {
                if (entry.LoadTypeInformation?.Name == null) {
                    continue;
                }

                if (entry.LoadTypeInformation.Name.Contains("Car Charging Electricity")) {
                    foundcar = true;
                }
            }

            if (!foundcar) {
                throw new LPGException("No car electricity found");
            }

            Logger.Info(wd);
        }

        //private static void RunDateTimeOnAllFiles([JetBrains.Annotations.NotNull] string firstTimestep,
        //                                          [JetBrains.Annotations.NotNull] string firstTimestamp, int targetLineCount)
        //{
        //    Config.CatchErrors = false;
        //    Config.IsInUnitTesting = true;
        //    var di = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "CHH3.RunTimeAxisTest"));
        //    var reportsDir = new DirectoryInfo(Path.Combine(di.FullName, "Reports"));
        //    var fis1 = reportsDir.GetFiles("*.csv");
        //    var resultsDir = new DirectoryInfo(Path.Combine(di.FullName, "Results"));
        //    var fis2 = resultsDir.GetFiles("*.csv");
        //    var debugDir = new DirectoryInfo(Path.Combine(di.FullName, "Debugging"));
        //    var fis3 = debugDir.GetFiles("*.csv");
        //    var fis = new List<FileInfo>();
        //    fis.AddRange(fis1);
        //    fis.AddRange(fis2);
        //    fis.AddRange(fis3);
        //    fis.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
        //    var sumFiles = new List<string> {
        //        "ActivationsPerHour",
        //        "ActivityFrequenciesPerMinute",
        //        "ActivityPercentage",
        //        "AffordanceEnergyUse",
        //        "AffordanceTaggingSet",
        //        "AffordanceTimeUse",
        //        "DeviceDurationCurves",
        //        "DeviceSums",
        //        "DeviceTaggingSet",
        //        "DurationCurve",
        //        "ExecutedActionsOverviewCount",
        //        "HouseholdPlan",
        //        "LocationStatistics",
        //        "ImportProfile",
        //        "TimeOfUseEnergyProfiles",
        //        "TimeOfUseProfiles",
        //        "TotalsPerLoadtype",
        //        "WeekdayProfiles"
        //    };
        //    var filesWithVaryingLineCount = new List<string> {
        //        "Actions.",
        //        "Locations.",
        //        "Thoughts."
        //    };
        //    var filesToCheck = new List<FileInfo>();
        //    foreach (var fi in fis) {
        //        var contains = false;
        //        foreach (var sumFile in sumFiles) {
        //            if (fi.Name.ToUpperInvariant().StartsWith(sumFile.ToUpperInvariant(), StringComparison.Ordinal)) {
        //                contains = true;
        //            }
        //        }

        //        if (!contains) {
        //            filesToCheck.Add(fi);
        //        }
        //    }

        //    var filesWithLineCounts = new List<Tuple<FileInfo, int>>();
        //    foreach (var fileInfo in filesToCheck) {
        //        var linecount = 1;
        //        using (var sr = new StreamReader(fileInfo.FullName)) {
        //            sr.ReadLine(); // header
        //            var firstLine = sr.ReadLine();

        //            if (firstLine == null) {
        //                throw new LPGException("Readline failed");
        //            }

        //            var arr = firstLine.Split(';');
        //            if (arr[0] != firstTimestep) {
        //                throw new LPGException("File: " + fileInfo.Name + ": First timestep was: " + arr[0] + " instead of " + firstTimestep);
        //            }

        //            if (arr[1] != firstTimestamp) {
        //                throw new LPGException("File: " + fileInfo.Name + ": First timestamp was: " + arr[1] + " instead of " + firstTimestamp);
        //            }

        //            while (!sr.EndOfStream) {
        //                sr.ReadLine();
        //                linecount++;
        //            }

        //            if (!filesWithVaryingLineCount.Any(x => fileInfo.Name.StartsWith(x, StringComparison.Ordinal))) {
        //                if (linecount != targetLineCount) {
        //                    throw new LPGException("The file " + fileInfo.Name + ": has " + linecount + " lines instead of " + targetLineCount);
        //                }
        //            }
        //        }

        //        Logger.Info(fileInfo.Name + ": " + linecount);
        //        filesWithLineCounts.Add(new Tuple<FileInfo, int>(fileInfo, linecount));
        //    }

        //    foreach (var pair in filesWithLineCounts) {
        //        Logger.Info(pair.Item1.Name + " " + pair.Item2);
        //    }
        //}

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void GenerateCalcoptionTests()
        {
            using var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            var sw = new StreamWriter(@"C:\Work\LPGDev\SimulationEngine.Tests\SystematicCalcOptionTests.cs");
            sw.WriteLine("using Automation;");
            sw.WriteLine("using Xunit;");
            sw.WriteLine("using Common.Tests;");
            sw.WriteLine("using Xunit.Abstractions;");
            sw.WriteLine("using JetBrains.Annotations;");
            sw.WriteLine("#pragma warning disable 8602");
            sw.WriteLine("namespace SimulationEngine.Tests {");
            sw.WriteLine("public class SystematicCalcOptionTests :UnitTestBaseClass {");
#pragma warning disable CS8605 // Unboxing a possibly null value.
            foreach (CalcOption opt in Enum.GetValues(typeof(CalcOption))) {
#pragma warning restore CS8605 // Unboxing a possibly null value.
                WriteCalcOptionFunction(sw, opt, sim.ModularHouseholds[0]);
            }

            sw.WriteLine(
                "public SystematicCalcOptionTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }");
            sw.WriteLine("}}");
            sw.Close();
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void GenerateSystematicHouseholdTests()
        {
            using var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
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
            var householdsToTest = sim.ModularHouseholds.Items.Where(x => x.CreationType == CreationType.ManuallyCreated)
                .ToList();
            var idx = 0;
            foreach (var household in householdsToTest) {
                WriteHouseholdTestFunction(sw, household, idx++);
            }

            sw.WriteLine(
                "public SystematicHouseholdTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }");
            sw.WriteLine("}}");
            sw.Close();
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void GenerateSystematicHouseholdTestsWithTransport()
        {
            using var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            var sw = new StreamWriter(@"C:\Work\LPGDev\SimulationEngine.Tests\SystematicTransportHouseholdTests.cs");
            sw.WriteLine("using Automation;");
            sw.WriteLine("using Automation.ResultFiles;");
            sw.WriteLine("using Xunit;");
            sw.WriteLine("using Common.Tests;");
            sw.WriteLine("using Xunit.Abstractions;");
            sw.WriteLine("using JetBrains.Annotations;");
            sw.WriteLine("#pragma warning disable 8602");
            sw.WriteLine("namespace SimulationEngine.Tests {");
            sw.WriteLine("public class SystematicTransportHouseholdTests :UnitTestBaseClass {");
            var householdsToTest = sim.ModularHouseholds.Items.Where(x => x.CreationType == CreationType.ManuallyCreated)
                .ToList();
            var idx = 0;
            foreach (var household in householdsToTest) {
                WriteHouseholdTestFunctionWithTransport(sw, household, idx++);
            }

            sw.WriteLine(
                "public SystematicTransportHouseholdTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }");
            sw.WriteLine("}}");
            sw.Close();
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void GenerateHouseTypeTests()
        {
            using var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            var sw = new StreamWriter(@"C:\Work\LPGDev\SimulationEngine.Tests\SystematicHousetypeTests.cs");
            sw.WriteLine("using Automation;");
            sw.WriteLine("using Xunit;");
            sw.WriteLine("using Common.Tests;");
            sw.WriteLine("using Xunit.Abstractions;");
            sw.WriteLine("using JetBrains.Annotations;");
            sw.WriteLine("#pragma warning disable 8602");
            sw.WriteLine("namespace SimulationEngine.Tests {");
            sw.WriteLine("public class SystematicHousetypeTests :UnitTestBaseClass {");
            var housetypeTests = sim.HouseTypes.Items.ToList();
            var idx = 0;
            foreach (var houeType in housetypeTests) {
                WriteHousetypeFunction(sw, houeType, idx++);
            }

            sw.WriteLine(
                "public SystematicHousetypeTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }");
            sw.WriteLine("}}");
            sw.Close();
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void TestHouseHouseholdVariation()
        {
            HouseJobTestHelper.RunSingleHouse(sim => {
                var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,
                    sim.ModularHouseholds[0].Guid.ToString(), TestDuration.OneMonth);
                if (hj.CalcSpec?.CalcOptions == null) {
                    throw new LPGException();
                }

                hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
                return hj;
            }, _ => { });
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void TestHouseJobFile()
        {
            HouseJobTestHelper.RunSingleHouse(_ => {
                var str = File.ReadAllText(@"C:\Work\pylpg\C1\calcspec.json");
                return JsonConvert.DeserializeObject<HouseCreationAndCalculationJob>(str);
                //hj.CalcSpec.CalcOptions.Add(CalcOption.ActionsLogfile);
            }, _ => { });
        }
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void FixMandatoryTraits()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentAssemblyVersion());
            Simulator sim = new Simulator(db.ConnectionString);
            foreach (var template in sim.HouseholdTemplates.Items) {
                foreach (HHTemplateEntry entry in template.Entries) {
                    if (entry.TraitTag.Name.Contains("Work / ") || entry.TraitTag.Name.Contains("Sleep / ") ||entry.TraitTag.Name == ("Child / School")) {
                        Logger.Info("setting tag to mandatory: " + template.Name + ": " + entry.TraitTag.Name);
                        entry.IsMandatory = true;
                    }
                    entry.SaveToDB();
                }
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void MakeJsonForKenish()
        {
            const string basepath = @"C:\Work\forKenish_v3";
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            {
                var hj = MakeKenishHouseJob(sim,1);
            if (hj.CalcSpec == null) {
                throw new LPGException("was null");
            }

            //hj.CalcSpec.OutputDirectory = "TestingData1";
            File.WriteAllText(Path.Combine(basepath,"Testingdata1.json"), JsonConvert.SerializeObject(hj, Formatting.Indented));
        }

            {
                var hj2 = MakeKenishHouseJob(sim,2);
                if (hj2.CalcSpec == null) {
                    throw new LPGException("was null");
                }
               // hj2.CalcSpec.OutputDirectory = "TestingData2";
                hj2.CalcSpec.InternalTimeResolution = "00:30:00";
                File.WriteAllText(Path.Combine(basepath, "TestingData2.json"), JsonConvert.SerializeObject(hj2, Formatting.Indented));
            }
            {
                var hj3 = MakeKenishHouseJob(sim,3);
                if (hj3.CalcSpec == null)
                {
                    throw new LPGException("was null");
                }
              //  hj3.CalcSpec.OutputDirectory = "TestingData3";
                hj3.CalcSpec.EndDate = new DateTime(2020, 12, 31);
                File.WriteAllText(Path.Combine(basepath, "TestingData3.json"), JsonConvert.SerializeObject(hj3, Formatting.Indented));
            }
            {
                var hj4 = MakeKenishHouseJob(sim,4);
                if (hj4.CalcSpec == null)
                {
                    throw new LPGException("was null");
                }
               // hj4.CalcSpec.OutputDirectory = "TestingData4";
                hj4.CalcSpec.InternalTimeResolution = "00:30:00";
                hj4.CalcSpec.EndDate = new DateTime(2020, 12, 31);
                File.WriteAllText(Path.Combine(basepath, "TestingData4.json"), JsonConvert.SerializeObject(hj4, Formatting.Indented));
            }
        }

        private static HouseCreationAndCalculationJob MakeKenishHouseJob(Simulator sim, int idx){
            var hj = new HouseCreationAndCalculationJob
            {
                CalcSpec = JsonCalcSpecification.MakeDefaultsForTesting()
            };
            hj.CalcSpec.StartDate = new DateTime(2020, 1, 1);
            hj.CalcSpec.EndDate = new DateTime(2020, 1, 3);
            hj.CalcSpec.DeleteDAT = false;
            hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
            hj.CalcSpec.DeleteSqlite = false;
            hj.CalcSpec.ExternalTimeResolution = "00:15:00";
            hj.CalcSpec.EnableTransportation = true;
            hj.CalcSpec.GeographicLocation = sim.GeographicLocations.FindFirstByNameNotNull("Berlin", FindMode.Partial).GetJsonReference();
            hj.CalcSpec.DeleteDAT = true;
            hj.CalcSpec.OutputDirectory = "TestingData_" + idx;
            if (hj.CalcSpec.CalcOptions == null) {
                throw new LPGException("was null");
            }
            hj.CalcSpec.CalcOptions.Add(CalcOption.TransportationStatistics);
            hj.CalcSpec.CalcOptions.Add(CalcOption.BodilyActivityStatistics);
            hj.CalcSpec.CalcOptions.Add(CalcOption.JsonHouseholdSumFiles);
            hj.CalcSpec.CalcOptions.Add(CalcOption.JsonDeviceProfilesIndividualHouseholds);
            hj.CalcSpec.CalcOptions.Add(CalcOption.JsonHouseSumFiles);
            hj.CalcSpec.CalcOptions.Add(CalcOption.SumProfileExternalIndividualHouseholdsAsJson);
            hj.CalcSpec.CalcOptions.Add(CalcOption.TansportationDeviceJsons);
            hj.House = new HouseData(StrGuid.FromString("houseguid"), "HT01", 1000, 100, "housename")
            {
                Households = new List<HouseholdData>()
            };
            var hhd = new HouseholdData("householdid",
                "householdname", sim.ChargingStationSets[0].GetJsonReference(),
                sim.TransportationDeviceSets[0].GetJsonReference(),
                sim.TravelRouteSets[0].GetJsonReference(), null,
                HouseholdDataSpecificationType.ByHouseholdName);
            var hh = sim.ModularHouseholds[0];
            hhd.HouseholdNameSpec = new HouseholdNameSpecification(hh.GetJsonReference());
            hj.House.Households.Add(hhd);

            var hhd2 = new HouseholdData("householdid",
                "householdname", sim.ChargingStationSets[1].GetJsonReference(),
                sim.TransportationDeviceSets[1].GetJsonReference(),
                sim.TravelRouteSets[1].GetJsonReference(), null,
                HouseholdDataSpecificationType.ByHouseholdName);
            var hh2 = sim.ModularHouseholds[1];
            hhd2.HouseholdNameSpec = new HouseholdNameSpecification(hh2.GetJsonReference());
            hj.House.Households.Add(hhd);
            return hj;

        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void TestHouseTypesHT01()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.HouseSumProfilesFromDetailedDats;
            HouseJobTestHelper.RunSingleHouse(sim =>
                    HouseJobCalcPreparer.PrepareNewHouseForHousetypeTesting(sim, hhguid),
                x => HouseJobTestHelper.CheckForResultfile(x, co));
        }
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsJsonDeviceProfilesIndividualHouseholds()
        {
            static void CheckElec(string wd)
            {
                DirectoryInfo di = new DirectoryInfo(wd);
                var fis = di.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (var fi in fis) {
                    Logger.Info(fi.FullName);
                    if (fi.Name == "DeviceProfiles.Electricity.HH1.json") {
                        string s = File.ReadAllText(fi.FullName);
                        var dso =  JsonConvert.DeserializeObject<JsonDeviceProfiles>(s);
                        var deviceNames = dso.DeviceProfiles.Select(x => x.Name).ToList();
                        RowCollection rc = new RowCollection("Devices");
                        foreach (var singleDeviceProfile in dso.DeviceProfiles) {
                            XlsRowBuilder rb = XlsRowBuilder.Start("Name" ,singleDeviceProfile.Name );
                            //rb.Add("Sum", singleDeviceProfile.Values.Sum());
                            rc.Add(rb);
                        }
                        XlsxDumper.WriteToXlsx(@"c:\work\devices.xlsx",rc);
                        var distinct = deviceNames.Distinct().ToList();
                        if (distinct.Count != deviceNames.Count) {
                            throw new LPGException("counts don't match:" + distinct.Count + " - " + deviceNames.Count);
                        }
                    }
                }
            }
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.JsonDeviceProfilesIndividualHouseholds;
            HouseJobTestHelper.RunSingleHouse((sim) => {
                var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.ThreeMonths);
                if (hj.CalcSpec?.CalcOptions == null) {
                    throw new LPGException("errr");
                }
                hj.CalcSpec.CalcOptions.Add(co);
                hj.CalcSpec.CalcOptions.Add(CalcOption.HouseholdContents);
                hj.CalcSpec.EndDate = new DateTime(2020,1,3);
                return hj;
            }, (x) => CheckElec(x));
        }
    }

    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class HouseholdDefinitionExporter:UnitTestBaseClass {

        public class LpghhSpec
        {

            [JetBrains.Annotations.NotNull]
            [ItemNotNull]
            public List<PersonData> Persons { get; set; } = new List<PersonData>();

            [ItemNotNull]
            public List<string>? HouseholdTags { get; set; } = new List<string>();

            public string HHName { get; set; } = "";

        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.ManualOnly)]
        public void ExportAllHouseholdDefinition()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            List<LpghhSpec> hhs  = new List<LpghhSpec>();
            foreach (var mhh in sim.HouseholdTemplates.Items) {
                LpghhSpec hhps = new LpghhSpec();
                hhps.HHName = mhh.Name;
                hhs.Add(hhps);
                foreach (var person in mhh.Persons) {
                    var pd = new PersonData(person.Person.Age, person.Person.Gender != PermittedGender.Male ? Gender.Female : Gender.Male, person.Name);
                    if (person.LivingPatternTag == null) {
                        throw new LPGException("was null");
                    }
                    pd.LivingPatternTag = person.LivingPatternTag.Name;
                    hhps.Persons.Add(pd);
                }

                if (hhps.HouseholdTags == null) {
                    throw new LPGException("was null");
                }

                foreach (var tag in mhh.TemplateTags) {
                        hhps.HouseholdTags.Add(tag.Tag.Name);
                }
            }

            var str = JsonConvert.SerializeObject(hhs, Formatting.Indented);
            File.WriteAllText("AllHouseholds.json",str);

        }

        public HouseholdDefinitionExporter([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }

    public class SingleCalculationTests: UnitTestBaseClass {
        public static HouseCreationAndCalculationJob PrepareNewHouseholdWithTemplateForTesting(Simulator sim)
        {
            var hj = new HouseCreationAndCalculationJob
            {
                CalcSpec = JsonCalcSpecification.MakeDefaultsForTesting()
            };
            hj.CalcSpec.StartDate = new DateTime(2020, 1, 1);
            hj.CalcSpec.EndDate = new DateTime(2020, 1, 3);
            hj.CalcSpec.DeleteDAT = false;
            hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.NoFiles;
            hj.CalcSpec.DeleteSqlite = false;
            hj.CalcSpec.ExternalTimeResolution = "00:15:00";
            hj.CalcSpec.EnableTransportation = false;
            var ht = sim.HouseTypes[0];
            hj.House = new HouseData(StrGuid.FromString("houseguid"), ht.HouseTypeCode, 1000, 100, "housename")
            {
                Households = new List<HouseholdData>()
            };
            var hhd = new HouseholdData("householdid",
                "householdname", sim.ChargingStationSets[0].GetJsonReference(),
                sim.TransportationDeviceSets[0].GetJsonReference(),
                sim.TravelRouteSets[0].GetJsonReference(), null,
                HouseholdDataSpecificationType.ByTemplateName);
            hhd.HouseholdTemplateSpec = new HouseholdTemplateSpecification(sim.HouseholdTemplates[0].Name);
            var hh = sim.ModularHouseholds[0];
            hhd.HouseholdNameSpec = new HouseholdNameSpecification(hh.GetJsonReference());
            hj.House.Households.Add(hhd);
            return hj;
        }

        [Fact]
        public void TestHouseJobs1()
        {
            //const CalcOption co = CalcOption.ActivationFrequencies;
            HouseJobTestHelper.RunSingleHouse(sim => {
                var hj = PrepareNewHouseholdWithTemplateForTesting(sim);
                if (hj.CalcSpec?.CalcOptions == null)
                {
                    throw new LPGException();
                }

                hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.All;
                //hj.CalcSpec.CalcOptions.Add(co);
                //hj.CalcSpec.CalcOptions.Add(co);
                hj.CalcSpec.EndDate = new DateTime(2020, 12, 31);
                return hj;
            }, x => { });
        }

        // ReSharper disable once RedundantNameQualifier
        public SingleCalculationTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
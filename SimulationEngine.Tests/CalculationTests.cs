using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.Tests;
using Database;
using Database.Helpers;
using FluentAssertions;
//using iTextSharp.text.pdf;
using JetBrains.Annotations;

using SimulationEngineLib;
using Xunit;
using Xunit.Abstractions;


namespace SimulationEngine.Tests
{
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class CalculationTests : UnitTestBaseClass
    {
        /*
        private static void RunBasicTestOutput(CalcOption option, string name)
        {
            //string[] arr = {addedOutput};
            RunBasicTestOutput(arr, name);
        }*/

        private static void RunBasicTestOutput(CalcOption option, [JetBrains.Annotations.NotNull] string name, [ItemNotNull][JetBrains.Annotations.NotNull] params string[] addedOptions)
        {
            //_calculationProfiler.Clear();
            Config.CatchErrors = false;
            Config.IsInUnitTesting = true;
            Logger.Info("Current directory before setup:" + Directory.GetCurrentDirectory());
            using (var wd = ProgramTests.SetupDB3("RunBasicTestOutput." + name))
            {
                Logger.Info("Current directory after setup:" + Directory.GetCurrentDirectory());
                var arguments = new List<string>
            {
                "Calculate",
                "-CalcObjectType",
                "ModularHousehold",
                "-CalcObjectNumber",
                "0",
                "-StartDate",
                "01.01.2015",
                "-EndDate",
                "05.01.2015",
                "-MeasureCalculationTimes",
                "-ForcePDF",
                "-OutputFileDefault",
                "None",
                "-OutputDirectory"
            };
                var directoryname = "HH0." + AutomationUtili.CleanFileName(name).Replace("--", string.Empty);
                arguments.Add(directoryname);
                arguments.Add("-CalcOption");
                arguments.Add(option.ToString());

                arguments.AddRange(addedOptions);
                MainSimEngine.Run(arguments.ToArray(),"simengine.exe");
                var di = new DirectoryInfo(directoryname);
                var fis = di.GetFiles("Overview*.pdf");
                fis.Length.Should().Be(1);
                fis[0].Length.Should().BeGreaterThan(0);
                //int numberOfPages;
                //TODO: add a reader for the number of pages again
                //using (var pdfReader = new PdfReader(fis[0].FullName))
                //{
                //numberOfPages = pdfReader.NumberOfPages;
                //}
                //Assert.GreaterOrEqual(numberOfPages, 1);
                //Logger.Info("Pages:" + numberOfPages);
                wd.CleanUp(1);
            }
        }

        private static void RunDateTimeOnAllFiles([JetBrains.Annotations.NotNull] string firstTimestep, [JetBrains.Annotations.NotNull] string firstTimestamp, int targetLineCount)
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
            var sumFiles = new List<string>
            {
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
            var filesWithVaryingLineCount = new List<string>
            {
                "Actions.",
                "Locations.",
                "Thoughts."
            };
            var filesToCheck = new List<FileInfo>();
            foreach (var fi in fis)
            {
                var contains = false;
                foreach (var sumFile in sumFiles)
                {
                    if (fi.Name.ToUpperInvariant().StartsWith(sumFile.ToUpperInvariant(), StringComparison.Ordinal))
                    {
                        contains = true;
                    }
                }
                if (!contains)
                {
                    filesToCheck.Add(fi);
                }
            }
            List<Tuple<FileInfo,int>> filesWithLineCounts = new List<Tuple<FileInfo, int>>();
            foreach (var fileInfo in filesToCheck)
            {
                var linecount = 1;
                using (var sr = new StreamReader(fileInfo.FullName))
                {
                    sr.ReadLine(); // header
                    var firstLine = sr.ReadLine();

                    if (firstLine == null)
                    {
                        throw new LPGException("Readline failed");
                    }
                    var arr = firstLine.Split(';');
                    if (arr[0] != firstTimestep)
                    {
                        throw new LPGException("File: " + fileInfo.Name + ": First timestep was: " + arr[0] +
                                               " instead of " + firstTimestep);
                    }
                    if (arr[1] != firstTimestamp)
                    {
                        throw new LPGException("File: " + fileInfo.Name + ": First timestamp was: " + arr[1] +
                                               " instead of " + firstTimestamp);
                    }

                    while (!sr.EndOfStream)
                    {
                        sr.ReadLine();
                        linecount++;
                    }
                    if (!filesWithVaryingLineCount.Any(x => fileInfo.Name.StartsWith(x, StringComparison.Ordinal)))
                    {
                        if (linecount != targetLineCount)
                        {
                            throw new LPGException("The file " + fileInfo.Name + ": has " + linecount +
                                                   " lines instead of " + targetLineCount);
                        }
                    }
                }
                Logger.Info(fileInfo.Name + ": " + linecount);
                filesWithLineCounts.Add(new Tuple<FileInfo, int>(fileInfo, linecount));
            }

            foreach (Tuple<FileInfo, int> pair in filesWithLineCounts) {
                Logger.Info(pair.Item1.Name + " " + pair.Item2);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTest()
        {
            Config.CatchErrors = false;
            Config.IsInUnitTesting = true;
            var startdir = Directory.GetCurrentDirectory();
            using (var wd = ProgramTests.SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var arguments = new List<string>
            {
                "Calculate",
                "-CalcObjectType",
                "ModularHousehold",
                "-CalcObjectNumber",
                "1",
                "-StartDate",
                "01.07.2015",
                "-EndDate",
                "03.07.2015",
                "-Testing"
            };
                MainSimEngine.Run(arguments.ToArray(), "simengine.exe");
                wd.CleanUp(1);
            }
            Directory.SetCurrentDirectory(startdir);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest5)]
        public void RunBasicTest2()
        {
            //_calculationProfiler.Clear();
            Config.CatchErrors = false;
            Config.IsInUnitTesting = true;
            var startdir = Directory.GetCurrentDirectory();
            using (var wd = ProgramTests.SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var arguments = new List<string>
            {
                "Calculate",
                "-CalcObjectType",
                "House",
                "-CalcObjectNumber",
                "0",
                "-StartDate",
                new DateTime(2017,1,1).ToShortDateString(),
                "-EndDate",
                new DateTime(2017,1,31).ToShortDateString(),
                // arguments.Add("-Testing");
                //            arguments.Add("-OutputDirectory");
                //          arguments.Add("001_(H001)House01HT20wit");
                "-LoadtypePriority",
                "RecommendedForHouses",
                "-SkipExisting",
                "-MeasureCalculationTimes",
                "-OutputFileDefault",
                "ForSettlementCalculations",
                "-TemperatureProfileIndex",
                "0"
            };
                MainSimEngine.Run(arguments.ToArray(),"simengine.exe");
                Directory.SetCurrentDirectory(startdir);
                wd.CleanUp(1);
            }
        }

        //  - - -   0 -GeographicLocationIndex 6 -StartDate 01.01.2017 -EndDate 31.12.2017 -EnergyIntensityType Random

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest3)]
        public void RunBasicTestAllOutput()
        {
            Config.CatchErrors = false;
            Config.IsInUnitTesting = true;

            using (var wd = ProgramTests.SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var arguments = new List<string>
            {
                "Calculate",
                "-CalcObjectType",
                "ModularHousehold",
                "-CalcObjectNumber",
                "14",
                "-StartDate",
                "01.01.2015",
                "-EndDate",
                "05.01.2015",
                "-OutputFileDefault",
                "ReasonableWithChartsAndPDF",
                "-SkipExisting",
                //  arguments.Add("-ImportFiles");
                "-OutputDirectory",
                "HH0.AllOutputs"
            };
                MainSimEngine.Run(arguments.ToArray(), "simengine.exe");
                Directory.SetCurrentDirectory(wd.PreviousCurrentDir);
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestAllOutputWithDelete()
        {
            Config.CatchErrors = false;
            Config.IsInUnitTesting = true;
            var wd = ProgramTests.SetupDB3(Utili.GetCurrentMethodAndClass());
            var arguments = new List<string>
            {
                "Calculate",
                "-CalcObjectType",
                "ModularHousehold",
                "-CalcObjectNumber",
                "0",
                "-StartDate",
                "01.01.2015",
                "-EndDate",
                "05.01.2015",
                "-OutputFileDefault",
                "Reasonable",
                "-DeleteAllButPDF",
                "-OutputDirectory",
                "HH0.AllOutputsDelete"
            };
            MainSimEngine.Run(arguments.ToArray(), "simengine.exe");
            Environment.CurrentDirectory = wd.PreviousCurrentDir;
            wd.CleanUp(1);
        }

       /* [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestCriticalViolations() => RunBasicTestOutput(CalcOption.CriticalViolations,
            "criticalviolations");*/

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestDaylightTimesToCSV() => RunBasicTestOutput(CalcOption.DaylightTimesList, "daylight");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestDesiresLogfile() => RunBasicTestOutput(CalcOption.DesiresLogfile, "desires");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestDeviceProfile() => RunBasicTestOutput(CalcOption.DeviceProfiles, "deviceprofiles");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestDeviceProfileExternal() => RunBasicTestOutput(CalcOption.DeviceProfileExternalEntireHouse,
            "device profile external", "-ExternalTimeResolution", "01:00:00");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestDurationCurve() => RunBasicTestOutput(CalcOption.DurationCurve, "durationcurve");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestEnergyCarpetPlot() => RunBasicTestOutput(CalcOption.EnergyCarpetPlot,
            "Energycarpetplot");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestHouseholdContents() => RunBasicTestOutput(CalcOption.HouseholdContents,
            "householdcontents");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestHouseholdPlan() => RunBasicTestOutput(CalcOption.HouseholdPlan, "householdplan");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestImportFiles() => RunBasicTestOutput(CalcOption.PolysunImportFiles, "importfiles");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestLocations() => RunBasicTestOutput(CalcOption.LocationsFile, "locations");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestNoOutput()
        {
            Config.CatchErrors = false;
            Config.IsInUnitTesting = true;
            using (var wd = ProgramTests.SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var arguments = new List<string>
            {
                "Calculate",
                "-CalcObjectType",
                "ModularHousehold",
                "-CalcObjectNumber",
                "0",
                "-StartDate",
                "01.01.2015",
                "-EndDate",
                "05.01.2015",
                "-OutputDirectory",
                "HH0.NoOutput",
                "-DeleteDat",
                "-OutputFileDefault",
                "None"
            };
                MainSimEngine.Run(arguments.ToArray(), "simengine.exe");
                var di = new DirectoryInfo(wd.WorkingDirectory);
                var fis = di.GetFiles("*.*", SearchOption.AllDirectories);
                var filtered = new List<string>
            {
                "profilegenerator.db3",
                "log.unittest.txt",
                "Logfile.txt",
                "finished.flag",
                "resultentries.xml",
                "devicetaggingsets.json",
                //"resultentries.json",
                "resultentries.hh1.txt",
                "transportationlogfile.hh1.txt",
                Constants.FinishedFileFlag,
                "results.general.sqlite",
                "logfile.commandlinecalculation.txt",
                "results.general.sqlite",
                "results.hh1.sqlite",
                "onlinedeviceenergyusage.sums.cold water.dat",
                "onlinedeviceenergyusage.sums.electricity.dat",
                "onlinedeviceenergyusage.sums.hot water.dat",
                "onlinedeviceenergyusage.sums.none.dat",
                "onlinedeviceenergyusage.sums.warm water.dat"
            };
                var filteredLow = filtered.Select(x => x.ToLowerInvariant()).ToList();
                var lowerNames = fis.Select(x => x.Name.ToLowerInvariant()).ToList();
                var excessFiles = lowerNames.Where(x => !filteredLow.Contains(x)).ToList();
                if (excessFiles.Count > 0)
                {
                    var s = "";
                    foreach (var fi in excessFiles)
                    {
                        s += fi + Environment.NewLine;
                    }
                    throw new LPGException("Too many files: " + Environment.NewLine + s);
                }
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestSumProfile() => RunBasicTestOutput(CalcOption.OverallSum, "sumprofile");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestSumProfileExternal()
        {
            RunBasicTestOutput(CalcOption.SumProfileExternalEntireHouse, "sumprofilexternal", "-ExternalTimeResolution",
                "00:15:00");
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestTemperatureFile() => RunBasicTestOutput(CalcOption.TemperatureFile, "temperaturefile");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestThoughtsLogfile() => RunBasicTestOutput(CalcOption.ThoughtsLogfile, "thoughtsfile");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestTimeOfUse() => RunBasicTestOutput(CalcOption.TimeOfUsePlot, "timeofuse");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestTimeProfileFile() => RunBasicTestOutput(CalcOption.TimeProfileFile, "timeprofilefile");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestTotalsPerDevice() => RunBasicTestOutput(CalcOption.TotalsPerDevice, "totalsperdevice");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestTotalsPerLoadtype() => RunBasicTestOutput(CalcOption.TotalsPerLoadtype,
            "totalsperloadtype");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunBasicTestWeekday() => RunBasicTestOutput(CalcOption.WeekdayProfiles, "weekday");

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest5)]
        public void RunHouseCheckFileRegistration()
        {
            Config.ReallyMakeAllFilesIncludingBothSums = true;
            Config.CatchErrors = false;
            Config.IsInUnitTesting = true;
            using (var wd = ProgramTests.SetupDB3(Utili.GetCurrentMethodAndClass(), true))
            {
                var arguments = new List<string>
            {
                "Calculate",
                "-CalcObjectType",
                "House",
                "-CalcObjectNumber",
                "25",
                "-StartDate",
                "01.01.2015",
                "-EndDate",
                "03.01.2015",
                "-OutputFileDefault",
                "All",
                "-LoadTypePriority",
                "RecommendedForHouses",
                "-OutputDirectory"
            };
                const string directoryname = "H25.Desires";
                arguments.Add(directoryname);
                MainSimEngine.Run(arguments.ToArray(), "simengine.exe");

                var di = new DirectoryInfo(Path.Combine(wd.WorkingDirectory, directoryname));
                SqlResultLoggingService srls = new SqlResultLoggingService(di.FullName);
                ResultFileEntryLogger rfel = new ResultFileEntryLogger(srls);
                var rfl = rfel.Load();
                var registeredFiles = rfl.Select(x => wd.Combine(x.FullFileName??throw new LPGException("was null")).ToLower(CultureInfo.InvariantCulture)).ToList();
                var fis = di.GetFiles("*.*", SearchOption.AllDirectories);
                var filteredFiles = new List<string>
            {
                Constants.FinishedFileFlag,
                "calculationprofiler.json",
                "logfile.commandlinecalculation.txt",
                "results.general.sqlite",
                "results.general.sqlite-shm",
                "results.general.sqlite-wal",
                "results.hh1.sqlite",
                "results.house.sqlite",
                "calculationdurationflamechart.calcmanager.png"
            };
                List<string> files = new List<string>();
                foreach (var fi in fis)
                {
                    var fullname = fi.FullName;
                    if (filteredFiles.Contains(fi.Name.ToLower(CultureInfo.InvariantCulture)))
                    {
                        continue;
                    }
                    if (!registeredFiles.Contains(fullname.ToLower(CultureInfo.InvariantCulture)))
                    {
                        files.Add(fi.Name.ToLower(CultureInfo.InvariantCulture));
                    }
                }

                if (files.Count > 0)
                {
                    var allfn = string.Join("\n", files);
                    throw new LPGException("unregistered file: " + allfn);
                }
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunHouseDesireOutput()
        {
            Config.CatchErrors = false;
            Config.IsInUnitTesting = true;
            using (var wd = ProgramTests.SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var arguments = new List<string>
            {
                "Calculate",
                "-CalcObjectType",
                "House",
                "-CalcObjectNumber",
                "25",
                "-StartDate",
                "01.01.2015",
                "-EndDate",
                "05.01.2015",
                "-OutputFileDefault",
                "None",
                "-LoadTypePriority",
                "RecommendedForHouses",
                "-ForcePDF",
                "-CalcOption",
#pragma warning disable RCS1015 // Use nameof operator.
                CalcOption.DesiresLogfile.ToString(),
#pragma warning restore RCS1015 // Use nameof operator.
                "-OutputDirectory"
            };
                const string directoryname = "H25.Desires";
                arguments.Add(directoryname);

                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                var di = new DirectoryInfo(directoryname);
                var fis = di.GetFiles("Overview*.pdf");
                fis.Length.Should().Be(1);
                fis[0].Length.Should().BeGreaterThan(0);
                //int numberOfPages;
                /*using (var pdfReader = new PdfReader(fis[0].FullName))
                {
                    numberOfPages = pdfReader.NumberOfPages;
                }
                Assert.GreaterOrEqual(numberOfPages, 1);
                Logger.Info("Pages:" + numberOfPages);*/
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest3)]
        public void RunHouseWithMultipleCheck()
        {
            Config.ReallyMakeAllFilesIncludingBothSums = true;
            Config.CatchErrors = false;
            Config.IsInUnitTesting = true;
            using (var wd = ProgramTests.SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                var db3File = Path.Combine(wd.WorkingDirectory, "profilegenerator.db3");
                var connectionString = "Data Source=" + db3File;
                var sim = new Simulator(connectionString);
                var house = sim.Houses.FindFirstByName("01, 02 and 03", FindMode.Partial);
                var idx = sim.Houses.It.IndexOf(house);

                var arguments = new List<string>
            {
                "Calculate",
                "-CalcObjectType",
                "House",
                "-CalcObjectNumber",
                idx.ToString(CultureInfo.CurrentCulture),
                "-StartDate",
                "01.01.2015",
                "-EndDate",
                "05.01.2015",
                "-OutputFileDefault",
                "All",
                "-LoadTypePriority",
                "RecommendedForHouses",
                "-OutputDirectory"
            };
                const string directoryname = "Testhouse";
                arguments.Add(directoryname);
                MainSimEngine.Run(arguments.ToArray(), "simengine.exe");

                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunRegistrationTest()
        {
            const string directoryname = @"E:\unittest\CalculationTests.RunHouseCheckFileRegistration\H25.Desires";

            var di = new DirectoryInfo(directoryname);
            SqlResultLoggingService srls = new SqlResultLoggingService(directoryname);
            ResultFileEntryLogger rfel = new ResultFileEntryLogger(srls);

            var rfl = rfel.Load();
            var registeredFiles = rfl.Select(x => x.FullFileName).ToList();
            var fis = di.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (var fi in fis)
            {
                var fullname = fi.FullName;
                if (!registeredFiles.Contains(fullname))
                {
                    throw new LPGException("unregistered file: " + fullname);
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "RunTime")]
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunTimeAxisTest2NoSettling()
        {
            using (var wd = ProgramTests.SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                Config.CatchErrors = false;
                Config.IsInUnitTesting = true;
                DateTime startdate = new DateTime(2015, 1, 1);
                DateTime enddate = new DateTime(2015, 1, 5);
                var arguments = new List<string>
            {
                "Calculate",
                "-CalcObjectType",
                "ModularHousehold",
                "-CalcObjectNumber",
                "2",
                "-StartDate",
                startdate.ToShortDateString(),
                "-EndDate",
                enddate.ToShortDateString(),
                "-OutputFileDefault",
                "Reasonable",
                "-OutputDirectory",
                "CHH3.RunTimeAxisTest"
            };
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");

                //this checks if the pre-calc period is correctly hidden
                var dt = new DateTime(2015, 1, 1);
                var dtstr = dt.ToShortDateString() + " " + dt.ToShortTimeString();
                RunDateTimeOnAllFiles("0", dtstr, 5 * 1440);
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest3)]
        public void TestCrashingHH()
        {
            Config.CatchErrors = false;
            Config.IsInUnitTesting = true;
            var oldDir = Directory.GetCurrentDirectory();
            using (var wd = ProgramTests.SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                Directory.SetCurrentDirectory(wd.WorkingDirectory);
                var arguments = new List<string>
            {
                "Calculate",
                "-CalcObjectType",
                "House",
                "-CalcObjectNumber",
                "1",
                "-OutputDirectory",
                "033_Diss(DissResults1PV0",
                "-SkipExisting",
                "-OutputFileDefault",
                "ReasonableWithChartsAndPDF",
                "-TemperatureProfileIndex",
                "0",
                "-GeographicLocationIndex",
                "6",
                "-DeviceSelectionName",
                "Only_200W_Light",
                "-LoadTypePriority",
                "RecommendedForHouses",

                "-StartDate",
                new DateTime(2015,1,1).ToString(CultureInfo.CurrentCulture),
                "-EndDate",
                new DateTime(2015,1,31).ToString(CultureInfo.CurrentCulture),
                "-EnergyIntensityType",
                "EnergySavingPreferMeasured"
            };
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                Directory.SetCurrentDirectory(oldDir);
                wd.CleanUp(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest3)]
        public void TestCrashingHH1()
        {
            Config.CatchErrors = false;
            Config.IsInUnitTesting = true;
            var oldDir = Directory.GetCurrentDirectory();
            using (var wd = ProgramTests.SetupDB3(Utili.GetCurrentMethodAndClass()))
            {
                Directory.SetCurrentDirectory(wd.WorkingDirectory);
                var arguments = new List<string>
            {
                "Calculate",
                "-CalcObjectType",
                "House",
                "-CalcObjectNumber",
                "0",
                "-OutputDirectory",
                "033_Diss(DissResults1PV0",
                "-SkipExisting",
                "-LoadTypePriority",
                "RecommendedForHouses",
                "-OutputFileDefault",
                "ReasonableWithChartsAndPDF",
                "-TemperatureProfileIndex",
                "0",
                "-GeographicLocationIndex",
                "6",
                "-StartDate",
                new DateTime(2015,1,1).ToString(CultureInfo.CurrentCulture),
                "-EndDate",
                new DateTime(2015,1,31).ToString(CultureInfo.CurrentCulture),
                "-EnergyIntensityType",
                "EnergySavingPreferMeasured"
            };
                MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
                Directory.SetCurrentDirectory(oldDir);
                wd.CleanUp(1);
            }
        }

        // todo: fix the plots
#pragma warning disable S125 // Sections of code should not be "commented out"
        /*  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "RunTime")]
                [Fact]
                public void RunTimeAxisTestWithSettling()
                {
                   WorkingDir wd =  ProgramTests.SetupDB3(Utili.GetCurrentMethodAndClass());
                    Config.CatchErrors = false;
                    Program.IsUnitTest = true;
                    List<string> arguments = new List<string>();
                    arguments.Add("--Calculate");
                    arguments.Add("--ModularHousehold");
                    arguments.Add("--CalcObjectNumber");
                    arguments.Add("2");
                    arguments.Add("--StartDate");
                    arguments.Add("01.01.2015");
                    arguments.Add("--EndDate");
                    arguments.Add("05.01.2015");
                    arguments.Add("--OutputFileDefault");
                    arguments.Add("4");
               //     arguments.Add("--ShowSettlingPeriod");
                    arguments.Add("--OutputDirectory");
                    arguments.Add("CHH3.RunTimeAxisTest");
                    Program.Main(arguments.ToArray());
                    RunDateTimeOnAllFiles("-4320", "29.12.2014 00:00", 8 * 1440);
                    wd.CleanUp(1);
                }*/
#pragma warning restore S125 // Sections of code should not be "commented out"
        public CalculationTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
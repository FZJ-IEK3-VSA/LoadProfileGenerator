using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.Integrity;
using CalculationController.Queue;
using Common;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using Common.Tests;
using Database;
using Database.Helpers;
using Database.Tests;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


//using Calculation.HouseholdElements;
//

namespace CalculationController.Tests {
    public class DeviceTimeOffsetBug : UnitTestBaseClass {
        private static void CheckForOverdoneOffsets([NotNull] string path, [NotNull] SqlResultLoggingService srls) {
            //var actionsName = Path.Combine(path, "Reports", "ActionsEachStep.HH1.csv");
            var wwDeviceProfiles = Path.Combine(path, "Results", "DeviceProfiles.Warm Water.csv");
            HouseholdKeyLogger hkl = new HouseholdKeyLogger(srls);
            var hhkeys = hkl.Load();
            if (hhkeys.Count != 2) {
                throw new LPGException("Unknown key");
            }
            var selectedKey = hhkeys[1].HHKey;
            //SingleTimestepActionEntryLogger stael = new SingleTimestepActionEntryLogger(srls);
            ActionEntryLogger ael = new ActionEntryLogger(srls);
           // var actionEachStep = stael.Read(selectedKey);
            var actions = ael.Read(selectedKey);

            List<EnergyUseEntry> eues;
            using (var sr2 = new StreamReader(wwDeviceProfiles)) {
                eues = new List<EnergyUseEntry>();
                var header = sr2.ReadLine();
                if (header == null) {
                    throw new LPGException("Readline failed");
                }
                var headerarr = header.Split(';');

                while (!sr2.EndOfStream) {
                    var s = sr2.ReadLine();
                    if (s == null) {
                        throw new LPGException("readline fail");
                    }
                    var eue = new EnergyUseEntry(s, headerarr);
                    eues.Add(eue);
                }
                sr2.Close();
            }
            var breakfast = actions.First(x => x.AffordanceName == "eat breakfast (1 h)");
            var brtimestep = breakfast.TimeStep.ExternalStep + 2; //+2 to cover varying duration.
            for (var i = 0; i < eues.Count; i++) {
                if (eues[i].Timestep != i) {
                    throw new LPGException("Invalid timestep.");
                }
            }
            var stepcount = 2;
            const string sinkname = "HH1 - Kitchen - Kitchen Sink (5L/min) [L]";
            while (Math.Abs(eues[brtimestep + stepcount].Values[sinkname]) < Constants.Ebsilon) {
                stepcount++;
            }
            if (stepcount > 25) {
                throw new LPGException("Stepcount between two warm water uses during breakfast was : " + stepcount +
                                       ", but it should be about 20. Timestep: " + brtimestep);
            }
        }
        private class EnergyUseEntry {
            public EnergyUseEntry([NotNull] string s, [NotNull][ItemNotNull] string[] headerDict) {
                var arr = s.Split(';');
                Timestep = Convert.ToInt32(arr[0], CultureInfo.InvariantCulture);
                for (var i = 2; i < arr.Length; i++) {
                    if (!string.IsNullOrWhiteSpace(arr[i])) {
                        var val = Convert.ToDouble(arr[i], CultureInfo.InvariantCulture);
                        Values.Add(headerDict[i], val);
                    }
                }
            }

            public int Timestep { get; }
            [NotNull]
            public Dictionary<string, double> Values { get; } = new Dictionary<string, double>();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TimeOffsetBugTest()
        {
            Config.AdjustTimesForSettlement = false;
            CleanTestBase.RunAutomatically(false);
            var start = DateTime.Now;
            using (var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                var path = wd1.WorkingDirectory;
                Config.MakePDFCharts = false;
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                Directory.CreateDirectory(path);
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim = new Simulator(db.ConnectionString);
                    var calcstart = DateTime.Now;
                    sim.MyGeneralConfig.StartDateUIString = "01.01.2015";
                    sim.MyGeneralConfig.EndDateUIString = "31.01.2015";
                    sim.MyGeneralConfig.InternalTimeResolution = "00:01:00";
                    sim.MyGeneralConfig.RandomSeed = 5;
                    sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.Reasonable);
                    sim.MyGeneralConfig.ShowSettlingPeriod = "false";
                    sim.MyGeneralConfig.CSVCharacter = ";";
                    sim.MyGeneralConfig.SelectedLoadTypePriority = LoadTypePriority.RecommendedForHouseholds;
                    SimIntegrityChecker.Run(sim, CheckingOptions.Default());

                    sim.Should().NotBeNull();

                    var cmf = new CalcManagerFactory();
                    //CalcDevice.UseRanges = true;
                    var geoloc = sim.GeographicLocations.FindFirstByName("Chemnitz", FindMode.Partial);
                    if (geoloc == null)
                    {
                        throw new LPGException("Geoloc was null");
                    }
                    var chh =
                        sim.ModularHouseholds.Items.First(x => x.Name.StartsWith("CHR09", StringComparison.Ordinal));
                    CalculationProfiler calculationProfiler = new CalculationProfiler();
                    var options = sim.MyGeneralConfig.AllEnabledOptions();
                    options.Add(CalcOption.DeviceProfilesIndividualHouseholds);
                    options.Add(CalcOption.DeviceProfilesHouse);
                    CalcStartParameterSet csps = new CalcStartParameterSet(geoloc,
                        sim.TemperatureProfiles[0], chh, EnergyIntensityType.Random,
                        false,  null,
                        sim.MyGeneralConfig.SelectedLoadTypePriority, null, null, null,
                        options, new DateTime(2015, 1, 1), new DateTime(2015, 1, 31),
                        new TimeSpan(0, 1, 0), ";", 5, new TimeSpan(0, 1, 0),
                        false, false, false, 3, 3,
                        calculationProfiler, wd1.WorkingDirectory,
                        false,false, ".",false);
                    var cm = cmf.GetCalcManager(sim, csps, false);

                    static bool ReportCancelFunc()
                    {
                        Logger.Info("canceled");
                        return true;
                    }
                    cm.Run(ReportCancelFunc);
                    db.Cleanup();
                    Logger.ImportantInfo("Duration:" + (DateTime.Now - start).TotalSeconds + " seconds");
                    Logger.ImportantInfo("Calc Duration:" + (DateTime.Now - calcstart).TotalSeconds + " seconds");
                    Logger.ImportantInfo("loading Duration:" + (calcstart - start).TotalSeconds + " seconds");
                }
                CheckForOverdoneOffsets(wd1.WorkingDirectory, wd1.SqlResultLoggingService);
                wd1.CleanUp();
            }
            CleanTestBase.RunAutomatically(true);
        }

        public DeviceTimeOffsetBug([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
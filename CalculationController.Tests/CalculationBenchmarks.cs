using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.Integrity;
using CalculationController.Queue;
using CalculationEngine;
using ChartCreator2.Tests;
using Common;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using Common.Tests;
using Database;
using Database.Helpers;
using Database.Tables;
using Database.Tables.Houses;
using Database.Tables.Transportation;
using Database.Tests;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace CalculationController.Tests {
    [TestFixture]
    public class CalculationBenchmarks {
        [Test]
        [Category(UnitTestCategories.LongTest5)]
        public void ActionCarpetPlotTestBenchmark()
        {
                CalculationProfiler cp = new CalculationProfiler();
            cp.StartPart(Utili.GetCurrentMethodAndClass());
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var start = DateTime.Now;

            Logger.Threshold = Severity.Error;
            var path = wd1.WorkingDirectory;
            Config.MakePDFCharts = true;
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            cp.StartPart("SimLoading");
            var sim = new Simulator(db.ConnectionString);
            cp.StopPart("SimLoading");
            cp.StartPart("Calculation");
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.OnlyOverallSum);
            sim.MyGeneralConfig.Enable(CalcOption.ActionsLogfile);
            sim.MyGeneralConfig.Enable(CalcOption.TotalsPerLoadtype);
            sim.MyGeneralConfig.Enable(CalcOption.TotalsPerDevice);
            //sim.MyGeneralConfig.Enable(CalcOption.ActionCarpetPlot);
            //sim.MyGeneralConfig.Enable(CalcOption.ActivationFrequencies);
            sim.MyGeneralConfig.CSVCharacter = ";";
            //ChartLocalizer.ShouldTranslate = true;
            //ConfigSetter.SetGlobalTimeParameters(sim.MyGeneralConfig);
            Assert.AreNotEqual(null, sim);

            var cmf = new CalcManagerFactory();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0],sim.Houses[0],EnergyIntensityType.Random,
                false,version,null,LoadTypePriority.RecommendedForHouses,null,null,null,
                sim.MyGeneralConfig.AllEnabledOptions(),new DateTime(2015,1,1),new DateTime(2015,1,31),
                new TimeSpan(0,1,0),";",5, new TimeSpan(0, 15, 0),false,false,false, 3,5,
                calculationProfiler);
            var cm = cmf.GetCalcManager(sim, path,csps,  false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            cm.Run(ReportCancelFunc);
            cp.StopPart("Calculation");
            db.Cleanup();
            Logger.ImportantInfo("Duration:" + (DateTime.Now - start).TotalSeconds + " seconds");
//            var imagefiles = FileFinder.GetRecursiveFiles(new DirectoryInfo(wd1.WorkingDirectory),
            //              "Carpetplot.*.png");
//            Assert.GreaterOrEqual(imagefiles.Count, 1);
            cp.StopPart(Utili.GetCurrentMethodAndClass());
            cp.LogToConsole();
            using (var sw = new StreamWriter(Path.Combine(wd1.WorkingDirectory, Constants.CalculationProfilerJson))) {
                cp.WriteJson(sw);
            }
            //      wd1.CleanUp();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void ActionEachStepTest()
        {
            CalculationProfiler cp = new CalculationProfiler();
            cp.StartPart(Utili.GetCurrentMethodAndClass());
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var start = DateTime.Now;

            Logger.Threshold = Severity.Error;
            var path = wd1.WorkingDirectory;
            Config.MakePDFCharts = true;
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            cp.StartPart("SimLoading");
            var sim = new Simulator(db.ConnectionString);
            cp.StopPart("SimLoading");
            cp.StartPart("Calculation");
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.OnlyOverallSum);
            sim.MyGeneralConfig.Enable(CalcOption.ActionsLogfile);
            sim.MyGeneralConfig.Enable(CalcOption.ActionsEachTimestep);
            sim.MyGeneralConfig.CSVCharacter = ";";
            Assert.AreNotEqual(null, sim);

            var cmf = new CalcManagerFactory();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], sim.Houses[0], EnergyIntensityType.Random,
                false, version, null, LoadTypePriority.RecommendedForHouses, null,null, null,
                sim.MyGeneralConfig.AllEnabledOptions(), new DateTime(2015, 1, 1), new DateTime(2015, 1, 15),
                new TimeSpan(0, 1, 0), ";", 5, new TimeSpan(0, 15, 0), false, false, false, 3, 5,
                calculationProfiler);
            var cm = cmf.GetCalcManager(sim, path, csps,  false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            cm.Run(ReportCancelFunc);
            cp.StopPart("Calculation");
            db.Cleanup();
            Logger.ImportantInfo("Duration:" + (DateTime.Now - start).TotalSeconds + " seconds");
            //            var imagefiles = FileFinder.GetRecursiveFiles(new DirectoryInfo(wd1.WorkingDirectory),
            //              "Carpetplot.*.png");
            //            Assert.GreaterOrEqual(imagefiles.Count, 1);
            cp.StopPart(Utili.GetCurrentMethodAndClass());
            cp.LogToConsole();
            using (var sw = new StreamWriter(Path.Combine(wd1.WorkingDirectory, Constants.CalculationProfilerJson)))
            {
                cp.WriteJson(sw);
            }
            //      wd1.CleanUp();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void HouseTypeTest()
        {
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Logger.Threshold = Severity.Error;
            const int startidx = 18;
            List<string> paths = new List<string>();
            for (int i = startidx; i < startidx+5 ; i++) {
                string path = wd.Combine("HT" + (i+1).ToString());
                paths.Add(path);
                var rc = CalculateOneHousetype(path, i);
                List<RowCollection> rcs = new List<RowCollection>();
                rcs.Add(rc);
                XlsxDumper.WriteToXlsx(wd.Combine("results.HT"+ (i+1)+ ".xlsx"), rcs.ToArray());
                CheckHouseResults(path);
            }
            //      wd1.CleanUp();
        }

        private static void CheckHouseResults([NotNull] string path)
        {
            var srls = new SqlResultLoggingService(path);
            TotalsPerDeviceLogger tpdl = new TotalsPerDeviceLogger(srls);
            var devices = tpdl.Read(Constants.HouseKey);

            TotalsPerLoadtypeEntryLogger tplt = new TotalsPerLoadtypeEntryLogger(srls);
            var totalEntries = tplt.Read(Constants.HouseKey);

            foreach (var device in devices)
            {
                if (device.Device.DeviceType == OefcDeviceType.Storage)
                {
                    device.PositiveValues.Should().BeApproximately(device.NegativeValues * -1, 20);
                    device.Loadtype.Name.Should().Be("Space Heating");
                }
            }
            /*
            foreach (var loadtypeEntry in totalEntries)
            {
                if (loadtypeEntry.Loadtype.Name == "Space Heating")
                {
                    loadtypeEntry.Value.Should().BeApproximately(0, 50, "space heating should be balanced");
                }
            }*/
        }

        [NotNull]
        private static RowCollection CalculateOneHousetype([NotNull] string path, int housetype)
        {
            if (Directory.Exists(path)) {
                try {
                    Directory.Delete(path, true);
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }

            Directory.CreateDirectory(path);
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.OnlyOverallSum);
            sim.MyGeneralConfig.Enable(CalcOption.DeviceProfiles);
            sim.MyGeneralConfig.Enable(CalcOption.TotalsPerDevice);
            sim.MyGeneralConfig.Enable(CalcOption.TotalsPerLoadtype);
            sim.MyGeneralConfig.Enable(CalcOption.EnergyStorageFile);
            sim.MyGeneralConfig.Enable(CalcOption.VariableLogFile);
            sim.MyGeneralConfig.CSVCharacter = ";";
            sim.MyGeneralConfig.ShowSettlingPeriodBool = true;
            Assert.AreNotEqual(null, sim);

            var cmf = new CalcManagerFactory();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var house = sim.Houses.CreateNewItem(sim.ConnectionString);
            house.GeographicLocation = sim.GeographicLocations[0];
            house.TemperatureProfile = sim.TemperatureProfiles[0];
            house.HouseType = sim.HouseTypes[housetype];
            house.Name = "housetest for " + house.HouseType.Name;
            house.SaveToDB();
            while (house.Households.Count > 0) {
                house.Households[0].DeleteFromDB();
            }

            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0],
                house,
                EnergyIntensityType.Random,
                false,
                version,
                null,
                LoadTypePriority.All,
                null,
                null,
                null,
                sim.MyGeneralConfig.AllEnabledOptions(),
                new DateTime(2015, 1, 1),
                new DateTime(2015, 12, 31),
                new TimeSpan(0, 1, 0),
                ";",
                5,
                new TimeSpan(0, 15, 0),
                false,
                false,
                false,
                3,
                5,
                calculationProfiler);
            var cm = cmf.GetCalcManager(sim, path, csps, false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            if (house.Households.Count > 0) {
                throw new LPGException("households detected");
            }

            cm.Run(ReportCancelFunc);
            db.Cleanup();
            if (house.Households.Count > 0) {
                throw new LPGException("households detected");
            }

            var srls = new SqlResultLoggingService(path);
            TotalsPerDeviceLogger tpdl = new TotalsPerDeviceLogger(srls);
            var devices = tpdl.Read(Constants.HouseKey);

            TotalsPerLoadtypeEntryLogger tplt = new TotalsPerLoadtypeEntryLogger(srls);
            var totalEntries = tplt.Read(Constants.HouseKey);


            if (house.HouseType == null) { throw new LPGException("housetype was null");}
            var rc = new RowCollection(house.HouseType.HouseTypeCode);
            var descrb= XlsRowBuilder.Start("Housetype", house.HouseType?.Name);
            rc.Add(descrb);
            foreach (var device in house.HouseType?.HouseTransformationDevices) {
                var traforb = XlsRowBuilder.Start("Transformation device", device.TransformationDevice?.Name);
                rc.Add(traforb);
            }
            foreach (var device in house.HouseType?.HouseEnergyStorages)
            {
                var energyrb = XlsRowBuilder.Start("Energy Storage", device.EnergyStorage?.Name);
                rc.Add(energyrb);
            }

            foreach (var entry in totalEntries) {
                var ltEntry = XlsRowBuilder.Start("Loadtype", entry.Loadtype.Name).Add("Value", entry.Value);
                rc.Add(ltEntry);
            }

            foreach (var device in devices) {
                var rb = XlsRowBuilder.Start("Device-"+device.Loadtype.Name, device.Device.Name);
                rb.Add("Value Positive "+ device.Loadtype.Name, device.PositiveValues);
                rb.Add("Value Negative " + device.Loadtype.Name, device.NegativeValues);
                rc.Add(rb);
            }
            return rc;
        }


        [Test]
        [Category(UnitTestCategories.LongTest5)]
        public void AllHouseholdFactoryTest()
        {
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
            //sim.MyGeneralConfig.Enable(CalcOption.LogAllMessages);
            Assert.AreNotEqual(null, sim);
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            SimIntegrityChecker.Run(sim);
            CalcManagerFactory.DoIntegrityRun = false;
            for (var i = 0; i < sim.ModularHouseholds.It.Count; i++) {
                if (sim.ModularHouseholds.It[i].CreationType != CreationType.ManuallyCreated) {
                    continue;
                }
                var cmf = new CalcManagerFactory();
                var di = new DirectoryInfo(wd1.WorkingDirectory);
                var files = di.GetFiles();
                foreach (var file in files) {
                    file.Delete();
                }
                var dirs = di.GetDirectories();
                foreach (var dir in dirs) {
                    dir.Delete(true);
                }
                CalculationProfiler calculationProfiler = new CalculationProfiler();
                CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                    sim.TemperatureProfiles[0], sim.ModularHouseholds[i], EnergyIntensityType.Random,
                    false, version, null, LoadTypePriority.RecommendedForHouses, null,null, null,
                    sim.MyGeneralConfig.AllEnabledOptions(), new DateTime(2015, 1, 1), new DateTime(2015, 1, 2), new TimeSpan(0, 1, 0), ";", 5,new TimeSpan(0,15,0),false,false,false, 3,5
                    ,calculationProfiler
                    );
                var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

                bool ReportCancelFunc()
                {
                    Logger.Info("canceled");
                    return true;
                }

                CalcManager.ExitCalcFunction = true;
                cm.Run(ReportCancelFunc);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            db.Cleanup();
            wd1.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }

        [Test]
        [Category(UnitTestCategories.LongTest3)]
        public void AllHousesFactoryTest()
        {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString) {MyGeneralConfig = {ExternalTimeResolution = "00:15:00"}};
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
            sim.MyGeneralConfig.Enable(CalcOption.TotalsPerDevice);
            Assert.AreNotEqual(null, sim);
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            SimIntegrityChecker.Run(sim);
            CalcManagerFactory.DoIntegrityRun = false;
            for (var i = 0; i < sim.Houses.It.Count; i++) {
                if (sim.Houses.It[i].CreationType != CreationType.ManuallyCreated) {
                    continue;
                }
                var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass() + "_" + i);
                CalculationProfiler calculationProfiler = new CalculationProfiler();
                CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                    sim.TemperatureProfiles[0], sim.Houses[i], EnergyIntensityType.Random,
                    false, version, null, LoadTypePriority.All, null,null, null,
                        sim.MyGeneralConfig.AllEnabledOptions(), new DateTime(2015, 1, 1), new DateTime(2015, 1, 2), new TimeSpan(0, 1, 0), ";", 5,
                    new TimeSpan(0,15,0),false,false,false, 3,3,
                    calculationProfiler
                    );
                var cmf = new CalcManagerFactory();
                var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

                bool ReportCancelFunc()
                {
                    Logger.Info("canceled");
                    return true;
                }

                CalcManager.ExitCalcFunction = false;
                cm.Run(ReportCancelFunc);
                cm.Logfile.Close();
                Thread.Sleep(500);
                wd1.CleanUp();
            }
            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void OverallSumFileTest()
        {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString) {MyGeneralConfig = {ExternalTimeResolution = "00:15:00"}};
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
            sim.MyGeneralConfig.Enable(CalcOption.OverallSum);
            sim.MyGeneralConfig.Enable(CalcOption.IndividualSumProfiles);
            sim.MyGeneralConfig.Enable(CalcOption.SumProfileExternalEntireHouse);
            Assert.AreNotEqual(null, sim);
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            SimIntegrityChecker.Run(sim);
            CalcManagerFactory.DoIntegrityRun = false;
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], sim.Houses[1], EnergyIntensityType.Random,
                false, version, null, LoadTypePriority.All, null,null, null,
                sim.MyGeneralConfig.AllEnabledOptions(), new DateTime(2015, 1, 1),
                new DateTime(2015, 1, 3),
                new TimeSpan(0, 1, 0), ";", 5,
                new TimeSpan(0, 15, 0), false, false, false, 3, 3,
                calculationProfiler
            );
            var cmf = new CalcManagerFactory();
            var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            CalcManager.ExitCalcFunction = false;
            cm.Run(ReportCancelFunc);
            cm.Logfile.Close();
            DirectoryInfo di = new DirectoryInfo(wd1.Combine("Results"));
            var files = di.GetFiles("*.csv");
            foreach (var file in files) {
                using (StreamReader sr = new StreamReader(file.FullName)) {
                    sr.ReadLine(); //header
                    string firstLine = sr.ReadLine();


                    Debug.Assert(firstLine != null, nameof(firstLine) + " != null");
                    var arr = firstLine.Split(';');
                    int firstnumber = Convert.ToInt32(arr[0]);
                    if (firstnumber != 0) {
                        throw new LPGException("negative numbers in first line in file: " + file.Name);
                    }
                }
            }
            Thread.Sleep(500);
              //  wd1.CleanUp();
                db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void CalcOptionTester()
        {
            // superslow test, only do manually

            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Logger.Threshold = Severity.Error;
            var path = wd1.WorkingDirectory;
            Config.MakePDFCharts = false;
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString) {MyGeneralConfig = {RandomSeed = 5}};

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }
            foreach (CalcOption option in Enum.GetValues(typeof(CalcOption))) {
                Logger.Info("Testing option " + option);
                var calcstart = DateTime.Now;
                sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
                sim.MyGeneralConfig.Enable(option);
                Assert.AreNotEqual(null, sim);

                var cmf = new CalcManagerFactory();
                var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                //CalcDevice.UseRanges = true;
                var geoloc = sim.GeographicLocations.FindFirstByName("Chemnitz", FindMode.Partial);
                if (geoloc == null) {
                    throw new LPGException("geoloc not found");
                }
                var chh = sim.ModularHouseholds.It[0];
                CalculationProfiler calculationProfiler = new CalculationProfiler();
                CalcStartParameterSet csps = new CalcStartParameterSet( geoloc,
                    sim.TemperatureProfiles[0], chh, EnergyIntensityType.Random,
                    false, version, null, LoadTypePriority.Mandatory, null,null, null,
                sim.MyGeneralConfig.AllEnabledOptions(), new DateTime(2015, 1, 1), new DateTime(2015, 1, 3), new TimeSpan(0, 1, 0), ";", 5, new TimeSpan(0,1,0),
                    false,false,false, 3,3,
                    calculationProfiler);
                var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

                cm.Run(ReportCancelFunc);
                Logger.Error("Calc Duration:" + (DateTime.Now - calcstart).TotalSeconds + " seconds");
                cm.CloseLogfile();
                wd1.ClearDirectory();
            }
            db.Cleanup();
            wd1.CleanUp();
        }

        [Test]
        [Category(UnitTestCategories.LongTest3)]
        public void CalculationBenchmarksBasicTest()
        {
            CleanTestBase.RunAutomatically(false);
            var start = DateTime.Now;
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var path = wd1.WorkingDirectory;
            Config.MakePDFCharts = false;
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            var calcstart = DateTime.Now;
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.All);
            //sim.MyGeneralConfig.Enable(CalcOption.AffordanceEnergyUse);
            sim.MyGeneralConfig.Disable(CalcOption.OverallSum);
            SimIntegrityChecker.Run(sim);
            //ConfigSetter.SetGlobalTimeParameters(sim.MyGeneralConfig);
            Assert.AreNotEqual(null, sim);

            var cmf = new CalcManagerFactory();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //CalcDevice.UseRanges = true;
            var geoloc = sim.GeographicLocations.FindFirstByName("Chemnitz", FindMode.Partial);
            if (geoloc == null) {
                throw new LPGException("Geoloc was null");
            }
            var chh =
                sim.ModularHouseholds.It.First(x => x.Name.StartsWith("CHR20", StringComparison.Ordinal));
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(geoloc,
                sim.TemperatureProfiles[0], chh, EnergyIntensityType.Random,
                false, version, null, LoadTypePriority.All, null,null, null, sim.MyGeneralConfig.AllEnabledOptions(),
                new DateTime(2015,1,1), new DateTime(2015,1,3),new TimeSpan(0,1,0),";",5, new TimeSpan(0, 1, 0),false,false,false,3,3,
                calculationProfiler);
            var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            cm.Run(ReportCancelFunc);
            db.Cleanup();
            Logger.ImportantInfo("Duration:" + (DateTime.Now - start).TotalSeconds + " seconds");
            Logger.ImportantInfo("Calc Duration:" + (DateTime.Now - calcstart).TotalSeconds + " seconds");
            Logger.ImportantInfo("loading Duration:" + (calcstart - start).TotalSeconds + " seconds");
            //wd1.CleanUp();
            //CleanTestBase.Run(true);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void SimpleLoading()
        {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            Assert.IsNotNull(sim);
            Console.WriteLine("Guids created: " + DBBase._GuidCreationCount);
            if(DBBase._GuidCreationCount > 0) {
                throw new LPGException("Guids were created, whatever the reason might be.");
            }

            DBBase._GuidCreationCount = 0;
            DBBase.GuidsToSave.Clear();
            var sim2 = new Simulator(db.ConnectionString);
            Assert.IsNotNull(sim2);
            Console.WriteLine("Guids created try 2: " + DBBase._GuidCreationCount);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CalculationBenchmarksBasicTestHouse()
        {
            //CleanTestBase.Run(false);
            var start = DateTime.Now;
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Logger.Threshold = Severity.Error;
            var path = wd1.WorkingDirectory;
            Config.MakePDFCharts = true;
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            //const string connstr = "Data Source=d:\\profilegenerator-r8.db3";
            var sim = new Simulator(db.ConnectionString);
            var calcstart = DateTime.Now;

            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
            sim.MyGeneralConfig.Disable(CalcOption.OverallSum);
            sim.MyGeneralConfig.Enable(CalcOption.IndividualSumProfiles);
            sim.MyGeneralConfig.Enable(CalcOption.MakeGraphics);
            sim.MyGeneralConfig.Enable(CalcOption.DeviceProfiles);

            //ChartLocalizer.ShouldTranslate = true;
            Assert.AreNotEqual(null, sim);
            var cmf = new CalcManagerFactory();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //CalcDevice.UseRanges = true;
            House houseToCalc = sim.Houses.It[0];
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], houseToCalc, EnergyIntensityType.Random,
                false, version, null, LoadTypePriority.RecommendedForHouses, null,null, null,
                sim.MyGeneralConfig.AllEnabledOptions(), new DateTime(2015,1,1),new DateTime(2015,1,3),
                new TimeSpan(0,1,0),";",5,new TimeSpan(0,1,0), false, false, false,3,3,
                calculationProfiler);
            var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            cm.Run(ReportCancelFunc);
            //db.Cleanup();
            Logger.ImportantInfo("Duration:" + (DateTime.Now - start).TotalSeconds + " seconds");
            Logger.ImportantInfo("Calc Duration:" + (DateTime.Now - calcstart).TotalSeconds + " seconds");
            wd1.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }

        [Test]
        [Category(UnitTestCategories.LongTest3)]
        public void CheckTimeAxisTests()
        {
            //_calcParameters.IsInTransportMode = false;
            CleanTestBase.RunAutomatically(false);
            var start = DateTime.Now;
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Logger.Threshold = Severity.Error;
            var path = wd1.WorkingDirectory;
            Config.MakePDFCharts = false;
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            var calcstart = DateTime.Now;

            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.ReasonableWithChartsAndPDF);

            Assert.AreNotEqual(null, sim);

            var cmf = new CalcManagerFactory();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //CalcDevice.UseRanges = true;
            var geoloc = sim.GeographicLocations.FindFirstByName("Chemnitz", FindMode.Partial);
            if (geoloc == null) {
                throw new LPGException("Geoloc not found.");
            }
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], sim.ModularHouseholds[0], EnergyIntensityType.Random,
                false, version, null, LoadTypePriority.Mandatory, null, null,null, sim.MyGeneralConfig.AllEnabledOptions(),
                new DateTime(2015,1,1),new DateTime(2015,1,5),new TimeSpan(0,1,0),";",5,new TimeSpan(0,1,0), false, false, false,3,3,
                calculationProfiler);
            var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            cm.Run(ReportCancelFunc);
            db.Cleanup();
            Logger.ImportantInfo("Duration:" + (DateTime.Now - start).TotalSeconds + " seconds");
            Logger.ImportantInfo("Calc Duration:" + (DateTime.Now - calcstart).TotalSeconds + " seconds");
            Logger.ImportantInfo("loading Duration:" + (calcstart - start).TotalSeconds + " seconds");
            wd1.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void DuplicateTotalsEntriesTest()
        {
            //_calcParameters.IsInTransportMode = false;
            CleanTestBase.RunAutomatically(false);
            var start = DateTime.Now;
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Logger.Threshold = Severity.Error;
            var path = wd1.WorkingDirectory;
            Config.MakePDFCharts = true;
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            var calcstart = DateTime.Now;

            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
            sim.MyGeneralConfig.Enable(CalcOption.TotalsPerLoadtype);
            //sim.MyGeneralConfig.Enable(CalcOption.ActivationFrequencies);
            //ChartLocalizer.ShouldTranslate = true;
            Assert.AreNotEqual(null, sim);
            var cmf = new CalcManagerFactory();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //CalcDevice.UseRanges = true;
            var house = sim.Houses.FindFirstByName("01, 02", FindMode.Partial);
            if (house == null) {
                throw new LPGException("House was null");
            }

            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], house, EnergyIntensityType.Random,
                false, version, null, LoadTypePriority.RecommendedForHouses, null,null, null,
                sim.MyGeneralConfig.AllEnabledOptions(),new DateTime(2015,1,1),new DateTime(2015,1,3),new TimeSpan(0,0,1,0),
                ";",5,new TimeSpan(0,1,0), false, false, false,3,3,
                calculationProfiler);
            var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            var cr = cm.Run(ReportCancelFunc);
            if (!cr) {
                throw new LPGException("Calculation failed");
            }
            /*
            if (cr.ResultFileEntries.Count == 0) {
                throw new LPGException("no result files");
            }
            foreach (var entry in cr.ResultFileEntries) {
                Logger.Info("Result File: " + entry.FullFileName);
            }*/
            db.Cleanup();
            Logger.ImportantInfo("Duration:" + (DateTime.Now - start).TotalSeconds + " seconds");
            Logger.ImportantInfo("Calc Duration:" + (DateTime.Now - calcstart).TotalSeconds + " seconds");
            TotalsPerLoadtypeEntryLogger tel = new TotalsPerLoadtypeEntryLogger(wd1.SqlResultLoggingService);
            var keyLogger = new HouseholdKeyLogger(wd1.SqlResultLoggingService);
            var keys = keyLogger.Load();
            foreach (HouseholdKeyEntry entry in keys) {
                if(entry.HouseholdKey == Constants.GeneralHouseholdKey) {
                    continue;
                }
                if (entry.KeyType == HouseholdKeyType.House) {
                    continue;
                }
                var totals = tel.Read(entry.HouseholdKey);
                Assert.That(totals.Count,Is.GreaterThan(0));
            }

            /*
             var di = new DirectoryInfo(wd1.WorkingDirectory);
             var fis = di.GetFiles("TotalsPerLoadtype.csv", SearchOption.AllDirectories);
            if (fis.Length != 1) {
                throw new LPGException("Missing totals file");
            }*/
            /*var loadtypes = new List<string>();
            using (var sr = new StreamReader(fis[0].FullName)) {
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (!string.IsNullOrWhiteSpace(s)) {
                        var arr = s.Split(';');
                        if (loadtypes.Contains(arr[0])) {
                            throw new LPGException("Loadtype duplicated in the file: " + arr[0]);
                        }
                        loadtypes.Add(arr[0]);
                    }
                }
                sr.Close();
            }*/
            // wd1.CleanUp();
            //CleanTestBase.Run(true);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void EnergyCarpetPlotTest()
        {
            //_calcParameters.IsInTransportMode = false;
            CleanTestBase.RunAutomatically(false);
            var start = DateTime.Now;
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Logger.Threshold = Severity.Error;
            var path = wd1.WorkingDirectory;
            Config.MakePDFCharts = true;
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
            sim.MyGeneralConfig.Enable(CalcOption.AffordanceEnergyUse);
            sim.MyGeneralConfig.Enable(CalcOption.EnergyCarpetPlot);
            sim.MyGeneralConfig.Enable(CalcOption.ActivationFrequencies);
            //ChartLocalizer.ShouldTranslate = true;
            Assert.AreNotEqual(null, sim);

            var cmf = new CalcManagerFactory();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], sim.ModularHouseholds[0], EnergyIntensityType.Random,
                false, version, null, LoadTypePriority.RecommendedForHouses, null,null, null,
                sim.MyGeneralConfig.AllEnabledOptions(), new DateTime(2015,1,1),new DateTime(2015,1,10),
                new TimeSpan(0,1,0),";",5,new TimeSpan(0,1,0), false, false, false,3,3,
                calculationProfiler);

            var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            cm.Run(ReportCancelFunc);
            db.Cleanup();
            Logger.ImportantInfo("Duration:" + (DateTime.Now - start).TotalSeconds + " seconds");
            var imagefiles = FileFinder.GetRecursiveFiles(new DirectoryInfo(wd1.WorkingDirectory),
                "EnergyCarpetplot.*.png");
            Assert.GreaterOrEqual(imagefiles.Count, 1);
            wd1.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }

        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void InvalidPersonActivitiesCheck()
        {
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString) {
                MyGeneralConfig = {
                    StartDateUIString = "01.01.2015",
                    EndDateUIString = "02.01.2015",
                    InternalTimeResolution = "00:01:00",
                    ExternalTimeResolution = "00:15:00",
                    RandomSeed = 5
                }
            };
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
            sim.MyGeneralConfig.Enable(CalcOption.TotalsPerLoadtype);
            sim.MyGeneralConfig.CSVCharacter = ";";
            Assert.AreNotEqual(null, sim);
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            SimIntegrityChecker.Run(sim);
            CalcManagerFactory.DoIntegrityRun = false;
            var mhh = sim.ModularHouseholds.FindFirstByName("x CHR08 Single woman, 2 children, with work 47");
            if (mhh == null) {
                throw new LPGException("Household not found");
            }
            var cmf = new CalcManagerFactory();
            var di = new DirectoryInfo(wd1.WorkingDirectory);
            var files = di.GetFiles();
            foreach (var file in files) {
                file.Delete();
            }
            var dirs = di.GetDirectories();
            foreach (var dir in dirs) {
                dir.Delete(true);
            }
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet( sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], mhh, EnergyIntensityType.Random,
                false, version, null, LoadTypePriority.RecommendedForHouses, null, null,null,
                sim.MyGeneralConfig.AllEnabledOptions(),new DateTime(2015,1,1),new DateTime(2015,1,2),
                new TimeSpan(0,1,0),";",5,new TimeSpan(0,15,0), false, false, false,3,3,
                calculationProfiler);
            var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            CalcManager.ExitCalcFunction = true;
            cm.Run(ReportCancelFunc);
            db.Cleanup();
            wd1.CleanUp();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void DoubleWorkCheck()
        {
            //_calcParameters.IsInTransportMode = false;
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString) {
                MyGeneralConfig = {
                    StartDateUIString = "01.01.2015",
                    EndDateUIString = "02.01.2015",
                    InternalTimeResolution = "00:01:00",
                    ExternalTimeResolution = "00:15:00",
                    RandomSeed = 5
                }
            };
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
            sim.MyGeneralConfig.Enable(CalcOption.TotalsPerLoadtype);
            sim.MyGeneralConfig.CSVCharacter = ";";

            Assert.AreNotEqual(null, sim);
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            SimIntegrityChecker.Run(sim);
            CalcManagerFactory.DoIntegrityRun = false;
            var mhh = sim.Houses[27];
            if (mhh == null)
            {
                throw new LPGException("Household not found");
            }
            var cmf = new CalcManagerFactory();
            var di = new DirectoryInfo(wd1.WorkingDirectory);
            var files = di.GetFiles();
            foreach (var file in files)
            {
                file.Delete();
            }
            var dirs = di.GetDirectories();
            foreach (var dir in dirs)
            {
                dir.Delete(true);
            }
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet( sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], mhh, EnergyIntensityType.Random,
                false, version, null, LoadTypePriority.RecommendedForHouses, null, null,null,
            sim.MyGeneralConfig.AllEnabledOptions(), new DateTime(2015, 1, 1), new DateTime(2015, 1, 2),
                new TimeSpan(0, 1, 0), ";", 5, new TimeSpan(0, 15, 0), false, false, false,3,3,
                calculationProfiler);
            var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            CalcManager.ExitCalcFunction = false;
            cm.Run(ReportCancelFunc);
            db.Cleanup();
            wd1.CleanUp();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void MissingTotalsEntriesTest()
        {
            //_calcParameters.IsInTransportMode = false;
            CleanTestBase.RunAutomatically(false);
            var start = DateTime.Now;
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Logger.Threshold = Severity.Error;
            var path = wd1.WorkingDirectory;
            Config.MakePDFCharts = true;
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            var calcstart = DateTime.Now;
            sim.MyGeneralConfig.StartDateUIString = "1.1.2015";
            sim.MyGeneralConfig.EndDateUIString = "3.1.2015";
            sim.MyGeneralConfig.InternalTimeResolution = "00:01:00";
            sim.MyGeneralConfig.RandomSeed = 5;
            sim.MyGeneralConfig.CSVCharacter = ";";

            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
            sim.MyGeneralConfig.Enable(CalcOption.TotalsPerLoadtype);
            sim.MyGeneralConfig.Enable(CalcOption.DeviceProfiles);
            //sim.MyGeneralConfig.Enable(CalcOption.ActivationFrequencies);
            //ChartLocalizer.ShouldTranslate = true;

            Assert.AreNotEqual(null, sim);
            var cmf = new CalcManagerFactory();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //CalcDevice.UseRanges = true;
            var house = sim.Houses.FindFirstByName("01, 02", FindMode.Partial);
            if (house == null) {
                throw new LPGException("House was null");
            }
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], house, EnergyIntensityType.Random,
                false, version, null, LoadTypePriority.RecommendedForHouses, null, null,null,
                sim.MyGeneralConfig.AllEnabledOptions(), new DateTime(2015, 1, 1), new DateTime(2015, 1, 3),
                new TimeSpan(0, 1, 0), ";", 5, new TimeSpan(0, 1, 0), false, false, false,3,3,
                calculationProfiler);
            var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }

            cm.Run(ReportCancelFunc);
            db.Cleanup();
            Logger.ImportantInfo("Duration:" + (DateTime.Now - start).TotalSeconds + " seconds");
            Logger.ImportantInfo("Calc Duration:" + (DateTime.Now - calcstart).TotalSeconds + " seconds");
            //var di = new DirectoryInfo(wd1.WorkingDirectory);
            //var ti = TotalsInformation.Read(Path.Combine(di.FullName, "Reports"));
            HouseholdKeyLogger hhkls = new HouseholdKeyLogger(wd1.SqlResultLoggingService);
            var hhkeys = hhkls.Load();
            TotalsPerLoadtypeEntryLogger tel = new TotalsPerLoadtypeEntryLogger(wd1.SqlResultLoggingService);
            foreach (HouseholdKeyEntry entry in hhkeys) {
                if(entry.KeyType == HouseholdKeyType.General) {
                    continue;
                }
                if (entry.KeyType == HouseholdKeyType.House)
                {
                    continue;
                }

                var ti = tel.Read(entry.HouseholdKey);
                //var totalEntries = ti.First(x => x.HouseholdKey == Constants.TotalsKey);
                //var others = ti.HouseholdEntries.Where(x => x.HouseholdKey != Constants.TotalsKey).ToList();
                Assert.That(ti.Count,Is.GreaterThan(0));
                //TODO: fix: implement a proper check of the sums vs. the profile sums
                /*foreach (var totalEntriesLoadTypeEntry in totalEntries.LoadTypeEntries) {
                    var totalSum = totalEntriesLoadTypeEntry.Total;
                    var otherEntries =
                        others.SelectMany(x => x.LoadTypeEntries)
                            .Where(y => y.LoadTypeInformation.Name ==
                                        totalEntriesLoadTypeEntry.LoadTypeInformation.Name)
                            .ToList();
                    var othersum = otherEntries.Select(x => x.Total).Sum();
                    if (Math.Abs(totalSum - othersum) > 0.00001) {
                        throw new LPGException(
                            "Something is wrong: " + totalEntriesLoadTypeEntry.LoadTypeInformation.Name
                                                   + Environment.NewLine + "total: " + totalSum + Environment.NewLine +
                                                   " individual sum:" + othersum);
                    }
                }*/
            }
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CalculationTransportationTest()
        {
            CleanTestBase.RunAutomatically(false);
            var start = DateTime.Now;
            Config.MakePDFCharts = false;
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            var calcstart = DateTime.Now;
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.None);
            sim.MyGeneralConfig.Enable(CalcOption.OverallSum);
            sim.MyGeneralConfig.Enable(CalcOption.HouseholdContents);
            //sim.MyGeneralConfig.Enable(CalcOption.LocationCarpetPlot);
            //sim.MyGeneralConfig.Enable(CalcOption.PersonStatus);
            sim.MyGeneralConfig.Enable(CalcOption.ActionCarpetPlot);
            sim.MyGeneralConfig.Enable(CalcOption.TransportationDeviceCarpetPlot);
            sim.MyGeneralConfig.Enable(CalcOption.LogAllMessages);
            sim.MyGeneralConfig.Enable(CalcOption.LogErrorMessages);
            sim.MyGeneralConfig.Enable(CalcOption.DeviceProfileExternalEntireHouse);
            sim.MyGeneralConfig.Enable(CalcOption.ActivationFrequencies);
            sim.MyGeneralConfig.Enable(CalcOption.TransportationStatistics);
            sim.MyGeneralConfig.Enable(CalcOption.DeviceProfiles);
            //sim.MyGeneralConfig.Enable(CalcOption.per);
            SimIntegrityChecker.Run(sim);
            Assert.AreNotEqual(null, sim);
            int count = 0;
            for (var index = 0; index < sim.ModularHouseholds.It.Count && index < 5; index++) {
                var modularHousehold = sim.ModularHouseholds.It[index];
                count++;
                Logger.Info("Processing household " + count);
                //              if(count<490) {//for skipping the first 50 for debugging
                //                continue;
                //          }
                var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
                var cmf = new CalcManagerFactory();
                var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                //CalcDevice.UseRanges = true;
                var geoloc = sim.GeographicLocations.FindFirstByName("Chemnitz", FindMode.Partial);
                if (geoloc == null) {
                    throw new LPGException("Geoloc was null");
                }

                //var chh =sim.ModularHouseholds.It.First(x => x.Name.StartsWith("CHR07", StringComparison.Ordinal));

                //TransportationDeviceSet transportationDeviceSet = null;
                //TravelRouteSet travelRouteSet = null;
                TransportationDeviceSet transportationDeviceSet = sim.TransportationDeviceSets.It[0];
                TravelRouteSet travelRouteSet = sim.TravelRouteSets[0];
                ChargingStationSet chargingStationSet = sim.ChargingStationSets[0];
                CalculationProfiler calculationProfiler = new CalculationProfiler();
                CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                    sim.TemperatureProfiles[0], modularHousehold, EnergyIntensityType.Random,
                    false, version, null, LoadTypePriority.All,
                    transportationDeviceSet,chargingStationSet, travelRouteSet,
                    sim.MyGeneralConfig.AllEnabledOptions(),
                    new DateTime(2015, 1, 1),
                    new DateTime(2015, 1, 2),
                    new TimeSpan(0, 1, 0), ";", 5,
                    new TimeSpan(0, 1, 0),
                    false, false, false, 3, 3,
                    calculationProfiler);
                var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

                //var cm = cmf.GetCalcManager(sim, path, chh, false, sim.TemperatureProfiles[0], geoloc,
                //EnergyIntensityType.Random, version, LoadTypePriority.All, null, transportationDeviceSet, travelRouteSet);
/*
                bool ReportCancelFunc()
                {
                    Logger.Info("canceled");
                    return true;
                }
                cm.Run(ReportCancelFunc);*/
                //db.Cleanup();
                cm.CloseLogfile();
                wd1.CleanUp();
            }

            Logger.ImportantInfo("Duration:" + (DateTime.Now - start).TotalSeconds + " seconds");
            Logger.ImportantInfo("Calc Duration:" + (DateTime.Now - calcstart).TotalSeconds + " seconds");
            Logger.ImportantInfo("loading Duration:" + (calcstart - start).TotalSeconds + " seconds");

           // CleanTestBase.Run(true);
        }
    }
}

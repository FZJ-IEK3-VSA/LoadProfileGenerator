﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.Queue;
using Common;
using Common.Tests;
using Database;
using Database.Tables;
using Database.Tests;
using JetBrains.Annotations;
using NUnit.Framework;

namespace CalculationController.Tests
{
    [TestFixture]
    public class PostProcessingTests : UnitTestBaseClass
    {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [NotNull]
        private static string GetCurrentMethod() {
            var st = new StackTrace();
            var sf = st.GetFrame(1);
            return sf.GetMethod().Name;
        }

        private static void RunTest([NotNull] Action<GeneralConfig> setOption, [NotNull] string name) {
            CleanTestBase.RunAutomatically(false);
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass() + name);
            Logger.Threshold = Severity.Error;
            var path = wd1.WorkingDirectory;

            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            Config.IsInUnitTesting = true;
            Config.ExtraUnitTestChecking = false;
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.NoFiles);
            sim.MyGeneralConfig.WriteExcelColumn = "False";
            if (setOption == null) {
                throw new LPGException("Action was null.");
            }
            setOption(sim.MyGeneralConfig);

            Logger.Info("Temperature:" + sim.MyGeneralConfig.SelectedTemperatureProfile);
            Logger.Info("Geographic:" + sim.MyGeneralConfig.GeographicLocation);
            Assert.AreNotEqual(null, sim);
            var cmf = new CalcManagerFactory();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], sim.ModularHouseholds[0], EnergyIntensityType.Random,
                false, version, null, LoadTypePriority.All, null,null, null,
                sim.MyGeneralConfig.AllEnabledOptions(),new DateTime(2018,1,1),new DateTime(2018,1,2),new TimeSpan(0,1,0),
                ";",5,new TimeSpan(0,10,0),false,false,false,3,3,
                calculationProfiler);
            var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps,  false);

            bool ReportCancelFunc()
            {
                Logger.Info("canceled");
                return true;
            }
            cm.Run(ReportCancelFunc);
            db.Cleanup();
            wd1.CleanUp();
            CleanTestBase.RunAutomatically(true);
        }

        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void PostProcessingTestCompareAllResultFiles() {
            //base.SkipEndCleaning = true;
            CleanTestBase.RunAutomatically(false);
            Config.ReallyMakeAllFilesIncludingBothSums = true;
            var start = DateTime.Now;
            var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Logger.Threshold = Severity.Error;
            var path = wd1.WorkingDirectory;

            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            Config.IsInUnitTesting = true;
            Config.ExtraUnitTestChecking = true;
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.NoFiles);
            sim.MyGeneralConfig.WriteExcelColumn = "False";
            sim.MyGeneralConfig.Enable(CalcOption.DeviceProfiles);
            sim.MyGeneralConfig.Enable(CalcOption.IndividualSumProfiles);
            sim.MyGeneralConfig.Enable(CalcOption.DeviceProfileExternalEntireHouse);
            sim.MyGeneralConfig.Enable(CalcOption.SumProfileExternalEntireHouse);
            sim.MyGeneralConfig.Enable(CalcOption.SumProfileExternalIndividualHouseholds);
            sim.MyGeneralConfig.Enable(CalcOption.PolysunImportFiles);
            sim.MyGeneralConfig.Enable(CalcOption.OverallSum);
            Logger.Info("Temperature:" + sim.MyGeneralConfig.SelectedTemperatureProfile);
            Logger.Info("Geographic:" + sim.MyGeneralConfig.GeographicLocation);
            Assert.AreNotEqual(null, sim);
            var cmf = new CalcManagerFactory();
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], sim.ModularHouseholds[0], EnergyIntensityType.Random,
                false, version, null, LoadTypePriority.All, null, null,null, sim.MyGeneralConfig.AllEnabledOptions(),
                new DateTime(2013,1,1),new DateTime(2013,1,2),new TimeSpan(0,1,0),";",5,new TimeSpan(0,10,0),false,false,false,3,3  ,
                calculationProfiler);
            var cm = cmf.GetCalcManager(sim, wd1.WorkingDirectory, csps, false);
            cm.Run(ReportCancelFunc);
            Logger.ImportantInfo("Duration:" + (DateTime.Now - start).TotalSeconds + " seconds");
            var pathdp = Path.Combine(wd1.WorkingDirectory,
                DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results), "DeviceProfiles.Electricity.csv");
            double sumDeviceProfiles = 0;
            using (var sr = new StreamReader(pathdp)) {
                sr.ReadLine(); // header
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("The file " + pathdp + " was broken");
                    }
                    var arr = s.Split(';');

                    for (var i = 2; i < arr.Length; i++) {
                        if (arr[i].Length > 0) {
                            var d = double.Parse(arr[i], CultureInfo.CurrentCulture);
                            sumDeviceProfiles += d;
                        }
                    }
                }
            }
            Logger.Info("SumDeviceProfiles: " + sumDeviceProfiles);
            var pathSumProfiles = Path.Combine(wd1.WorkingDirectory,
                DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results), "SumProfiles.Electricity.csv");
            double sumSumProfiles = 0;
            using (var sr = new StreamReader(pathSumProfiles)) {
                sr.ReadLine(); // header
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s != null) {
                        var arr = s.Split(';');

                        for (var i = 2; i < arr.Length; i++) {
                            if (arr[i].Length > 0) {
                                var d = double.Parse(arr[i], CultureInfo.CurrentCulture);
                                sumSumProfiles += d;
                            }
                        }
                    }
                }
            }
            Assert.That(sumDeviceProfiles, Is.EqualTo(sumSumProfiles).Within(0.1).Percent);
            Logger.Info("sumSumProfiles: " + sumSumProfiles);
            var pathExtSumProfiles = Path.Combine(wd1.WorkingDirectory,
                DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results), "SumProfiles_600s.Electricity.csv");
            double sumExtSumProfiles = 0;
            using (var sr = new StreamReader(pathExtSumProfiles)) {
                sr.ReadLine(); // header
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s != null) {
                        var arr = s.Split(';');

                        for (var i = 2; i < arr.Length; i++) {
                            if (arr[i].Length > 0) {
                                var d = double.Parse(arr[i], CultureInfo.CurrentCulture);
                                sumExtSumProfiles += d;
                            }
                        }
                    }
                }
            }
            Logger.Info("sumExtSumProfiles: " + sumExtSumProfiles);
            Assert.That(sumDeviceProfiles, Is.EqualTo(sumExtSumProfiles).Within(0.1).Percent);

            var pathExtDeviceProfiles = Path.Combine(wd1.WorkingDirectory,
                DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results),
                "DeviceProfiles_600s.Electricity.csv");
            double sumExtDeviceProfiles = 0;
            using (var sr = new StreamReader(pathExtDeviceProfiles)) {
                sr.ReadLine(); // header
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s != null) {
                        var arr = s.Split(';');

                        for (var i = 2; i < arr.Length; i++) {
                            if (arr[i].Length > 0) {
                                var d = double.Parse(arr[i], CultureInfo.CurrentCulture);
                                sumExtDeviceProfiles += d;
                            }
                        }
                    }
                }
            }
            Logger.Info("sumExtDeviceProfiles: " + sumExtDeviceProfiles);
            Assert.That(sumDeviceProfiles, Is.EqualTo(sumExtDeviceProfiles).Within(0.1).Percent);
            var pathImportFile = Path.Combine(wd1.WorkingDirectory,
                DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results), "ImportProfile.900s.Electricity.csv");
            double sumImportFile = 0;
            using (var sr = new StreamReader(pathImportFile)) {
                for (int i = 0; i < 5; i++) {
                    sr.ReadLine(); // header
                }

                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s != null) {
                        var arr = s.Split(';');
                        if (arr.Length > 1 && arr[1].Length > 0) {
                                var d = double.Parse(arr[1], CultureInfo.InvariantCulture);
                                sumImportFile += d;
                        }
                    }
                }
            }
            Logger.Info("sumImportFile: " + sumImportFile);
            var supposedValue = sumDeviceProfiles ; // convert to watt/5 min
            Logger.Info("supposedValue: " + supposedValue);
            Assert.That(supposedValue, Is.EqualTo(sumImportFile).Within(1).Percent);

            var pathExtSumProfileshh1 = Path.Combine(wd1.WorkingDirectory,
                DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results), "SumProfiles_600s.HH1.Electricity.csv");
            double sumExtSumProfilesHH1 = 0;
            using (var sr = new StreamReader(pathExtSumProfileshh1))
            {
                sr.ReadLine(); // header
                while (!sr.EndOfStream)
                {
                    var s = sr.ReadLine();
                    if (s != null)
                    {
                        var arr = s.Split(';');

                        for (var i = 2; i < arr.Length; i++)
                        {
                            if (arr[i].Length > 0)
                            {
                                var d = double.Parse(arr[i], CultureInfo.CurrentCulture);
                                sumExtSumProfilesHH1 += d;
                            }
                        }
                    }
                }
            }
            Logger.Info("sumExtSumProfiles.hh1: " + sumExtSumProfilesHH1);
            Assert.That(sumDeviceProfiles, Is.EqualTo(sumExtSumProfilesHH1).Within(0.1).Percent);

            var pathOverallSum = Path.Combine(wd1.WorkingDirectory,
                DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results), "Overall.SumProfiles.Electricity.csv");
            double overallSum = 0;
            using (var sr = new StreamReader(pathOverallSum))
            {
                sr.ReadLine(); // header
                while (!sr.EndOfStream)
                {
                    var s = sr.ReadLine();
                    if (s != null)
                    {
                        var arr = s.Split(';');

                        for (var i = 2; i < arr.Length; i++)
                        {
                            if (arr[i].Length > 0)
                            {
                                var d = double.Parse(arr[i], CultureInfo.CurrentCulture);
                                overallSum += d;
                            }
                        }
                    }
                }
            }
            Logger.Info("overallSum: " + overallSum);
            Assert.That(sumDeviceProfiles, Is.EqualTo(overallSum).Within(0.1).Percent);

            if (!SkipEndCleaning) {
                wd1.CleanUp(1);
                db.Cleanup();
                CleanTestBase.RunAutomatically(true);
            }
        }

        private static bool ReportCancelFunc()
        {
            Logger.Info("canceled");
            return true;
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RunOnlyDevice() {
            RunTest(x => x.Enable(CalcOption.DeviceProfiles), GetCurrentMethod());
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RunOnlyDeviceProfileExternal() {
            RunTest(x => x.Enable(CalcOption.DeviceProfileExternalEntireHouse), GetCurrentMethod());
        }
    }
}
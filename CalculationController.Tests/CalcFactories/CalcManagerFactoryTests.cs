//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.Queue;
using CalculationEngine.HouseholdElements;
using Common;
using Common.SQLResultLogging.InputLoggers;
using Common.Tests;
using Database;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using Database.Tests;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace CalculationController.Tests.CalcFactories {
    public class CalcManagerFactoryTests:UnitTestBaseClass {
        private static void CalculateOneHousehold([NotNull] string path) {
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            using var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var sim = new Simulator(db.ConnectionString);
            Config.IsInUnitTesting = true;
            sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.Reasonable);
            sim.MyGeneralConfig.Enable(CalcOption.DeviceProfilesIndividualHouseholds);
            sim.MyGeneralConfig.ShowSettlingPeriodBool = true;
            //ConfigSetter.SetGlobalTimeParameters(sim.MyGeneralConfig);

            sim.Should().NotBeNull();
            var cmf = new CalcManagerFactory();
            CalculationProfiler calculationProfiler = new CalculationProfiler();
            //todo: put in a full house with transportation
            //var house = sim.Houses.CreateNewItem()
            CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                sim.TemperatureProfiles[0], sim.ModularHouseholds[0],
                EnergyIntensityType.Random, false,
                 null, LoadTypePriority.Mandatory, null,null, null,sim.MyGeneralConfig.AllEnabledOptions(),
                new DateTime(2015,1,15),
                new DateTime(2015,1,18),
                new TimeSpan(0,1,0),";" ,
                5 , new TimeSpan(0,1,0) ,
                false,false,false,3,
                sim.MyGeneralConfig.RepetitionCount,
                calculationProfiler, path,false);

            var cm = cmf.GetCalcManager(sim,csps,  false);

            bool success = cm.Run(ReportCancelFunc);
            if(!success) {
                throw new LPGException("Calculation failed");
            }

            db.Cleanup();
        }


        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void GetCalcManagerHouseholdTest()
        {
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim = new Simulator(db.ConnectionString);
                    sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.OnlyOverallSum);

                    sim.Should().NotBeNull();
                    sim.MyGeneralConfig.RandomSeed = 10;

                    var cmf = new CalcManagerFactory();
                    CalculationProfiler calculationProfiler = new CalculationProfiler();
                    CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                        sim.TemperatureProfiles[0], sim.ModularHouseholds[0], EnergyIntensityType.Random, false,
                         null, LoadTypePriority.Mandatory, null, null, null, sim.MyGeneralConfig.AllEnabledOptions(), new DateTime(2018, 1, 15),
                        new DateTime(2018, 1, 18), new TimeSpan(0, 1, 0), ";", -1, new TimeSpan(0, 1, 0), false, false, false, 3, 3,
                        calculationProfiler, wd.WorkingDirectory,false);

                    var cm = cmf.GetCalcManager(sim, csps, false);

                    cm.Run(ReportCancelFunc);
                    db.Cleanup();
                }
                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void GetCalcManagerHouseTest()
        {
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim = new Simulator(db.ConnectionString);
                    sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.OnlyOverallSum);

                    //ConfigSetter.SetGlobalTimeParameters(sim.MyGeneralConfig);
                    sim.Should().NotBeNull();

                    var cmf = new CalcManagerFactory();
                    CalculationProfiler calculationProfiler = new CalculationProfiler();
                    CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                        sim.TemperatureProfiles[0], sim.Houses[sim.Houses.MyItems.Count - 1], EnergyIntensityType.Random, false,
                         null, LoadTypePriority.RecommendedForHouses, null, null, null, sim.MyGeneralConfig.AllEnabledOptions(),
                            new DateTime(2015, 1, 15), new DateTime(2015, 1, 18), new TimeSpan(0, 1, 0), ";", -1, new TimeSpan(0, 1, 0), false, false, false, 3, 3,
                        calculationProfiler, wd.WorkingDirectory,false);

                    var cm = cmf.GetCalcManager(sim, csps, false);

                    cm.Run(ReportCancelFunc);
                    db.Cleanup();
                }
                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void GetCalcManagerModularHousehold03Test()
        {
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    Config.IsInUnitTesting = true;
                    var sim = new Simulator(db.ConnectionString);
                    sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.ReasonableWithChartsAndPDF);
                    DeviceCategory light = null;
                    foreach (var deviceCategory in sim.DeviceCategories.MyItems)
                    {
                        deviceCategory.RefreshSubDevices();
                        if (deviceCategory.Name.Contains("Light"))
                        {
                            light = deviceCategory;
                        }
                    }
                    if (light != null)
                    {
                        Logger.Info(light.SubDevices.Count.ToString(CultureInfo.CurrentCulture));
                    }
                    sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.NoFiles);
                    sim.MyGeneralConfig.Enable(CalcOption.TotalsPerLoadtype);
                    sim.MyGeneralConfig.Enable(CalcOption.TotalsPerDevice);
                    sim.MyGeneralConfig.Enable(CalcOption.MakePDF);
                    sim.MyGeneralConfig.Enable(CalcOption.HouseholdContents);
                    sim.Should().NotBeNull();
                    var cmf = new CalcManagerFactory();
                    ModularHousehold chs3 = null;
                    foreach (var modularHousehold in sim.ModularHouseholds.MyItems)
                    {
                        if (modularHousehold.Name.StartsWith("CHS01", StringComparison.Ordinal))
                        {
                            chs3 = modularHousehold;
                        }
                    }
                    if (chs3 == null)
                    {
                        throw new LPGException("Could not find the household CHS01");
                    }
                    Logger.Info(chs3.ToString());
                    CalculationProfiler calculationProfiler = new CalculationProfiler();
                    CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                        sim.TemperatureProfiles[0], chs3, EnergyIntensityType.Random, false,
                         null, LoadTypePriority.Mandatory, null, null, null, sim.MyGeneralConfig.AllEnabledOptions(),
                        new DateTime(2015, 1, 15), new DateTime(2015, 1, 18), new TimeSpan(0, 1, 0), ";", -1, new TimeSpan(0, 1, 0), false, false, false, 3, 3,
                        calculationProfiler, wd.WorkingDirectory,false);

                    var cm = cmf.GetCalcManager(sim, csps, false);

                    cm.Run(ReportCancelFunc);
                    cm.Dispose();
                    db.Cleanup();
                }
                wd.CleanUp();
            }
        }

        private static bool ReportCancelFunc()
        {
            Logger.Info("canceled");
            return true;
        }
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void GetCalcManagerModularHouseholdTest()
        {
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim = new Simulator(db.ConnectionString);
                    DeviceCategory light = null;
                    foreach (var deviceCategory in sim.DeviceCategories.MyItems)
                    {
                        deviceCategory.RefreshSubDevices();
                        if (deviceCategory.Name.Contains("Light"))
                        {
                            light = deviceCategory;
                        }
                    }
                    if (light != null)
                    {
                        Logger.Info("Light devices:" + light.SubDevices.Count);
                    }
                    sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.NoFiles);
                    sim.MyGeneralConfig.Enable(CalcOption.OverallSum);
                     sim.Should().NotBeNull();

                    var cmf = new CalcManagerFactory();
                    CalculationProfiler calculationProfiler = new CalculationProfiler();
                    CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                        sim.TemperatureProfiles[0], sim.ModularHouseholds[0], EnergyIntensityType.Random, false,
                         null, LoadTypePriority.Mandatory, null, null, null, sim.MyGeneralConfig.AllEnabledOptions(),
                        new DateTime(2015, 1, 15), new DateTime(2015, 1, 18), new TimeSpan(0, 1, 0), ";", -1, new TimeSpan(0, 1, 0), false, false, false, 3, 3,
                        calculationProfiler, wd.WorkingDirectory,false);

                    var cm = cmf.GetCalcManager(sim, csps, false);

                    cm.Run(ReportCancelFunc);
                    db.Cleanup();
                }
                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void GetCalcManagerModularHouseholdTestForDevicePicking()
        {
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim = new Simulator(db.ConnectionString);
                    DeviceCategory light = null;
                    foreach (var deviceCategory in sim.DeviceCategories.MyItems)
                    {
                        deviceCategory.RefreshSubDevices();
                        if (deviceCategory.Name.Contains("Light"))
                        {
                            light = deviceCategory;
                        }
                    }
                    if (light != null)
                    {
                        Logger.Info("Light devices:" + light.SubDevices.Count);
                    }
                    sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.OnlyOverallSum);
                    sim.Should().NotBeNull();
                    var cmf = new CalcManagerFactory();
                    CalculationProfiler calculationProfiler = new CalculationProfiler();
                    CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                        sim.TemperatureProfiles[0], sim.ModularHouseholds[0], EnergyIntensityType.Random, false,
                         null, LoadTypePriority.Mandatory, null, null, null, sim.MyGeneralConfig.AllEnabledOptions(),
                        new DateTime(2015, 1, 15), new DateTime(2015, 1, 18), new TimeSpan(0, 1, 0), ";", -1, new TimeSpan(0, 1, 0), false, false, false, 3, 3,
                        calculationProfiler, wd.WorkingDirectory,false);

                    var cm = cmf.GetCalcManager(sim, csps, false);

                    var chh = (CalcHousehold)cm.CalcObject;
                    var devicenameByCategoryAndLocationID = new Dictionary<string, string>();
                    if (chh == null)
                    {
                        throw new LPGException("xxx");
                    }

                    foreach (var device in chh.CollectDevices())
                    {
                        var key = device.DeviceCategoryGuid + "##" + device.CalcLocation.Name;
                        if (devicenameByCategoryAndLocationID.ContainsKey(key))
                        {
                            var otherDev = devicenameByCategoryAndLocationID[key];

                            Logger.Warning("Suspicious: " + key + " dev 1:" + otherDev + " dev 2:" + device.Name);
                        }
                        else
                        {
                            devicenameByCategoryAndLocationID.Add(key, device.Name);
                        }
                    }
                    Logger.Info("-----");
                    foreach (var autoDev in chh.CollectAutoDevs())
                    {
                        var key = autoDev.DeviceCategoryGuid + "##" + autoDev.CalcLocation.Name;
                        if (devicenameByCategoryAndLocationID.ContainsKey(key))
                        {
                            var devicename = devicenameByCategoryAndLocationID[key];
                            if (devicename != autoDev.Name)
                            {
                                Logger.Warning("For " + key + " it should be " + devicename + " but it is " + autoDev.Name);
                            }
                        }
                    }
                    cm.CalcRepo.Logfile.Dispose();
                    db.Cleanup();
                }
                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void GetDuplicateCalcManagerHouseholdTest()
        {
            using (var wd1 = new WorkingDir("GetDuplicateCalcManagerHouseholdTest1"))
            {
                CalculateOneHousehold(wd1.WorkingDirectory);
                using (var wd2 = new WorkingDir("GetDuplicateCalcManagerHouseholdTest2"))
                {
                    CalculateOneHousehold(wd2.WorkingDirectory);

                    var hhkeys = HouseholdKeyLogger.Load(wd1.SqlResultLoggingService);
                    var afts1 = new CalcDeviceDtoLogger(wd1.SqlResultLoggingService);
                    var aft1 = afts1.Load(hhkeys.Where(x => x.KeyType == HouseholdKeyType.Household).ToList());
                    var afts2 = new CalcDeviceDtoLogger(wd2.SqlResultLoggingService);
                    var aft2 = afts2.Load(hhkeys.Where(x => x.KeyType == HouseholdKeyType.Household).ToList());
                    var devices1 = aft1.Select(x => x.Name).OrderBy(x => x).ToList();
                    var devices2 = aft2.Select(x => x.Name).OrderBy(x => x).ToList();
                    devices1.Should().BeEquivalentTo(devices2);

                    var rfel1 = new ResultFileEntryLogger(wd1.SqlResultLoggingService);
                    var rfes1 = rfel1.Load();
                    var rfel2 = new ResultFileEntryLogger(wd2.SqlResultLoggingService);
                    var rfes2 = rfel2.Load();
                    rfes1.Should().BeEquivalentTo(rfes2, o => o.Excluding(
                         x => x.SelectedMemberPath.EndsWith("FullFileName", StringComparison.InvariantCultureIgnoreCase)));

                    CompareCsv(rfes1, rfes2);
                    wd1.CleanUp();
                    wd2.CleanUp();
                }
            }
        }

        private static void CompareCsv([NotNull] List<ResultFileEntry> rfes1, List<ResultFileEntry> rfes2)
        {
            List<string> filesToIgnore = new List<string>
            {
                "LocationStatistics.HH1.csv"
            };
            foreach (var fileEntry1 in rfes1) {
                if (filesToIgnore.Contains(fileEntry1.FileName)) {
                    continue;
                }
                if (fileEntry1.FileName?.ToUpperInvariant().EndsWith(".CSV", StringComparison.Ordinal)==true) {
                    foreach (var fileEntry2 in rfes2) {
                        if (fileEntry1.FileName == fileEntry2.FileName) {
                            Logger.Info("comparing " + fileEntry1.Name + " with " + fileEntry2.Name);
                            if (fileEntry1.FullFileName == null) {
                                throw new LPGException("fileEntry1.FullFileName was null");
                            }

                            if (fileEntry2.FullFileName == null) {
                                throw new LPGException("fileEntry2.FullFileName was null");
                            }
                            using var sr1 = new StreamReader(fileEntry1.FullFileName);
                            using (var sr2 = new StreamReader(fileEntry2.FullFileName)) {
                                while (!sr1.EndOfStream) {
                                    var s1 = sr1.ReadLine();
                                    var s2 = sr2.ReadLine();
                                    s1.Should().Be(s2);
                                }

                                sr1.Close();
                                sr2.Close();
                            }
                        }
                    }
                }
            }
        }

        public CalcManagerFactoryTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
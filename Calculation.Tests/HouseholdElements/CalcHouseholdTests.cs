using System;
using System.Collections;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.DtoFactories;
using CalculationController.Integrity;
using CalculationController.Queue;
using CalculationEngine;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.Tests;
using Database;
using Database.Tests;
using FluentAssertions;
using JetBrains.Annotations;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Calculation.HouseholdElements.Tests {
    public class CalcHouseholdTests : UnitTestBaseClass {
        public CalcHouseholdTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void DumpHouseholdContentsToTextTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass())) {
                using (var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass())) {
                    var sim = new Simulator(db.ConnectionString) {
                        MyGeneralConfig = {
                            StartDateUIString = "01.01.2015",
                            EndDateUIString = "02.01.2015",
                            InternalTimeResolution = "00:01:00",
                            ExternalTimeResolution = "00:15:00",
                            RandomSeed = 5
                        }
                    };
                    sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.NoFiles);
                    sim.MyGeneralConfig.Enable(CalcOption.TotalsPerDevice);
                    sim.MyGeneralConfig.CSVCharacter = ";";
                    //ConfigSetter.SetGlobalTimeParameters(sim.MyGeneralConfig);
                    sim.Should().NotBeNull();
                    SimIntegrityChecker.Run(sim, CheckingOptions.Default());
                    CalcManagerFactory.DoIntegrityRun = false;

                    var cmf = new CalcManagerFactory();
                    var calculationProfiler = new CalculationProfiler();
                    var csps = new CalcStartParameterSet(sim.GeographicLocations[0], sim.TemperatureProfiles[0], sim.ModularHouseholds[0],
                        EnergyIntensityType.Random, false, null, LoadTypePriority.RecommendedForHouses, null, null, null,
                        sim.MyGeneralConfig.AllEnabledOptions(), new DateTime(2015, 1, 1), new DateTime(2015, 1, 2), new TimeSpan(0, 1, 0), ";", 5,
                        new TimeSpan(0, 15, 0), false, false, false, 3, 3, calculationProfiler, wd1.WorkingDirectory, false,false);
                    var cm = cmf.GetCalcManager(sim, csps, false);
                    //,, wd1.WorkingDirectory, sim.ModularHouseholds[0], false,
                    //sim.TemperatureProfiles[0], sim.GeographicLocations[0], EnergyIntensityType.Random, version,
                    //LoadTypePriority.RecommendedForHouses, null,null
                    var dls = new DayLightStatus(new BitArray(100));
                    if (cm.CalcObject == null) {
                        throw new LPGException("CalcObject was null");
                    }

                    cm.CalcObject.Init(dls, 1);
                    CalcManager.ExitCalcFunction = true;
                    cm.CalcObject.DumpHouseholdContentsToText();
                    cm.Dispose();

                    db.Cleanup();
                    wd1.CleanUp();
                }
            }
        }
    }
}

namespace Calculation.Tests.HouseholdElements {
    public class CalcHouseholdTests : UnitTestBaseClass {
        public CalcHouseholdTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void MatchAutonomousDevicesWithNormalDevicesTest()
        {
            // make different devices at different locations
            // only a single one should be matched.
            var startdate = new DateTime(2018, 1, 1);
            var enddate = startdate.AddMinutes(100);
            var calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate);

            //_calcParameters.InitializeTimeSteps(startdate, enddate, new TimeSpan(0, 1, 0), 3, false);

            Config.IsInUnitTesting = true;
            var cloc1 = new CalcLocation("loc1", Guid.NewGuid().ToStrGuid());
            var cloc2 = new CalcLocation("loc1", Guid.NewGuid().ToStrGuid());
            var cdevload = new List<CalcDeviceLoad>();
            var clt = new CalcLoadType("calcloadtype", "power", "sum", 1, true, Guid.NewGuid().ToStrGuid());
            cdevload.Add(new CalcDeviceLoad("load", 100, clt, 0, 0));
            var devCategoryGuid = Guid.NewGuid().ToStrGuid();
            IMock<IOnlineDeviceActivationProcessor> iodap = new Mock<IOnlineDeviceActivationProcessor>();
            using (var calcRepo = new CalcRepo(iodap.Object, calcParameters: calcParameters)) {
                var cdd1 = new CalcDeviceDto("cdevice1", devCategoryGuid, new HouseholdKey("HH1"), OefcDeviceType.Device, "category", "",
                    Guid.NewGuid().ToStrGuid(), cloc1.Guid, cloc1.Name);

                var cdLoc1 = new CalcDevice(new List<CalcDeviceLoad>(), cloc1, cdd1, calcRepo);
                var cdd2 = new CalcDeviceDto("cdevice1", devCategoryGuid, new HouseholdKey("HH1"), OefcDeviceType.Device, "category", "",
                    Guid.NewGuid().ToStrGuid(), cloc1.Guid, cloc1.Name);
                var cdLoc1B = new CalcDevice(new List<CalcDeviceLoad>(), cloc1, cdd2, calcRepo);
                var cdd3 = new CalcDeviceDto("cdevice1", devCategoryGuid, new HouseholdKey("HH1"), OefcDeviceType.Device, "category", "",
                    Guid.NewGuid().ToStrGuid(), cloc2.Guid, cloc2.Name);
                var cdLoc2 = new CalcDevice(new List<CalcDeviceLoad>(), cloc2, cdd3, calcRepo);
                var cp = new CalcProfile("cp1", Guid.NewGuid().ToStrGuid(), TimeSpan.FromMilliseconds(1), ProfileType.Absolute, "blub");
                //CalcVariableRepository crv = new CalcVariableRepository();
                //VariableRequirement vr = new VariableRequirement("");
                var requirements = new List<VariableRequirement>();
                var cdd4 = new CalcDeviceDto("cdevice1", devCategoryGuid, new HouseholdKey("HH1"), OefcDeviceType.Device, "category", "",
                    Guid.NewGuid().ToStrGuid(), cloc1.Guid, cloc1.Name);
                var cadp = new CalcAutoDevProfile(cp, clt, 1);
                var cadps = new List<CalcAutoDevProfile>() { cadp };
                var cadLoc1 = new CalcAutoDev(cadps, cdevload, 0,  cloc1, requirements, cdd4, calcRepo);
                var autodevs = new List<CalcAutoDev> {
                    cadLoc1
                };
                var normalDevices = new List<CalcDevice> {
                    cdLoc1,
                    cdLoc1B,
                    cdLoc2
                };
                CalcHousehold.MatchAutonomousDevicesWithNormalDevices(autodevs, normalDevices);
                //var totalmatchcount = 0;
                foreach (var device in normalDevices) {
                    Logger.Info(device.Name);
                    foreach (var matchingAutoDev in device.MatchingAutoDevs) {
                        //      totalmatchcount++;
                        Logger.Info("\t" + matchingAutoDev.Name);
                    }
                }

                cdLoc1.MatchingAutoDevs.Count.Should().Be(1);
            }

            //(totalmatchcount).Should().Be(1);
        }
    }
}
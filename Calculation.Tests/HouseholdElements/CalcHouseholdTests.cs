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
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Calculation.HouseholdElements.Tests {
    [TestFixture]
    public class CalcHouseholdTests : UnitTestBaseClass
    {
        public CalcHouseholdTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void DumpHouseholdContentsToTextTest()
        {
            using (var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim = new Simulator(db.ConnectionString)
                    {
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
                    Assert.AreNotEqual(null, sim);
                    SimIntegrityChecker.Run(sim);
                    CalcManagerFactory.DoIntegrityRun = false;

                    var cmf = new CalcManagerFactory();
                    CalculationProfiler calculationProfiler = new CalculationProfiler();
                    CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0], sim.TemperatureProfiles[0],
                        sim.ModularHouseholds[0], EnergyIntensityType.Random, false,
                        null, LoadTypePriority.RecommendedForHouses, null, null, null, sim.MyGeneralConfig.AllEnabledOptions(),
                        new DateTime(2015, 1, 1), new DateTime(2015, 1, 2), new TimeSpan(0, 1, 0), ";", 5, new TimeSpan(0, 15, 0), false, false, false, 3, 3,
                        calculationProfiler, wd1.WorkingDirectory);
                    var cm = cmf.GetCalcManager(sim, csps, false);
                    //,, wd1.WorkingDirectory, sim.ModularHouseholds[0], false,
                    //sim.TemperatureProfiles[0], sim.GeographicLocations[0], EnergyIntensityType.Random, version,
                    //LoadTypePriority.RecommendedForHouses, null,null
                    DayLightStatus dls = new DayLightStatus(new BitArray(100));
                    if (cm.CalcObject == null)
                    {
                        throw new LPGException("xxx");
                    }

                    cm.CalcObject.Init(dls, 1);
                    CalcManager.ExitCalcFunction = true;
                    cm.CalcObject.DumpHouseholdContentsToText();
                    cm.Dispose();
                    db.Cleanup();
                }
                wd1.CleanUp();
            }
        }
    }
}

namespace Calculation.Tests.HouseholdElements {
    [TestFixture]
    public class CalcHouseholdTests : UnitTestBaseClass
    {
        public CalcHouseholdTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void MatchAutonomousDevicesWithNormalDevicesTest()
        {
            // make different devices at different locations
            // only a single one should be matched.
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(100);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate);

            //_calcParameters.InitializeTimeSteps(startdate, enddate, new TimeSpan(0, 1, 0), 3, false);

            Config.IsInUnitTesting = true;
            var cloc1 = new CalcLocation("loc1", Guid.NewGuid().ToStrGuid());
            var cloc2 = new CalcLocation("loc1", Guid.NewGuid().ToStrGuid());
            var cdevload = new List<CalcDeviceLoad>();
            var clt = new CalcLoadType("calcloadtype", "power", "sum", 1, true, Guid.NewGuid().ToStrGuid());
            cdevload.Add(new CalcDeviceLoad("load", 100, clt, 0, 0));
            var devCategoryGuid = Guid.NewGuid().ToStrGuid();
            IMock<IOnlineDeviceActivationProcessor> iodap = new Mock<IOnlineDeviceActivationProcessor>();
            using (CalcRepo calcRepo = new CalcRepo(odap: iodap.Object, calcParameters: calcParameters))
            {
                CalcDeviceDto cdd1 = new CalcDeviceDto("cdevice1", devCategoryGuid, new HouseholdKey("HH1"),
OefcDeviceType.Device, "category", "", Guid.NewGuid().ToStrGuid(),
cloc1.Guid, cloc1.Name);

                var cdLoc1 = new CalcDevice(new List<CalcDeviceLoad>(),
                     cloc1, cdd1, calcRepo);
                CalcDeviceDto cdd2 = new CalcDeviceDto("cdevice1", devCategoryGuid, new HouseholdKey("HH1"),
                    OefcDeviceType.Device, "category", "", Guid.NewGuid().ToStrGuid(),
                    cloc1.Guid, cloc1.Name);
                var cdLoc1B = new CalcDevice(new List<CalcDeviceLoad>(), cloc1,
                     cdd2, calcRepo);
                CalcDeviceDto cdd3 = new CalcDeviceDto("cdevice1", devCategoryGuid, new HouseholdKey("HH1"),
                    OefcDeviceType.Device, "category", "", Guid.NewGuid().ToStrGuid(),
                    cloc2.Guid, cloc2.Name);
                var cdLoc2 = new CalcDevice(new List<CalcDeviceLoad>(), cloc2,
                     cdd3, calcRepo);
                var cp = new CalcProfile("cp1", Guid.NewGuid().ToStrGuid(), TimeSpan.FromMilliseconds(1),
                    ProfileType.Absolute, "blub");
                //CalcVariableRepository crv = new CalcVariableRepository();
                //VariableRequirement vr = new VariableRequirement("");
                List<VariableRequirement> requirements = new List<VariableRequirement>();
                CalcDeviceDto cdd4 = new CalcDeviceDto("cdevice1", devCategoryGuid, new HouseholdKey("HH1"),
                    OefcDeviceType.Device, "category", "", Guid.NewGuid().ToStrGuid(),
                    cloc1.Guid, cloc1.Name);
                var cadLoc1 = new CalcAutoDev(cp, clt, cdevload, 0,
                    1, cloc1, requirements, cdd4, calcRepo);
                var autodevs = new List<CalcAutoDev>
            {
                cadLoc1
            };
                var normalDevices = new List<CalcDevice>
            {
                cdLoc1,
                cdLoc1B,
                cdLoc2
            };
                CalcHousehold.MatchAutonomousDevicesWithNormalDevices(autodevs, normalDevices);
                //var totalmatchcount = 0;
                foreach (var device in normalDevices)
                {
                    Logger.Info(device.Name);
                    foreach (var matchingAutoDev in device.MatchingAutoDevs)
                    {
                        //      totalmatchcount++;
                        Logger.Info("\t" + matchingAutoDev.Name);
                    }
                }
                Assert.AreEqual(1, cdLoc1.MatchingAutoDevs.Count);
            }
            //Assert.AreEqual(1, totalmatchcount);
        }
    }
}
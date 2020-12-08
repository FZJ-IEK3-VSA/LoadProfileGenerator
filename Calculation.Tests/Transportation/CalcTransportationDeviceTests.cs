using System;
using System.Collections;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using CalculationEngine.OnlineLogging;
using CalculationEngine.Transportation;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace Calculation.Tests.Transportation {
    public class CalcTransportationDeviceTests : UnitTestBaseClass {
        public CalcTransportationDeviceTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void CalcDistanceDurationTest()
        {
            //_calcParameters.InitializeTimeSteps(new DateTime(2015,1,1), new DateTime(2015, 5, 1),new TimeSpan(0, 1, 0),3,false);
            //CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
            var internalStepSize = new TimeSpan(0, 1, 0);
            var val = CalcTransportationDevice.CalculateDurationOfTimestepsForDistance(1000, 1, internalStepSize);
            val.Should().Be(1000 / 60 + 1);
            var val2 = CalcTransportationDevice.CalculateDurationOfTimestepsForDistance(2000, 1, internalStepSize);
            val2.Should().Be(2000 / 60 + 1);
            val2 = CalcTransportationDevice.CalculateDurationOfTimestepsForDistance(2000, 0.5, internalStepSize);
            val2.Should().Be(2000 / 60 * 2 + 1);
            val2 = CalcTransportationDevice.CalculateDurationOfTimestepsForDistance(2000, 2, internalStepSize);
            val2.Should().Be(2000 / 60 / 2 + 1);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void CalcTransportationDeviceDriveTest()
        {
            using var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            using var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "blub", wd.InputDataLogger);
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
            //_calcParameters.CSVCharacter = ";";_calcParameters.InitializeTimeSteps(new DateTime(2018,1,1),new DateTime(2018,1,31),new TimeSpan(0,1,0),3,true  );
            var calcParameters = CalcParametersFactory.MakeGoodDefaults();

            var category = new CalcTransportationDeviceCategory("category", true, Guid.NewGuid().ToStrGuid());
            var lt2 = new CalcLoadType("driving load", "km/h", "km", 10000, false, Guid.NewGuid().ToStrGuid());
            var rnd = new Random(1);
            var nr = new NormalRandom(0, 0.1, rnd);
            //SqlResultLoggingService srls = new SqlResultLoggingService(wd.WorkingDirectory);
            var dsc = new DateStampCreator(calcParameters);
            using var old = new OnlineLoggingData(dsc, wd.InputDataLogger, calcParameters);
            using var lf = new LogFile(calcParameters, fft, true);
            var key = new HouseholdKey("hh1");
            fft.RegisterHousehold(key, "Household", HouseholdKeyType.Household, "Description", null, null);
            fft.RegisterGeneralHouse();
            var chargingCalcLoadType = new CalcLoadType("charging load", "W", "kWh", 0.50, false, Guid.NewGuid().ToStrGuid());
            var odap = new OnlineDeviceActivationProcessor(old, calcParameters, fft);
            using var calcRepo = new CalcRepo(rnd: rnd, normalRandom: nr, lf: lf, calcParameters: calcParameters, odap: odap,
                onlineLoggingData: old);
            var srcSite = new CalcSite("srcsite", Guid.NewGuid().ToStrGuid(), key);
            var dstSite = new CalcSite("dstSite", Guid.NewGuid().ToStrGuid(), key);
            var isavailable = new BitArray(calcRepo.CalcParameters.InternalTimesteps);
            var station = new CalcChargingStation(category, chargingCalcLoadType, 500, "stationname", "stationguid".ToStrGuid(),
                key, chargingCalcLoadType, calcRepo,isavailable);
            dstSite.ChargingDevices.Add(station);
            var calcSites = new List<CalcSite> {
                srcSite,
                dstSite
            };
            var cdd = new CalcDeviceDto("transport device", category.Guid, key, OefcDeviceType.Transportation, category.Name,
                string.Empty, Guid.NewGuid().ToStrGuid(), StrGuid.Empty, string.Empty);
            var loads = new List<CalcDeviceLoad> {
                new CalcDeviceLoad("load1", 10, lt2, 10000, 0)
            };
            var ctd = new CalcTransportationDevice(category, 10, loads, 10000, 1, 1000, chargingCalcLoadType, calcSites, cdd,
                calcRepo);
            var start = new TimeStep(1, 0, false);
            var end = new TimeStep(11, 0, false);
            ctd.Activate(start, 10, srcSite, dstSite, "myroute", "myperson", start, end);

            ctd.AvailableRangeInMeters.Should().Be(10000);
            //TODO: fix this and comment out
            //station.IsAvailable = false;
            double prevrange = 0;
            for (var i = 1; i < 11; i++) {
                var ts = new TimeStep(i, 0, false);
                ctd.DriveAndCharge(ts);
                odap.ProcessOneTimestep(ts);
                var diffRange = prevrange - ctd.AvailableRangeInMeters;
                Logger.Info("timestep: " + i + " Range: " + ctd.AvailableRangeInMeters + " diff:" + diffRange);
                prevrange = ctd.AvailableRangeInMeters;
                ctd.Currentsite.Should().BeNull();
            }

            //no charging
            ctd.AvailableRangeInMeters.Should().Be(10000 - 10 * 60 * 10); //10m/s = 600m/minute
            Logger.Info("currentSite:" + ctd.Currentsite?.Name);

            //station.IsAvailable = true;
            for (var i = 11; i < 50; i++) {
                var ts = new TimeStep(i, 0, false);
                ctd.DriveAndCharge(ts);
                odap.ProcessOneTimestep(ts);
                Logger.Info("timestep: " + i + " Range: " + ctd.AvailableRangeInMeters);
            }

            ctd.Currentsite.Should().Be(dstSite);

          //  wd.CleanUp(1);
        }
    }
}
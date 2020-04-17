using System;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.Helper;
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
using NUnit.Framework;

namespace Calculation.Tests.Transportation
{
    public class CalcTransportationDeviceTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CalcDistanceDurationTest()
        {
            //_calcParameters.InitializeTimeSteps(new DateTime(2015,1,1), new DateTime(2015, 5, 1),new TimeSpan(0, 1, 0),3,false);
            //CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
            TimeSpan internalStepSize = new TimeSpan(0,1,0);
            int val = CalcTransportationDevice.CalculateDurationOfTimestepsForDistance(1000, 1,internalStepSize);
            Assert.That(val,Is.EqualTo(1000/60+1));
            int val2 = CalcTransportationDevice.CalculateDurationOfTimestepsForDistance(2000, 1, internalStepSize);
            Assert.That(val2, Is.EqualTo(2000 / 60 + 1));
            val2 = CalcTransportationDevice.CalculateDurationOfTimestepsForDistance(2000, 0.5, internalStepSize);
            Assert.That(val2, Is.EqualTo(2000 / 60*2 + 1));
            val2 = CalcTransportationDevice.CalculateDurationOfTimestepsForDistance(2000, 2, internalStepSize);
            Assert.That(val2, Is.EqualTo(2000 / 60 / 2 + 1));
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CalcTransportationDeviceDriveTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            //_calcParameters.CSVCharacter = ";";_calcParameters.InitializeTimeSteps(new DateTime(2018,1,1),new DateTime(2018,1,31),new TimeSpan(0,1,0),3,true  );
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();

            CalcTransportationDeviceCategory category = new CalcTransportationDeviceCategory("category",true, Guid.NewGuid().ToString());
            CalcLoadType lt2 = new CalcLoadType("driving load",  "km/h", "km", 10000, false, Guid.NewGuid().ToString());
           
            Random rnd  =new Random(1);
            NormalRandom nr = new NormalRandom(0,0.1,rnd);
            var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "blub", wd.InputDataLogger);
            //SqlResultLoggingService srls = new SqlResultLoggingService(wd.WorkingDirectory);
            DateStampCreator dsc = new DateStampCreator(calcParameters);
            InputDataLogger idl = new InputDataLogger(new IDataSaverBase[0]);
            OnlineLoggingData old = new OnlineLoggingData(dsc,idl,calcParameters);
            LogFile lf = new LogFile(calcParameters,fft,old, wd.SqlResultLoggingService, true);
            HouseholdKey key = new HouseholdKey("hh1");
            lf.InitHousehold(key,"Household",HouseholdKeyType.Household,"Description", null, null);
            CalcLoadType chargingCalcLoadType = new CalcLoadType("charging load","W","kWh",0.50,false, Guid.NewGuid().ToString());
            OnlineDeviceActivationProcessor odap = new OnlineDeviceActivationProcessor(nr,lf,calcParameters);
            CalcSite srcSite = new CalcSite("srcsite",  Guid.NewGuid().ToString(),key);
            CalcSite dstSite = new CalcSite("dstSite", Guid.NewGuid().ToString(),key);
            CalcChargingStation station = new CalcChargingStation(category,
                chargingCalcLoadType, 500,lf.OnlineLoggingData,
                "stationname","stationguid",key,chargingCalcLoadType);
            dstSite.ChargingDevices.Add(station);
            List<CalcSite> calcSites = new List<CalcSite>
            {
                srcSite,
                dstSite
            };
            CalcDeviceDto cdd = new CalcDeviceDto("transport device",
                category.Guid,key,OefcDeviceType.Transportation,
                category.Name,string.Empty, Guid.NewGuid().ToString(),
                string.Empty,string.Empty);
            List<CalcDeviceLoad> loads = new List<CalcDeviceLoad>
            {
                new CalcDeviceLoad("load1", 10, lt2, 10000, 0,Guid.NewGuid().ToString())
            };
            CalcTransportationDevice ctd = new CalcTransportationDevice(category,
                10,loads,odap,
                10000,1,1000,
                chargingCalcLoadType,calcSites,
                calcParameters, old, cdd);
            TimeStep start = new TimeStep(1, 0, false);
            TimeStep end = new TimeStep(11, 0, false);
            ctd.Activate(start, 10,srcSite,dstSite,"myroute","myperson", start,
                end);

            Assert.That(ctd.AvailableRangeInMeters,Is.EqualTo(10000));
            //TODO: fix this and comment out
            //station.IsAvailable = false;
            double prevrange = 0;
            for (int i = 1; i < 11; i++) {
                TimeStep ts = new TimeStep(i, 0,false);
                ctd.DriveAndCharge(ts);
                odap.ProcessOneTimestep(ts);
                double diffRange = prevrange - ctd.AvailableRangeInMeters;
                Console.WriteLine("timestep: " + i +  " Range: " + ctd.AvailableRangeInMeters + " diff:" + diffRange);
                prevrange = ctd.AvailableRangeInMeters;
                Assert.That(ctd.Currentsite, Is.EqualTo(null));
            }
            //no charging
            Assert.That(ctd.AvailableRangeInMeters, Is.EqualTo(10000-10*60*10)); //10m/s = 600m/minute
            Console.WriteLine("currentSite:" + ctd.Currentsite?.Name);

            //station.IsAvailable = true;
            for (int i = 11; i < 50; i++)
            {
                TimeStep ts = new TimeStep(i, 0,false);
                ctd.DriveAndCharge(ts);
                odap.ProcessOneTimestep(ts);
                Console.WriteLine("timestep: " + i + " Range: " + ctd.AvailableRangeInMeters);
            }
            Assert.That(ctd.Currentsite, Is.EqualTo(dstSite));
            wd.CleanUp(1);
        }
    }
}

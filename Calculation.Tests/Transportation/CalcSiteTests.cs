using System;
using System.Collections.Generic;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using CalculationEngine.OnlineLogging;
using CalculationEngine.Transportation;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.Tests;
using Moq;
using NUnit.Framework;

namespace Calculation.Tests.Transportation {
    [TestFixture]
    public class CalcSiteTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void TestCalcSite()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(2018,1,1).SetEndDate(2018,2,1).SetSettlingDays(0).EnableShowSettlingPeriod();
            HouseholdKey hhkey = new HouseholdKey("hh0");
            var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hhname0",wd.InputDataLogger);

            fft.RegisterHousehold(hhkey,"hhname0",HouseholdKeyType.Household,"desc", null, null);
            DateStampCreator dsc =new DateStampCreator(calcParameters);
            LogFile lf = new LogFile(calcParameters,
                fft,
                new OnlineLoggingData(dsc,
                    wd.InputDataLogger,
                    calcParameters),
                null,
                true);
            Random r = new Random(1);
            CalcSite src = new CalcSite("src",Guid.NewGuid().ToString(),hhkey);
            CalcSite dst = new CalcSite("dst", Guid.NewGuid().ToString(),hhkey);
            TransportationHandler th = new TransportationHandler(r);
            //List<CalcTravelRoute> routes = src.GetViableTrafficRoutes(dst);
            //Assert.That(routes.Count,Is.EqualTo( 0));

            CalcTravelRoute firstRoute = new CalcTravelRoute("route1",src,dst, th.VehicleDepot,
                th.LocationUnlimitedDevices,lf,hhkey, Guid.NewGuid().ToString());
            CalcTransportationDeviceCategory transcategory =
                new CalcTransportationDeviceCategory("car-category",true, Guid.NewGuid().ToString());
            firstRoute.AddTravelRouteStep("step1", transcategory,1,3600, Guid.NewGuid().ToString());
            src.AddRoute(firstRoute);
            //List<CalcTravelRoute> routes2 = src.GetViableTrafficRoutes(dst);
            //Assert.That(routes2.Count,Is.EqualTo(0));
            const double distanceToEnergyFactor = 1;
            List<CalcSite> calcSites = new List<CalcSite>
            {
                src,
                dst
            };
            CalcLoadType chargingLoadType = new CalcLoadType("chargingloadtype","w","kwh",1,false, Guid.NewGuid().ToString());
            var cdls = new List<CalcDeviceLoad>();
            CalcDeviceLoad cdl = new CalcDeviceLoad("name",1,chargingLoadType,1,1, Guid.NewGuid().ToString());
            cdls.Add(cdl);
            var iodap = new Mock<IOnlineDeviceActivationProcessor>();
            CalcTransportationDevice ctd = new CalcTransportationDevice("car-device",transcategory,1,
                cdls,iodap.Object,hhkey,100,distanceToEnergyFactor,1000,chargingLoadType,
                calcSites,calcParameters, Guid.NewGuid().ToString(), lf.OnlineLoggingData);
            th.VehicleDepot.Add(ctd);
            //List<CalcTravelRoute> routes3 = src.GetViableTrafficRoutes(dst);
            //Assert.That(routes3.Count, Is.EqualTo(1));
            TimeStep ts = new TimeStep(1,0,false);
            int? duration = firstRoute.GetDuration(ts, "name",  r,new List<CalcTransportationDevice>());
            Console.WriteLine("Duration: " + duration);
            Assert.That(duration,Is.EqualTo(60)); // 3600 m bei 1 m/s
            int? duration2 = firstRoute.GetDuration(ts, "name", r, new List<CalcTransportationDevice>());
            Assert.That(duration, Is.EqualTo(duration2)); // 3600 m bei 1 m/s*/
            wd.CleanUp();
        }
    }
}
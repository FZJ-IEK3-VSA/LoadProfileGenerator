using System;
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
using Moq;

using Xunit;
using Xunit.Abstractions;


namespace Calculation.Tests.Transportation {
    public class CalcSiteTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TestCalcSite()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService) );
                CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(2018, 1, 1).SetEndDate(2018, 2, 1).SetSettlingDays(0).EnableShowSettlingPeriod();
                HouseholdKey hhkey = new HouseholdKey("hh0");
                using (var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hhname0", wd.InputDataLogger))
                {
                    fft.RegisterHousehold(hhkey, "hhname0", HouseholdKeyType.Household, "desc", null, null);
                    fft.RegisterGeneralHouse();
                    OnlineLoggingData old = new OnlineLoggingData(new DateStampCreator(calcParameters),wd.InputDataLogger,calcParameters);
                    using (LogFile lf = new LogFile(calcParameters,
                        fft, true))
                    {
                        Random r = new Random(1);
                        CalcSite src = new CalcSite("src", Guid.NewGuid().ToStrGuid(), hhkey);
                        CalcSite dst = new CalcSite("dst", Guid.NewGuid().ToStrGuid(), hhkey);
                        TransportationHandler th = new TransportationHandler();
                        //List<CalcTravelRoute> routes = src.GetViableTrafficRoutes(dst);
                        //Assert.That(routes.Count,Is.EqualTo( 0));
                        var iodap = new Mock<IOnlineDeviceActivationProcessor>();
                        using (CalcRepo calcRepo = new CalcRepo(odap: iodap.Object, lf: lf, rnd: r, calcParameters: calcParameters, onlineLoggingData: old))
                        {
                            CalcTravelRoute firstRoute = new CalcTravelRoute("route1", 0, 100, Common.Enums.PermittedGender.All, null, null, 1.0, src, dst, th.VehicleDepot,
    th.LocationUnlimitedDevices, hhkey, Guid.NewGuid().ToStrGuid(), calcRepo);
                            CalcTransportationDeviceCategory transcategory =
                                new CalcTransportationDeviceCategory("car-category", true, Guid.NewGuid().ToStrGuid());
                            firstRoute.AddTravelRouteStep("step1", transcategory, 1, 3600, Guid.NewGuid().ToStrGuid());
                            src.AddRoute(firstRoute);
                            //List<CalcTravelRoute> routes2 = src.GetViableTrafficRoutes(dst);
                            //Assert.That(routes2.Count,Is.EqualTo(0));
                            const double distanceToEnergyFactor = 1;
                            List<CalcSite> calcSites = new List<CalcSite>
            {
                src,
                dst
            };
                            CalcLoadType chargingLoadType = new CalcLoadType("chargingloadtype", "w", "kwh", 1, false, Guid.NewGuid().ToStrGuid());
                            var cdls = new List<CalcDeviceLoad>();
                            CalcDeviceLoad cdl = new CalcDeviceLoad("name", 1, chargingLoadType, 1, 1);
                            cdls.Add(cdl);
                            CalcDeviceDto dto = new CalcDeviceDto("car-device", transcategory.Guid,
                                hhkey, OefcDeviceType.Transportation, transcategory.Name, string.Empty,
                                Guid.NewGuid().ToStrGuid(), StrGuid.Empty, string.Empty, FlexibilityType.NoFlexibility, 0);
                            CalcTransportationDevice ctd = new CalcTransportationDevice(transcategory, 1,
                                cdls, 100, distanceToEnergyFactor,
                                1000, chargingLoadType,
                                calcSites, dto, calcRepo);
                            th.VehicleDepot.Add(ctd);
                            //List<CalcTravelRoute> routes3 = src.GetViableTrafficRoutes(dst);
                            //(1).Should().Be(routes3.Count);
                            TimeStep ts = new TimeStep(1, 0, false);
                            int? duration = firstRoute.GetDuration(ts, "name", new List<CalcTransportationDevice>());
                            Logger.Info("Duration: " + duration);
                            duration.Should().Be(60); // 3600 m bei 1 m/s
                            int? duration2 = firstRoute.GetDuration(ts, "name", new List<CalcTransportationDevice>());
                            duration.Should().Be(duration2); // 3600 m bei 1 m/s*/
                        }
                    }
                }
                wd.CleanUp();
            }
        }

        public CalcSiteTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
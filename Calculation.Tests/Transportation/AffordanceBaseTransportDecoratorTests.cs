using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging.InputLoggers;
using Common.Tests;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;

namespace Calculation.Tests.Transportation
{
    public class AffordanceBaseTransportDecoratorTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RunDoubleActivationTest()
        {
            var rnd = new Random(1);
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
            HouseholdKey key = new HouseholdKey("hh1");
            SetupFullWorkingTransportationExample(wd, rnd, out var nr, out var srcloc, out var dstloc,
                out var dstSite, out var transportationHandler, out var abt,calcParameters,key);
            //make sure there is a travel route
            const string personname = "activator";
            TimeStep ts = new TimeStep(0,0,false);
            var travelroute = transportationHandler.GetTravelRouteFromSrcLoc(srcloc, dstSite, ts, personname);
            Assert.NotNull(travelroute);
            // find if busy
            var isbusy = abt.IsBusy(ts, nr, rnd, srcloc,personname, false);
            Assert.IsFalse(isbusy);
            var affs = dstloc.Affordances.ToList();

            Logger.Info("Activating affordance for time 0");
            affs[0].Activate(ts, personname,  srcloc, out _);
            //should throw exception the second time.
            Logger.Info("Activating affordance again for time 0");
            //this should throw, since it is already busy
            //Assert.Throws<LPGException>(() =>affs[0].Activate(0, "activator", null, srcloc, new Dictionary<int, CalcProfile>(), out _));
            CalcAffordance.DoubleCheckBusyArray = false;
            wd.CleanUp();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RunTransportDecoratorTest()
        {
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            Config.IsInUnitTesting = true;
            var rnd = new Random(1);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
            HouseholdKey key = new HouseholdKey("hh1");
            SetupFullWorkingTransportationExample(wd, rnd, out var nr, out var srcloc, out var dstloc,
                out var dstSite, out var transportationHandler, out var abt,calcParameters,key);
            //make sure there is a travel route
            const string personName = "activator";
            TimeStep ts = new TimeStep(0, 0, false);
            var travelroute = transportationHandler.GetTravelRouteFromSrcLoc(srcloc, dstSite,ts, personName);
            Assert.NotNull(travelroute);
            // find if busy
            var isbusy = abt.IsBusy(ts, nr, rnd, srcloc, "", false);
            Assert.IsFalse(isbusy);
            var affs = dstloc.Affordances.ToList();

            Logger.Info("Activating affordance for time 0");
            travelroute.GetDuration(ts, personName, rnd,new List<CalcTransportationDevice>());
            affs[0].Activate(ts, "activator",  srcloc, out var _);
            //should throw exception the second time.
            Logger.Info("Activating affordance again for time 0");

            CalcAffordance.DoubleCheckBusyArray = false;
            wd.CleanUp();
        }

        private static void SetupFullWorkingTransportationExample([NotNull] WorkingDir wd, [NotNull] Random rnd, [NotNull] out NormalRandom nr,
            [NotNull] out CalcLocation srcloc, [NotNull] out CalcLocation dstloc, [NotNull] out CalcSite dstSite,
            [NotNull] out TransportationHandler transportationHandler, [NotNull] out AffordanceBaseTransportDecorator abt, [NotNull] CalcParameters calcParameters,
                                                                  [NotNull] HouseholdKey key)
        {
            Config.IsInUnitTesting = true;
            CalcAffordance.DoubleCheckBusyArray = true;
            nr = new NormalRandom(0, 0.1, rnd);
            var calcprofilevalues = new List<double> {
                10,
                20,
                30
            };

            var cp = new CalcProfile("calcprofile", Guid.NewGuid().ToString(), calcprofilevalues,  ProfileType.Absolute, "syn");
            srcloc = new CalcLocation("srclocation",  Guid.NewGuid().ToString());
            dstloc = new CalcLocation("dstlocation", Guid.NewGuid().ToString());
            var calcdesire =
                new CalcDesire("calcdesire", 1, 0.5m, 10, 1, 1, 60, 0.1m, null, "sourcetrait", "desirecat");
            var calcdesires = new List<CalcDesire> {
                calcdesire
            };
            CalcVariableRepository crv =new CalcVariableRepository();
            BitArray isBusy = new BitArray(calcParameters.InternalTimesteps,false);
            var ca = new CalcAffordance("calcaffordance",  cp, dstloc, false, calcdesires,
                18, 50, PermittedGender.All, false, 0.1, LPGColors.Blue, "affordance category", false,
                false, new List<CalcAffordanceVariableOp>(), new List<VariableRequirement>(),
                ActionAfterInterruption.GoBackToOld, "timelimitname", 1, false, "srctrait", calcParameters,
                Guid.NewGuid().ToString(),crv, new List<CalcAffordance.DeviceEnergyProfileTuple>(), isBusy);

            var srcSite = new CalcSite("srcsite", Guid.NewGuid().ToString(),key);
            srcSite.Locations.Add(srcloc);
            dstSite = new CalcSite("dstSite",  Guid.NewGuid().ToString(),key);
            dstSite.Locations.Add(dstloc);
            var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hh0", wd.InputDataLogger);
            var mock = new Mock<IOnlineLoggingData>();
            var lf = new LogFile(calcParameters,fft, mock.Object,wd.SqlResultLoggingService,  true);
            lf.InitHousehold(new HouseholdKey("hh0"),"hh0-prettyname",HouseholdKeyType.Household,
                "Desc", null, null);
            transportationHandler = new TransportationHandler(rnd);
            transportationHandler.AddSite(srcSite);
            abt = new AffordanceBaseTransportDecorator(ca, dstSite, transportationHandler,
                "travel to dstsite",   lf, new HouseholdKey( "hh0"),calcParameters, Guid.NewGuid().ToString());
            dstloc.AddTransportationAffordance(abt);

            var ctr = new CalcTravelRoute("myRoute1",  srcSite, dstSite,
                transportationHandler.VehicleDepot, transportationHandler.LocationUnlimitedDevices, lf, new HouseholdKey(  "hh0"), Guid.NewGuid().ToString());
            var myCategory = new CalcTransportationDeviceCategory("mycategory",  false, Guid.NewGuid().ToString());
            ctr.AddTravelRouteStep("driving",  myCategory, 1, 36000, Guid.NewGuid().ToString());
            transportationHandler.TravelRoutes.Add(ctr);
            CalcLoadType chargingloadtype = new CalcLoadType("chargingloadtype","W","kwh",1,true, Guid.NewGuid().ToString());
            List<CalcSite> calcSites = new List<CalcSite>
            {
                srcSite,
                dstSite
            };
            var list = new List<CalcDeviceLoad>();
            CalcDeviceLoad cdl = new CalcDeviceLoad("bla",1,chargingloadtype,1,1, Guid.NewGuid().ToString());
            list.Add(cdl);
            Mock<IOnlineDeviceActivationProcessor> iodap = new Mock<IOnlineDeviceActivationProcessor>();
            var transportationDevice =
                new CalcTransportationDevice("bus", myCategory, 1,list , iodap.Object, new HouseholdKey( "hh1"),100,
                    10,1000,chargingloadtype,calcSites,calcParameters, Guid.NewGuid().ToString(),lf.OnlineLoggingData);
            transportationHandler.LocationUnlimitedDevices.Add(transportationDevice);
        }

        //TODO: test for interrupting while driving
        //TODO: only interrupt at the same site
    }
}
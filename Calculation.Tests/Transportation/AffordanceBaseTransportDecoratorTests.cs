using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
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
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;


namespace Calculation.Tests.Transportation
{
    public class AffordanceBaseTransportDecoratorTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunDoubleActivationTest()
        {
            var rnd = new Random(1);
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
                HouseholdKey key = new HouseholdKey("hh1");
                using (var calcRepo = SetupFullWorkingTransportationExample(wd, rnd, out _, out var srcloc, out var dstloc, out var srcSite, out var dstSite, out var sourceAff, out var transportationHandler, out var abt, calcParameters, key))
                {
                    //make sure there is a travel route
                    TimeStep ts = new TimeStep(0, 0, false);
                    var affs = dstloc.Affordances.ToList();
                    var aff = affs[0];
                    var person = new CalcPersonDto("activator", null, 30, PermittedGender.All, null, null, null, -1, null, null);
                    var travelroute = transportationHandler.GetTravelRouteFromSrcLoc(srcloc, dstSite,
                        ts, person, sourceAff, calcRepo);
                    Assert.NotNull(travelroute);
                    // find if busy
                    var isbusy = abt.IsBusy(ts, srcloc, person, false);
                    isbusy.Should().Be(BusynessType.NotBusy);

                    Logger.Info("Activating affordance for time 0");
                    aff.Activate(ts, person.Name, srcloc, out _);
                }
                //should throw exception the second time.
                Logger.Info("Activating affordance again for time 0");
                //this should throw, since it is already busy
                //Assert.Throws<LPGException>(() =>affs[0].Activate(0, "activator", null, srcloc, new Dictionary<int, CalcProfile>(), out _));
                CalcAffordance.DoubleCheckBusyArray = false;
                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunTransportDecoratorTest()
        {
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                Config.IsInUnitTesting = true;
                var rnd = new Random(1);
                CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
                HouseholdKey key = new HouseholdKey("hh1");
                using (var calcRepo = SetupFullWorkingTransportationExample(wd, rnd, out _, out var srcloc, out var dstloc, out var srcSite,
                    out var dstSite, out var sourceAff, out var transportationHandler, out var abt, calcParameters, key))
                {
                    //make sure there is a travel route
                    TimeStep ts = new TimeStep(0, 0, false);
                    var affs = dstloc.Affordances.ToList();
                    var aff = affs[0];
                    var person = new CalcPersonDto("activator", null, 30, PermittedGender.All, null, null, null, -1, null, null);

                    var travelroute = transportationHandler.GetTravelRouteFromSrcLoc(srcloc,
                        dstSite, ts, person, sourceAff, calcRepo);
                    Assert.NotNull(travelroute);
                    // find if busy
                    var isbusy = abt.IsBusy(ts, srcloc, person, false);
                    isbusy.Should().Be(BusynessType.NotBusy);

                    Logger.Info("Activating affordance for time 0");
                    var ownerships = new DeviceOwnershipMapping<string, CalcTransportationDevice>();
                    travelroute.GetDuration(ts, person, new List<CalcTransportationDevice>(), ownerships);
                    aff.Activate(ts, "activator", srcloc, out var _);
                }
                //should throw exception the second time.
                Logger.Info("Activating affordance again for time 0");

                CalcAffordance.DoubleCheckBusyArray = false;
                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void RunRouteFilteringTest()
        {
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                Config.IsInUnitTesting = true;
                var rnd = new Random(1);
                CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
                HouseholdKey key = new HouseholdKey("hh1");
                using (var calcRepo = SetupFullWorkingTransportationExample(wd, rnd, out _, out var srcloc, out var dstloc, out var srcSite, out var dstSite, out var sourceAff, out var transportationHandler, out var abt, calcParameters, key))
                {
                    TimeStep ts = new TimeStep(0, 0, false);
                    var affs = dstloc.Affordances.ToList();
                    var aff = affs[0];
                    var person = new CalcPersonDto("activator", null, 30, PermittedGender.All, null, null, null, -1, null, null);

                    var route = transportationHandler.GetTravelRouteFromSrcLoc(srcloc, dstSite, ts, person, sourceAff, calcRepo);
                    Assert.Equal("myRoute2", route.Name); // due to affordance tag


                    var untaggedAff = new CalcAffordance("UntaggedAffordance", new CalcProfile("calcprofile", Guid.NewGuid().ToStrGuid(), null, ProfileType.Absolute, "syn"),
                        dstloc, false, null, 18, 50, PermittedGender.All, false, 0.1, LPGColors.Blue, "affordance category", false, false, new List<CalcAffordanceVariableOp>(),
                        new List<VariableRequirement>(), ActionAfterInterruption.GoBackToOld, "timelimitname", 1, false, "srctrait", Guid.NewGuid().ToStrGuid(), null,
                        new List<CalcAffordance.DeviceEnergyProfileTuple>(), new BitArray(0, false), BodilyActivityLevel.Low, calcRepo, null);

                    person = new CalcPersonDto("activator", null, 30, PermittedGender.Female, null, null, null, -1, null, null);
                    route = transportationHandler.GetTravelRouteFromSrcLoc(srcloc, dstSite, ts, person, untaggedAff, calcRepo);
                    Assert.Equal("myRoute2", route.Name); // due to gender

                    person = new CalcPersonDto("activator", null, 10, PermittedGender.All, null, null, null, -1, null, null);
                    route = transportationHandler.GetTravelRouteFromSrcLoc(srcloc, dstSite, ts, person, untaggedAff, calcRepo);
                    Assert.Equal("myRoute1", route.Name); // due to minimum age

                    person = new CalcPersonDto("activator", null, 100, PermittedGender.All, null, null, null, -1, null, null);
                    route = transportationHandler.GetTravelRouteFromSrcLoc(srcloc, dstSite, ts, person, untaggedAff, calcRepo);
                    Assert.Equal("myRoute2", route.Name); // due to maximum age
                }
                CalcAffordance.DoubleCheckBusyArray = false;
                wd.CleanUp();
            }
        }

    [JetBrains.Annotations.NotNull]
        private static CalcRepo SetupFullWorkingTransportationExample([JetBrains.Annotations.NotNull] WorkingDir wd, [JetBrains.Annotations.NotNull] Random rnd, [JetBrains.Annotations.NotNull] out NormalRandom nr,
                                                                      [JetBrains.Annotations.NotNull] out CalcLocation srcloc, [JetBrains.Annotations.NotNull] out CalcLocation dstloc, [JetBrains.Annotations.NotNull] out CalcSite srcSite, [JetBrains.Annotations.NotNull] out CalcSite dstSite,
                                                                      [JetBrains.Annotations.NotNull] out CalcAffordance affordance, [JetBrains.Annotations.NotNull] out TransportationHandler transportationHandler, [JetBrains.Annotations.NotNull] out AffordanceBaseTransportDecorator abt, [JetBrains.Annotations.NotNull] CalcParameters calcParameters,
                                                                      [JetBrains.Annotations.NotNull] HouseholdKey key)
        {
            Config.IsInUnitTesting = true;
            CalcAffordance.DoubleCheckBusyArray = true;
            nr = new NormalRandom(0, 0.1, rnd);
            var calcprofilevalues = new List<double> {
                10,
                20,
                30
            };

            var cp = new CalcProfile("calcprofile", Guid.NewGuid().ToStrGuid(), calcprofilevalues, ProfileType.Absolute, "syn");
            srcloc = new CalcLocation("srclocation", Guid.NewGuid().ToStrGuid());
            dstloc = new CalcLocation("dstlocation", Guid.NewGuid().ToStrGuid());
            var calcdesire =
                new CalcDesire("calcdesire", 1, 0.5m, 10, 1, 1, 60, 0.1m, null, "sourcetrait", "desirecat");
            var calcdesires = new List<CalcDesire> {
                calcdesire
            };
            CalcVariableRepository crv = new CalcVariableRepository();
            var hhkey = new HouseholdKey("hh1");
            Mock<IOnlineDeviceActivationProcessor> iodap = new Mock<IOnlineDeviceActivationProcessor>();
            var old = new Mock<IOnlineLoggingData>();
            using (var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hh0", wd.InputDataLogger))
            {
                using (var lf = new LogFile(calcParameters, fft, true))
                {
                    var calcRepo = new CalcRepo(odap: iodap.Object, calcParameters: calcParameters, rnd: rnd, normalRandom: nr, onlineLoggingData: old.Object, lf: lf);
                    BitArray isBusy = new BitArray(calcParameters.InternalTimesteps, false);
                    affordance = new CalcAffordance("calcaffordance", cp, dstloc, false, calcdesires,
                        18, 50, PermittedGender.All, false, 0.1, LPGColors.Blue, "affordance category", false,
                        false, new List<CalcAffordanceVariableOp>(), new List<VariableRequirement>(),
                        ActionAfterInterruption.GoBackToOld, "timelimitname", 1, false,
                        "srctrait",
                        Guid.NewGuid().ToStrGuid(), crv, new List<CalcAffordance.DeviceEnergyProfileTuple>(),
                        isBusy, BodilyActivityLevel.Low, calcRepo, hhkey);

                    srcSite = new CalcSite("srcsite", true, Guid.NewGuid().ToStrGuid(), key);
                    srcSite.Locations.Add(srcloc);
                    dstSite = new CalcSite("dstSite", true, Guid.NewGuid().ToStrGuid(), key);
                    dstSite.Locations.Add(dstloc);
                    fft.RegisterHousehold(new HouseholdKey("hh0"), "hh0-prettyname", HouseholdKeyType.Household,
                        "Desc", null, null);
                    transportationHandler = new TransportationHandler();
                    transportationHandler.AddSite(srcSite);
                    abt = new AffordanceBaseTransportDecorator(affordance, dstSite, transportationHandler,
                        "travel to dstsite", new HouseholdKey("hh0"), Guid.NewGuid().ToStrGuid(), calcRepo);
                    dstloc.AddTransportationAffordance(abt);

                    var myCategory = new CalcTransportationDeviceCategory("mycategory", false, Guid.NewGuid().ToStrGuid());
                    var route1 = new CalcTravelRoute("myRoute1", -1, 50, PermittedGender.Male, "mytaggingset", "cooking", 1.0, srcSite, dstSite,
                        transportationHandler.VehicleDepot, transportationHandler.LocationUnlimitedDevices,
                         new HouseholdKey("hh0"), Guid.NewGuid().ToStrGuid(), calcRepo);
                    route1.AddTravelRouteStep("driving", myCategory, 1, 36000, Guid.NewGuid().ToStrGuid());
                    transportationHandler.TravelRoutes.Add(route1);
                    var route2 = new CalcTravelRoute("myRoute2", 20, -1, PermittedGender.Female, "mytaggingset", "working", 1.0, srcSite, dstSite,
                        transportationHandler.VehicleDepot, transportationHandler.LocationUnlimitedDevices,
                         key, Guid.NewGuid().ToStrGuid(), calcRepo);
                    route2.AddTravelRouteStep("driving", myCategory, 1, 36000, Guid.NewGuid().ToStrGuid());
                    transportationHandler.TravelRoutes.Add(route2);

                    // build an AffordanceTaggingSet for the the TransportationHandler
                    var affTagSet = new CalcAffordanceTaggingSetDto("mytaggingset", false);
                    affTagSet.AddTag(affordance.Name, "working");
                    transportationHandler.AffordanceTaggingSets.Add(affTagSet.Name, affTagSet);

                    CalcLoadType chargingloadtype = new CalcLoadType("chargingloadtype", "W", "kwh", 1, true, Guid.NewGuid().ToStrGuid());
                    List<CalcSite> calcSites = new List<CalcSite>
            {
                srcSite,
                dstSite
            };
                    var list = new List<CalcDeviceLoad>();
                    CalcDeviceLoad cdl = new CalcDeviceLoad("bla", 1, chargingloadtype, 1, 1);
                    list.Add(cdl);
                    CalcDeviceDto cdd = new CalcDeviceDto("bus", myCategory.Guid,
                        hhkey, OefcDeviceType.Transportation, myCategory.Name, string.Empty,
                        Guid.NewGuid().ToStrGuid(), string.Empty.ToStrGuid(), string.Empty, FlexibilityType.NoFlexibility, 0);
                    var transportationDevice =
                        new CalcTransportationDevice(myCategory, 1, list, 100,
                            10, 1000, chargingloadtype, calcSites,
                            cdd, calcRepo);
                    transportationHandler.LocationUnlimitedDevices.Add(transportationDevice);
                    return calcRepo;
                }
            }
        }

        //TODO: test for interrupting while driving
        //TODO: only interrupt at the same site
        public AffordanceBaseTransportDecoratorTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
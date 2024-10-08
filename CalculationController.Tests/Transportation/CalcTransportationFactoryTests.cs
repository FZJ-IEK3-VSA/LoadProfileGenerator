﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Autofac;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.DtoFactories;
using CalculationController.Helpers;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using Common.Tests;
using Database;
using Database.Tests;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace CalculationController.Tests.Transportation {
    public class CalcTransportationFactoryTests : UnitTestBaseClass {
        [JetBrains.Annotations.NotNull]
        public CalcHouseholdDto MakeSingleFactory([JetBrains.Annotations.NotNull] WorkingDir wd, [JetBrains.Annotations.NotNull] DatabaseSetup db)
        {
            var builder = new ContainerBuilder();
            wd.InputDataLogger.AddSaver(new CalcPersonDtoLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new CalcAffordanceDtoLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new CalcVariableDtoLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new BridgeDayEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new HouseholdDtoLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new CalcSiteDtoLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new CalcTransportationDeviceDtoLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new CalcTravelRouteDtoLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new TransportationStatusLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new TransportationEventLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));

            var sim = new Simulator(db.ConnectionString);

            var mhh = sim.ModularHouseholds[0];
            var r = new Random(1);
            var ltdtoDict = CalcLoadTypeDtoFactory.MakeLoadTypes(sim.LoadTypes.Items, new TimeSpan(0, 1, 0),
                LoadTypePriority.RecommendedForHouseholds);
            var ltdict = CalcLoadTypeFactory.MakeLoadTypes(ltdtoDict);
            var parameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(2018, 1, 1)
                .SetEndDate(new DateTime(2018, 1, 1, 2, 0, 0)).SetSettlingDays(0).EnableShowSettlingPeriod();
            builder.Register(_ => parameters).As<CalcParameters>().SingleInstance();
            builder.Register(_ => new DateStampCreator(parameters)).As<DateStampCreator>().SingleInstance();
            builder.Register(_ => ltdict).As<CalcLoadTypeDictionary>().SingleInstance();
            builder.Register(_ => ltdtoDict).As<CalcLoadTypeDtoDictionary>().SingleInstance();
            builder.Register(_ => new NormalRandom(0, 1, r)).As<NormalRandom>().SingleInstance();
            builder.Register(_ => new FileFactoryAndTracker(wd.WorkingDirectory, mhh.Name, wd.InputDataLogger))
                .As<FileFactoryAndTracker>()
                .SingleInstance();
            builder.Register(_ => new SqlResultLoggingService(wd.WorkingDirectory)).As<SqlResultLoggingService>()
                .SingleInstance();
            builder.Register(_ => wd.InputDataLogger).As<IInputDataLogger>().SingleInstance();

            builder.Register(c => new OnlineLoggingData(c.Resolve<DateStampCreator>(), c.Resolve<IInputDataLogger>(),
                    c.Resolve<CalcParameters>()))
                .As<OnlineLoggingData>().As<IOnlineLoggingData>().SingleInstance();

            builder.Register(c => new LogFile(parameters,
                c.Resolve<FileFactoryAndTracker>())).As<ILogFile>().SingleInstance();
            builder.RegisterType<OnlineDeviceActivationProcessor>().As<IOnlineDeviceActivationProcessor>()
                .SingleInstance();
            builder.RegisterType<CalcModularHouseholdFactory>().As<CalcModularHouseholdFactory>().SingleInstance();
            builder.Register(_ => new DeviceCategoryPicker(r, null)).As<IDeviceCategoryPicker>().SingleInstance();

            builder.Register(_ => r).As<Random>().SingleInstance();
            builder.RegisterType<CalcDeviceFactory>().As<CalcDeviceFactory>().SingleInstance();
            builder.RegisterType<CalcDeviceDtoFactory>().As<CalcDeviceDtoFactory>().SingleInstance();
            builder.RegisterType<CalcLocationFactory>().As<CalcLocationFactory>().SingleInstance();
            builder.RegisterType<CalcLocationDtoFactory>().As<CalcLocationDtoFactory>().SingleInstance();
            builder.RegisterType<CalcPersonFactory>().As<CalcPersonFactory>().SingleInstance();
            builder.RegisterType<CalcPersonDtoFactory>().As<CalcPersonDtoFactory>().SingleInstance();
            builder.RegisterType<CalcAffordanceFactory>().As<CalcAffordanceFactory>().SingleInstance();
            builder.RegisterType<CalcAffordanceDtoFactory>().As<CalcAffordanceDtoFactory>().SingleInstance();
            builder.RegisterType<CalcTransportationFactory>().As<CalcTransportationFactory>().SingleInstance();
            builder.RegisterType<CalcModularHouseholdDtoFactory>().As<CalcModularHouseholdDtoFactory>()
                .SingleInstance();
            builder.RegisterType<CalcVariableDtoFactory>().As<CalcVariableDtoFactory>().SingleInstance();
            builder.RegisterType<CalcTransportationDtoFactory>().As<CalcTransportationDtoFactory>().SingleInstance();
            builder.RegisterType<VacationDtoFactory>().As<VacationDtoFactory>().SingleInstance();
            builder.RegisterType<AvailabilityDtoRepository>().As<AvailabilityDtoRepository>().SingleInstance();
            builder.RegisterType<CalcVariableRepository>().As<CalcVariableRepository>().SingleInstance();
            builder.RegisterType<CalcRepo>().As<CalcRepo>().SingleInstance();
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope()) {
                var hhdtofac = scope.Resolve<CalcModularHouseholdDtoFactory>();

                var tds = sim.TransportationDeviceSets[0];
                tds.SaveToDB();
                var trs = sim.TravelRouteSets[0];
                trs.SaveToDB();
                var css = sim.ChargingStationSets[0];
                css.SaveToDB();
                var fft = scope.Resolve<FileFactoryAndTracker>();
                fft.RegisterGeneralHouse();
                var dtohh = hhdtofac.MakeCalcModularHouseholdDto(sim,
                    mhh,
                    sim.TemperatureProfiles[0],
                    new HouseholdKey("hh1"),
                    sim.GeographicLocations[0],
                    out _,
                    tds,
                    trs,
                    EnergyIntensityType.Random,
                    css);
                fft.Dispose();
                db.Cleanup();
                return dtohh;
            }
        }

        [Fact]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void DuplicateDtoFactoryTest()
        {
            using var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass() + "1");
            using var db1 = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var dto1 = MakeSingleFactory(wd1,db1);
            using var wd2 = new WorkingDir(Utili.GetCurrentMethodAndClass() + "2");
            using var db2 = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var dto2 = MakeSingleFactory(wd2,db2);
            var devices1 = dto1.DeviceDtos.Select(x => x.Name).ToList();
            var devices2 = dto2.DeviceDtos.Select(x => x.Name).ToList();
            devices1.Should().BeEquivalentTo(devices2);
        }

        [Fact]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void TestSavingAndLoadingHouseholdDto()
        {
            using var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass() + "1");
            using var db1 = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            var dto1 = MakeSingleFactory(wd1, db1);
            HouseholdDtoLogger hdl = new HouseholdDtoLogger(wd1.SqlResultLoggingService);
            HouseholdKeyLogger hkl = new HouseholdKeyLogger(wd1.SqlResultLoggingService);
            var json = JsonConvert.SerializeObject(dto1, Formatting.Indented);
             var keys = hkl.Load();
             foreach (var key in keys) {
                 if (key.KeyType != HouseholdKeyType.Household) {
                     continue;
                 }

                 var loadeddto = hdl.Load(key.HHKey);
                 loadeddto.Should().BeEquivalentTo(dto1);
             }
        }


        //[Fact]
        //[SuppressMessage("ReSharper", "UnusedVariable")]
        //[Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        //public void DuplicateDevicesFromFactoryTest()
        //{
        //    var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass() + "1");
        //    using var db1 = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
        //    var dto1 = MakeSingleFactory(wd1, db1);
        //    var devices1 = dto1.AutoDevices.Select(x => x).ToList();
        //    RowCollection rc = new RowCollection("devices");
        //    foreach (var device in devices1) {
        //        rc.Add(XlsRowBuilder.GetAllProperties(device));
        //    }
        //    XlsxDumper.WriteToXlsx(wd1.Combine("devices.xlsx"),rc);
        //}

        [Fact]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunMakeCalcTest()
        {
            //TODO: fix the container registering
            var builder = new ContainerBuilder();
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass())) {
                var path = wd.WorkingDirectory;
                var inputlogger = wd.InputDataLogger;
                wd.InputDataLogger.AddSaver(new CalcPersonDtoLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new CalcAffordanceDtoLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new CalcVariableDtoLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new BridgeDayEntryLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new HouseholdDtoLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new CalcSiteDtoLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new CalcTransportationDeviceDtoLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new CalcTravelRouteDtoLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new TransportationStatusLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new TransportationEventLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));

                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass())) {
                    var sim = new Simulator(db.ConnectionString);

                    var mhh = sim.ModularHouseholds[0];
                    var r = new Random(1);
                    var dcp = new DeviceCategoryPicker(r, null);
                    var ltdtoDict = CalcLoadTypeDtoFactory.MakeLoadTypes(sim.LoadTypes.Items, new TimeSpan(0, 1, 0),
                        LoadTypePriority.RecommendedForHouseholds);
                    var ltdict = CalcLoadTypeFactory.MakeLoadTypes(ltdtoDict);
                    //var picker = new DeviceCategoryPicker(r,null);
                    var nr = new NormalRandom(0, 0.1, r);
                    var parameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(2018, 1, 1)
                        .SetEndDate(new DateTime(2018, 1, 1, 2, 0, 0)).SetSettlingDays(0).EnableShowSettlingPeriod();
                    parameters.TransportationEnabled = true;
                    builder.Register(_ => parameters).As<CalcParameters>().SingleInstance();
                    builder.Register(_ => new DateStampCreator(parameters)).As<DateStampCreator>().SingleInstance();
                    builder.Register(_ => ltdict).As<CalcLoadTypeDictionary>().SingleInstance();
                    builder.Register(_ => ltdtoDict).As<CalcLoadTypeDtoDictionary>().SingleInstance();
                    builder.Register(_ => new NormalRandom(0, 1, r)).As<NormalRandom>().SingleInstance();
                    builder.Register(_ => new FileFactoryAndTracker(path, mhh.Name, inputlogger))
                        .As<FileFactoryAndTracker>().SingleInstance();
                    builder.Register(_ => new SqlResultLoggingService(path)).As<SqlResultLoggingService>()
                        .SingleInstance();
                    builder.Register(_ => inputlogger).As<IInputDataLogger>().As<InputDataLogger>().SingleInstance();

                    builder.Register(c => new OnlineLoggingData(c.Resolve<DateStampCreator>(),
                            c.Resolve<IInputDataLogger>(), c.Resolve<CalcParameters>())).As<OnlineLoggingData>().As<IOnlineLoggingData>()
                        .SingleInstance();

                    builder.Register(c => new LogFile(parameters, c.Resolve<FileFactoryAndTracker>())).As<ILogFile>()
                        .SingleInstance();
                    builder.RegisterType<OnlineDeviceActivationProcessor>().As<IOnlineDeviceActivationProcessor>()
                        .SingleInstance();
                    builder.RegisterType<CalcModularHouseholdFactory>().As<CalcModularHouseholdFactory>()
                        .SingleInstance();
                    builder.Register(_ => new DeviceCategoryPicker(r, null)).As<IDeviceCategoryPicker>()
                        .SingleInstance();

                    builder.Register(_ => r).As<Random>().SingleInstance();
                    builder.RegisterType<CalcDeviceFactory>().As<CalcDeviceFactory>().SingleInstance();
                    builder.RegisterType<CalcDeviceDtoFactory>().As<CalcDeviceDtoFactory>().SingleInstance();
                    builder.RegisterType<CalcLocationFactory>().As<CalcLocationFactory>().SingleInstance();
                    builder.RegisterType<CalcLocationDtoFactory>().As<CalcLocationDtoFactory>().SingleInstance();
                    builder.RegisterType<CalcPersonFactory>().As<CalcPersonFactory>().SingleInstance();
                    builder.RegisterType<CalcPersonDtoFactory>().As<CalcPersonDtoFactory>().SingleInstance();
                    builder.RegisterType<CalcAffordanceFactory>().As<CalcAffordanceFactory>().SingleInstance();
                    builder.RegisterType<CalcAffordanceDtoFactory>().As<CalcAffordanceDtoFactory>().SingleInstance();
                    builder.RegisterType<CalcTransportationFactory>().As<CalcTransportationFactory>().SingleInstance();
                    builder.RegisterType<CalcModularHouseholdDtoFactory>().As<CalcModularHouseholdDtoFactory>()
                        .SingleInstance();
                    builder.RegisterType<CalcVariableDtoFactory>().As<CalcVariableDtoFactory>().SingleInstance();
                    builder.RegisterType<CalcTransportationDtoFactory>().As<CalcTransportationDtoFactory>()
                        .SingleInstance();
                    builder.RegisterType<VacationDtoFactory>().As<VacationDtoFactory>().SingleInstance();
                    builder.RegisterType<AvailabilityDtoRepository>().As<AvailabilityDtoRepository>().SingleInstance();
                    builder.RegisterType<CalcVariableRepository>().As<CalcVariableRepository>().SingleInstance();
                    builder.RegisterType<CalcRepo>().As<CalcRepo>().SingleInstance();
                    var container = builder.Build();
                    using (var scope = container.BeginLifetimeScope()) {
                        var hhdtofac = scope.Resolve<CalcModularHouseholdDtoFactory>();

                        var tds = sim.TransportationDeviceSets[0];
                        tds.SaveToDB();
                        var trs = sim.TravelRouteSets[0];
                        trs.SaveToDB();
                        var css = sim.ChargingStationSets[0];
                        css.SaveToDB();
                        /*Site home = sim.Sites.CreateNewItem(sim.ConnectionString);
                        home.Name = "home";
                        home.SaveToDB();
                        Site outside = sim.Sites.CreateNewItem(sim.ConnectionString);
                        outside.Name = "outside";
                        outside.SaveToDB();
                        home.AddLocation(sim.Locations.SafeFindByName("Living room", FindMode.IgnoreCase));
                        home.AddLocation(sim.Locations.SafeFindByName("Kitchen", FindMode.IgnoreCase));
                        home.AddLocation(sim.Locations.SafeFindByName("Bath", FindMode.IgnoreCase));
                        home.AddLocation(sim.Locations.SafeFindByName("Bedroom", FindMode.IgnoreCase));
                        home.AddLocation(sim.Locations.SafeFindByName("Children's room", FindMode.IgnoreCase));
                        outside.AddLocation(sim.Locations.SafeFindByName("Dance Studio", FindMode.IgnoreCase));
                        outside.AddLocation(sim.Locations.SafeFindByName("Supermarket", FindMode.IgnoreCase));
                        outside.AddLocation(sim.Locations.SafeFindByName("Garden", FindMode.IgnoreCase));
                        outside.AddLocation(sim.Locations.SafeFindByName("Museum", FindMode.IgnoreCase));
                        outside.AddLocation(sim.Locations.SafeFindByName("School", FindMode.IgnoreCase));
                        outside.AddLocation(sim.Locations.SafeFindByName("Food Market", FindMode.IgnoreCase));
                        outside.AddLocation(sim.Locations.SafeFindByName("Sidewalk", FindMode.IgnoreCase));
                        outside.AddLocation(sim.Locations.SafeFindByName("Office Workplace 1", FindMode.IgnoreCase));
                        TravelRoute tr = new TravelRoute(null, db.ConnectionString, "tr1", "desc", home, outside);
                        tr.SaveToDB();
                        trs.AddRoute(tr);*/
                        var fft = scope.Resolve<FileFactoryAndTracker>();
                        fft.RegisterGeneralHouse();
                        var ctf = scope.Resolve<CalcTransportationFactory>();
                        //LogFile lf = new LogFile(wd.WorkingDirectory, "hh1", true);
                        //lf.RegisterKey("hh1", "hh1-prettyname");
                        var dtohh = hhdtofac.MakeCalcModularHouseholdDto(sim, mhh,
                            sim.TemperatureProfiles[0], new HouseholdKey("hh1"), sim.GeographicLocations[0],
                            out var dtolocs, tds, trs,
                            EnergyIntensityType.Random, css);
                        var cvdto = scope.Resolve<CalcVariableDtoFactory>();
                        var cvr = scope.Resolve<CalcVariableRepository>();
                        foreach (var v in cvdto.VariableDtos.Values) {
                            cvr.RegisterVariable(new CalcVariable(v.Name, v.Guid, v.Value,
                                v.LocationName, v.LocationGuid, v.HouseholdKey));
                        }

                        var cmhf = scope.Resolve<CalcModularHouseholdFactory>();
                        CalcTransportationDtoFactory dtoFactory = new CalcTransportationDtoFactory(ltdtoDict);
                        //dtoFactory.MakeTransportationDtos(sim, sim.ModularHouseholds[0], tds, trs,css, out var sites,out var transportationDevices, out var routes, dtohh.LocationDtos, dtohh.HouseholdKey);
                        var calcRepo = scope.Resolve<CalcRepo>();
                        var affordanceTaggingSets = new System.Collections.Generic.List<CalcAffordanceTaggingSetDto>();
                        var chh = cmhf.MakeCalcModularHousehold(dtohh, out var dtoCalcLocationDict, null, null, affordanceTaggingSets,
                            calcRepo);
                        //ctf.MakeTransportation(dtohh,dtoCalcLocationDict,chh);
                        if (chh.TransportationHandler == null) {
                            throw new LPGException("no transportation handler");
                        }

                        var src = chh.TransportationHandler.CalcSites[0].Locations.ElementAt(0);
                        var dst = chh.TransportationHandler.CalcSites[1].Locations.ElementAt(0);
                        var ts = new TimeStep(1, parameters);
                        var person = new CalcPersonDto("personname", null, 30, PermittedGender.Male, null, null, null, -1, null, null);
                        dst.Affordances[0].IsBusy(ts, src, person, false);
                        dst.Affordances[0].Activate(ts, person.Name, src, out var personTimeProfile);
                        fft.Dispose();
                    }

                    db.Cleanup();
                }

                wd.CleanUp();
            }
        }

        public CalcTransportationFactoryTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
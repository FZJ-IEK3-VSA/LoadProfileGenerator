using System;
using System.Diagnostics.CodeAnalysis;
using Autofac;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.DtoFactories;
using CalculationController.Helpers;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using Common.Tests;
using Database;
using Database.Tables.ModularHouseholds;
using Database.Tables.Transportation;
using Database.Tests;
using NUnit.Framework;

namespace CalculationController.Tests.Transportation
{
    [TestFixture]
    public class CalcTransportationFactoryTests : UnitTestBaseClass
    {
        [Test]
        [SuppressMessage("ReSharper", "UnusedVariable")]
        [Category(UnitTestCategories.BasicTest)]
        public void RunMakeCalcTest()
        {
            //TODO: fix the container registering
            var builder = new ContainerBuilder();
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
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

            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(),DatabaseSetup.TestPackage.CalcController);
            Simulator sim = new Simulator(db.ConnectionString);

            ModularHousehold mhh = sim.ModularHouseholds[0];
            Random r = new  Random(1);
            DeviceCategoryPicker dcp = new   DeviceCategoryPicker(r,null);
            var ltdtoDict = CalcLoadTypeDtoFactory.MakeLoadTypes(sim.LoadTypes.It, new TimeSpan(0, 1, 0),
                LoadTypePriority.RecommendedForHouseholds);
            var ltdict = CalcLoadTypeFactory.MakeLoadTypes(ltdtoDict);
            //var picker = new DeviceCategoryPicker(r,null);
            NormalRandom nr = new NormalRandom(0,0.1,r);
            CalcParameters parameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(2018,1,1).SetEndDate(new DateTime(2018, 1, 1, 2, 0, 0)).SetSettlingDays(0).EnableShowSettlingPeriod();
            builder.Register(x => parameters).As<CalcParameters>().SingleInstance();
            builder.Register(x => new DateStampCreator(parameters)).As<DateStampCreator>().SingleInstance();
            builder.Register(c => ltdict).As<CalcLoadTypeDictionary>().SingleInstance();
            builder.Register(c => ltdtoDict).As<CalcLoadTypeDtoDictionary>().SingleInstance();
            builder.Register(c => new NormalRandom(0,1,r)).As<NormalRandom>().SingleInstance();
            builder.Register(c => new FileFactoryAndTracker(wd.WorkingDirectory, mhh.Name,wd.InputDataLogger)).As<FileFactoryAndTracker>().SingleInstance();
            builder.Register(c => new SqlResultLoggingService(wd.WorkingDirectory)).As<SqlResultLoggingService>().SingleInstance();
            builder.Register(c=> wd.InputDataLogger).As<IInputDataLogger>().SingleInstance();

            builder.Register(c => new OnlineLoggingData(c.Resolve<DateStampCreator>(), c.Resolve<IInputDataLogger>(),c.Resolve<CalcParameters>())).As<OnlineLoggingData>().SingleInstance();

            builder.Register(c => new LogFile(parameters,c.Resolve<FileFactoryAndTracker>(),
                c.Resolve<OnlineLoggingData>(),
                c.Resolve<SqlResultLoggingService>())).As<ILogFile>().SingleInstance();
            builder.RegisterType<OnlineDeviceActivationProcessor>().As<IOnlineDeviceActivationProcessor>().SingleInstance();
            builder.RegisterType<CalcModularHouseholdFactory>().As<CalcModularHouseholdFactory>().SingleInstance();
            builder.Register(x => new DeviceCategoryPicker(r, null)).As<IDeviceCategoryPicker>().SingleInstance();

            builder.Register(x => r).As<Random>().SingleInstance();
            builder.RegisterType<CalcDeviceFactory>().As<CalcDeviceFactory>().SingleInstance();
            builder.RegisterType<CalcDeviceDtoFactory>().As<CalcDeviceDtoFactory>().SingleInstance();
            builder.RegisterType<CalcLocationFactory>().As<CalcLocationFactory>().SingleInstance();
            builder.RegisterType<CalcLocationDtoFactory>().As<CalcLocationDtoFactory>().SingleInstance();
            builder.RegisterType<CalcPersonFactory>().As<CalcPersonFactory>().SingleInstance();
            builder.RegisterType<CalcPersonDtoFactory>().As<CalcPersonDtoFactory>().SingleInstance();
            builder.RegisterType<CalcAffordanceFactory>().As<CalcAffordanceFactory>().SingleInstance();
            builder.RegisterType<CalcAffordanceDtoFactory>().As<CalcAffordanceDtoFactory>().SingleInstance();
            builder.RegisterType<CalcTransportationFactory>().As<CalcTransportationFactory>().SingleInstance();
            builder.RegisterType<CalcModularHouseholdDtoFactory>().As<CalcModularHouseholdDtoFactory>().SingleInstance();
            builder.RegisterType<CalcVariableDtoFactory>().As<CalcVariableDtoFactory>().SingleInstance();
            builder.RegisterType<CalcTransportationDtoFactory>().As<CalcTransportationDtoFactory>().SingleInstance();
            builder.RegisterType<VacationDtoFactory>().As<VacationDtoFactory>().SingleInstance();
            builder.RegisterType<AvailabilityDtoRepository>().As<AvailabilityDtoRepository>().SingleInstance();
            builder.RegisterType<CalcVariableRepository>().As<CalcVariableRepository>().SingleInstance();
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope())
            {
                var hhdtofac = scope.Resolve<CalcModularHouseholdDtoFactory>();

                TransportationDeviceSet tds = sim.TransportationDeviceSets[0];
                tds.SaveToDB();
                TravelRouteSet trs = sim.TravelRouteSets[0];
                trs.SaveToDB();
                ChargingStationSet css = sim.ChargingStationSets[0];
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
                var lf = scope.Resolve<ILogFile>();
                lf.InitHousehold(Constants.GeneralHouseholdKey, "General", HouseholdKeyType.General,"desc",null,null);
                var ctf = scope.Resolve<CalcTransportationFactory>();
                //LogFile lf = new LogFile(wd.WorkingDirectory, "hh1", true);
                //lf.RegisterKey("hh1", "hh1-prettyname");
                var dtohh = hhdtofac.MakeCalcModularHouseholdDto(sim, mhh,

                    sim.TemperatureProfiles[0], new HouseholdKey("hh1"), sim.GeographicLocations[0],
                    out var dtolocs, tds,trs,
                    EnergyIntensityType.Random,css,parameters);
                CalcVariableDtoFactory cvdto = scope.Resolve<CalcVariableDtoFactory>();
                CalcVariableRepository cvr = scope.Resolve<CalcVariableRepository>();
                foreach (CalcVariableDto v in cvdto.VariableDtos.Values) {
                    cvr.RegisterVariable(new CalcVariable(v.Name,v.Guid,v.Value,
                        v.LocationName,v.LocationGuid,v.HouseholdKey));
                }
                var cmhf = scope.Resolve<CalcModularHouseholdFactory>();
                //CalcTransportationDtoFactory dtoFactory = new CalcTransportationDtoFactory(ltdtoDict);
                //dtoFactory.MakeTransportationDtos(sim, sim.ModularHouseholds[0], tds, trs, out var sites,out var transportationDevices, out var routes, dtohh.LocationDtos, dtohh.HouseholdKey);
                CalcHousehold chh = cmhf.MakeCalcModularHousehold(dtohh,out var dtoCalcLocationDict,null,null);
                //ctf.MakeTransportation(dtohh,dtoCalcLocationDict,chh);
                if(chh.TransportationHandler == null) {
                    throw new LPGException("no transportation handler");
                }

                CalcLocation src = chh.TransportationHandler.CalcSites[0].Locations[0];
                CalcLocation dst = chh.TransportationHandler.CalcSites[1].Locations[0];
                const string personname = "personname";
                TimeStep ts = new TimeStep(1,parameters);
                dst.Affordances[0].IsBusy(ts, nr, r, src, personname, false);
                dst.Affordances[0].Activate(ts, personname,  src,  out var personTimeProfile);
                lf.Close();
            }

            db.Cleanup();
            wd.CleanUp();
        }
    }
}

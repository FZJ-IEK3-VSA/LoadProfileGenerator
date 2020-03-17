using System.IO;
using Automation;
using CalculationController.DtoFactories;
using CalculationController.InputLoggers;
using Common;
using Common.SQLResultLogging;
using Common.Tests;
using Database;
using Database.Tests;
using NUnit.Framework;

namespace CalculationController.Tests.InputLoggers {
    public class TemperatureLoggerTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RunTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            SqlResultLoggingService srls =
                new SqlResultLoggingService(Path.Combine(wd.WorkingDirectory));
            var calcParameters = CalcParametersFactory.MakeGoodDefaults();
            calcParameters.Enable(CalcOption.TemperatureFile);
            DatabaseSetup ds = new DatabaseSetup(Utili.GetCurrentMethodAndClass(),
                DatabaseSetup.TestPackage.CalcController);
            Simulator sim = new Simulator(ds.ConnectionString);
            TemperatureDataLogger tdl = new TemperatureDataLogger(srls, calcParameters);
            tdl.Run(Constants.GeneralHouseholdKey, sim.TemperatureProfiles[0]);
            ds.Cleanup();
            wd.CleanUp();
        }
        /*
        [Test]
        public void RunTest2()
        {
            ContainerBuilder cb = new ContainerBuilder();
            cb.RegisterType<CalcHouseholdDtoSaver>().As<IDataSaverBase>();
            cb.RegisterType<CalcHouseDtoSaver>().As<IDataSaverBase>();
            cb.RegisterType<CalObjectSaver>().As<IDataSaverBase>();
            cb.RegisterType<InputLoggerProto>().As<InputLoggerProto>();
            var container = cb.Build();
            using (var scope = container.BeginLifetimeScope()) {
                var ipl = scope.Resolve<InputLoggerProto>();
                string s = "bla";
                ipl.Save(s);
            }
        }*/
    }
}

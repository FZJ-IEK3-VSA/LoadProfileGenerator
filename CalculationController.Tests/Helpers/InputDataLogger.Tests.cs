using Automation;
using NUnit.Framework;

namespace CalculationController.Tests.Helpers
{
    internal class InputDataLoggerTests
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RunConfigSaveTest()
        {
            //TODO: Fix
            //WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            //SqlResultLoggingService srls = new SqlResultLoggingService(Path.Combine(wd.WorkingDirectory,"mysqlfile.sqlite"));
            /*CalcStartParameterSet csps = new CalcStartParameterSet(wd.WorkingDirectory,
                new GeographicLocation("blub",1,1,1,1,1,1,"1","2","",null),new TemperatureProfile("blub",null,"a",null),
                null,EnergyIntensityType.Random,false,"1.1.1",null,LoadTypePriority.All,null,null,new List<CalcOption>(),
                new DateTime(2015, 1, 1), new DateTime(2015, 1, 3), new TimeSpan(0, 1, 0), ";", 5, new TimeSpan(0, 1, 0),false,false,false,3);*/
            //CalcParameters
            //InputDataLogger idl = new InputDataLogger(srls, csps, null);
            //
            //idl.SaveCalculationParameters();
            //wd.CleanUp();
        }
    }
}

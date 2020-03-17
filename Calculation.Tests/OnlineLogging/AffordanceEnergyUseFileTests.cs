using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using Common.Tests;
using NUnit.Framework;

namespace Calculation.Tests.OnlineLogging
{
    [TestFixture]
    public class AffordanceEnergyUseFileTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RegisterDeviceActivationTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new DeviceActivationEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
            CalcParameters cp = CalcParametersFactory.MakeGoodDefaults().EnableShowSettlingPeriod();
            var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hhname",wd.InputDataLogger);
            var key = new HouseholdKey(" hh1");
            fft.HouseholdRegistry.RegisterHousehold(key, "hh key", HouseholdKeyType.Household, wd.InputDataLogger,"desc", null, null);
            fft.HouseholdRegistry.RegisterHousehold(Constants.GeneralHouseholdKey, "general", HouseholdKeyType.General, wd.InputDataLogger, "desc", null, null);
            DateStampCreator dsc = new DateStampCreator(cp);
            OnlineLoggingData old = new OnlineLoggingData(dsc,wd.InputDataLogger,cp);
            using (LogFile lf = new LogFile(cp,fft, old, wd.SqlResultLoggingService)) {
                fft.HouseholdRegistry.RegisterHousehold( key,"hh key",HouseholdKeyType.Household,wd.InputDataLogger,"desc", null, null);
                CalcLoadTypeDto clt = new CalcLoadTypeDto("lt","unitofpower","unitofsum",1,true,"guid");
                DeviceActivationEntry aeue = new DeviceActivationEntry(key, "affname", clt,
                    1, "activatorname", "devname", 1);
                lf.OnlineLoggingData.RegisterDeviceActivation(aeue);
                lf.OnlineLoggingData.FinalSaveToDatabase();
            }
            wd.CleanUp();
        }
    }
}
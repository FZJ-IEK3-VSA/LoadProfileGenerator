using System;
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
                fft.HouseholdRegistry.RegisterHousehold( key,"hh key",HouseholdKeyType.Household,wd.InputDataLogger,"desc", null, null);
                CalcLoadTypeDto clt = new CalcLoadTypeDto("lt","unitofpower","unitofsum",1,true,"guid");
                TimeStep ts = new TimeStep(1,1,true);
                CalcDeviceDto cdd = new CalcDeviceDto("devname","",key,
                    OefcDeviceType.Device,"devcatname","", Guid.NewGuid().ToString(),"locguid","locname");
                DeviceActivationEntry aeue = new DeviceActivationEntry( "affname", clt, 1, "activatorname", 1,ts, cdd);
                old.RegisterDeviceActivation(aeue);
                old.FinalSaveToDatabase();
            wd.CleanUp();
        }
    }
}
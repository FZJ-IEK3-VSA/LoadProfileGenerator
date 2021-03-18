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
using Xunit;
using Xunit.Abstractions;

namespace Calculation.Tests.OnlineLogging
{
    public class AffordanceEnergyUseFileTests : UnitTestBaseClass
    {
        public AffordanceEnergyUseFileTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RegisterDeviceActivationTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new DeviceActivationEntryLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
                CalcParameters cp = CalcParametersFactory.MakeGoodDefaults().EnableShowSettlingPeriod();
                using (var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hhname", wd.InputDataLogger))
                {
                    var key = new HouseholdKey(" hh1");
                    fft.HouseholdRegistry.RegisterHousehold(key, "hh key", HouseholdKeyType.Household, wd.InputDataLogger, "desc", null, null);
                    fft.HouseholdRegistry.RegisterHousehold(Constants.GeneralHouseholdKey, "general", HouseholdKeyType.General, wd.InputDataLogger, "desc", null, null);
                    DateStampCreator dsc = new DateStampCreator(cp);
                    using OnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger, cp);
                    fft.HouseholdRegistry.RegisterHousehold(key, "hh key", HouseholdKeyType.Household, wd.InputDataLogger, "desc", null, null);
                    CalcLoadTypeDto clt = new CalcLoadTypeDto("lt", "unitofpower", "unitofsum", 1, true, "guid".ToStrGuid());
                    TimeStep ts = new TimeStep(1, 1, true);
                    CalcDeviceDto cdd = new CalcDeviceDto("devname", "".ToStrGuid(), key,
                        OefcDeviceType.Device, "devcatname", "", Guid.NewGuid().ToStrGuid(), "locguid".ToStrGuid(), "locname", FlexibilityType.NoFlexibility, 0);
                    DeviceActivationEntry aeue = new DeviceActivationEntry("affname", clt, 1, "activatorname", 1, ts, cdd);
                    old.RegisterDeviceActivation(aeue);
                    old.FinalSaveToDatabase();
                }
                wd.CleanUp();
            }
        }
    }
}
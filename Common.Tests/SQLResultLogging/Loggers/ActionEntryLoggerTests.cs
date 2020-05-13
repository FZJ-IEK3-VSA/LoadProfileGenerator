using Automation;
using Automation.ResultFiles;
using System;
using System.Collections.Generic;
using Common.CalcDto;
using Common.Enums;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Common.Tests.SQLResultLogging.Loggers
{

    public class CalcLoadtypeDtoLoggerTest {
        [Test()]
        [Category(UnitTestCategories.BasicTest)]
        public void RunCalcLoadtypeDtoLoggerTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                CalcLoadTypeDtoLogger cldtl = new CalcLoadTypeDtoLogger(wd.SqlResultLoggingService);
                CalcLoadTypeDto cdls = new CalcLoadTypeDto("name", "unit", "unit", 5, true, "guid".ToStrGuid());
                List<CalcLoadTypeDto> cdlsList = new List<CalcLoadTypeDto>();
                cdlsList.Add(cdls);
                cldtl.Run(Constants.GeneralHouseholdKey, cdlsList);
                cldtl.Load();
            }
        }
    }

    [TestFixture()]
    public class ActionEntryLoggerTests : UnitTestBaseClass
    {
        [Test()]
        [Category(UnitTestCategories.BasicTest)]
        public void RunTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                ActionEntryLogger ael = new ActionEntryLogger(wd.SqlResultLoggingService);
                HouseholdKey key = new HouseholdKey("hhkey");
                List<IDataSaverBase> savers = new List<IDataSaverBase>
            {
                ael
            };
                InputDataLogger idl = new InputDataLogger(savers.ToArray());
                TimeStep ts = new TimeStep(1, 0, true);
                ActionEntry ae1 = new ActionEntry("blub", key, ts, DateTime.Now, "123".ToStrGuid(), "name", false,
                    "affname", "affguid".ToStrGuid(), 0, BodilyActivityLevel.Low);
                List<IHouseholdKey> aes = new List<IHouseholdKey>
            {
                ae1
            };
                idl.SaveList(aes);
                var res = ael.Read(key);
                var s1 = JsonConvert.SerializeObject(aes, Formatting.Indented);
                var s2 = JsonConvert.SerializeObject(res, Formatting.Indented);
                Assert.AreEqual(s1, s2);
                wd.CleanUp();
            }
        }
    }
}
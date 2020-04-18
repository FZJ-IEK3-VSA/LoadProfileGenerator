﻿using Automation;
using Automation.ResultFiles;
using System;
using System.Collections.Generic;
using Common.CalcDto;
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
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            CalcLoadTypeDtoLogger cldtl = new CalcLoadTypeDtoLogger(wd.SqlResultLoggingService);
            CalcLoadTypeDto cdls = new CalcLoadTypeDto("name", "unit", "unit", 5, true, "guid");
            cldtl.Run(Constants.GeneralHouseholdKey, cdls);
            cldtl.Load();
        }
    }

    [TestFixture()]
    public class ActionEntryLoggerTests : UnitTestBaseClass
    {
        [Test()]
        [Category(UnitTestCategories.BasicTest)]
        public void RunTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            ActionEntryLogger ael = new ActionEntryLogger(wd.SqlResultLoggingService);
            HouseholdKey key = new HouseholdKey("hhkey");
            List<IDataSaverBase> savers = new List<IDataSaverBase>
            {
                ael
            };
            InputDataLogger idl = new InputDataLogger(savers.ToArray());
            TimeStep ts = new TimeStep(1,0,true);
            ActionEntry ae1 = new ActionEntry("blub",key,ts,DateTime.Now, "123","name",false,
                "affname","affguid",0);
            List<IHouseholdKey> aes = new List<IHouseholdKey>
            {
                ae1
            };
            idl.SaveList(aes);
            var res = ael.Read(key);
            var s1 = JsonConvert.SerializeObject(aes, Formatting.Indented);
            var s2 = JsonConvert.SerializeObject(res, Formatting.Indented);
            Assert.AreEqual(s1,s2);
            wd.CleanUp();
        }
    }
}
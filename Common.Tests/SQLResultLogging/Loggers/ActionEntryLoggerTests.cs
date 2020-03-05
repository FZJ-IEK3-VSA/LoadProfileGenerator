using Automation.ResultFiles;

namespace Common.Tests.SQLResultLogging.Loggers
{
    using System;
    using System.Collections.Generic;
    using Common.SQLResultLogging;
    using Common.SQLResultLogging.Loggers;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture()]
    public class ActionEntryLoggerTests : UnitTestBaseClass
    {
        [Test()]
        [Category("BasicTest")]
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
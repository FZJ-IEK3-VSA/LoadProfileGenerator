using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Common.Tests.SQLResultLogging.InputLoggers {
    [TestFixture]
    public class ColumnEntryLoggerTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RunTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            ColumnEntryLogger ael = new ColumnEntryLogger(wd.SqlResultLoggingService);
            HouseholdKey key = new HouseholdKey("hhkey");
            List<IDataSaverBase> savers = new List<IDataSaverBase>
            {
                ael
            };
            InputDataLogger idl = new InputDataLogger(savers.ToArray());
            CalcLoadTypeDto cltd = new CalcLoadTypeDto("ltname", "kw", "kwh", 1, false, "guid");
            ColumnEntry ce = new ColumnEntry("name", 1, "locname", "guid", key, cltd, "oefckey", "devicecategory");
            List<ColumnEntry> aes = new List<ColumnEntry>
            {
                ce
            };
            idl.Save(aes);
            var res = ael.Read(key);
            var s1 = JsonConvert.SerializeObject(aes, Formatting.Indented);
            var s2 = JsonConvert.SerializeObject(res, Formatting.Indented);
            Assert.AreEqual(s1, s2);
            wd.CleanUp();
        }
    }
}
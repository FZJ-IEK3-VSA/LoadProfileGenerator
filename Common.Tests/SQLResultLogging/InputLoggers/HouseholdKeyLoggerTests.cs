using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Common.Tests.SQLResultLogging.InputLoggers
{
    [TestFixture()]
    public class HouseholdKeyLoggerTests : UnitTestBaseClass
    {
        [Test()]
        [Category(UnitTestCategories.BasicTest)]
        public void RunTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                HouseholdKeyLogger ael = new HouseholdKeyLogger(wd.SqlResultLoggingService);
                HouseholdKey key = new HouseholdKey("hhkey");
                List<IDataSaverBase> savers = new List<IDataSaverBase>
            {
                ael
            };
                InputDataLogger idl = new InputDataLogger(savers.ToArray());
                HouseholdKeyEntry ae1 = new HouseholdKeyEntry(key, "hhname", HouseholdKeyType.House, "desc", null, null);
                idl.Save(ae1);

                List<HouseholdKeyEntry> aes = new List<HouseholdKeyEntry>
            {
                ae1
            };
                var res = ael.Load();
                var s1 = JsonConvert.SerializeObject(aes, Formatting.Indented);
                var s2 = JsonConvert.SerializeObject(res, Formatting.Indented);
                Assert.AreEqual(s1, s2);
                wd.CleanUp();
            }
        }
    }
}
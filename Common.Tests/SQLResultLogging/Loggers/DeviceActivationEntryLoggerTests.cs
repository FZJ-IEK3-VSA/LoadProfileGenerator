using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Common.Tests.SQLResultLogging.Loggers
{
    [TestFixture()]
    public class DeviceActivationEntryLoggerTests : UnitTestBaseClass
    {
        [Test()]
        [Category(UnitTestCategories.BasicTest)]
        public void ReadTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                DeviceActivationEntryLogger ael = new DeviceActivationEntryLogger(wd.SqlResultLoggingService);
                HouseholdKey key = new HouseholdKey("hhkey");
                List<IDataSaverBase> savers = new List<IDataSaverBase>
            {
                ael
            };
                InputDataLogger idl = new InputDataLogger(savers.ToArray());
                CalcLoadTypeDto cldto = new CalcLoadTypeDto("loadtype", "kw", "kwh", 1, true, "guid".ToStrGuid());
                TimeStep ts = new TimeStep(1, 1, true);
                CalcDeviceDto cdd = new CalcDeviceDto("devicename", "device".ToStrGuid(),
                    key, OefcDeviceType.Device, "devicecategoryname",
                    "additionalname", "deviceguid".ToStrGuid(), "locationguid".ToStrGuid(), "locationname");
                DeviceActivationEntry ae1 = new DeviceActivationEntry("affordancename",
                    cldto, 1, "activator", 1, ts, cdd);
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
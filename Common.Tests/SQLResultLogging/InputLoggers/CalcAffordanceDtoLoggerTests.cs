using System;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.Enums;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Common.Tests.SQLResultLogging.InputLoggers
{
    [TestFixture()]
    public class CalcAffordanceDtoLoggerTests : UnitTestBaseClass
    {
        [Test()]
        [Category(UnitTestCategories.BasicTest)]
        public void CalcAffordanceDtoLoggerTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            CalcAffordanceDtoLogger ael = new CalcAffordanceDtoLogger(wd.SqlResultLoggingService);
            HouseholdKey key = new HouseholdKey("hhkey");
            List<IDataSaverBase> savers = new List<IDataSaverBase>
            {
                ael
            };
            InputDataLogger idl = new InputDataLogger(savers.ToArray());
            //CalcLoadTypeDto cldto = new CalcLoadTypeDto("loadtype", 1, "kw", "kwh", 1, true, "guid");
            CalcProfileDto cpd = new CalcProfileDto("name",1,ProfileType.Absolute,"source", Guid.NewGuid().ToString());
            AvailabilityDataReferenceDto adrd = new AvailabilityDataReferenceDto("blub","availguid");
            CalcAffordanceDto cadto = new CalcAffordanceDto("blub",1,cpd,"locname","locguid",true,new List<CalcDesireDto>(),
                1,100,PermittedGender.All,true,1,1,1,1,"affcat",false,false,new List<CalcAffordanceVariableOpDto>(),
                new List<VariableRequirementDto>(),ActionAfterInterruption.GoBackToOld,
                "timelimitname",100,true,"srctrait","guid", adrd,key);

            cadto.AddDeviceTuple("devname","deviguid",cpd,"calcloadtypename","loadtypeguid",1,new TimeSpan(0,1,0),1,1 );
            List<IHouseholdKey> aes = new List<IHouseholdKey>
            {
                cadto
            };
            idl.SaveList(aes);
            var res = ael.Load(key);
            var s1 = JsonConvert.SerializeObject(aes, Formatting.Indented);
            var s2 = JsonConvert.SerializeObject(res, Formatting.Indented);
            Assert.AreEqual(s1, s2);
            wd.CleanUp();
        }
    }
}
using System;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.CalcDto;
using Common.Enums;
using Common.Extensions;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using FluentAssertions;
using Newtonsoft.Json;

using Xunit;
using Xunit.Abstractions;


namespace Common.Tests.SQLResultLogging.InputLoggers
{
    public class CalcAffordanceDtoLoggerTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcAffordanceDtoLoggerTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                CalcAffordanceDtoLogger ael = new CalcAffordanceDtoLogger(wd.SqlResultLoggingService);
                HouseholdKey key = new HouseholdKey("hhkey");
                List<IDataSaverBase> savers = new List<IDataSaverBase>
            {
                ael
            };
                InputDataLogger idl = new InputDataLogger(savers.ToArray());
                //CalcLoadTypeDto cldto = new CalcLoadTypeDto("loadtype", 1, "kw", "kwh", 1, true, "guid");
                CalcProfileDto cpd = new CalcProfileDto("name", 1, ProfileType.Absolute, "source", Guid.NewGuid().ToStrGuid());
                AvailabilityDataReferenceDto adrd = new AvailabilityDataReferenceDto("blub", StringExtensions.ToStrGuid("availguid"));
                CalcAffordanceDto cadto = new CalcAffordanceDto("blub", 1, cpd, "locname", StringExtensions.ToStrGuid("locguid"), true, new List<CalcDesireDto>(),
                    1, 100, PermittedGender.All, true, 1, 1, 1, 1, "affcat", false, false, new List<CalcAffordanceVariableOpDto>(),
                    new List<VariableRequirementDto>(), ActionAfterInterruption.GoBackToOld,
                    "timelimitname", 100, true, "srctrait", StringExtensions.ToStrGuid("guid"), adrd, key, BodilyActivityLevel.Low);

                cadto.AddDeviceTuple("devname", StringExtensions.ToStrGuid("deviguid"), cpd, "calcloadtypename", StringExtensions.ToStrGuid("loadtypeguid"), 1, new TimeSpan(0, 1, 0), 1, 1);
                List<IHouseholdKey> aes = new List<IHouseholdKey>
            {
                cadto
            };
                idl.SaveList<CalcAffordanceDto>(aes);
                var res = ael.Load(key);
                var s1 = JsonConvert.SerializeObject(aes, Formatting.Indented);
                var s2 = JsonConvert.SerializeObject(res, Formatting.Indented);
                s1.Should().Be(s2);
                wd.CleanUp();
            }
        }

        public CalcAffordanceDtoLoggerTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
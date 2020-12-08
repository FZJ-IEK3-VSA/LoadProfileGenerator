using System;
using Automation;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;

namespace Common.Tests.SQLResultLogging.InputLoggers
{
    public class CalcParameterLoggerTests:UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcParameterLoggerTest()
        {
            CalcParameters cp = CalcParameters.GetNew();
            cp.SetStartDate(2017, 1, 1);
            cp.SetEndDate(2018, 1, 1);
            cp.Enable(CalcOption.ActivationsPerHour);
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                SqlResultLoggingService srls = new SqlResultLoggingService(wd.WorkingDirectory);
                CalcParameterLogger cpl = new CalcParameterLogger(srls);
                cpl.Run(Constants.GeneralHouseholdKey, cp);

                GC.Collect();
                GC.WaitForPendingFinalizers();

                CalcParameters cp2 = cpl.Load();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                string s1 = JsonConvert.SerializeObject(cp, Formatting.Indented);
                string s2 = JsonConvert.SerializeObject(cp2, Formatting.Indented);
                s1.Should().Be(s2);
                cp2.IsSet(CalcOption.ActivationsPerHour).Should().BeTrue();
                Assert.NotNull(cp2);
                wd.CleanUp();
            }
        }

        public CalcParameterLoggerTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
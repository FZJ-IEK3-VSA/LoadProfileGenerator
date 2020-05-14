using System;
using Automation;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Common.Tests.SQLResultLogging.InputLoggers
{
    using Newtonsoft.Json;

    [TestFixture()]
    public class CalcParameterLoggerTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
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
                Assert.AreEqual(s1, s2);
                Assert.IsTrue(cp2.IsSet(CalcOption.ActivationsPerHour));
                Assert.NotNull(cp2);
                wd.CleanUp();
            }
        }

        public CalcParameterLoggerTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
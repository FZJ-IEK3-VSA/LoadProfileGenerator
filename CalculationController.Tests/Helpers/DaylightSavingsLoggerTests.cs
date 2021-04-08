using System.Collections;
using Automation;
using CalculationController.DtoFactories;
using CalculationController.InputLoggers;
using CalculationEngine.Helper;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using Common.Tests;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

namespace CalculationController.Tests.Helpers
{
    public class DaylightSavingsLoggerTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunDaylightSavingsLoggerTests()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                SqlResultLoggingService sqrls = new SqlResultLoggingService(wd.WorkingDirectory);

                BitArray ba = new BitArray(365 * 24 * 60);
                for (int i = 0; i < ba.Length; i++)
                {
                    if (i % 3 == 0)
                    {
                        ba[i] = true;
                    }
                }
                DayLightStatus dls = new DayLightStatus(ba);
                CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
                DaylightTimesLogger dsl = new DaylightTimesLogger(sqrls, calcParameters);
                dsl.Run(Constants.GeneralHouseholdKey, dls);
                wd.CleanUp();
            }
        }

        public DaylightSavingsLoggerTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}

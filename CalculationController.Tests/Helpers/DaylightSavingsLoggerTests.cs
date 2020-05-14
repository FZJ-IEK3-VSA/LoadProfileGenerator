using System.Collections;
using Automation;
using CalculationController.DtoFactories;
using CalculationController.InputLoggers;
using CalculationEngine.Helper;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using NUnit.Framework;
using Xunit;

namespace CalculationController.Tests.Helpers
{
    public class DaylightSavingsLoggerTests
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
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
    }
}

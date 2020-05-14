using System.Collections.Generic;
using Automation;
using CalculationController.CalcFactories;
using CalculationController.DtoFactories;
using CalculationController.Helpers;
using Common;
using Common.CalcDto;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.Tests;
using Database;
using Database.Tables.BasicHouseholds;
using Database.Tests;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;

namespace CalculationController.Tests.InputLoggers
{
    using Common.JSON;

    public class DeviceTaggingSetLoggerTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void RunTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                SqlResultLoggingService srls = new SqlResultLoggingService(wd.WorkingDirectory);
                var calcParameters = CalcParametersFactory.MakeGoodDefaults();
                CalcLoadTypeDtoDictionary cltd = new CalcLoadTypeDtoDictionary(new Dictionary<VLoadType, CalcLoadTypeDto>());
                CalcDeviceTaggingSetFactory cdtsf = new CalcDeviceTaggingSetFactory(calcParameters, cltd);
                using (DatabaseSetup ds = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    Simulator sim = new Simulator(ds.ConnectionString);
                    CalcDeviceTaggingSets devicetaggingset = cdtsf.GetDeviceTaggingSets(sim, 2);
                    DeviceTaggingSetLogger dtsl = new DeviceTaggingSetLogger(srls);
                    List<DeviceTaggingSetInformation> cdts = new List<DeviceTaggingSetInformation>();
                    cdts.AddRange(devicetaggingset.AllCalcDeviceTaggingSets);
                    dtsl.Run(Constants.GeneralHouseholdKey, cdts);
                    wd.CleanUp();
                    ds.Cleanup();
                }
            }
        }

        public DeviceTaggingSetLoggerTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}

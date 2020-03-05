using System.Collections.Generic;
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
using NUnit.Framework;

namespace CalculationController.Tests.InputLoggers
{
    using Common.JSON;

    public class DeviceTaggingSetLoggerTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void RunTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            SqlResultLoggingService srls = new SqlResultLoggingService(wd.WorkingDirectory);
            var calcParameters = CalcParametersFactory.MakeGoodDefaults();
            CalcLoadTypeDtoDictionary cltd = new CalcLoadTypeDtoDictionary(new Dictionary<VLoadType, CalcLoadTypeDto>());
            CalcDeviceTaggingSetFactory cdtsf = new CalcDeviceTaggingSetFactory(calcParameters,cltd);
            DatabaseSetup ds = new DatabaseSetup(Utili.GetCurrentMethodAndClass(),DatabaseSetup.TestPackage.CalcController);
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

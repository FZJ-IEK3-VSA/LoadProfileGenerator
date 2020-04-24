using System;
using System.Globalization;
using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineLogging;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using Common.Tests;
using NUnit.Framework;

namespace Calculation.Tests.OnlineLogging {
    [TestFixture]
    public class VariableLogfileTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void VariableLogfileTests1()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            wd.InputDataLogger.AddSaver(new VariableEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().EnableShowSettlingPeriod();
            var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "blub", wd.InputDataLogger);
            HouseholdKey key = new HouseholdKey("hh1");
            fft.RegisterHousehold(key, "householdname", HouseholdKeyType.Household,"desc", null, null);
            DateStampCreator dsc = new DateStampCreator(calcParameters);
            IOnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger, calcParameters);
            CalcVariableRepository cvr = new CalcVariableRepository();
            //using (LogFile lf = new LogFile(calcParameters,fft, old,wd.SqlResultLoggingService, true))
            CalcLocation cloc = new CalcLocation("loc1", Guid.NewGuid().ToString());
            CalcVariable cv = new CalcVariable("mycalcvar", Guid.NewGuid().ToString(),
                0, cloc.Name, cloc.Guid, key);
            cvr.RegisterVariable(cv);
            TimeStep ts = new TimeStep(0,calcParameters);
            old.AddVariableStatus(new CalcVariableEntry(cv.Name, cv.Guid, 1, cv.LocationName, cv.LocationGuid,
                cv.HouseholdKey, ts));
            old.AddVariableStatus(new CalcVariableEntry(cv.Name, cv.Guid, 2, cv.LocationName, cv.LocationGuid,
                cv.HouseholdKey, ts));
            old.AddVariableStatus(new CalcVariableEntry(cv.Name, cv.Guid, 3, cv.LocationName, cv.LocationGuid,
                cv.HouseholdKey, ts));
            old.FinalSaveToDatabase();
            VariableEntryLogger vel = new VariableEntryLogger(wd.SqlResultLoggingService);
            var varEntries = vel.Read(key);
            foreach (CalcVariableEntry entry in varEntries) {
                Logger.Info(entry.Value.ToString(CultureInfo.InvariantCulture));
            }

            Assert.That(varEntries[0].Value, Is.EqualTo(1));
            Assert.That(varEntries[1].Value, Is.EqualTo(2));
            Assert.That(varEntries[2].Value, Is.EqualTo(3));
            /*old.
            cloc.Add();
            cloc[0].Variables.Add("trig1", 1.0);
            tlf.WriteLine(0, cloc);
            cloc[0].Variables["trig1"] = 2.0;
            tlf.WriteLine(1, cloc);
            lf.Close(null);*/
            wd.CleanUp();
        }
    }
}
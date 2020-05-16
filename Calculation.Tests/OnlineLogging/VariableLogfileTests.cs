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
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Calculation.Tests.OnlineLogging {
    public class VariableLogfileTests : UnitTestBaseClass
    {
        public VariableLogfileTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void VariableLogfileTests1()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                wd.InputDataLogger.AddSaver(new VariableEntryLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
                CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().EnableShowSettlingPeriod();
                using (var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "blub", wd.InputDataLogger))
                {
                    HouseholdKey key = new HouseholdKey("hh1");
                    fft.RegisterGeneralHouse();
                    fft.RegisterHousehold(key, "householdname", HouseholdKeyType.Household, "desc", null, null);
                    DateStampCreator dsc = new DateStampCreator(calcParameters);
                    using (IOnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger, calcParameters))
                    {
                        CalcVariableRepository cvr = new CalcVariableRepository();
                        //using (LogFile lf = new LogFile(calcParameters,fft, old,wd.SqlResultLoggingService, true))
                        CalcLocation cloc = new CalcLocation("loc1", Guid.NewGuid().ToStrGuid());
                        CalcVariable cv = new CalcVariable("mycalcvar", Guid.NewGuid().ToStrGuid(),
                            0, cloc.Name, cloc.Guid, key);
                        cvr.RegisterVariable(cv);
                        TimeStep ts = new TimeStep(0, calcParameters);
                        old.AddVariableStatus(new CalcVariableEntry(cv.Name, cv.Guid, 1, cv.LocationName, cv.LocationGuid,
                            cv.HouseholdKey, ts));
                        old.AddVariableStatus(new CalcVariableEntry(cv.Name, cv.Guid, 2, cv.LocationName, cv.LocationGuid,
                            cv.HouseholdKey, ts));
                        old.AddVariableStatus(new CalcVariableEntry(cv.Name, cv.Guid, 3, cv.LocationName, cv.LocationGuid,
                            cv.HouseholdKey, ts));
                        old.FinalSaveToDatabase();
                    }
                    VariableEntryLogger vel = new VariableEntryLogger(wd.SqlResultLoggingService);
                    var varEntries = vel.Read(key);
                    foreach (CalcVariableEntry entry in varEntries)
                    {
                        Logger.Info(entry.Value.ToString(CultureInfo.InvariantCulture));
                    }
                    varEntries[0].Value.Should().Be(1);
                    varEntries[1].Value.Should().Be(2);
                    varEntries[2].Value.Should().Be(3);
                }
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
}
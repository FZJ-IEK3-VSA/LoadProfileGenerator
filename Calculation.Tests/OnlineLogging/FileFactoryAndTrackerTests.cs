using System;
using System.Collections;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging.InputLoggers;
using Common.Tests;
using FluentAssertions;
using JetBrains.Annotations;
using Moq;

using Xunit;
using Xunit.Abstractions;


namespace Calculation.Tests.OnlineLogging {
    public class FileFactoryAndTrackerTests : UnitTestBaseClass
    {
        public FileFactoryAndTrackerTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void FileFactoryAndTrackerTest()
        {
            Config.IsInUnitTesting = true;
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
                CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
                var clt = new CalcLoadType("calcloadtype", "kwh", "kW", 0.001, true, Guid.NewGuid().ToStrGuid());
                BitArray isSick = new BitArray(calcParameters.InternalTimesteps);
                BitArray isOnVacation = new BitArray(calcParameters.InternalTimesteps);
                var personDto = CalcPersonDto.MakeExamplePerson();
                Random r = new Random();
                Mock<ILogFile> lf = new Mock<ILogFile>();
                using (CalcRepo calcRepo = new CalcRepo(rnd: r, lf: lf.Object, calcParameters: calcParameters))
                {
                    CalcLocation cloc = new CalcLocation("blub", Guid.NewGuid().ToStrGuid());
                    var cp = new CalcPerson(personDto,
                        cloc, isSick, isOnVacation, calcRepo);
                    /*"personname", 1, 1, null, 1, PermittedGender.Female, lf: null,
                    householdKey: "hh1", startingLocation: null, traitTag: "traittag",
                    householdName: "hhname0",calcParameters:calcParameters,isSick:isSick, guid:Guid.NewGuid().ToStrGuid());
                    */
                    var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "testhh", wd.InputDataLogger);
                    fft.RegisterHousehold(new HouseholdKey("hh1"), "test key", HouseholdKeyType.Household, "desc", null, null);
                    fft.RegisterHousehold(Constants.GeneralHouseholdKey, "general key", HouseholdKeyType.General, "desc", null, null);
                    fft.MakeFile<StreamWriter>("file1", "desc", true, ResultFileID.Actions, new HouseholdKey("hh1"), TargetDirectory.Charts, TimeSpan.FromMinutes(1), clt.ConvertToLoadTypeInformation(),
                        cp.MakePersonInformation());
                    //fft.ResultFileList.WriteResultEntries(wd.WorkingDirectory);
                    ResultFileEntryLogger rfel = new ResultFileEntryLogger(wd.SqlResultLoggingService);
                    var rfes = rfel.Load();
                    rfes.Count.Should().BeGreaterThan(0);
                    //ResultFileList.ReadResultEntries(wd.WorkingDirectory);
                    fft.GetResultFileEntry(ResultFileID.Actions, clt.Name, new HouseholdKey("hh1"), cp.MakePersonInformation(), null);
                    fft.Dispose();
                }
                wd.CleanUp();
            }
        }
    }
}
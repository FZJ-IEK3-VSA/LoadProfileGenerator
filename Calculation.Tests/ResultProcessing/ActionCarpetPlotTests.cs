////-----------------------------------------------------------------------

//// <copyright>
////
//// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
//// Written by Noah Pflugradt.
//// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
////
//// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
////  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
//// in the documentation and/or other materials provided with the distribution.
////  All advertising materials mentioning features or use of this software must display the following acknowledgement:
////  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
////  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
////  derived from this software without specific prior written permission.
////
//// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
//// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
//// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
//// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
//// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
//// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

//// </copyright>

////-----------------------------------------------------------------------

//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.IO;
//using System.Linq;
//using Automation;
//using Automation.ResultFiles;
//using CalcPostProcessor.GeneralHouseholdSteps;
//using CalcPostProcessor.Steps;
//using CalculationController.DtoFactories;
//using Common;
//using Common.CalcDto;
//using Common.Enums;
//using Common.JSON;
//using Common.SQLResultLogging;
//using Common.SQLResultLogging.InputLoggers;
//using Common.SQLResultLogging.Loggers;
//

//namespace Calculation.Tests.ResultProcessing
//{
//    
//    public class ActionCarpetPlotTests : TestBasis
//    {
//        [Fact]
//        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
//        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
//        public void ActionCarpetPlotTest()
//        {
//            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
//            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
//            wd.InputDataLogger.AddSaver(new CalcParameterLogger(wd.SqlResultLoggingService));
//            wd.InputDataLogger.AddSaver(new CalcAffordanceDtoLogger(wd.SqlResultLoggingService));
//            wd.InputDataLogger.AddSaver(new CalcAffordanceTaggingSetDtoLogger(wd.SqlResultLoggingService));
//            wd.InputDataLogger.AddSaver(new CalcPersonDtoLogger(wd.SqlResultLoggingService));
//            wd.InputDataLogger.AddSaver(new ActionEntryLogger(wd.SqlResultLoggingService));
//            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
//            CalculationProfiler profiler = new CalculationProfiler();
//            Config.IsInUnitTesting = true;
//            DateTime startdate = new DateTime(2013, 1, 1);
//            DateTime enddate = new DateTime(2013, 1, 31);
//            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate).SetSettlingDays(0).EnableShowSettlingPeriod();
//            calcParameters.Enable(CalcOption.ActionsLogfile);
//            calcParameters.Enable(CalcOption.ActionCarpetPlot);
//            wd.InputDataLogger.Save(calcParameters);
//            FileFactoryAndTracker fft = new FileFactoryAndTracker(wd.WorkingDirectory,"object",wd.InputDataLogger);
//            HouseholdKey key = new HouseholdKey("hh1");
//            fft.RegisterHousehold(key,"testhousehold",HouseholdKeyType.Household,"desc", null, null);
//            CalcProfileDto personProfile = new CalcProfileDto("bla", 1, ProfileType.Absolute, "bla", Guid.NewGuid().ToStrGuid());
//            CalcLocationDto cloc = new CalcLocationDto("cloc", 1,Guid.NewGuid().ToStrGuid());
//            AvailabilityDataReferenceDto adr = new AvailabilityDataReferenceDto("blub",Guid.NewGuid().ToStrGuid());
//            CalcAffordanceDto calcAffordance = new CalcAffordanceDto(name: "affName", id: 1, personProfile: personProfile,
//                calcLocationName: cloc.Name, randomEffect: false, satisfactionvalues: new List<CalcDesireDto>(),
//                miniumAge: 0, maximumAge: 99, permittedGender: PermittedGender.All, needsLight: false,
//                timeStandardDeviation: 0, isInterruptable: false, isInterrupting: false,
//                variableOps: new List<CalcAffordanceVariableOpDto>(),
//                variableRequirements: new List<VariableRequirementDto>(),
//                actionAfterInterruption: ActionAfterInterruption.GoBackToOld, timeLimitName: "",
//                weight: 1, requireAllDesires: false, srcTrait: "",calcLocationGuid:cloc.Guid,colorR: 200,
//                colorG:100, colorB: 100, affCategory:
//                "affcatory",guid:Guid.NewGuid().ToStrGuid(),isBusyArray:adr, householdKey: key);
//            List<CalcAffordanceDto> affs = new List<CalcAffordanceDto>
//            {
//                calcAffordance
//            };
//            wd.InputDataLogger.SaveList(affs.ConvertAll(x=> (IHouseholdKey) x).ToList());
//            //var lf = new Moq.Mock<ILogFile>();
//            CalcPersonDto cp = new CalcPersonDto("person","personguid",18,PermittedGender.Female,
//                key,new List<DateSpan>(),new List<DateSpan>(),1,"traittag","hhname"  );
//            List<CalcPersonDto> persons = new List<CalcPersonDto>
//            {
//                cp
//            };
//            wd.InputDataLogger.SaveList(persons.ConvertAll(x=> (IHouseholdKey)x));
//            TimeStep ts = new TimeStep(0, 0, false);
//            ActionEntry aeEntry = new ActionEntry("category", key, ts,new DateTime(2017,1,1),
//                                "personguid","personname",false,"affName","affguid",1);
//            List<ActionEntry> entries = new List<ActionEntry>
//            {
//                aeEntry
//            };
//            wd.InputDataLogger.SaveList(entries.ConvertAll(x => (IHouseholdKey)x));
//            /*
//            // trash the headline
//            Dictionary<string, Color> colordict = new Dictionary<string, Color>
//            {
//                [ae.StrAction] = Color.FromRgb(255, 0, 0)
//            };
            
//            calcParameters.DisableShowSettlingPeriod();
//            */
//            List<CalcAffordanceTaggingSetDto> taggingSets = new List<CalcAffordanceTaggingSetDto>();
//            CalcAffordanceTaggingSetDto catdto = new CalcAffordanceTaggingSetDto("set",true);
//            taggingSets.Add(catdto);
//            wd.InputDataLogger.Save(taggingSets);
//            CalcDataRepository cdp = new CalcDataRepository(wd.SqlResultLoggingService);
//            ActionCarpetPlot acp = new ActionCarpetPlot(profiler,cdp,fft);
//            HouseholdKeyEntry hhke= new HouseholdKeyEntry(key,"hhname",
//                HouseholdKeyType.Household,"description",null,null);
//            HouseholdStepParameters hhsp = new HouseholdStepParameters(hhke);
//            acp.Run(hhsp);
//            string chartdir = Path.Combine(wd.WorkingDirectory,
//                DirectoryNames.CalculateTargetdirectory(TargetDirectory.Charts));
//            var di = new DirectoryInfo(chartdir);
//            var files = di.GetFiles();
//            Assert.That(files.Length,Is.EqualTo(3));
//            wd.CleanUp();
//        }
//    }
//}
//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using Common.SQLResultLogging.Loggers;
using Common.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace Calculation.HouseholdElements.Tests
{
    public class CalcPersonTests : UnitTestBaseClass
    {
        public CalcPersonTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void PickRandomAffordanceFromEquallyAttractiveOnesTest()
        {
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass())) {
                CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
                calcParameters.AffordanceRepetitionCount = 0;
                wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
                using (var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "blub", wd.InputDataLogger)) {
                    fft.RegisterHousehold(Constants.GeneralHouseholdKey, "general", HouseholdKeyType.General, "desc", null, null);
                    //SqlResultLoggingService srls = new SqlResultLoggingService(wd.WorkingDirectory);
                    //DateStampCreator dsc = new DateStampCreator(calcParameters);
                    Random rnd = new Random();
                    //OnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger, calcParameters);
                    using (var lf = new LogFile(calcParameters, fft)) {
                        CalcProfile cp = new CalcProfile("cp1", Guid.NewGuid().ToStrGuid(), TimeSpan.FromMinutes(1), ProfileType.Absolute, "bla");
                        CalcVariableRepository crv = new CalcVariableRepository();
                        BitArray isBusy = new BitArray(calcParameters.InternalTimesteps, false);
                        using CalcRepo calcRepo = new CalcRepo(lf: lf, calcParameters: calcParameters, rnd: rnd);
                        var hhkey = new HouseholdKey("HH1");
                        CalcAffordance aff1 = new CalcAffordance("aff1", cp, null, false, new List<CalcDesire>(), 10, 20, PermittedGender.All, true,
                            1, LPGColors.AliceBlue, null, false, false, null, null, ActionAfterInterruption.GoBackToOld, "", 900, false, "",
                            Guid.NewGuid().ToStrGuid(), crv, new List<CalcAffordance.DeviceEnergyProfileTuple>(), isBusy, BodilyActivityLevel.Low,
                            calcRepo, hhkey);
                        CalcAffordance aff2 = new CalcAffordance("aff2", cp, null, false, new List<CalcDesire>(), 10, 20, PermittedGender.All, true,
                            1, LPGColors.AliceBlue, null, false, false, null, null, ActionAfterInterruption.GoBackToOld, "", 100, false, "",
                            Guid.NewGuid().ToStrGuid(), crv, new List<CalcAffordance.DeviceEnergyProfileTuple>(), isBusy, BodilyActivityLevel.Low,
                            calcRepo, hhkey);

                        List<ICalcAffordanceBase> affs = new List<ICalcAffordanceBase> {
                            aff1,
                            aff2
                        };
                        int aff1C = 0;
                        int aff2C = 0;
                        BitArray isSick = new BitArray(calcParameters.InternalTimesteps);
                        BitArray isOnVacation = new BitArray(calcParameters.InternalTimesteps);
                        CalcPersonDto calcPerson = CalcPersonDto.MakeExamplePerson();
                        CalcPerson cperson = new CalcPerson(calcPerson, null, isSick, isOnVacation, calcRepo);
                        TimeStep ts = new TimeStep(1, 0, true);
                        for (int i = 0; i < 1000; i++) {
                            ICalcAffordanceBase cab = cperson.PickRandomAffordanceFromEquallyAttractiveOnes(affs, ts, null, new HouseholdKey("bla"));
                            if (cab == aff1) {
                                aff1C++;
                            }

                            if (cab == aff2) {
                                aff2C++;
                            }
                        }

                        aff1C.Should().BeApproximatelyWithinPercent(900,0.1);
                        Logger.Info("Number of selections for 90%:" + aff1C + ", 10%:" + aff2C);
                    }
                }
                wd.CleanUp();
            }
        }
    }
}

namespace Calculation.Tests.HouseholdElements {
    public class CalcPersonTests : CalcUnitTestBase {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void NextStepTest() {
            using var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(100);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate);
            calcParameters.AffordanceRepetitionCount = 0;
            //var r = new Random(1);
            //var nr = new NormalRandom(0, 1, r);
            var desire1 = new CalcDesire("desire1", 1, 0.5m, 4, 1, 1, 60, -1, null,"","");

            var lt = new CalcLoadType("calcLoadtype1",  "kwh", "W", 1, true, Guid.NewGuid().ToStrGuid());
            wd.InputDataLogger.AddSaver(new ActionEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new LocationEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
            using var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "blub",wd.InputDataLogger);
            fft.RegisterHousehold(Constants.GeneralHouseholdKey,"general",HouseholdKeyType.General,"Desc",null,null);
            //SqlResultLoggingService srls = new SqlResultLoggingService(wd.WorkingDirectory);
            DateStampCreator dsc = new DateStampCreator(calcParameters);
            var hhkey = new HouseholdKey("hh1");
            using OnlineLoggingData old = new OnlineLoggingData(dsc,wd.InputDataLogger,calcParameters);
            using (var lf = new LogFile(calcParameters, fft)) {
                var cloc = new CalcLocation("loc1",  Guid.NewGuid().ToStrGuid());
                var clocs = new List<CalcLocation>
                {
                    cloc
                };
                BitArray isSick = new BitArray(calcParameters.InternalTimesteps);
                BitArray isOnVacation = new BitArray(calcParameters.InternalTimesteps);
                CalcPersonDto calcPerson = CalcPersonDto.MakeExamplePerson();
                var odap = new OnlineDeviceActivationProcessor( old, calcParameters,fft);
                Random rnd = new Random();
                NormalRandom nr = new NormalRandom(0, 1, rnd);
                using CalcRepo calcRepo = new CalcRepo(lf:lf, odap: odap, calcParameters:calcParameters, rnd:rnd, normalRandom:nr, onlineLoggingData:old);
                var cp = new CalcPerson(calcPerson,   cloc,  isSick, isOnVacation,calcRepo);
                    //20, PermittedGender.Male, lf, "HH1", cloc, "traittag","hhname0", calcParameters,isSick, Guid.NewGuid().ToStrGuid());
                cp.PersonDesires.AddDesires(desire1);
                cp.SicknessDesires.AddDesires(desire1);
                var deviceLoads = new List<CalcDeviceLoad>
                {
                    new CalcDeviceLoad("devload1",  1, lt, 100, 1)
                };
                var deviceCategoryGuid = Guid.NewGuid().ToStrGuid();
                CalcDeviceDto cdd1 = new CalcDeviceDto("cdevice1", deviceCategoryGuid, new HouseholdKey("HH1"),
                    OefcDeviceType.Device, "category", "", Guid.NewGuid().ToStrGuid(),
                    cloc.Guid, cloc.Name, FlexibilityType.NoFlexibility, 0);

                var cdev1 = new CalcDevice(  deviceLoads,  cloc, cdd1,calcRepo);

                CalcDeviceDto cdd2 = new CalcDeviceDto("cdevice2", deviceCategoryGuid, new HouseholdKey("HH1"),
                    OefcDeviceType.Device, "category", "", Guid.NewGuid().ToStrGuid(),
                    cloc.Guid, cloc.Name, FlexibilityType.NoFlexibility, 0);
                var cdev2 = new CalcDevice(deviceLoads, cloc,  cdd2, calcRepo);
                cloc.Devices.Add(cdev1);
                cloc.Devices.Add(cdev2);
                var daylight = new BitArray(calcParameters.InternalTimesteps);
                daylight.SetAll(true);
                DayLightStatus dls = new DayLightStatus(daylight);
                double[] newValues = {1, 1, 1.0};
                var valueList = new List<double>(newValues);
                var cprof = new CalcProfile("cp1", Guid.NewGuid().ToStrGuid(),valueList, ProfileType.Relative, "bla");
                var desires = new List<CalcDesire>
                {
                    desire1
                };
                var color = new ColorRGB(255, 0, 0);
                CalcVariableRepository crv = new CalcVariableRepository();
                BitArray isBusy1 = new BitArray(calcParameters.InternalTimesteps, false);
                var aff1 = new CalcAffordance("aff1",  cprof, cloc,
                    false, desires, 1, 100,
                    PermittedGender.All, false, 0, color,
                    "aff category", true, false,
                    new List<CalcAffordanceVariableOp>(), new List<VariableRequirement>(),
                    ActionAfterInterruption.GoBackToOld,"blub",100,false,"",
                    Guid.NewGuid().ToStrGuid(),crv, new List<CalcAffordance.DeviceEnergyProfileTuple>(),
                    isBusy1, BodilyActivityLevel.Low,calcRepo, hhkey);
                aff1.AddDeviceTuple(cdev1, cprof, lt, 0, calcParameters.InternalStepsize, 1, 1);
                cloc.AddAffordance(aff1);
                BitArray isBusy2 = new BitArray(calcParameters.InternalTimesteps, false);
                var aff2 = new CalcAffordance("aff2", cprof, cloc, false, desires, 1, 100,
                    PermittedGender.All, false, 0,
        color, "aff category", false, false,
                    new List<CalcAffordanceVariableOp>(), new List<VariableRequirement>(),
                    ActionAfterInterruption.GoBackToOld,"bla",100,false,"",
                    Guid.NewGuid().ToStrGuid(),crv, new List<CalcAffordance.DeviceEnergyProfileTuple>(),
                    isBusy2, BodilyActivityLevel.Low, calcRepo, hhkey);
                aff2.AddDeviceTuple(cdev2, cprof, lt, 0, calcParameters.InternalStepsize, 1, 1);
                cloc.AddAffordance(aff2);
                var persons = new List<CalcPerson>
                {
                    cp
                };
                //var variableOperator = new VariableOperator();
                TimeStep ts = new TimeStep(0, 0, true);
                for (var i = 0; i < 100; i++) {
                    cp.NextStep(ts, clocs, dls, hhkey, persons, 1);
                    ts = ts.AddSteps(1);
                }
            }
            //wd.CleanUp();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TestInterruptionTest() {
            using var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(100);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate).SetSettlingDays(0).EnableShowSettlingPeriod().DisableShowSettlingPeriod().SetAffordanceRepetitionCount(1);
            //var r = new Random(1);
            //var nr = new NormalRandom(0, 1, r);
            var desire1 = new CalcDesire("desire1", 1, 0.5m, 4, 1, 1, 60, -1, null,"","");
            var lt = new CalcLoadType("calcLoadtype1",  "kwh", "W", 1, true, Guid.NewGuid().ToStrGuid());
            //var variableOperator = new VariableOperator();
            using var fft = new FileFactoryAndTracker(wd.WorkingDirectory,"blub",wd.InputDataLogger);
            var key = new HouseholdKey("HH1");
            wd.InputDataLogger.AddSaver(new ActionEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new LocationEntryLogger(wd.SqlResultLoggingService));
            fft.RegisterHousehold(key,"hh1",HouseholdKeyType.Household, "desc",null,null);
            fft.RegisterHousehold(Constants.GeneralHouseholdKey, "General",HouseholdKeyType.General,"Desc", null, null);
            //SqlResultLoggingService srls = new SqlResultLoggingService(wd.WorkingDirectory);
            DateStampCreator dsc = new DateStampCreator(calcParameters);
            using OnlineLoggingData old = new OnlineLoggingData( dsc,wd.InputDataLogger,calcParameters);
            using var lf = new LogFile(calcParameters,fft);
            var cloc = new CalcLocation("loc1",  Guid.NewGuid().ToStrGuid());
            BitArray isSick = new BitArray(calcParameters.InternalTimesteps);
            BitArray isOnVacation = new BitArray(calcParameters.InternalTimesteps);
            CalcPersonDto calcPerson = CalcPersonDto.MakeExamplePerson();
            var odap = new OnlineDeviceActivationProcessor( old, calcParameters,fft);
            Random rnd = new Random();
            NormalRandom nr = new NormalRandom(0, 1, rnd);
            using CalcRepo calcRepo = new CalcRepo(lf:lf, odap:odap, calcParameters:calcParameters, rnd: rnd, normalRandom: nr, onlineLoggingData:old);
            var cp = new CalcPerson(calcPerson, cloc,  isSick, isOnVacation, calcRepo);
            //"blub", 1, 1, r,20, PermittedGender.Male, lf, "HH1", cloc,"traittag","hhname0", calcParameters,isSick, Guid.NewGuid().ToStrGuid());
            cp.PersonDesires.AddDesires(desire1);
            cp.SicknessDesires.AddDesires(desire1);
            var deviceLoads = new List<CalcDeviceLoad>
            {
                new CalcDeviceLoad("devload1", 1, lt, 100, 1)
            };
            var devCategoryGuid = Guid.NewGuid().ToStrGuid();
            CalcDeviceDto cdd1 = new CalcDeviceDto("cdevice1", devCategoryGuid, key,
                OefcDeviceType.Device, "category","", Guid.NewGuid().ToStrGuid(),
                cloc.Guid,cloc.Name, FlexibilityType.NoFlexibility, 0);
            var cdev1 = new CalcDevice(  deviceLoads,   cloc, cdd1, calcRepo);
            CalcDeviceDto cdd2 = new CalcDeviceDto("cdevice2", devCategoryGuid, key,
                OefcDeviceType.Device, "category", "", Guid.NewGuid().ToStrGuid(),
                cloc.Guid, cloc.Name, FlexibilityType.NoFlexibility, 0);
            var cdev2 = new CalcDevice(  deviceLoads,  cloc,cdd2, calcRepo);
            CalcDeviceDto cdd3 = new CalcDeviceDto("cdevice3", devCategoryGuid, key,
                OefcDeviceType.Device, "category", "", Guid.NewGuid().ToStrGuid(),
                cloc.Guid, cloc.Name, FlexibilityType.NoFlexibility, 0);
            var cdev3 = new CalcDevice( deviceLoads,   cloc, cdd3, calcRepo);
            cloc.Devices.Add(cdev1);
            cloc.Devices.Add(cdev2);
            cloc.Devices.Add(cdev3);
            var daylight = new BitArray(100);
            daylight.SetAll(true);
            DayLightStatus dls = new DayLightStatus(daylight);
            double[] newValues = {1, 1, 1.0, 1, 1, 1, 1, 1};
            var newList = new List<double>(newValues);
            var cprof = new CalcProfile("cp1", Guid.NewGuid().ToStrGuid(), newList,  ProfileType.Relative, "bla");

            var desires = new List<CalcDesire>
            {
                desire1
            };
            var color = new ColorRGB(255, 0, 0);
            CalcVariableRepository crv = new CalcVariableRepository();
            var hhkey = new HouseholdKey("hh1");
            BitArray isBusy = new BitArray(calcParameters.InternalTimesteps, false);
            var aff1 = new CalcAffordance("aff1",  cprof, cloc, false, desires, 1, 100,
                PermittedGender.All, false, 0, color, "aff category", true, false,
                new List<CalcAffordanceVariableOp>(), new List<VariableRequirement>(),
                ActionAfterInterruption.GoBackToOld,"bla",100,false,"",
                Guid.NewGuid().ToStrGuid(),crv
                , new List<CalcAffordance.DeviceEnergyProfileTuple>(),isBusy, BodilyActivityLevel.Low,calcRepo, hhkey);
            aff1.AddDeviceTuple(cdev1, cprof, lt, 0, calcParameters.InternalStepsize, 1, 1);
            cloc.AddAffordance(aff1);
            var aff2 = new CalcAffordance("aff2", cprof, cloc, false, desires, 1, 100,
                PermittedGender.All, false, 0, color, "aff category", false, false,
                new List<CalcAffordanceVariableOp>(), new List<VariableRequirement>(),
                ActionAfterInterruption.GoBackToOld,"bla",100,false,"", Guid.NewGuid().ToStrGuid(),crv
                , new List<CalcAffordance.DeviceEnergyProfileTuple>(),isBusy, BodilyActivityLevel.Low,calcRepo, hhkey);
            aff2.AddDeviceTuple(cdev2, cprof, lt, 0, calcParameters.InternalStepsize, 1, 1);
            cloc.AddAffordance(aff2);
            var clocs = new List<CalcLocation>
            {
                cloc
            };
            BitArray isBusySub = new BitArray(calcParameters.InternalTimesteps, false);
            var calcSubAffordance = new CalcSubAffordance("subaffname", cloc, desires, 0,
                100, 1,
                PermittedGender.All, "subaff", false, true,
                aff1, new List<CalcAffordanceVariableOp>(),100,
                "testing", Guid.NewGuid().ToStrGuid(), isBusySub,crv, BodilyActivityLevel.Low,
                calcRepo,hhkey);
            aff2.SubAffordances.Add(calcSubAffordance);
            calcSubAffordance.SetDurations(2);
            var persons = new List<CalcPerson>
            {
                cp
            };
            for (var i = 0; i < 100; i++) {
                TimeStep ts = new TimeStep(i,0,true);
                cp.NextStep(ts, clocs, dls, hhkey, persons, 1);
            }

            //wd.CleanUp();
        }

        public CalcPersonTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base( testOutputHelper)
        {
        }
    }
}
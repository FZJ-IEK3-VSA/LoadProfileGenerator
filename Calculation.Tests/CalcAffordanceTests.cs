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
using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Common.Tests;
using FluentAssertions;
using Moq;

using Xunit;
using Xunit.Abstractions;


namespace Calculation.Tests {
    public class CalcAffordanceTests : UnitTestBaseClass
    {

        public CalcAffordanceTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper):base(testOutputHelper)
        {
        }

        private static void SetupProbabilityTest([JetBrains.Annotations.NotNull] out CalcAffordance aff, [JetBrains.Annotations.NotNull] out CalcLoadType lt,
                                                 [JetBrains.Annotations.NotNull] out CalcDevice cd,
                                                 [JetBrains.Annotations.NotNull] out CalcLocation loc, int stepcount, double probability, bool enableFlexibility = false)
        {
            Config.IsInUnitTesting = true;
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(stepcount);
            CalcParameters calcParameters =
                CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate);
            calcParameters.FlexibilityEnabled = enableFlexibility;
            var timeStep = new TimeSpan(0, 1, 0);
            var cp = new CalcProfile("profile", Guid.NewGuid().ToStrGuid(), timeStep, ProfileType.Absolute, "blub");
            cp.AddNewTimepoint(new TimeSpan(0), 100);
            cp.AddNewTimepoint(new TimeSpan(0, 10, 0), 0);
            cp.ConvertToTimesteps();
            loc = new CalcLocation(Utili.GetCurrentMethodAndClass(), Guid.NewGuid().ToStrGuid());
            CalcVariableRepository cvr = new CalcVariableRepository();
            BitArray isBusy = new BitArray(100, false);
            var r = new Random(0);
            var hhkey = new HouseholdKey("HH1");
            var nr = new NormalRandom(0, 0.1, r);
            IMock<IOnlineDeviceActivationProcessor> iodap = new Mock<IOnlineDeviceActivationProcessor>();
            using CalcRepo calcRepo = new CalcRepo(calcParameters: calcParameters, normalRandom: nr, rnd: r, odap: iodap.Object);
            aff = new CalcAffordance("bla",
                cp,
                loc,
                false,
                new List<CalcDesire>(),
                0,
                99,
                PermittedGender.All,
                false,
                0.1,
                new ColorRGB(0, 0, 0),
                "bla",
                false,
                false,
                new List<CalcAffordanceVariableOp>(),
                new List<VariableRequirement>(),
                ActionAfterInterruption.GoBackToOld,
                "bla",
                100,
                false,
                "",
                Guid.NewGuid().ToStrGuid(),
                cvr,
                new List<DeviceEnergyProfileTuple>(),
                isBusy,
                BodilyActivityLevel.Low,
                calcRepo, hhkey);
            lt = new CalcLoadType("load", "unit1", "unit2", 1, true, Guid.NewGuid().ToStrGuid());
            var cdl = new CalcDeviceLoad("cdl", 1, lt, 1, 0.1);
            var devloads = new List<CalcDeviceLoad> {
                cdl
            };
            CalcDeviceDto cdd = new CalcDeviceDto("device",
                "devcategoryguid".ToStrGuid(),
                hhkey,
                OefcDeviceType.Device,
                "category",
                string.Empty,
                Guid.NewGuid().ToStrGuid(),
                loc.Guid,
                loc.Name,FlexibilityType.NoFlexibility, 0);
            cd = new CalcDevice(devloads, loc, cdd, calcRepo);
            aff.AddDeviceTuple(cd, cp, lt, 0, timeStep, 10, probability);
        }

        private static void CheckForBusyness([JetBrains.Annotations.NotNull] CalcLocation loc,
                                             [JetBrains.Annotations.NotNull] CalcAffordance aff,
                                             [JetBrains.Annotations.NotNull] CalcDevice cd, [JetBrains.Annotations.NotNull] CalcLoadType lt )
        {
            Logger.Info("------------");
            TimeStep ts1 = new TimeStep(0,0,true);
            var person = new CalcPersonDto("person", null, 30, PermittedGender.Male, null, null, null, -1, null, null);
            Logger.Info("aff.isbusy 0: " + aff.IsBusy(ts1, loc, person, false));
            var prevstate = BusynessType.NotBusy;
            for (var i = 0; i < 100; i++) {
                TimeStep ts2 = new TimeStep(i, 0,true);
                if (aff.IsBusy(ts2, loc, person, false) != prevstate) {
                    prevstate = aff.IsBusy(ts2, loc, person, false);
                    Logger.Info("aff.isbusy:" + i + ": " + prevstate);
                }
            }

            var prevstate1 = false;
            TimeStep ts3 = new TimeStep(0, 0,false);
            Logger.Info("cd.isbusyarray 0:   " + cd.GetIsBusyForTesting(ts3, lt));
            for (var i = 0; i < 100; i++) {
                TimeStep ts4 = new TimeStep(i, 0, false);
                if (cd.GetIsBusyForTesting(ts4, lt) != prevstate1) {
                    prevstate1 = cd.GetIsBusyForTesting(ts4, lt);
                    Logger.Info("cd.isbusyarray: " + i + ": " + prevstate);
                }
            }
            TimeStep ts5 = new TimeStep(100, 0, false);
            Logger.Info("cd.isbusyarray 100: " + cd.GetIsBusyForTesting(ts5, lt));
        }

        [Fact]
        [Trait("Category",UnitTestCategories.BasicTest)]
        public void CalcAffordanceActivateTest0Percent()
        {
            Logger.Info("hi");
            var calcParameters = CalcParameters.GetNew();
            const int stepcount = 150;
            SetupProbabilityTest(out var aff, out var lt, out var cd, out var loc, stepcount, 0);
            var trueCount = 0;
            TimeStep ts1 = new TimeStep(0,0,true);
            var person = new CalcPersonDto("name", null, 30, PermittedGender.Male, null, null, null, -1, null, null);
            var result = aff.IsBusy(ts1, loc, person);
            result.Should().Be(BusynessType.NotBusy);
            const int resultcount = stepcount - 20;
            for (var i = 0; i < resultcount; i++) {
                for (var j = 0; j < stepcount; j++) {
                    TimeStep ts = new TimeStep(j,calcParameters);
                    cd.SetIsBusyForTesting(ts, false, lt);
                    cd.IsBusyForLoadType[lt][ts.InternalStep] = false;
                }
                TimeStep ts2 = new TimeStep(i, calcParameters);
                aff.IsBusy(ts2, loc, person);
                //var variableOperator = new VariableOperator();
                aff.Activate( ts2, "blub", loc, out var _);
                if (cd.GetIsBusyForTesting(ts2, lt)) {
                    trueCount++;
                }
            }

            Logger.Info("Truecount: " + trueCount);
            trueCount.Should().BeApproximately(0,0.1);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcAffordanceActivateTest100Percent()
        {
            const int stepcount = 150;
            SetupProbabilityTest(out var aff, out var lt, out var cd, out var loc, stepcount, 1);
            var trueCount = 0;
            TimeStep ts = new TimeStep(0,0,false);
            var person = new CalcPersonDto("name", null, 30, PermittedGender.Male, null, null, null, -1, null, null);
            var result = aff.IsBusy(ts, loc, person);
            result.Should().Be(BusynessType.NotBusy);
            const int resultcount = stepcount - 20;
            for (var i = 0; i < resultcount; i++) {

                for (var j = 0; j < stepcount; j++) {
                    TimeStep ts2 = new TimeStep(j, 0, false);
                    cd.SetIsBusyForTesting(ts2, false, lt);
                    cd.IsBusyForLoadType[lt][j] = false;
                }
                TimeStep ts3 = new TimeStep(i, 0, false);
                aff.IsBusy(ts3, loc, person);
                //var variableOperator = new VariableOperator();
                aff.Activate(ts3, "blub", loc, out var _);
                if (cd.GetIsBusyForTesting(ts3, lt)) {
                    trueCount++;
                }
            }

            Logger.Info("Truecount: " + trueCount);
            trueCount.Should().BeApproximately(resultcount, 0.1);
        }


        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcAffordanceActivateTest25Percent()
        {
            const int stepcount = 150;
            SetupProbabilityTest(out var aff, out var lt, out CalcDevice cd, out var loc, stepcount, 0.25);
            var trueCount = 0;
            TimeStep ts = new TimeStep(0, 0, false);
            var person = new CalcPersonDto("name", null, 30, PermittedGender.Male, null, null, null, -1, null, null);
            var result = aff.IsBusy(ts, loc, person);
            result.Should().Be(BusynessType.NotBusy);
            const int resultcount = stepcount - 20;
            for (var i = 0; i < resultcount; i++) {
                for (var j = 0; j < stepcount; j++) {
                    TimeStep ts2 = new TimeStep(j, 0, false);
                    cd.SetIsBusyForTesting(ts2, false, lt);
                    cd.IsBusyForLoadType[lt][j] = false;
                }
                TimeStep ts3 = new TimeStep(i, 0, false);
                aff.IsBusy(ts3, loc, person);
                //var variableOperator = new VariableOperator();
                aff.Activate(ts3, "blub", loc, out var _);
                if (cd.GetIsBusyForTesting(ts3, lt)) {
                    trueCount++;
                }
            }

            Logger.Info("Truecount: " + trueCount);
#pragma warning disable VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.

            trueCount.Should().BeApproximately(resultcount/4,0.1);
#pragma warning restore VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcAffordanceActivateTest50Percent()
        {
            const int stepcount = 150;
            SetupProbabilityTest(out var aff, out var lt, out var cd, out var loc, stepcount, 0.5);
            var trueCount = 0;
            TimeStep ts = new TimeStep(0, 0, false);
            var person = new CalcPersonDto("name", null, 30, PermittedGender.Male, null, null, null, -1, null, null);
            var result = aff.IsBusy(ts, loc, person);
            result.Should().Be(BusynessType.NotBusy);
            const int resultcount = stepcount - 20;
            for (var i = 0; i < resultcount; i++) {
                for (var j = 0; j < stepcount; j++) {
                    TimeStep ts2 = new TimeStep(j, 0, false);
                    cd.SetIsBusyForTesting(ts2, false, lt);
                    cd.IsBusyForLoadType[lt][j] = false;
                }
                TimeStep ts3 = new TimeStep(i, 0, false);
                aff.IsBusy(ts3, loc, person);
                //var variableOperator = new VariableOperator();
                aff.Activate(ts3, "blub", loc, out var _);
                if (cd.GetIsBusyForTesting(ts3, lt)) {
                    trueCount++;
                }
            }

            Logger.Info("Truecount: " + trueCount);
#pragma warning disable VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
            trueCount.Should().BeApproximately(resultcount/2,0.1);
#pragma warning restore VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcAffordanceActivateTest75Percent()
        {
            const int stepcount = 150;
            SetupProbabilityTest(out var aff, out var lt, out var cd, out var loc, stepcount, 0.75);
            var trueCount = 0;
            TimeStep ts = new TimeStep(0, 0, false);
            var person = new CalcPersonDto("name", null, 30, PermittedGender.Male, null, null, null, -1, null, null);
            var result = aff.IsBusy(ts, loc, person);
            result.Should().Be(BusynessType.NotBusy);
            const int resultcount = stepcount - 20;
            for (var i = 0; i < resultcount; i++) {
                for (var j = 0; j < stepcount; j++) {
                    TimeStep ts2 = new TimeStep(j, 0, false);
                    cd.SetIsBusyForTesting(ts2, false, lt);
                    cd.IsBusyForLoadType[lt][j] = false;
                }
                TimeStep ts3 = new TimeStep(i, 0, false);
                aff.IsBusy(ts3, loc, person);
                //var variableOperator = new VariableOperator();
                aff.Activate(ts3, "blub", loc, out var _);
                if (cd.GetIsBusyForTesting(ts3, lt)) {
                    trueCount++;
                }
            }

            Logger.Info("Truecount: " + trueCount);
            trueCount.Should().BeApproximatelyWithinPercent(resultcount * 0.75, 0.1);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcAffordanceVariableTestAdd()
        {
            var r = new Random(0);
            var nr = new NormalRandom(0, 0.1, r);
            const int stepcount = 150;
            Config.IsInUnitTesting = true;
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(stepcount);
            CalcParameters calcParameters =
                CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate);
            var timeStep = new TimeSpan(0, 1, 0);
            var cp = new CalcProfile("profile", Guid.NewGuid().ToStrGuid(), timeStep, ProfileType.Absolute, "blub");
            cp.AddNewTimepoint(new TimeSpan(0), 100);
            cp.AddNewTimepoint(new TimeSpan(0, 10, 0), 0);
            cp.ConvertToTimesteps();
            //var variableOperator = new VariableOperator();
            var variables = new List<CalcAffordanceVariableOp>();
            var variableReqs = new List<VariableRequirement>();
            var loc = new CalcLocation("loc", Guid.NewGuid().ToStrGuid());
            var variableGuid = Guid.NewGuid().ToStrGuid();
            CalcVariableRepository variableRepository = new CalcVariableRepository();
            HouseholdKey key = new HouseholdKey("hh1");
            CalcVariable cv = new CalcVariable("calcvar1", variableGuid, 0, loc.Name, loc.Guid, key);
            variableRepository.RegisterVariable(cv);
            variables.Add(new CalcAffordanceVariableOp(cv.Name, 1, loc, VariableAction.Add,
                VariableExecutionTime.Beginning, cv.Guid));
            BitArray isBusy = new BitArray(100, false);
            var hhkey = new HouseholdKey("HH1");
            using CalcRepo calcRepo = new CalcRepo(rnd: r,normalRandom:nr,calcParameters:calcParameters, odap: new Mock<IOnlineDeviceActivationProcessor>().Object);
            var aff = new CalcAffordance("bla", cp, loc, false, new List<CalcDesire>(), 0, 99,
                PermittedGender.All, false, 0.1, new ColorRGB(0, 0, 0), "bla", false, false, variables, variableReqs,
                ActionAfterInterruption.GoBackToOld, "bla", 100, false, "",  Guid.NewGuid().ToStrGuid(),
                variableRepository,
                new List<DeviceEnergyProfileTuple>(), isBusy, BodilyActivityLevel.Low,calcRepo, hhkey);
            var lt = new CalcLoadType("load", "unit1", "unit2", 1, true, Guid.NewGuid().ToStrGuid());
            var cdl = new CalcDeviceLoad("cdl", 1, lt, 1, 0.1);
            var devloads = new List<CalcDeviceLoad> {
                cdl
            };
            var categoryGuid = Guid.NewGuid().ToStrGuid();
            CalcDeviceDto dto = new CalcDeviceDto("device", categoryGuid,hhkey ,
                OefcDeviceType.Device, "category", string.Empty, Guid.NewGuid().ToStrGuid(),
                loc.Guid,loc.Name,FlexibilityType.NoFlexibility, 0);
            var cd = new CalcDevice( devloads, loc, dto,calcRepo );

            //loc.Variables.Add("Variable1", 0);
            aff.AddDeviceTuple(cd, cp, lt, 0, timeStep, 10, 1);
            TimeStep ts = new TimeStep(0, 0, false);
            var person = new CalcPersonDto("name", null, 30, PermittedGender.Male, null, null, null, -1, null, null);
            aff.IsBusy(ts, loc, person);
            aff.Activate(ts, "blub", loc, out var _);
             variableRepository.GetValueByGuid(variableGuid).Should().Be(1);
            for (var i = 0; i < 15; i++) {
                TimeStep ts1 = new TimeStep(i, 0, false);
                cd.SetIsBusyForTesting(ts1, false, lt);
            }

            aff.IsBusy(ts, loc, person);
            aff.Activate(ts, "blub", loc, out var _);
            variableRepository.GetValueByGuid(variableGuid).Should().Be(2);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcAffordanceVariableTestSet()
        {
            var deviceCategoryGuid = Guid.NewGuid().ToStrGuid();
            //var r = new Random(0);
            //var nr = new NormalRandom(0, 0.1, r);
            const int stepcount = 150;
            Config.IsInUnitTesting = true;
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(stepcount);
            //_calcParameters.InitializeTimeSteps(startdate, enddate, new TimeSpan(0, 1, 0), 3, false);
            CalcParameters calcParameters =
                CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate);
            var timeStep = new TimeSpan(0, 1, 0);
            var cp = new CalcProfile("profile", Guid.NewGuid().ToStrGuid(), timeStep, ProfileType.Absolute, "blub");
            cp.AddNewTimepoint(new TimeSpan(0), 100);
            cp.AddNewTimepoint(new TimeSpan(0, 10, 0), 0);
            cp.ConvertToTimesteps();
            var variables = new List<CalcAffordanceVariableOp>();
            var variableReqs = new List<VariableRequirement>();
            var loc = new CalcLocation("loc", Guid.NewGuid().ToStrGuid());
            CalcVariableRepository calcVariableRepository = new CalcVariableRepository();
            var variableGuid = Guid.NewGuid().ToStrGuid();
            HouseholdKey key = new HouseholdKey("hh1");
            CalcVariable cv = new CalcVariable("varname", variableGuid, 0, loc.Name, loc.Guid, key);
            calcVariableRepository.RegisterVariable(cv);
            variables.Add(new CalcAffordanceVariableOp(cv.Name, 1, loc, VariableAction.SetTo,
                VariableExecutionTime.Beginning, variableGuid));
            BitArray isBusy = new BitArray(100, false);
            Random rnd = new Random();
            NormalRandom nr = new NormalRandom(0,1,rnd);
            using CalcRepo calcRepo = new CalcRepo(calcParameters: calcParameters, odap: new Mock<IOnlineDeviceActivationProcessor>().Object, normalRandom:nr, rnd:rnd);
            var aff = new CalcAffordance("bla", cp, loc, false, new List<CalcDesire>(), 0, 99,
                PermittedGender.All, false, 0.1, new ColorRGB(0, 0, 0), "bla", false, false, variables, variableReqs,
                ActionAfterInterruption.GoBackToOld, "bla", 100, false, "", Guid.NewGuid().ToStrGuid(),
                calcVariableRepository,
                new List<DeviceEnergyProfileTuple>(), isBusy, BodilyActivityLevel.Low, calcRepo, key);
            var lt = new CalcLoadType("load", "unit1", "unit2", 1, true, Guid.NewGuid().ToStrGuid());
            var cdl = new CalcDeviceLoad("cdl", 1, lt, 1, 0.1);
            var devloads = new List<CalcDeviceLoad> {
                cdl
            };
            CalcDeviceDto cdd = new CalcDeviceDto("device", deviceCategoryGuid,key,
                OefcDeviceType.Device,"category",string.Empty,
                Guid.NewGuid().ToStrGuid(),loc.Guid,loc.Name, FlexibilityType.NoFlexibility, 0);
            var cd = new CalcDevice( devloads,  loc,
                 cdd, calcRepo);
            //loc.Variables.Add("Variable1", 0);
            aff.AddDeviceTuple(cd, cp, lt, 0, timeStep, 10, 1);
            TimeStep ts = new TimeStep(0, 0, false);
            var person = new CalcPersonDto("name", null, 30, PermittedGender.Male, null, null, null, -1, null, null);
            aff.IsBusy(ts, loc, person);
            //var variableOperator = new VariableOperator();
            aff.Activate(ts, "blub", loc, out var _);
             calcVariableRepository.GetValueByGuid(variableGuid).Should().Be(1);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcAffordanceVariableTestSubtract()
        {
            //var r = new Random(0);
            //var nr = new NormalRandom(0, 0.1, r);
            const int stepcount = 150;
            HouseholdKey key = new HouseholdKey("hh1");
            Config.IsInUnitTesting = true;
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(stepcount);
            CalcParameters calcParameters =
                CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate);
            var timeStep = new TimeSpan(0, 1, 0);
            var cp = new CalcProfile("profile", Guid.NewGuid().ToStrGuid(), timeStep, ProfileType.Absolute, "blub");
            cp.AddNewTimepoint(new TimeSpan(0), 100);
            cp.AddNewTimepoint(new TimeSpan(0, 10, 0), 0);
            cp.ConvertToTimesteps();
            var variables = new List<CalcAffordanceVariableOp>();
            var variableReqs = new List<VariableRequirement>();
            var loc = new CalcLocation("loc", Guid.NewGuid().ToStrGuid());
            var variableGuid = Guid.NewGuid().ToStrGuid();

            variables.Add(new CalcAffordanceVariableOp("Variable1", 1, loc, VariableAction.Subtract,
                VariableExecutionTime.Beginning, variableGuid));
            CalcVariableRepository crv = new CalcVariableRepository();
            crv.RegisterVariable(new CalcVariable("Variable1", variableGuid, 0, loc.Name, loc.Guid,
                key));
            BitArray isBusy = new BitArray(calcParameters.InternalTimesteps, false);
            Random rnd = new Random();
            NormalRandom nr = new NormalRandom(0, 1, rnd);
            using  CalcRepo calcRepo = new CalcRepo(calcParameters: calcParameters, odap: new Mock<IOnlineDeviceActivationProcessor>().Object,
                rnd:rnd, normalRandom:nr);
            var aff = new CalcAffordance("bla", cp, loc, false, new List<CalcDesire>(), 0, 99,
                PermittedGender.All, false, 0.1, new ColorRGB(0, 0, 0), "bla", false, false, variables, variableReqs,
                ActionAfterInterruption.GoBackToOld, "bla", 100, false, "", Guid.NewGuid().ToStrGuid(),
                crv,
                new List<DeviceEnergyProfileTuple>(), isBusy, BodilyActivityLevel.Low, calcRepo,key);
            var lt = new CalcLoadType("load", "unit1", "unit2", 1, true, Guid.NewGuid().ToStrGuid());
            var cdl = new CalcDeviceLoad("cdl", 1, lt, 1, 0.1);
            var devloads = new List<CalcDeviceLoad> {
                cdl
            };
            var devCatGuid = Guid.NewGuid().ToStrGuid();
            CalcDeviceDto cdd = new CalcDeviceDto("device",
                devCatGuid,
                key,
                OefcDeviceType.Device,
                "category",
                string.Empty, Guid.NewGuid().ToStrGuid(),loc.Guid,loc.Name, FlexibilityType.NoFlexibility, 0);
            var cd = new CalcDevice( devloads, loc,
                cdd, calcRepo);

            //loc.Variables.Add("Variable1", 0);
            aff.AddDeviceTuple(cd, cp, lt, 0, timeStep, 10, 1);
            TimeStep ts = new TimeStep(0, 0, false);
            var person = new CalcPersonDto("name", null, 30, PermittedGender.Male, null, null, null, -1, null, null);
            aff.IsBusy(ts, loc, person);
            //var variableOperator = new VariableOperator();
            aff.Activate(ts, "blub", loc, out var _);
             crv.GetValueByGuid(variableGuid).Should().Be(-1);
            for (var i = 0; i < 15; i++) {
                TimeStep ts1 = new TimeStep(i, 0, false);
                cd.SetIsBusyForTesting(ts1, false, lt);
            }

            aff.IsBusy(ts, loc, person);
            aff.Activate(ts, "blub", loc, out var _);
            crv.GetValueByGuid(variableGuid).Should().Be(-2);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunDeviceOffsetTest()
        {
            //var r = new Random(0);
            //var nr = new NormalRandom(0, 0.1, r);
            const int stepcount = 150;
            var devCategoryGuid = Guid.NewGuid().ToStrGuid();
            Config.IsInUnitTesting = true;
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(stepcount);
            CalcParameters calcParameters =
                CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate);
            var timeStep = new TimeSpan(0, 1, 0);
            var cp = new CalcProfile("profile", Guid.NewGuid().ToStrGuid(), timeStep, ProfileType.Absolute, "blub");
            cp.AddNewTimepoint(new TimeSpan(0), 100);
            cp.AddNewTimepoint(new TimeSpan(0, 10, 0), 0);
            cp.ConvertToTimesteps();
            var loc = new CalcLocation(Utili.GetCurrentMethodAndClass(), Guid.NewGuid().ToStrGuid());
            CalcVariableRepository crv = new CalcVariableRepository();
            BitArray isBusy = new BitArray(calcParameters.InternalTimesteps, false);
            Random rnd = new Random();
            NormalRandom nr = new NormalRandom(0, 1, rnd);
            HouseholdKey key = new HouseholdKey("HH1");
            using CalcRepo calcRepo = new CalcRepo(calcParameters: calcParameters, odap:new Mock<IOnlineDeviceActivationProcessor>().Object,
                rnd: rnd, normalRandom: nr);
            var aff = new CalcAffordance("bla", cp, loc, false, new List<CalcDesire>(), 0, 99, PermittedGender.All,
                false, 0, new ColorRGB(0, 0, 0), "bla", false, false, new List<CalcAffordanceVariableOp>(),
                new List<VariableRequirement>(), ActionAfterInterruption.GoBackToOld, "bla", 100, false, "",
                Guid.NewGuid().ToStrGuid(), crv,
                new List<DeviceEnergyProfileTuple>(), isBusy, BodilyActivityLevel.Low, calcRepo,key);
            var lt = new CalcLoadType("load", "unit1", "unit2", 1, true, Guid.NewGuid().ToStrGuid());
            var cdl = new CalcDeviceLoad("cdl", 1, lt, 1, 0.1);
            //var variableOperator = new VariableOperator();
            var devloads = new List<CalcDeviceLoad> {
                cdl
            };

            CalcDeviceDto cdd = new CalcDeviceDto("device",
                devCategoryGuid,
                key,
                OefcDeviceType.Device,
                "category",
                string.Empty,
                Guid.NewGuid().ToStrGuid(),
                loc.Guid,
                loc.Name, FlexibilityType.NoFlexibility, 0);
            var cd = new CalcDevice( devloads,  loc, cdd, calcRepo);

            aff.AddDeviceTuple(cd, cp, lt, 20, timeStep, 10, 1);

            //bool result = aff.IsBusy(0, nr, r, loc);
            //(result).Should().BeFalse();
            CheckForBusyness(loc, aff, cd, lt);
            TimeStep ts = new TimeStep(0, 0, false);
            aff.Activate(ts.AddSteps(10), "blub", loc, out var _);
            CheckForBusyness( loc, aff, cd, lt);
            var person = new CalcPersonDto("name", null, 30, PermittedGender.Male, null, null, null, -1, null, null);
            aff.IsBusy(ts.AddSteps(1), loc, person, false).Should().NotBe(BusynessType.NotBusy);
            aff.IsBusy(ts.AddSteps(19), loc, person, false).Should().NotBe(BusynessType.NotBusy);
            aff.IsBusy(ts, loc, person, false).Should().Be(BusynessType.NotBusy);
            aff.IsBusy(ts.AddSteps(20), loc, person, false).Should().Be(BusynessType.NotBusy);
        }
    }
}
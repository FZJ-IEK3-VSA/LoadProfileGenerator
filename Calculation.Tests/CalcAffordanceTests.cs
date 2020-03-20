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
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.Enums;
using Common.JSON;
using Common.Tests;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Calculation.Tests {
    [TestFixture]
    public class CalcAffordanceTests : UnitTestBaseClass
    {
        private static void SetupProbabilityTest([NotNull] out CalcAffordance aff, [NotNull] out CalcLoadType lt,
                                                 [NotNull] out CalcDevice cd,
                                                 [NotNull] out CalcLocation loc, int stepcount, double probability)
        {
            Config.IsInUnitTesting = true;
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(stepcount);
            CalcParameters calcParameters =
                CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate);
            var timeStep = new TimeSpan(0, 1, 0);
            var cp = new CalcProfile("profile", Guid.NewGuid().ToString(), timeStep, ProfileType.Absolute, "blub");
            cp.AddNewTimepoint(new TimeSpan(0), 100);
            cp.AddNewTimepoint(new TimeSpan(0, 10, 0), 0);
            cp.ConvertToTimesteps();
            loc = new CalcLocation(Utili.GetCurrentMethodAndClass(), Guid.NewGuid().ToString());
            CalcVariableRepository cvr = new CalcVariableRepository();
            BitArray isBusy = new BitArray(100, false);
            aff = new CalcAffordance("bla", cp, loc, false, new List<CalcDesire>(), 0, 99, PermittedGender.All,
                false, 0.1,  new ColorRGB(0, 0, 0), "bla", false, false, new List<CalcAffordanceVariableOp>(),
                new List<VariableRequirement>(), ActionAfterInterruption.GoBackToOld, "bla", 100, false, "",
                calcParameters,
                Guid.NewGuid().ToString(), cvr, new List<CalcAffordance.DeviceEnergyProfileTuple>(), isBusy);
            lt = new CalcLoadType("load", "unit1", "unit2", 1, true, Guid.NewGuid().ToString());
            var cdl = new CalcDeviceLoad("cdl", 1, lt, 1, 0.1, Guid.NewGuid().ToString());
            var devloads = new List<CalcDeviceLoad> {
                cdl
            };
            string categoryGuid = Guid.NewGuid().ToString();
            cd = new CalcDevice("device", devloads, categoryGuid, null, loc, new HouseholdKey("HH1"),
                OefcDeviceType.Device, "category",
                string.Empty, calcParameters, Guid.NewGuid().ToString());
            aff.AddDeviceTuple(cd, cp, lt, 0, timeStep, 10, probability);
        }

        private static void CheckForBusyness([NotNull] Random r, [NotNull] NormalRandom nr, [NotNull] CalcLocation loc,
                                             [NotNull] CalcAffordance aff,
                                             [NotNull] CalcDevice cd, [NotNull] CalcLoadType lt)
        {
            Logger.Info("------------");
            TimeStep ts1 = new TimeStep(0,0,true);
            Logger.Info("aff.isbusy 0: " + aff.IsBusy(ts1, nr, r, loc, "person", false));
            var prevstate = false;
            for (var i = 0; i < 100; i++) {
                TimeStep ts2 = new TimeStep(i, 0,true);
                if (aff.IsBusy(ts2, nr, r, loc, "person", false) != prevstate) {
                    prevstate = aff.IsBusy(ts2, nr, r, loc, "name", false);
                    Logger.Info("aff.isbusy:" + i + ": " + prevstate);
                }
            }

            Logger.Info("aff.isbusy 100: " + aff.IsBusyArray[100]);

            prevstate = false;
            Logger.Info("aff.isbusyarray 0:   " + aff.IsBusyArray[0]);
            for (var i = 0; i < 100; i++) {
                if (aff.IsBusyArray[i] != prevstate) {
                    prevstate = aff.IsBusyArray[i];
                    Logger.Info("aff.isbusyarray: " + i + ": " + prevstate);
                }
            }

            Logger.Info("aff.isbusyarray 100: " + aff.IsBusyArray[100]);

            prevstate = false;
            TimeStep ts3 = new TimeStep(0, 0,false);
            Logger.Info("cd.isbusyarray 0:   " + cd.GetIsBusyForTesting(ts3, lt));
            for (var i = 0; i < 100; i++) {
                TimeStep ts4 = new TimeStep(i, 0, false);
                if (cd.GetIsBusyForTesting(ts4, lt) != prevstate) {
                    prevstate = cd.GetIsBusyForTesting(ts4, lt);
                    Logger.Info("cd.isbusyarray: " + i + ": " + prevstate);
                }
            }
            TimeStep ts5 = new TimeStep(100, 0, false);
            Logger.Info("cd.isbusyarray 100: " + cd.GetIsBusyForTesting(ts5, lt));
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CalcAffordanceActivateTest0Percent()
        {
            var calcParameters = CalcParameters.GetNew();
            var r = new Random(0);
            var nr = new NormalRandom(0, 0.1, r);
            const int stepcount = 150;
            SetupProbabilityTest(out var aff, out var lt, out var cd, out var loc, stepcount, 0);
            var trueCount = 0;
            TimeStep ts1 = new TimeStep(0,0,true);
            var result = aff.IsBusy(ts1, nr, r, loc, "name");
            Assert.IsFalse(result);
            const int resultcount = stepcount - 20;
            for (var i = 0; i < resultcount; i++) {
                for (var j = 0; j < stepcount; j++) {
                    TimeStep ts = new TimeStep(j,calcParameters);
                    cd.SetIsBusyForTesting(ts, false, lt);
                    cd.IsBusyForLoadType[lt][ts.InternalStep] = false;
                }
                TimeStep ts2 = new TimeStep(i, calcParameters);
                aff.IsBusy(ts2, nr, r, loc, "name");
                //var variableOperator = new VariableOperator();
                aff.Activate( ts2, "blub", loc, out var _);
                if (cd.GetIsBusyForTesting(ts2, lt)) {
                    trueCount++;
                }
            }

            Logger.Info("Truecount: " + trueCount);
            Assert.That(trueCount, Is.EqualTo(0).Within(10).Percent);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CalcAffordanceActivateTest100Percent()
        {
            var r = new Random(0);
            var nr = new NormalRandom(0, 0.1, r);
            const int stepcount = 150;
            SetupProbabilityTest(out var aff, out var lt, out var cd, out var loc, stepcount, 1);
            var trueCount = 0;
            TimeStep ts = new TimeStep(0,0,false);
            var result = aff.IsBusy(ts, nr, r, loc, "name");
            Assert.IsFalse(result);
            const int resultcount = stepcount - 20;
            for (var i = 0; i < resultcount; i++) {

                for (var j = 0; j < stepcount; j++) {
                    TimeStep ts2 = new TimeStep(j, 0, false);
                    cd.SetIsBusyForTesting(ts2, false, lt);
                    cd.IsBusyForLoadType[lt][j] = false;
                }
                TimeStep ts3 = new TimeStep(i, 0, false);
                aff.IsBusy(ts3, nr, r, loc, "name");
                //var variableOperator = new VariableOperator();
                aff.Activate(ts3, "blub", loc, out var _);
                if (cd.GetIsBusyForTesting(ts3, lt)) {
                    trueCount++;
                }
            }

            Logger.Info("Truecount: " + trueCount);
            Assert.That(trueCount, Is.EqualTo(resultcount).Within(10).Percent);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CalcAffordanceActivateTest25Percent()
        {
            var r = new Random(0);
            var nr = new NormalRandom(0, 0.1, r);
            const int stepcount = 150;
            SetupProbabilityTest(out var aff, out var lt, out CalcDevice cd, out var loc, stepcount, 0.25);
            var trueCount = 0;
            TimeStep ts = new TimeStep(0, 0, false);
            var result = aff.IsBusy(ts, nr, r, loc, "name");
            Assert.IsFalse(result);
            const int resultcount = stepcount - 20;
            for (var i = 0; i < resultcount; i++) {
                for (var j = 0; j < stepcount; j++) {
                    TimeStep ts2 = new TimeStep(j, 0, false);
                    cd.SetIsBusyForTesting(ts2, false, lt);
                    cd.IsBusyForLoadType[lt][j] = false;
                }
                TimeStep ts3 = new TimeStep(i, 0, false);
                aff.IsBusy(ts3, nr, r, loc, "name");
                //var variableOperator = new VariableOperator();
                aff.Activate(ts3, "blub", loc, out var _);
                if (cd.GetIsBusyForTesting(ts3, lt)) {
                    trueCount++;
                }
            }

            Logger.Info("Truecount: " + trueCount);
#pragma warning disable VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
            Assert.That(trueCount, Is.EqualTo(resultcount / 4).Within(10).Percent);
#pragma warning restore VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CalcAffordanceActivateTest50Percent()
        {
            var r = new Random(0);
            var nr = new NormalRandom(0, 0.1, r);
            const int stepcount = 150;
            SetupProbabilityTest(out var aff, out var lt, out var cd, out var loc, stepcount, 0.5);
            var trueCount = 0;
            TimeStep ts = new TimeStep(0, 0, false);
            var result = aff.IsBusy(ts, nr, r, loc, "name");
            Assert.IsFalse(result);
            const int resultcount = stepcount - 20;
            for (var i = 0; i < resultcount; i++) {
                for (var j = 0; j < stepcount; j++) {
                    TimeStep ts2 = new TimeStep(j, 0, false);
                    cd.SetIsBusyForTesting(ts2, false, lt);
                    cd.IsBusyForLoadType[lt][j] = false;
                }
                TimeStep ts3 = new TimeStep(i, 0, false);
                aff.IsBusy(ts3, nr, r, loc, "name");
                //var variableOperator = new VariableOperator();
                aff.Activate(ts3, "blub", loc, out var _);
                if (cd.GetIsBusyForTesting(ts3, lt)) {
                    trueCount++;
                }
            }

            Logger.Info("Truecount: " + trueCount);
#pragma warning disable VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
            Assert.That(trueCount, Is.EqualTo(resultcount / 2).Within(10).Percent);
#pragma warning restore VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CalcAffordanceActivateTest75Percent()
        {
            var r = new Random(0);
            var nr = new NormalRandom(0, 0.1, r);
            const int stepcount = 150;
            SetupProbabilityTest(out var aff, out var lt, out var cd, out var loc, stepcount, 0.75);
            var trueCount = 0;
            TimeStep ts = new TimeStep(0, 0, false);
            var result = aff.IsBusy(ts, nr, r, loc, "name");
            Assert.IsFalse(result);
            const int resultcount = stepcount - 20;
            for (var i = 0; i < resultcount; i++) {
                for (var j = 0; j < stepcount; j++) {
                    TimeStep ts2 = new TimeStep(j, 0, false);
                    cd.SetIsBusyForTesting(ts2, false, lt);
                    cd.IsBusyForLoadType[lt][j] = false;
                }
                TimeStep ts3 = new TimeStep(i, 0, false);
                aff.IsBusy(ts3, nr, r, loc, "name");
                //var variableOperator = new VariableOperator();
                aff.Activate(ts3, "blub", loc, out var _);
                if (cd.GetIsBusyForTesting(ts3, lt)) {
                    trueCount++;
                }
            }

            Logger.Info("Truecount: " + trueCount);
            Assert.That(trueCount, Is.EqualTo(resultcount * 0.75).Within(10).Percent);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
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
            var cp = new CalcProfile("profile", Guid.NewGuid().ToString(), timeStep, ProfileType.Absolute, "blub");
            cp.AddNewTimepoint(new TimeSpan(0), 100);
            cp.AddNewTimepoint(new TimeSpan(0, 10, 0), 0);
            cp.ConvertToTimesteps();
            //var variableOperator = new VariableOperator();
            var variables = new List<CalcAffordanceVariableOp>();
            var variableReqs = new List<VariableRequirement>();
            var loc = new CalcLocation("loc", Guid.NewGuid().ToString());
            string variableGuid = Guid.NewGuid().ToString();
            CalcVariableRepository variableRepository = new CalcVariableRepository();
            HouseholdKey key = new HouseholdKey("hh1");
            CalcVariable cv = new CalcVariable("calcvar1", variableGuid, 0, loc.Name, loc.Guid, key);
            variableRepository.RegisterVariable(cv);
            variables.Add(new CalcAffordanceVariableOp(cv.Name, 1, loc, VariableAction.Add,
                VariableExecutionTime.Beginning, cv.Guid));
            BitArray isBusy = new BitArray(100, false);
            var aff = new CalcAffordance("bla", cp, loc, false, new List<CalcDesire>(), 0, 99,
                PermittedGender.All, false, 0.1, new ColorRGB(0, 0, 0), "bla", false, false, variables, variableReqs,
                ActionAfterInterruption.GoBackToOld, "bla", 100, false, "", calcParameters, Guid.NewGuid().ToString(),
                variableRepository,
                new List<CalcAffordance.DeviceEnergyProfileTuple>(), isBusy);
            var lt = new CalcLoadType("load", "unit1", "unit2", 1, true, Guid.NewGuid().ToString());
            var cdl = new CalcDeviceLoad("cdl", 1, lt, 1, 0.1, Guid.NewGuid().ToString());
            var devloads = new List<CalcDeviceLoad> {
                cdl
            };
            string categoryGuid = Guid.NewGuid().ToString();
            var cd = new CalcDevice("device", devloads, categoryGuid, null, loc, new HouseholdKey("HH1"),
                OefcDeviceType.Device, "category",
                string.Empty, calcParameters, Guid.NewGuid().ToString());

            //loc.Variables.Add("Variable1", 0);
            aff.AddDeviceTuple(cd, cp, lt, 0, timeStep, 10, 1);
            TimeStep ts = new TimeStep(0, 0, false);
            aff.IsBusy(ts, nr, r, loc, "name");
            aff.Activate(ts, "blub", loc, out var _);
            Assert.AreEqual(1, variableRepository.GetValueByGuid(variableGuid));
            for (var i = 0; i < 15; i++) {
                TimeStep ts1 = new TimeStep(i, 0, false);
                cd.SetIsBusyForTesting(ts1, false, lt);
            }

            aff.IsBusy(ts, nr, r, loc, "name");
            aff.Activate(ts, "blub", loc, out var _);
            Assert.AreEqual(2, variableRepository.GetValueByGuid(variableGuid));
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CalcAffordanceVariableTestSet()
        {
            string deviceCategoryGuid = Guid.NewGuid().ToString();
            var r = new Random(0);
            var nr = new NormalRandom(0, 0.1, r);
            const int stepcount = 150;
            Config.IsInUnitTesting = true;
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(stepcount);
            //_calcParameters.InitializeTimeSteps(startdate, enddate, new TimeSpan(0, 1, 0), 3, false);
            CalcParameters calcParameters =
                CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate);
            var timeStep = new TimeSpan(0, 1, 0);
            var cp = new CalcProfile("profile", Guid.NewGuid().ToString(), timeStep, ProfileType.Absolute, "blub");
            cp.AddNewTimepoint(new TimeSpan(0), 100);
            cp.AddNewTimepoint(new TimeSpan(0, 10, 0), 0);
            cp.ConvertToTimesteps();
            var variables = new List<CalcAffordanceVariableOp>();
            var variableReqs = new List<VariableRequirement>();
            var loc = new CalcLocation("loc", Guid.NewGuid().ToString());
            CalcVariableRepository calcVariableRepository = new CalcVariableRepository();
            string variableGuid = Guid.NewGuid().ToString();
            HouseholdKey key = new HouseholdKey("hh1");
            CalcVariable cv = new CalcVariable("varname", variableGuid, 0, loc.Name, loc.Guid, key);
            calcVariableRepository.RegisterVariable(cv);
            variables.Add(new CalcAffordanceVariableOp(cv.Name, 1, loc, VariableAction.SetTo,
                VariableExecutionTime.Beginning, variableGuid));
            BitArray isBusy = new BitArray(100, false);
            var aff = new CalcAffordance("bla", cp, loc, false, new List<CalcDesire>(), 0, 99,
                PermittedGender.All, false, 0.1, new ColorRGB(0, 0, 0), "bla", false, false, variables, variableReqs,
                ActionAfterInterruption.GoBackToOld, "bla", 100, false, "", calcParameters, Guid.NewGuid().ToString(),
                calcVariableRepository,
                new List<CalcAffordance.DeviceEnergyProfileTuple>(), isBusy);
            var lt = new CalcLoadType("load", "unit1", "unit2", 1, true, Guid.NewGuid().ToString());
            var cdl = new CalcDeviceLoad("cdl", 1, lt, 1, 0.1, Guid.NewGuid().ToString());
            var devloads = new List<CalcDeviceLoad> {
                cdl
            };
            var cd = new CalcDevice("device", devloads, deviceCategoryGuid, null, loc, new HouseholdKey("HH1"),
                OefcDeviceType.Device, "category",
                string.Empty, calcParameters, Guid.NewGuid().ToString());
            //loc.Variables.Add("Variable1", 0);
            aff.AddDeviceTuple(cd, cp, lt, 0, timeStep, 10, 1);
            TimeStep ts = new TimeStep(0, 0, false);
            aff.IsBusy(ts, nr, r, loc, "name");
            //var variableOperator = new VariableOperator();
            aff.Activate(ts, "blub", loc, out var _);
            Assert.AreEqual(1, calcVariableRepository.GetValueByGuid(variableGuid));
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CalcAffordanceVariableTestSubtract()
        {
            var r = new Random(0);
            var nr = new NormalRandom(0, 0.1, r);
            const int stepcount = 150;
            HouseholdKey key = new HouseholdKey("hh1");
            Config.IsInUnitTesting = true;
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(stepcount);
            CalcParameters calcParameters =
                CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate);
            var timeStep = new TimeSpan(0, 1, 0);
            var cp = new CalcProfile("profile", Guid.NewGuid().ToString(), timeStep, ProfileType.Absolute, "blub");
            cp.AddNewTimepoint(new TimeSpan(0), 100);
            cp.AddNewTimepoint(new TimeSpan(0, 10, 0), 0);
            cp.ConvertToTimesteps();
            var variables = new List<CalcAffordanceVariableOp>();
            var variableReqs = new List<VariableRequirement>();
            var loc = new CalcLocation("loc", Guid.NewGuid().ToString());
            string variableGuid = Guid.NewGuid().ToString();

            variables.Add(new CalcAffordanceVariableOp("Variable1", 1, loc, VariableAction.Subtract,
                VariableExecutionTime.Beginning, variableGuid));
            CalcVariableRepository crv = new CalcVariableRepository();
            crv.RegisterVariable(new CalcVariable("Variable1", variableGuid, 0, loc.Name, loc.Guid,
                key));
            BitArray isBusy = new BitArray(calcParameters.InternalTimesteps, false);
            var aff = new CalcAffordance("bla", cp, loc, false, new List<CalcDesire>(), 0, 99,
                PermittedGender.All, false, 0.1, new ColorRGB(0, 0, 0), "bla", false, false, variables, variableReqs,
                ActionAfterInterruption.GoBackToOld, "bla", 100, false, "", calcParameters, Guid.NewGuid().ToString(),
                crv,
                new List<CalcAffordance.DeviceEnergyProfileTuple>(), isBusy);
            var lt = new CalcLoadType("load", "unit1", "unit2", 1, true, Guid.NewGuid().ToString());
            var cdl = new CalcDeviceLoad("cdl", 1, lt, 1, 0.1, Guid.NewGuid().ToString());
            var devloads = new List<CalcDeviceLoad> {
                cdl
            };
            string devCatGuid = Guid.NewGuid().ToString();
            var cd = new CalcDevice("device", devloads, devCatGuid, null, loc, key, OefcDeviceType.Device, "category",
                string.Empty, calcParameters, Guid.NewGuid().ToString());

            //loc.Variables.Add("Variable1", 0);
            aff.AddDeviceTuple(cd, cp, lt, 0, timeStep, 10, 1);
            TimeStep ts = new TimeStep(0, 0, false);
            aff.IsBusy(ts, nr, r, loc, "name");
            //var variableOperator = new VariableOperator();
            aff.Activate(ts, "blub", loc, out var _);
            Assert.AreEqual(-1, crv.GetValueByGuid(variableGuid));
            for (var i = 0; i < 15; i++) {
                TimeStep ts1 = new TimeStep(i, 0, false);
                cd.SetIsBusyForTesting(ts1, false, lt);
            }

            aff.IsBusy(ts, nr, r, loc, "name");
            aff.Activate(ts, "blub", loc, out var _);
            Assert.AreEqual(-2, crv.GetValueByGuid(variableGuid));
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void RunDeviceOffsetTest()
        {
            var r = new Random(0);
            var nr = new NormalRandom(0, 0.1, r);
            const int stepcount = 150;
            string devCategoryGuid = Guid.NewGuid().ToString();
            Config.IsInUnitTesting = true;
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(stepcount);
            CalcParameters calcParameters =
                CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate);
            var timeStep = new TimeSpan(0, 1, 0);
            var cp = new CalcProfile("profile", Guid.NewGuid().ToString(), timeStep, ProfileType.Absolute, "blub");
            cp.AddNewTimepoint(new TimeSpan(0), 100);
            cp.AddNewTimepoint(new TimeSpan(0, 10, 0), 0);
            cp.ConvertToTimesteps();
            var loc = new CalcLocation(Utili.GetCurrentMethodAndClass(), Guid.NewGuid().ToString());
            CalcVariableRepository crv = new CalcVariableRepository();
            BitArray isBusy = new BitArray(calcParameters.InternalTimesteps, false);
            var aff = new CalcAffordance("bla", cp, loc, false, new List<CalcDesire>(), 0, 99, PermittedGender.All,
                false, 0, new ColorRGB(0, 0, 0), "bla", false, false, new List<CalcAffordanceVariableOp>(),
                new List<VariableRequirement>(), ActionAfterInterruption.GoBackToOld, "bla", 100, false, "",
                calcParameters,
                Guid.NewGuid().ToString(), crv,
                new List<CalcAffordance.DeviceEnergyProfileTuple>(), isBusy);
            var lt = new CalcLoadType("load", "unit1", "unit2", 1, true, Guid.NewGuid().ToString());
            var cdl = new CalcDeviceLoad("cdl", 1, lt, 1, 0.1, Guid.NewGuid().ToString());
            //var variableOperator = new VariableOperator();
            var devloads = new List<CalcDeviceLoad> {
                cdl
            };

            var cd = new CalcDevice("device", devloads, devCategoryGuid, null, loc, new HouseholdKey("HH1"),
                OefcDeviceType.Device, "category",
                string.Empty, calcParameters, Guid.NewGuid().ToString());

            aff.AddDeviceTuple(cd, cp, lt, 20, timeStep, 10, 1);

            //bool result = aff.IsBusy(0, nr, r, loc);
            //Assert.IsFalse(result);
            CheckForBusyness(r, nr, loc, aff, cd, lt);
            TimeStep ts = new TimeStep(0, 0, false);
            aff.Activate(ts.AddSteps(10), "blub", loc, out var _);
            CheckForBusyness(r, nr, loc, aff, cd, lt);
            Assert.IsTrue(aff.IsBusy(ts.AddSteps(1), nr, r, loc, "name", false));
            Assert.IsTrue(aff.IsBusy( ts.AddSteps(19), nr, r, loc, "name", false));
            Assert.IsFalse(aff.IsBusy(ts, nr, r, loc, "name", false));
            Assert.IsFalse(aff.IsBusy(ts.AddSteps(20), nr, r, loc, "name", false));
        }
    }
}
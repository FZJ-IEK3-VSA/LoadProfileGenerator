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
using System.Collections.Generic;
using System.Globalization;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.Tests;
using NUnit.Framework;

namespace Calculation.Tests.OnlineDeviceLogging {
    [TestFixture]
    public class OnlineDeviceStateMachineTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void OnlineDeviceStateMachineTest()
        {
            var startdate = new DateTime(2018, 1, 1);
            var enddate = startdate.AddMinutes(200);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults()
                .SetStartDate(startdate).SetEndDate(enddate).SetSettlingDays(0).EnableShowSettlingPeriod();
            var values = new double[10];
            for (var i = 0; i < values.Length; i++) {
                values[i] = i + 1;
                Logger.Info(values[i].ToString(CultureInfo.CurrentCulture));
            }

            var valueList = new List<double>(values);
            var r = new Random(1);
            var nr = new NormalRandom(0, 1, r);
            string devGuid = Guid.NewGuid().ToString();
            string locGuid = Guid.NewGuid().ToString();
            var clt = new CalcLoadType("lt", "kWh", "W", 1, true, Guid.NewGuid().ToString());
            var key = new OefcKey(new HouseholdKey("hh1"), OefcDeviceType.Device, devGuid, locGuid, clt.Guid,"mycategory");
            var cp = new CalcProfile("mycalcprofile", Guid.NewGuid().ToString(), valueList, ProfileType.Absolute, "bla");
            TimeStep ts = new TimeStep(5, 0, false);
            var odsm = new OnlineDeviceStateMachine(cp, ts, 0, 1, nr, clt.ConvertToDto(), "device", key,"affordance",
                calcParameters);
            calcParameters.SetDummyTimeSteps(0);
            Assert.That( odsm.CalculateOfficialEnergyUse(),Is.EqualTo(55)); // all
            calcParameters.SetDummyTimeSteps(6);
            Assert.AreEqual(54, odsm.CalculateOfficialEnergyUse()); // not the first
            //_calcParameters.InternalTimesteps = 20;
            calcParameters.SetDummyTimeSteps(15);
            Assert.AreEqual(0, odsm.CalculateOfficialEnergyUse()); // none
            calcParameters.SetDummyTimeSteps(14);
            Assert.AreEqual(10, odsm.CalculateOfficialEnergyUse()); // only the last
            Logger.Info(odsm.CalculateOfficialEnergyUse().ToString(CultureInfo.CurrentCulture));
            startdate = new DateTime(2018, 1, 1);
            enddate = startdate.AddMinutes(10);
            calcParameters.DisableShowSettlingPeriod();
            calcParameters.SetStartDate(startdate).SetEndDate(enddate)
                .SetDummyTimeSteps(5);
            Assert.That( odsm.CalculateOfficialEnergyUse(),Is.EqualTo(15)); // only the first 5
            //_calcParameters.InternalTimesteps = 10; // only 5
            calcParameters.SetDummyTimeSteps(9);
            var val = odsm.CalculateOfficialEnergyUse();
            Assert.AreEqual(5, val); // only #5

            Logger.Info(odsm.CalculateOfficialEnergyUse().ToString(CultureInfo.CurrentCulture));
        }
    }
}
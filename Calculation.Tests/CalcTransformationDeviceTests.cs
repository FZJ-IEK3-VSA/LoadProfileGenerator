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
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.HouseElements;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using NUnit.Framework;

namespace Calculation.Tests {
    [TestFixture]
    public class CalcTransformationDeviceTests : TestBasis {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void ProcessOneTransformationDeviceTimestepTest()
        {
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
            var lf = MakeLogfile(wd.WorkingDirectory,calcParameters,wd.InputDataLogger,wd.SqlResultLoggingService);

            var odap = new OnlineDeviceActivationProcessor(NormalRandom, lf,calcParameters);
            var clt = new CalcLoadType("clt1", "W", "kWh", 1, true, Guid.NewGuid().ToString());
            string deviceGuid = Guid.NewGuid().ToString();
            string locGuid = Guid.NewGuid().ToString();
            var key = new OefcKey(new HouseholdKey("HH1"), OefcDeviceType.Device, deviceGuid, locGuid,clt.Guid, "mycategory");
            odap.RegisterDevice("dev1", key, "loc", clt.ConvertToDto());
            double[] timestepValues = {1.0, 0};
            var tmplist = new List<double>(timestepValues);
            var cp = new CalcProfile("myCalcProfile", Guid.NewGuid().ToString(), tmplist,  ProfileType.Absolute,
                "synthetic"
            );
            TimeStep ts1 = new TimeStep(1, 0, true);
            odap.AddNewStateMachine(cp,  ts1,0, 10,  "dev1", clt.ConvertToDto(), "blub",  "name1", "p1", "syn",key);
            double[] resultValues = {0, 10.0, 0, 0, 0, 0, 0, 0, 0, 0};
            double[] resultValuesRow1 = {0, 20.0, 0, 0, 0, 0, 0, 0, 0, 0};
            double[] resultValuesRow2 = {0, 30.0, 0, 0, 0, 0, 0, 0, 0, 0};
            var ctd = new CalcTransformationDevice("trafo1",  odap, -1, 080, -1000, 1000,
                new HouseholdKey("housekey"), Guid.NewGuid().ToString());
            ctd.SetInputLoadtype(clt);
            var clt2 = new CalcLoadType("clt2", "W2", "kWh2", 1, true, Guid.NewGuid().ToString());
            ctd.AddOutputLoadType(clt2, 2, TransformationOutputFactorType.Fixed);
            var clt3 = new CalcLoadType("clt3", "W2", "kWh2", 1, true, Guid.NewGuid().ToString());
            ctd.AddOutputLoadType(clt3, 3, TransformationOutputFactorType.Fixed);
            for (var i = 0; i < 10; i++) {
                TimeStep ts = new TimeStep(i,0,true);
                var filerows = odap.ProcessOneTimestep(ts);
                Assert.AreEqual(3, filerows.Count);
                Assert.AreEqual(1, filerows[0].EnergyEntries.Count);
                var sb = new StringBuilder("row0 before:");
                sb.Append(filerows[0].EnergyEntries[0]);
                sb.Append(" row1 before:");
                sb.Append(filerows[1].EnergyEntries[0]);
                Assert.AreEqual(resultValues[i], filerows[0].EnergyEntries[0]);
                Assert.AreEqual(0, filerows[1].EnergyEntries[0]);
                Assert.AreEqual(0, filerows[2].EnergyEntries[0]);
                ctd.ProcessOneTimestep(filerows, null);
                Assert.AreEqual(resultValuesRow1[i], filerows[1].EnergyEntries[0]);
                Assert.AreEqual(resultValuesRow2[i], filerows[2].EnergyEntries[0]);
                sb.Append(" row0 after:");
                sb.Append(filerows[0].EnergyEntries[0]);
                sb.Append(" row1 after:");
                sb.Append(filerows[1].EnergyEntries[0]);
                sb.Append(" row2 after:");
                sb.Append(filerows[2].EnergyEntries[0]);
                Logger.Info(Utili.GetCurrentMethodAndClass() + " " + sb);
            }

            wd.CleanUp();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void ProcessOneTimestepTestVariableFactor()
        {
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
            var lf = MakeLogfile(wd.WorkingDirectory,calcParameters, wd.InputDataLogger,wd.SqlResultLoggingService);
            var odap = new OnlineDeviceActivationProcessor(NormalRandom, lf, calcParameters);
            var clt = new CalcLoadType("clt1",  "W", "kWh", 1, true, Guid.NewGuid().ToString());
            var devguid = Guid.NewGuid().ToString();
            var locguid = Guid.NewGuid().ToString();
            var key = new OefcKey(new HouseholdKey("HH1"), OefcDeviceType.Device,devguid, locguid,clt.Guid, "mycategory");
            odap.RegisterDevice("dev1", key, "loc", clt.ConvertToDto());
            double[] timestepValues = {0, 5, 10.0, 20, 30, 40, 0};
            var cp = new CalcProfile("myCalcProfile", Guid.NewGuid().ToString(), timestepValues.ToList(), ProfileType.Absolute,
                "synthetic");
            TimeStep ts1 = new TimeStep(1,0,true);
            odap.AddNewStateMachine(cp, ts1, 0, 1, "dev1", clt.ConvertToDto(), "blub", "name1", "p1", "syn",key);
            double[] resultValues = {0, 0, 5, 10.0, 20, 30, 40, 0};
            double[] resultValuesRow1 = {0, 0, 5, 10, 200, 3000, 4000, 0};
            var ctd = new CalcTransformationDevice("trafo1",  odap, -1, 080, -1000, 1000, new HouseholdKey(  "housekey"), Guid.NewGuid().ToString());
            ctd.SetInputLoadtype(clt);
            var clt2 = new CalcLoadType("clt2",  "W2", "kWh2", 1, true, Guid.NewGuid().ToString());
            ctd.AddOutputLoadType(clt2,2, TransformationOutputFactorType.Interpolated);

            ctd.AddDatapoint(10, 1);
            ctd.AddDatapoint(20, 10);
            ctd.AddDatapoint(30, 100);
            for (var i = 0; i < resultValues.Length; i++) {
                TimeStep ts = new TimeStep(i,calcParameters);
                var filerows = odap.ProcessOneTimestep(ts);
                Assert.AreEqual(2, filerows.Count);
                Assert.AreEqual(1, filerows[0].EnergyEntries.Count);
                var sb = new StringBuilder("row0 before:");
                sb.Append(filerows[0].EnergyEntries[0]);
                sb.Append(" row1 before:");
                sb.Append(filerows[1].EnergyEntries[0]);
                Assert.AreEqual(resultValues[i], filerows[0].EnergyEntries[0]);
                Assert.AreEqual(0, filerows[1].EnergyEntries[0]);
                ctd.ProcessOneTimestep(filerows, null);
                Assert.AreEqual(resultValuesRow1[i], filerows[1].EnergyEntries[0]);
                sb.Append(" row0 after:");
                sb.Append(filerows[0].EnergyEntries[0]);
                sb.Append(" row1 after:");
                sb.Append(filerows[1].EnergyEntries[0]);
                Logger.Info(Utili.GetCurrentMethodAndClass() + " " + sb);
            }

            wd.CleanUp();
        }
    }
}
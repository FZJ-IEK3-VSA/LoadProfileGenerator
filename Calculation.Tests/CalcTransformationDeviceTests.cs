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
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Calculation.Tests {
    public class CalcTransformationDeviceTests : CalcUnitTestBase
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void ProcessOneTimestepTestVariableFactor()
        {
            var calcParameters = CalcParametersFactory.MakeGoodDefaults();
            using var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            using var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "name", wd.InputDataLogger);
            using var old = new OnlineLoggingData(new DateStampCreator(calcParameters), wd.InputDataLogger, calcParameters);
            var odap = new OnlineDeviceActivationProcessor(old, calcParameters, fft);
            var clt = new CalcLoadType("clt1", "W", "kWh", 1, true, Guid.NewGuid().ToStrGuid());
            var devguid = Guid.NewGuid().ToStrGuid();
            var locguid = Guid.NewGuid().ToStrGuid();
            var cdd = new CalcDeviceDto("dev1", "devcatguid".ToStrGuid(), new HouseholdKey("HH1"), OefcDeviceType.Device, "devcatname", "", devguid, locguid,
                "loc");
            var key = new OefcKey(cdd, clt.Guid);
            odap.RegisterDevice(clt.ConvertToDto(), cdd);
            double[] timestepValues = { 0, 5, 10.0, 20, 30, 40, 0 };
            var cp = new CalcProfile("myCalcProfile", Guid.NewGuid().ToStrGuid(), timestepValues.ToList(), ProfileType.Absolute, "synthetic");
            var ts1 = new TimeStep(1, 0, true);
            CalcDeviceLoad cdl = new CalcDeviceLoad("",1,clt,0,0);
            var sv = StepValues.MakeStepValues(cp,  1, RandomValueProfile.MakeStepValues(cp, NormalRandom, 0), cdl);
            odap.AddNewStateMachine(ts1, clt.ConvertToDto(),  "name1", "p1",  key, cdd, sv);
            double[] resultValues = { 0, 0, 5, 10.0, 20, 30, 40, 0 };
            //double[] resultValuesRow1 = {0, 0, 5, 10, 200, 3000, 4000, 0};
            var ctd = new CalcTransformationDevice(odap, -1, 080, -1000, 1000, cdd, clt);
            var clt2 = new CalcLoadType("clt2", "W2", "kWh2", 1, true, Guid.NewGuid().ToStrGuid());
            ctd.AddOutputLoadType(clt2, 2, TransformationOutputFactorType.Interpolated);

            ctd.AddDatapoint(10, 1);
            ctd.AddDatapoint(20, 10);
            ctd.AddDatapoint(30, 100);
            for (var i = 0; i < resultValues.Length; i++)
            {
                var ts = new TimeStep(i, calcParameters);
                var filerows = odap.ProcessOneTimestep(ts);
                Assert.Equal(2, filerows.Count);
                Assert.Equal(1, filerows[0].EnergyEntries.Count);
                var sb = new StringBuilder("row0 before:");
                sb.Append(filerows[0].EnergyEntries[0]);
                sb.Append(" row1 before:");
                sb.Append(filerows[1].EnergyEntries[0]);
                Assert.Equal(resultValues[i], filerows[0].EnergyEntries[0]);
                Assert.Equal(0, filerows[1].EnergyEntries[0]);
                ctd.ProcessOneTimestep(filerows, null);
                //(filerows[1].EnergyEntries[0]).Should().Be(resultValuesRow1[i]);
                sb.Append(" row0 after:");
                sb.Append(filerows[0].EnergyEntries[0]);
                sb.Append(" row1 after:");
                sb.Append(filerows[1].EnergyEntries[0]);
                Logger.Info(Utili.GetCurrentMethodAndClass() + " " + sb);
            }

            wd.CleanUp();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void ProcessOneTransformationDeviceTimestepTest()
        {
            var calcParameters = CalcParametersFactory.MakeGoodDefaults();
            calcParameters.EnableShowSettlingPeriod();
            using var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            using var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hh1", wd.InputDataLogger);
            fft.RegisterGeneralHouse();
            using var old = new OnlineLoggingData(new DateStampCreator(calcParameters), wd.InputDataLogger, calcParameters);
            var odap = new OnlineDeviceActivationProcessor(old, calcParameters, fft);
            var clt = new CalcLoadType("clt1", "W", "kWh", 1, true, Guid.NewGuid().ToStrGuid());
            var deviceGuid = Guid.NewGuid().ToStrGuid();
            var locGuid = Guid.NewGuid().ToStrGuid();
            var cdd = new CalcDeviceDto("dev1", "devcatguid".ToStrGuid(), new HouseholdKey("HH1"), OefcDeviceType.Device, "devcatname", string.Empty,
                deviceGuid, locGuid, "loc");
            var key = new OefcKey(cdd, clt.Guid);
            odap.RegisterDevice(clt.ConvertToDto(), cdd);
            double[] timestepValues = {1.0, 0};
            var tmplist = new List<double>(timestepValues);
            var cp = new CalcProfile("myCalcProfile", Guid.NewGuid().ToStrGuid(), tmplist, ProfileType.Absolute, "synthetic");
            var ts1 = new TimeStep(1, 0, true);
            var cdl = new CalcDeviceLoad("",10,clt,0,0);
            var sv = StepValues.MakeStepValues(cp, 1, RandomValueProfile.MakeStepValues(cp, NormalRandom, 0), cdl);
            odap.AddNewStateMachine(ts1, clt.ConvertToDto(),
                "name1", "p1", key, cdd, sv);
            double[] resultValues = {0, 10.0, 0, 0, 0, 0, 0, 0, 0, 0};
            double[] resultValuesRow1 = {0, 20.0, 0, 0, 0, 0, 0, 0, 0, 0};
            double[] resultValuesRow2 = {0, 30.0, 0, 0, 0, 0, 0, 0, 0, 0};
            var trafocdd = new CalcDeviceDto("trafo1", "devcatguid".ToStrGuid(), new HouseholdKey("housekey"), OefcDeviceType.Device, "devcatname",
                string.Empty, Guid.NewGuid().ToStrGuid(), locGuid, "loc");
            var ctd = new CalcTransformationDevice(odap, -1, 080, -1000, 1000, trafocdd, clt);
            var clt2 = new CalcLoadType("clt2", "W2", "kWh2", 1, true, Guid.NewGuid().ToStrGuid());
            ctd.AddOutputLoadType(clt2, 2, TransformationOutputFactorType.FixedFactor);
            var clt3 = new CalcLoadType("clt3", "W2", "kWh2", 1, true, Guid.NewGuid().ToStrGuid());
            ctd.AddOutputLoadType(clt3, 3, TransformationOutputFactorType.FixedFactor);
            for (var i = 0; i < 10; i++) {
                var ts = new TimeStep(i, 0, true);
                var filerows = odap.ProcessOneTimestep(ts);
                Assert.Equal(3, filerows.Count);
                Assert.Equal(1, filerows[0].EnergyEntries.Count);
                var sb = new StringBuilder("row0 before:");
                sb.Append(filerows[0].EnergyEntries[0]);
                sb.Append(" row1 before:");
                sb.Append(filerows[1].EnergyEntries[0]);
                Assert.Equal(resultValues[i], filerows[0].EnergyEntries[0]);
                Assert.Equal(0, filerows[1].EnergyEntries[0]);
                Assert.Equal(0, filerows[2].EnergyEntries[0]);
                ctd.ProcessOneTimestep(filerows, null);
                Assert.Equal(resultValuesRow1[i], filerows[1].EnergyEntries[0]);
                Assert.Equal(resultValuesRow2[i], filerows[2].EnergyEntries[0]);
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

        public CalcTransformationDeviceTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
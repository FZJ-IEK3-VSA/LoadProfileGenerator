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
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Calculation.Tests {
    public class CalcEnergyStorageTests : CalcUnitTestBase {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void ProcessOneEnergyStorageTimestepTest()
        {
            using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
                using (OnlineLoggingData old = new OnlineLoggingData(new DateStampCreator(calcParameters), wd.InputDataLogger, calcParameters))
                {
                    using (FileFactoryAndTracker fft = new FileFactoryAndTracker(wd.WorkingDirectory, "name", wd.InputDataLogger))
                    {
                        var odap = new OnlineDeviceActivationProcessor(old, calcParameters, fft);
                        var clt = new CalcLoadType("clt1", "W", "kWh", 1, true, Guid.NewGuid().ToStrGuid());
                        var deviceGuid = Guid.NewGuid().ToStrGuid();
                        HouseholdKey hhkey = new HouseholdKey("HH1");
                        var locationGuid = Guid.NewGuid().ToStrGuid();
                        CalcDeviceDto cdd = new CalcDeviceDto("dev1", "devcatguid".ToStrGuid(),
                            hhkey, OefcDeviceType.Device, "devcatname", "", deviceGuid, locationGuid, "loc");
                        var key = new OefcKey(cdd, clt.Guid);
                        odap.RegisterDevice(clt.ConvertToDto(), cdd);
                        double[] timestepValue = { 1.0, 0 };
                        var timestepValues = new List<double>(timestepValue);
                        var cp = new CalcProfile("myCalcProfile", Guid.NewGuid().ToStrGuid(), timestepValues,
                            ProfileType.Absolute, "synthetic");
                        CalcDeviceLoad cdl1 = new CalcDeviceLoad("", -10, clt, 0, 0);
                        StepValues sv1 = StepValues.MakeStepValues(cp,  1, RandomValueProfile.MakeStepValues(cp,NormalRandom,0),cdl1);
                        odap.AddNewStateMachine(new TimeStep(1, 0, true),
                            clt.ConvertToDto(), "name1", "p1", key, cdd, sv1);
                        CalcDeviceLoad cdl2 = new CalcDeviceLoad("", -100, clt, 0, 0);
                        StepValues sv2 = StepValues.MakeStepValues(cp, 1,RandomValueProfile.MakeStepValues(cp, NormalRandom, 0), cdl2);
                        odap.AddNewStateMachine(new TimeStep(3, 0, true),
                             clt.ConvertToDto(),  "name2",  "syn", key, cdd, sv2);
                        CalcDeviceLoad cdl3 = new CalcDeviceLoad("", -10, clt, 0, 0);
                        StepValues sv3 = StepValues.MakeStepValues(cp,1, RandomValueProfile.MakeStepValues(cp, NormalRandom, 0), cdl3);
                        odap.AddNewStateMachine(new TimeStep(5, 0, true),
                            clt.ConvertToDto(),  "name3",  "syn", key, cdd, sv3);
                        CalcDeviceLoad cdl4 = new CalcDeviceLoad("", 100, clt, 0, 0);
                        StepValues sv4 = StepValues.MakeStepValues(cp,  1, RandomValueProfile.MakeStepValues(cp, NormalRandom, 0), cdl4);
                        odap.AddNewStateMachine(new TimeStep(7, 0, true),
                            clt.ConvertToDto(),  "name4", "syn", key, cdd, sv4);
                        double[] resultValues = { 0, -10.0, 0, -100, 0, 10, 0, 100, 0, 0 };
                        var ces = new CalcEnergyStorage(odap, clt.ConvertToDto(),
                            100, 7, 0, 0, 5, 20, null,
                                            cdd);
                        List<OnlineEnergyFileRow> rawRows = new List<OnlineEnergyFileRow>();
                        int keyidx = 0;
                        foreach (var keys in odap.Oefc.ColumnEntriesByLoadTypeByDeviceKey.Values)
                        {
                            foreach (KeyValuePair<OefcKey, ColumnEntry> pair in keys)
                            {
                                Logger.Info("Key " + keyidx + " " + pair.Key.ToString() + " - " + pair.Value);
                                keyidx++;
                            }
                        }
                        for (var i = 0; i < 10; i++)
                        {
                            TimeStep ts = new TimeStep(i, 0, true);
                            var filerows = odap.ProcessOneTimestep(ts);
                            rawRows.Add(filerows[0]);
                            filerows.Count.Should().Be(1);
                            filerows[0].EnergyEntries.Count.Should().Be(1);
                            var sb = new StringBuilder("row0 before:");
                            sb.Append(filerows[0].EnergyEntries[0]);
                            sb.Append(" : ");
                            //sb.Append(filerows[0].EnergyEntries[1]);
                             filerows[0].EnergyEntries[0].Should().Be(resultValues[i]);
                            for (var j = 0; j < 5; j++)
                            {
                                ces.ProcessOneTimestep(filerows, ts, null);
                            }

                            sb.Append(" row0 after:");
                            sb.Append(filerows[0].EnergyEntries[0]);
                            sb.Append(" : ");
                            //sb.Append(filerows[0].EnergyEntries[1]);
                            sb.Append(" :StorageLevel ");
                            sb.Append(ces.PreviousFillLevel);
                            sb.Append(" :Expected ");
                            sb.Append(resultValues[i]);
                            Logger.Info(sb.ToString());
                        }
                        rawRows.Count.Should().Be(10);
                    }
                }
                wd.CleanUp();
            }
        }

        public CalcEnergyStorageTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
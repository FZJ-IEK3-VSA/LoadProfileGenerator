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
using Automation;
using CalculationEngine.HouseElements;
using CalculationEngine.HouseholdElements;
using Common;
using Common.JSON;
using Common.Tests;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Calculation.Tests
{
    [TestFixture]
    public class CalcEnergyStorageSignalTests : UnitTestBaseClass
    {
        private static void TestCess([NotNull] CalcEnergyStorageSignal cess, [NotNull] TimeStep timestep, double capacity, double currentfill, [NotNull] List<double> values)
        {
            double d = cess.GetValue(timestep, capacity, currentfill);
            Logger.Info("Timestep:" + timestep + " StorageLevel:" + currentfill + " Result:" + d);
            values.Add(d);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void EnergyStorageSignalTests()
        {
            CalcParameters cp = CalcParameters.GetNew();
            cp.DummyCalcSteps = 0;
            CalcLoadType clt = new CalcLoadType("clt","power","sum",1,true, Guid.NewGuid().ToString());
            CalcEnergyStorageSignal cess = new CalcEnergyStorageSignal("blub",  100, 50, 2, clt, Guid.NewGuid().ToString());
            List<double> values = new List<double>();
            int i = 0;
            TestCess(cess,new TimeStep(  i++,cp), 100, 100, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 100, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 60, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 50, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 40, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 40, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 40, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 60, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 90, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 100, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 90, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 10, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 20, values);
            TestCess(cess, new TimeStep(i, cp), 100, 100, values);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void EnergyStorageSignalTests2()
        {
            CalcParameters cp = CalcParameters.GetNew();
            cp.DummyCalcSteps = 0;
            CalcLoadType clt = new CalcLoadType("clt",  "power", "sum", 1, true, Guid.NewGuid().ToString());
            CalcEnergyStorageSignal cess = new CalcEnergyStorageSignal("blub", 100, 50, 2, clt, Guid.NewGuid().ToString());
            List<double> values = new List<double>();
            int i = 0;
            TestCess(cess, new TimeStep(i++, cp), 100, 100, values);
            TestCess(cess, new TimeStep(i++, cp) , 100, 5, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 100, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 5, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 100, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 5, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 100, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 5, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 100, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 5, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 100, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 5, values);
            TestCess(cess, new TimeStep(i++, cp), 100, 100, values);
            TestCess(cess, new TimeStep(i, cp), 100, 5, values);
        }
    }
}
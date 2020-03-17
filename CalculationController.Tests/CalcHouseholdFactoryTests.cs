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
using Automation;
using CalculationController.CalcFactories;
using Common;
using Common.Tests;
using Database.Tables.BasicElements;
using Database.Tests;
using NUnit.Framework;

namespace CalculationController.Tests {
    [TestFixture]
    public class CalcHouseholdFactoryTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void GetCalcProfileTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.CalcController);

            var tp = new TimeBasedProfile("blub", null, db.ConnectionString, TimeProfileType.Relative,
                "fake", Guid.NewGuid().ToString());
            tp.SaveToDB();
            tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 100, false);
            tp.AddNewTimepoint(new TimeSpan(0, 2, 0), 0, false);
            tp.AddNewTimepoint(new TimeSpan(0, 4, 0), 100, false);
            tp.AddNewTimepoint(new TimeSpan(0, 6, 0), 0, false);
            var ctp = CalcDeviceFactory.GetCalcProfile(tp, new TimeSpan(0, 0, 30));
            Assert.AreEqual(4, ctp.TimeSpanDataPoints.Count);
            Assert.AreEqual(12, ctp.StepValues.Count);
            var v = ctp.StepValues;
            Assert.AreEqual(v[0], 1);
            Assert.AreEqual(v[1], 1);
            Assert.AreEqual(v[2], 1);
            Assert.AreEqual(v[3], 1);
            db.Cleanup();
        }
    }
}
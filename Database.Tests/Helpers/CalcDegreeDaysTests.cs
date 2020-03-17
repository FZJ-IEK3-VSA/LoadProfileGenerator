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
using Common;
using Common.Tests;
using Database.Helpers;
using NUnit.Framework;

namespace Database.Tests.Helpers {
    [TestFixture]
    public class CalcDegreeDaysTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void MakeDegreeDaysTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            var temperaturProfiles = db.LoadTemperatureProfiles();

            var startTime = new DateTime(2012, 1, 1);
            var endTime = new DateTime(2012, 12, 31);
            var ldd = MakeDegreeDaysClass.MakeDegreeDays(temperaturProfiles[0], startTime, endTime, 15, 20,
                10000, false, 0);
            double sumHeating = 0;
            double sumPercentages = 0;
            var temperatures = temperaturProfiles[0].GetTemperatureArray(startTime, endTime,
                new TimeSpan(1, 0, 0, 0));
            for (var i = 0; i < temperatures.Length; i++) {
                Assert.Less(Math.Abs(ldd[i].AverageTemperature - temperatures[i]), 0.001);
            }
            foreach (var day in ldd) {
                sumHeating += day.HeatingAmount;
                sumPercentages += day.Percentage;
            }
            Assert.Less(Math.Abs(10000 - sumHeating), 0.001);
            Assert.Less(Math.Abs(1 - sumPercentages), 0.001);
            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void MakeDegreeDaysTestWithPrecalcPeriod()
        {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            var temperaturProfiles = db.LoadTemperatureProfiles();

            var startTime = new DateTime(2011, 12, 27);
            var endTime = new DateTime(2012, 12, 31);
            var ldd = MakeDegreeDaysClass.MakeDegreeDays(temperaturProfiles[0], startTime, endTime, 15, 20,
                10000, false, 0);
            double sumHeating = 0;
            double sumPercentages = 0;
            var temperatures = temperaturProfiles[0].GetTemperatureArray(startTime, endTime,
                new TimeSpan(1, 0, 0, 0));
            for (var i = 0; i < temperatures.Length; i++)
            {
                Assert.Less(Math.Abs(ldd[i].AverageTemperature - temperatures[i]), 0.001);
            }
            foreach (var day in ldd)
            {
                if(day.Date.Year != 2012)
                {
                     continue; //only count values from 2012
                }
                sumHeating += day.HeatingAmount;
                sumPercentages += day.Percentage;
            }
            Assert.Less(Math.Abs(10000 - sumHeating), 0.001);
            Assert.Less(Math.Abs(1 - sumPercentages), 0.001);
            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void MakeDegreeDaysTest3Days() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            var temperaturProfiles = db.LoadTemperatureProfiles();
            var startTime = new DateTime(2012, 1, 1);
            var endTime = new DateTime(2012, 1, 3);
            var ldd = MakeDegreeDaysClass.MakeDegreeDays(temperaturProfiles[0], startTime, endTime, 15, 20,
                10000, false, 0);
            double sumHeating = 0;
            double sumPercentages = 0;
            var temperatures = temperaturProfiles[0].GetTemperatureArray(startTime, endTime,
                new TimeSpan(1, 0, 0, 0));
            for (var i = 0; i < temperatures.Length; i++) {
                Assert.Less(Math.Abs(ldd[i].AverageTemperature - temperatures[i]), 0.001);
            }
            foreach (var day in ldd) {
                sumHeating += day.HeatingAmount;
                sumPercentages += day.Percentage;
            }
            Assert.Less(Math.Abs(101 - sumHeating), 100);
            Assert.Less(Math.Abs(0.01 - sumPercentages), 0.01);
            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void MakeDegreeDaysTest3Years() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            var temperaturProfiles = db.LoadTemperatureProfiles();
            var startTime = new DateTime(2012, 1, 1);
            var endTime = new DateTime(2014, 12, 31);
            var ldd = MakeDegreeDaysClass.MakeDegreeDays(temperaturProfiles[0], startTime, endTime, 15, 20,
                10000, false, 0);
            double sumHeating = 0;
            double sumPercentages = 0;
            var temperatures = temperaturProfiles[0].GetTemperatureArray(startTime, endTime,
                new TimeSpan(1, 0, 0, 0));
            for (var i = 0; i < temperatures.Length; i++) {
                Assert.Less(Math.Abs(ldd[i].AverageTemperature - temperatures[i]), 0.001);
            }
            foreach (var day in ldd) {
                sumHeating += day.HeatingAmount;
                sumPercentages += day.Percentage;
            }
            Assert.Less(Math.Abs(30000 - sumHeating), 0.001);
            Assert.Less(Math.Abs(3 - sumPercentages), 0.001);
            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void MakeDegreeDaysTestMinus3Days() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            var temperaturProfiles = db.LoadTemperatureProfiles();

            var startTime = new DateTime(2011, 12, 29);
            var endTime = new DateTime(2012, 1, 3);
            var ldd = MakeDegreeDaysClass.MakeDegreeDays(temperaturProfiles[0], startTime, endTime, 15, 20,
                10000, false, 0);
            double sumHeating = 0;
            double sumPercentages = 0;
            var temperatures = temperaturProfiles[0].GetTemperatureArray(startTime, endTime,
                new TimeSpan(1, 0, 0, 0));
            for (var i = 0; i < temperatures.Length; i++) {
                Assert.Less(Math.Abs(ldd[i].AverageTemperature - temperatures[i]), 0.001);
            }
            foreach (var day in ldd) {
                sumHeating += day.HeatingAmount;
                sumPercentages += day.Percentage;
            }
            Assert.Less(Math.Abs(225 - sumHeating), 200);
            const double val = 225.0 / 10000;
            Assert.Less(Math.Abs(val - sumPercentages), 0.1);
            db.Cleanup();
        }
    }
}
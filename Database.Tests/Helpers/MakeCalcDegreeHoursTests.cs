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
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Helpers {

    public class MakeCalcDegreeHoursTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MakeDegreeDaysTest3Days()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var temperaturProfiles = db.LoadTemperatureProfiles();
                var startTime = new DateTime(2012, 1, 1);
                var endTime = new DateTime(2012, 1, 3);
                var ldd = DbCalcDegreeHour.GetCalcDegreeHours(temperaturProfiles[0], startTime, endTime, 19,
                    10000, false, 0);
                double sumHeating = 0;
                double sumPercentages = 0;
                var temperatures = temperaturProfiles[0].GetTemperatureArray(startTime, endTime, new TimeSpan(1, 0, 0));
                for (var i = 0; i < temperatures.Length; i++)
                {
                    Math.Abs(ldd[i].AverageTemperature - temperatures[i]).Should().BeLessThan( 0.001);
                }
                foreach (var day in ldd)
                {
                    sumHeating += day.CoolingAmount;
                    sumPercentages += day.Percentage;
                }
                Math.Abs(101 - sumHeating).Should().BeLessThan(102);
                Math.Abs(0.01 - sumPercentages).Should().BeLessThan( 0.011);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MakeDegreeDaysTest3Years()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var temperaturProfiles = db.LoadTemperatureProfiles();
                var startTime = new DateTime(2012, 1, 1);
                var endTime = new DateTime(2014, 12, 31);
                var ldd = DbCalcDegreeHour.GetCalcDegreeHours(temperaturProfiles[0], startTime, endTime, 19,
                    10000, false, 0);
                double sumHeating = 0;
                double sumPercentages = 0;
                var temperatures = temperaturProfiles[0].GetTemperatureArray(startTime, endTime, new TimeSpan(1, 0, 0));
                for (var i = 0; i < temperatures.Length; i++)
                {
                    Math.Abs(ldd[i].AverageTemperature - temperatures[i]).Should().BeLessThan( 0.001);
                }
                foreach (var day in ldd)
                {
                    sumHeating += day.CoolingAmount;
                    sumPercentages += day.Percentage;
                }
                Math.Abs(30000 - sumHeating).Should().BeLessThan( 0.001);
                Math.Abs(3 - sumPercentages).Should().BeLessThan( 0.001);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MakeDegreeDaysTestMinus3Days()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var temperaturProfiles = db.LoadTemperatureProfiles();

                var startTime = new DateTime(2011, 12, 29);
                var endTime = new DateTime(2012, 1, 3);
                var ldd = DbCalcDegreeHour.GetCalcDegreeHours(temperaturProfiles[0], startTime, endTime, 19,
                    10000, false, 0);
                double sumHeating = 0;
                double sumPercentages = 0;
                var temperatures = temperaturProfiles[0].GetTemperatureArray(startTime, endTime, new TimeSpan(1, 0, 0));
                for (var i = 0; i < temperatures.Length; i++)
                {
                    Math.Abs(ldd[i].AverageTemperature - temperatures[i]).Should().BeLessThan( 0.001);
                }
                foreach (var day in ldd)
                {
                    sumHeating += day.CoolingAmount;
                    sumPercentages += day.Percentage;
                }
                Math.Abs(225 - sumHeating).Should().BeLessThan( 226);
                const double val = 225.0 / 10000;
                Math.Abs(val - sumPercentages).Should().BeLessThan( 0.1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void MakeDegreeHoursTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var temperaturProfiles = db.LoadTemperatureProfiles();

                var startTime = new DateTime(2012, 1, 1);
                var endTime = new DateTime(2012, 12, 31);
                var ldd = DbCalcDegreeHour.GetCalcDegreeHours(temperaturProfiles[0], startTime, endTime, 19,
                    10000, false, 0);
                double sumHeating = 0;
                double sumPercentages = 0;
                var temperatures = temperaturProfiles[0].GetTemperatureArray(startTime, endTime, new TimeSpan(1, 0, 0));
                for (var i = 0; i < temperatures.Length; i++)
                {
                    Math.Abs(ldd[i].AverageTemperature - temperatures[i]).Should().BeLessThan( 0.001);
                }
                foreach (var day in ldd)
                {
                    sumHeating += day.CoolingAmount;
                    sumPercentages += day.Percentage;
                }
                Math.Abs(10000 - sumHeating).Should().BeLessThan( 0.001);
                Math.Abs(1 - sumPercentages).Should().BeLessThan( 0.001);
                db.Cleanup();
            }
        }

        public MakeCalcDegreeHoursTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
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
using CalculationController.DtoFactories;
using CalculationEngine.HouseholdElements;
using Common;
using Common.JSON;
using Common.Tests;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Calculation.Tests.HouseholdElements {
    [TestFixture]
    public class CalcPersonDesiresTests : UnitTestBaseClass
    {
        public CalcPersonDesiresTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcPersonDesiresTest()
        {
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
            var r = new Random(1);
            using (CalcRepo calcRepo = new CalcRepo(calcParameters: calcParameters, rnd: r))
            {
                var cpd = new CalcPersonDesires(calcRepo);
                var cd1 = new CalcDesire("blub0", 1, 0.5m, 0.1m, 1, 1, 1, -1, null, "", "");
                cpd.AddDesires(cd1);
                var cd2 = new CalcDesire("blub1", 2, 0.5m, 0.1m, 1, 1, 1, -1, null, "", "");
                cpd.AddDesires(cd2);
                Logger.Info("CalcPersonDesiresTest:" + cd1.Value + " ; " + cd2.Value);
                // satisfaction
                var satis1 = new CalcDesire("blub", 1, 0.5m, 12, 1, 1, 60, -1, null, "", "")
                {
                    Value = 1
                };
                var satisfactionValues = new List<CalcDesire>
            {
                satis1
            };
                Logger.Info(cd1.Value + " ; " + cd2.Value);
                for (var i = 0; i < 20; i++)
                {
                    TimeStep ts = new TimeStep(i, 0, false);
                    cpd.ApplyDecay(ts);
                    cpd.ApplyAffordanceEffect(satisfactionValues, true, "blub");
                    Logger.Info(cd1.Value + " ; " + cd2.Value);
                }
            }
        }

        /// <summary>
        ///     Run two desires 20 steps decay without a shared desire value between them
        ///     therefore the desires should be unequal
        /// </summary>
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcPersonNonSharedDesiresTest()
        {
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
            var r = new Random(1);
            using (CalcRepo calcRepo = new CalcRepo(calcParameters: calcParameters, rnd: r))
            {
                var cpd = new CalcPersonDesires(calcRepo);
                var cd1 = new CalcDesire("blub0", 1, 0.5m, 1m, 1, 1, 1, -1, null, "", "");
                cpd.AddDesires(cd1);
                var cd2 = new CalcDesire("blub1", 2, 0.5m, 2m, 1, 1, 60, -1, null, "", "");
                cpd.AddDesires(cd2);

                Logger.Info("CalcPersonDesiresTest:" + cd1.Value + " ; " + cd2.Value);
                var satis1 = new CalcDesire("blub", 1, 0.5m, 12, 1, 1, 60, -1, null, "", "")
                {
                    Value = 1
                };
                var satisfactionValues = new List<CalcDesire>
            {
                satis1
            };
                for (var i = 0; i < 20; i++)
                {
                    TimeStep ts = new TimeStep(i, 0, true);
                    cpd.ApplyDecay(ts);
                    if (i % 5 == 0)
                    {
                        cpd.ApplyAffordanceEffect(satisfactionValues, false, "blub");
                    }
                    Logger.Info(i + ": " + cd1.Value + " ; " + cd2.Value);
                    Assert.AreNotEqual(cd1.Value, cd2.Value);
                }
                // satisfaction
                Logger.Info(cd1.Value + " ; " + cd2.Value);
            }
        }

        /// <summary>
        ///     Run two desires 20 steps decay with a shared desire value between them
        ///     therefore the desires should be equal
        /// </summary>
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcPersonSharedDesiresTest()
        {
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
            var r = new Random(1);
            using (CalcRepo calcRepo = new CalcRepo(calcParameters: calcParameters, rnd: r))
            {
                var sdv = new SharedDesireValue(1, new TimeStep(0, 0, true));
                var cpd = new CalcPersonDesires(calcRepo);
                var cd1 = new CalcDesire("blub0", 1, 0.5m, 1m, 1, 1, 1, -1, sdv, "", "");
                cpd.AddDesires(cd1);
                var cd2 = new CalcDesire("blub1", 2, 0.5m, 2m, 1, 1, 60, -1, sdv, "", "");
                cpd.AddDesires(cd2);

                Logger.Info("CalcPersonDesiresTest:" + cd1.Value + " ; " + cd2.Value);
                var satis1 = new CalcDesire("blub", 1, 0.5m, 12, 1, 1, 60, -1, null, "", "")
                {
                    Value = 1
                };
                var satisfactionValues = new List<CalcDesire>
            {
                satis1
            };
                for (var i = 0; i < 20; i++)
                {
                    TimeStep ts = new TimeStep(0, 0, true);
                    cpd.ApplyDecay(ts);
                    if (i % 5 == 0)
                    {
                        cpd.ApplyAffordanceEffect(satisfactionValues, false, "blub");
                    }
                    Logger.Info(cd1.Value + " ; " + cd2.Value);
                    Assert.AreEqual(cd1.Value, cd2.Value);
                }

                // satisfaction

                Logger.Info(cd1.Value + " ; " + cd2.Value);
            }
        }
    }
}
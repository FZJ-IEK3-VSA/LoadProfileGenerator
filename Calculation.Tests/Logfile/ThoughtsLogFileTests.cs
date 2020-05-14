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
using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging.InputLoggers;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Calculation.Tests.Logfile
{
    [TestFixture]
    public class ThoughtsLogFileTests : TestBasis
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void BasicTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().EnableShowSettlingPeriod().SetSettlingDays(5);
                using (FileFactoryAndTracker fft = new FileFactoryAndTracker(wd.WorkingDirectory, "blub", wd.InputDataLogger))
                {
                    wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
                    wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                    fft.RegisterHousehold(new HouseholdKey("HH1"), "test", HouseholdKeyType.Household, "desc", null, null);
                    ThoughtsLogFile tlf = new ThoughtsLogFile(fft, calcParameters);
                    Random rnd = new Random();
                    //NormalRandom nr = new NormalRandom(0, 0.1, rnd);
                    // this array is pure nonsense and only to make it stop crashing the unit test
                    //_calcParameters.InternalDateTimeForSteps = new List<DateTime>(4);
                    //for (int i = 0; i < 4; i++)
                    //{
                    //  _calcParameters.InternalDateTimeForSteps.Add(DateTime.Now);
                    //}
                    CalcLocation cloc = new CalcLocation("cloc", Guid.NewGuid().ToStrGuid());
                    BitArray isSick = new BitArray(calcParameters.InternalTimesteps);
                    BitArray isOnVacation = new BitArray(calcParameters.InternalTimesteps);
                    CalcPersonDto dto = CalcPersonDto.MakeExamplePerson();
                    Mock<ILogFile> lf = new Mock<ILogFile>();
                    NormalRandom nr = new NormalRandom(0, 0.1, rnd);
                    using (CalcRepo calcRepo = new CalcRepo(rnd: rnd, lf: lf.Object, calcParameters: calcParameters, normalRandom: nr))
                    {
                        CalcPerson cp = new CalcPerson(dto,
cloc, isSick, isOnVacation, calcRepo);
                        //"personName", 0, 1, rnd, 1, PermittedGender.Male, null, "HH1" ,cloc,"traittag", "hhname0",calcParameters,isSick,Guid.NewGuid().ToStrGuid());
                        TimeStep ts = new TimeStep(0, 0, false);
                        ThoughtEntry te = new ThoughtEntry(cp, ts, "blua");
                        calcParameters.SetSettlingDays(0);
                        tlf.WriteEntry(te, new HouseholdKey("HH1"));
                    }
                    tlf.Dispose();
                }
                Assert.AreEqual(true, true);
                wd.CleanUp();
            }
        }

        public ThoughtsLogFileTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
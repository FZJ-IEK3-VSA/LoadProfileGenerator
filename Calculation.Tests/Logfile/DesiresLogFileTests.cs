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
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging.InputLoggers;
using NUnit.Framework;

namespace Calculation.Tests.Logfile
{
    [TestFixture]
    public class DesiresLogFileTests : TestBasis
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TestBasics()
        {
            // needs major redesign of calcperson class
            // StreamFactory sf = new StreamFactory();

            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(100);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate).EnableShowSettlingPeriod().SetSettlingDays(0);
            WorkingDir wd = new WorkingDir("desiresLogfile");
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
            var fft = new FileFactoryAndTracker(wd.WorkingDirectory, "test1",wd.InputDataLogger);
            fft.RegisterHousehold(Constants.GeneralHouseholdKey,"general",HouseholdKeyType.General,"desc", null, null);
            //SqlResultLoggingService srls = new SqlResultLoggingService(wd.WorkingDirectory);
            CalculationProfiler profiler = new CalculationProfiler();
            CalcRepo calcRepo = CalcRepo.Make(calcParameters, wd.InputDataLogger, wd.WorkingDirectory, "name", profiler);
                DesiresLogFile dlf = new DesiresLogFile(fft, calcParameters);
                CalcDesire cd1 = new CalcDesire("desire1", 1, 0.5m, 12, 1, 1, 60, -1, null,"","");

                //NormalRandom nr = new NormalRandom(0, 0.1, r);
                CalcLocation cloc = new CalcLocation("cloc",Guid.NewGuid().ToString());
                BitArray isSick = new BitArray(calcParameters.InternalTimesteps);
                BitArray isOnVacation = new BitArray(calcParameters.InternalTimesteps);
                CalcPersonDto calcPerson = CalcPersonDto.MakeExamplePerson();
                CalcPerson cp = new CalcPerson(calcPerson,  cloc,
                     isSick, isOnVacation,calcRepo);
                    //"bla", 1, 5, r, 48, PermittedGender.Male, lf, "HH1", cloc, "traittag", "hhname0",calcParameters,isSick,Guid.NewGuid().ToString());
                cp.PersonDesires.AddDesires(cd1);
                dlf.RegisterDesires(cp.PersonDesires.Desires.Values);
                TimeStep ts = new TimeStep(0,0,true);
                DesireEntry de = new DesireEntry(cp, ts, cp.PersonDesires, dlf,calcParameters);
                fft.RegisterHousehold(new HouseholdKey("hh1"), "bla", HouseholdKeyType.Household,"desc",null,null);
                dlf.WriteEntry(de, new HouseholdKey("hh1"));
                dlf.Dispose();
                Assert.AreEqual(true, true);
            wd.CleanUp();
        }
    }
}
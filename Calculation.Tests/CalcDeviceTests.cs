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
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.HouseholdElements;
using CalculationEngine.OnlineDeviceLogging;
using CalculationEngine.OnlineLogging;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.InputLoggers;
using NUnit.Framework;

namespace Calculation.Tests {
    [TestFixture]
    public class CalcDeviceTests : TestBasis {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void SetTimeprofileTest()
        {
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(10);
            CalcParameters calcParameters =
                CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate);

            CalcLoadType clt = MakeCalcLoadType();
            CalcLocation cloc = new CalcLocation("blub", Guid.NewGuid().ToString());
            CalcDeviceLoad cdl = new CalcDeviceLoad("cdl1", 1, clt, 1, 0.1, Guid.NewGuid().ToString());
            List<CalcDeviceLoad> cdls = new List<CalcDeviceLoad>();
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            wd.InputDataLogger.AddSaver(new ColumnEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
            wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
            FileFactoryAndTracker fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hh1", wd.InputDataLogger);
            fft.RegisterHousehold(Constants.GeneralHouseholdKey, "General", HouseholdKeyType.General, "desc",null,null);
            //SqlResultLoggingService srls = new SqlResultLoggingService(wd.WorkingDirectory);
            DateStampCreator dsc = new DateStampCreator(calcParameters);

            IOnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger, calcParameters);
            using (LogFile lf = new LogFile(calcParameters, fft, old, wd.SqlResultLoggingService)) {
                cdls.Add(cdl);

                OnlineDeviceActivationProcessor odap =
                    new OnlineDeviceActivationProcessor( lf, calcParameters);
                string deviceCategoryGuid = Guid.NewGuid().ToString();
                CalcDeviceDto cdd = new CalcDeviceDto("bla", deviceCategoryGuid
                    , new HouseholdKey("HH-6"), OefcDeviceType.Device, "category",
                    string.Empty, Guid.NewGuid().ToString(),cloc.Guid,cloc.Name);
                CalcRepo calcRepo = new CalcRepo(odap:odap, calcParameters:calcParameters, normalRandom:NormalRandom);
                CalcDevice cd = new CalcDevice( cdls,   cloc,
                     cdd,calcRepo );
                CalcProfile cp = MakeCalcProfile5Min100();
                TimeStep ts1 = new TimeStep(1,calcParameters);
                cd.SetAllLoadTypesToTimeprofile(cp, ts1, "test", "name1", 1);
                TimeStep ts = new TimeStep(0, calcParameters);
                Assert.AreEqual(false, cd.IsBusyDuringTimespan(ts, 1, 1, clt));
                Assert.AreEqual(true, cd.IsBusyDuringTimespan(ts.AddSteps(1), 1, 1, clt));
                Assert.AreEqual(true, cd.IsBusyDuringTimespan(ts.AddSteps(2), 1, 1, clt));
                Assert.AreEqual(true, cd.IsBusyDuringTimespan(ts.AddSteps(3), 1, 1, clt));
                Assert.AreEqual(true, cd.IsBusyDuringTimespan(ts.AddSteps(4), 1, 1, clt));
                Assert.AreEqual(true, cd.IsBusyDuringTimespan(ts.AddSteps(5), 1, 1, clt));
                Assert.AreEqual(false, cd.IsBusyDuringTimespan(ts.AddSteps(6), 0, 1, clt));
            }

            wd.CleanUp();
        }
    }
}
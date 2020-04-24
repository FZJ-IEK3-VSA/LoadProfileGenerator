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
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.OnlineLogging;
using Common;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Calculation.Tests.Logfile
{
    [TestFixture]
    public class LocationLogFileTests : TestBasis
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TestLocationEntryBasics()
        {
            Config.IsInUnitTesting = true;
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(1000);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate).SetSettlingDays(0).EnableShowSettlingPeriod();
            //FileFactoryAndTracker fft = new FileFactoryAndTracker(wd.WorkingDirectory,"blub",wd.InputDataLogger);
            //CalcLocation cl = new CalcLocation("blub", 1, Guid.NewGuid().ToString());
            //Mock<ILogFile> lf = new Mock<ILogFile>();
            //CalcPerson cp = MakeCalcPerson(cl,calcParameters,lf.Object);
            HouseholdKey key = new HouseholdKey("hh1");
            TimeStep ts = new TimeStep(1,0,false);
            LocationEntry le = new LocationEntry(key,"personName","personGuid", ts, "locname",
                "locguid");
            DateStampCreator dsc = new DateStampCreator(calcParameters);
            OnlineLoggingData old = new OnlineLoggingData(dsc,wd.InputDataLogger,calcParameters);
            wd.InputDataLogger.AddSaver(new LocationEntryLogger(wd.SqlResultLoggingService));
            old.AddLocationEntry(le);
            old.FinalSaveToDatabase();
            var lel = new LocationEntryLogger(wd.SqlResultLoggingService);
            var e = lel.Load(key);
            Assert.That(e.Count, Is.EqualTo(1));
            wd.CleanUp();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void TestLocationEntryBasicsWithFile()
        {
            Config.IsInUnitTesting = true;
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            wd.InputDataLogger.AddSaver(new LocationEntryLogger(wd.SqlResultLoggingService));
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(1000);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate).SetSettlingDays(0).EnableShowSettlingPeriod();

            //FileFactoryAndTracker fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hhname", wd.InputDataLogger);
            //LocationsLogFile llf = new LocationsLogFile(true, fft, calcParameters);
            //CalcLocation cl = new CalcLocation("blub", 1, Guid.NewGuid().ToString());
            //Mock<ILogFile> lf = new Mock<ILogFile>();
            //CalcPerson cp = MakeCalcPerson(cl,calcParameters,lf.Object);
            //LocationEntry le = new LocationEntry(cp, 1, cl,calcParameters);
            //llf.WriteEntry(le, new HouseholdKey("HH1"));
            //llf.WriteEntry(le, new HouseholdKey("HH2"));
            //llf.WriteEntry(le, new HouseholdKey("HH3"));
            //llf.Close();
            Assert.AreEqual(true, true);
            HouseholdKey key1 = new HouseholdKey("hh1");
            TimeStep ts = new TimeStep(1, 0, false);
            LocationEntry le1 = new LocationEntry(key1, "personName", "personGuid", ts, "locname",
                "locguid");
            HouseholdKey key2 = new HouseholdKey("hh2");
            LocationEntry le2 = new LocationEntry(key2, "personName", "personGuid", ts, "locname",
                "locguid");
            HouseholdKey key3 = new HouseholdKey("hh3");
            LocationEntry le3 = new LocationEntry(key3, "personName", "personGuid", ts, "locname",
                "locguid");
            DateStampCreator dsc = new DateStampCreator(calcParameters);
            OnlineLoggingData old = new OnlineLoggingData(dsc, wd.InputDataLogger,calcParameters);
            old.AddLocationEntry(le1);
            old.AddLocationEntry(le2);
            old.AddLocationEntry(le3);
            old.FinalSaveToDatabase();
            LocationEntryLogger lel = new LocationEntryLogger(wd.SqlResultLoggingService);
            var le = lel.Load(key1);
            Logger.Info("count: " + le.Count);
            string prev = JsonConvert.SerializeObject(le1, Formatting.Indented);
            string loaded = JsonConvert.SerializeObject(le[0],Formatting.Indented);
            Assert.That(loaded,Is.EqualTo(prev));
            wd.CleanUp();
        }
    }
}
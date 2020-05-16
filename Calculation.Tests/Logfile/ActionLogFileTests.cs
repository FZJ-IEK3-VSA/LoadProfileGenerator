/*
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Media;
using CalcController.CalcFactories;
using Calculation.HouseholdElements;
using Calculation.OnlineLogging;
using CommonDataWPF;
using CommonDataWPF.Enums;
using CommonDataWPF.JSON;


namespace Calculation.Tests.Logfile {
    
    public class ActionLogFileTests : TestBasis {
        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public static void TestActionLogFileTestsBasics() {
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(1000);
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate).EnableShowSettlingPeriod();
            Config.IsInUnitTesting = true;
            var sf = new StreamFactory();
            var fft = sf.GetFFT(wd.WorkingDirectory);
            fft.RegisterHouseholdKey("HH1-6","hh1-6");
            var alf = new ActionLogFile(fft, "HH1-6",calcParameters );
            var cl = new CalcLocation("loc", 1);
            var r = new Random();
            //var nr = new NormalRandom(0, 1, r);
            BitArray isSick = new BitArray(calcParameters.InternalTimesteps);
            var cp = new CalcPerson("name", 1, 1, r, 1, PermittedGender.Male, null, "HH1-6", cl,"traittag", "hhname0",calcParameters,isSick);
            var calcProfile = new CalcProfile("bla",new List<double>(),new Dictionary<int, CalcProfile>(),ProfileType.Absolute, "source"  );
            //var ae = new ActionEntry(cp, 0, string.Empty, "cat1", "true");
            CalcAffordance calcAffordance = new CalcAffordance("blub",1,calcProfile,cl,false,new List<CalcDesire>(),
                0,99,PermittedGender.All, false,0,Colors.AliceBlue,"",false,false,new List<CalcAffordanceVariableOp>(),
                new List<CalcAffordanceVariableRequirement>(),ActionAfterInterruption.GoBackToOld, "",1,false,"",calcParameters );
            alf.WriteEntry( "HH1-6",0,cp,calcAffordance,false);
            alf.Close();
            (true).Should().Be(true);
            wd.CleanUp();
        }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void TestLoading() {
            SetUp();
            DateTime startdate = new DateTime(2018, 1, 1);
            DateTime enddate = startdate.AddMinutes(1000);
            CalcParameters calcParameters =  CalcParametersFactory.MakeGoodDefaults().SetStartDate(startdate).SetEndDate(enddate).EnableShowSettlingPeriod();
            Config.IsInUnitTesting = true;
            var sf = new StreamFactory();
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            calcParameters.Enable(CalcOption.ActionsLogfile);
            var fft = sf.GetFFT(wd.WorkingDirectory);
            fft.RegisterHouseholdKey("HH1","bla");

            var alf = new ActionLogFile(fft, "HH1",calcParameters);
            var cloc = new CalcLocation("cloc", 1);
            var cp = MakeCalcPerson(cloc,calcParameters);
            var ae = new ActionEntry(cp, 0, string.Empty, "cat1", "true",calcParameters);
            var calcProfile = new CalcProfile("bla", new List<double>(), new Dictionary<int, CalcProfile>(), ProfileType.Absolute, "source");
            CalcAffordance calcAffordance = new CalcAffordance("blub", 1, calcProfile, cloc, false, new List<CalcDesire>(),
                0, 99, PermittedGender.All, false, 0, Colors.AliceBlue, "", false, false, new List<CalcAffordanceVariableOp>(),
                new List<CalcAffordanceVariableRequirement>(), ActionAfterInterruption.GoBackToOld, "", 1, false, "",calcParameters);
            alf.WriteEntry("HH1",0,cp, calcAffordance,false);
            alf.Flush();
            var ms = (MemoryStream) fft.GetResultFileEntry(ResultFileID.Actions, null, "HH1",null,null).Stream;
            if(ms == null) {
                throw new LPGException("Stream was null");
            }
            ms.Seek(0, SeekOrigin.Begin);
            string s;
            using (var sr = new StreamReader(ms)) {
                sr.ReadLine();
                s = sr.ReadLine();
            }
            if (s == null) {
                throw new LPGException("s was null");
            }
            var readae = new ActionEntry(s,calcParameters);
            alf.Close();
            (readae.StrPerson).Should().Be(ae.CPerson?.Name);
            wd.CleanUp();
            TearDown();
        }
    }
}*/
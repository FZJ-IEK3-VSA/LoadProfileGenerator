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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Automation.ResultFiles;
using CalculationEngine.Helper;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.Tests;
using NUnit.Framework;

namespace Calculation.Tests
{
    public class TestBasis : UnitTestBaseClass
    {
        [JetBrains.Annotations.NotNull]
        protected NormalRandom NormalRandom { get; private set; } = new NormalRandom(0, 0.1, new Random());

        [JetBrains.Annotations.NotNull]
        protected static CalcLoadType MakeCalcLoadType() => new CalcLoadType("loadtype1", "Watt", "kWh", 1 / 1000.0,
            true, Guid.NewGuid().ToString());

        /*
        [NotNull]
        protected static CalcPerson MakeCalcPerson([NotNull] CalcLocation cloc, [NotNull] CalcParameters calcParameters, [NotNull] ILogFile lf)
        {
            BitArray isSick = new BitArray(calcParameters.InternalTimesteps);
            BitArray isOnVacation = new BitArray(calcParameters.InternalTimesteps);
            CalcPersonDto pdto = CalcPersonDto.MakeExamplePerson();
            //calcpersonname", 1, 1, new Random(), 1, PermittedGender.Male, null,
            //"hh-6", cloc, "traittag", "testhh", calcParameters,isSick, Guid.NewGuid().ToString()
            return new CalcPerson(pdto,new Random(), lf,cloc,calcParameters,isSick,isOnVacation);
        }*/

        [JetBrains.Annotations.NotNull]
        protected static CalcProfile MakeCalcProfile5Min100()
        {
            var cp = new CalcProfile("5min100%", Guid.NewGuid().ToString(),  new TimeSpan(0, 1, 0), ProfileType.Relative, "foo");
            cp.AddNewTimepoint(new TimeSpan(0, 0, 0), 1);
            cp.AddNewTimepoint(new TimeSpan(0, 5, 0), 0);
            cp.ConvertToTimesteps();
            return cp;
        }


        [SuppressMessage("Microsoft.Naming",
            "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SetUp")]
        [SetUp]
        protected void SetUp()
        {
            var st = new StackTrace(1);
            var sf = st.GetFrame(0);
            var declaringType = sf.GetMethod().DeclaringType;
            if (declaringType == null)
            {
                throw new LPGException("type was null.");
            }
            var msg = declaringType.FullName + "." + sf.GetMethod().Name;
            var rnd = new Random();
            NormalRandom = new NormalRandom(0, 1, rnd);
            Logger.Info("locked by " + msg);

            Monitor.Enter(MyLock.Locker);
        }

        [SuppressMessage("Microsoft.Naming",
            "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TearDown")]
        [TearDown]
        protected void TearDown()
        {
            Monitor.Exit(MyLock.Locker);
            var st = new StackTrace(1);
            var sf = st.GetFrame(0);
            var type = sf.GetMethod().DeclaringType;
            if (type == null)
            {
                throw new LPGException("Type was null.");
            }
            var msg = type.FullName + "." + sf.GetMethod().Name;
            Logger.LogToFile = false;
            Logger.Info("unlocked by " + msg);
        }

        private static class MyLock
        {
            [JetBrains.Annotations.NotNull]
            public static object Locker { get; } = new object();
        }
    }
}
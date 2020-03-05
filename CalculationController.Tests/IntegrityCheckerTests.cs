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
using CalculationController.Integrity;
using Common;
using Common.Tests;
using Database;
using Database.Tables.BasicHouseholds;
using Database.Tests;
using NUnit.Framework;

namespace CalculationController.Tests
{
    [TestFixture]
    public class IntegrityCheckerTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void CheckSimIntegrityCheckerTest()
        {
            CleanTestBase.RunAutomatically(false);
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.CalcController);
            Simulator sim = new Simulator(db.ConnectionString);
            Assert.AreNotEqual(null, sim);
            Vacation vac = sim.Vacations.CreateNewItem(sim.ConnectionString);
            vac.AddVacationTime(new DateTime(2017,1,1),new DateTime(2017,2,1),VacationType.GoAway  );
            vac.AddVacationTime(new DateTime(2017, 2, 1), new DateTime(2017, 2, 15), VacationType.GoAway);
            SimIntegrityChecker.Run(sim);
            db.Cleanup();
            CleanTestBase.RunAutomatically(true);
        }

        [Test]
        [Category("BasicTest")]
        public void CheckSimIntegrityCheckerBenchmark()
        {
            CleanTestBase.RunAutomatically(false);
            const int runcount = 1;
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.CalcController);
            Simulator sim = new Simulator(db.ConnectionString) {MyGeneralConfig = {PerformCleanUpChecks = "True"}};
            Assert.AreNotEqual(null, sim);
            DateTime start = DateTime.Now;
            for (int i = 0; i < runcount; i++)
            {
                SimIntegrityChecker.Run(sim);
            }

            DateTime end = DateTime.Now;
            var duration = end - start;
            Logger.Info("Duration was:" + duration.TotalMilliseconds/runcount + " milliseconds");
            db.Cleanup();
            CleanTestBase.RunAutomatically(true);
        }

        [Test]
        [Category("BasicTest")]
        public void SimHouseIntegiryChecker()
        {
            CleanTestBase.RunAutomatically(false);
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.CalcController);
            Simulator sim = new Simulator(db.ConnectionString);
            Assert.AreNotEqual(null, sim);
            foreach (var house in sim.Houses.MyItems)
            {
                HouseIntegrityChecker.Run(house, sim);
            }
            db.Cleanup();
            CleanTestBase.RunAutomatically(true);
        }
    }
}
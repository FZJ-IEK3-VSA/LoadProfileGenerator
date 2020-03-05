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
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.Helper;
using Common;
using Common.Enums;
using Common.JSON;
using Common.Tests;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using NUnit.Framework;

namespace CalculationController.Tests.CalcFactories {
    [TestFixture]
    public class CalcPersonFactoryTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void AddMoreDesiresTest()
        {
            Config.IsInUnitTesting = true;
            CalcParameters parameters = CalcParametersFactory.MakeGoodDefaults();
            HouseholdKey key = new HouseholdKey( "hh5");
            var persons = new List<ModularHouseholdPerson>();
            var p = new Person("blub", 1, 1, 1, 1,
                PermittedGender.Male, string.Empty, string.Empty, Guid.NewGuid().ToString());
            var tt = new TraitTag("traittag", "", TraitLimitType.NoLimit, TraitPriority.All, Guid.NewGuid().ToString());
            var mhp = new ModularHouseholdPerson(null, -1, p.PrettyName, "", p, tt, Guid.NewGuid().ToString());
            persons.Add(mhp);
            var hhVacations = new List<VacationTimeframe>();
            Random r = new Random(1);
            NormalRandom nr = new NormalRandom(0,1,r);
            //DeviceCategoryPicker picker = new DeviceCategoryPicker(r,null);
            //var parameters = new CalcFactoryParameters(picker);
            //CalcFactoryParameters.SetSkipChecking(true);
            //var cloc = new CalcLocation("cloc", 1, Guid.NewGuid().ToString());
            //var mock = new Mock<ILogFile>();
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
            VacationDtoFactory vfac = new VacationDtoFactory(calcParameters,r);
            CalcPersonDtoFactory cpf = new CalcPersonDtoFactory(parameters, r,nr,vfac);
            var hhtDesires =
                new List<ModularHousehold.PersonTraitDesireEntry>();
            var d = new Desire("desire", 1, 1, 1, string.Empty, 1, false, 1, "", Guid.NewGuid().ToString());
            var hhtDesire = new HHTDesire(1, 1, 1, d, HealthStatus.Healthy, 1, 1, string.Empty, "name", 1, 100,
                PermittedGender.All, Guid.NewGuid().ToString());
            var hht = new HouseholdTrait("blub", null, "", "", "", 1, 1, 1, 1, 1, TimeType.Day, 1, 1, TimeType.Day, 1,
                1, EstimateType.FromCalculations,
                "", Guid.NewGuid().ToString());
            hhtDesires.Add(
                new ModularHousehold.PersonTraitDesireEntry(
                    ModularHouseholdTrait.ModularHouseholdTraitAssignType.Age, null, hhtDesire, hht));
            //var sharedDesireValues = new Dictionary<Desire, SharedDesireValue>();
            var cpersons = cpf.MakePersonDtos(persons, key, hhVacations, hhtDesires, "hhname");
            Assert.AreEqual(1, cpersons.Count);
            var cp = cpersons[0];
            //CalcPersonFactory.AddTraitDesires(hhtDesires, cpersons, 1, "name", sharedDesireValues);
            Assert.AreEqual(1, cp.Desires.Count);
            Assert.AreEqual(1, cp.Desires.Count);
            Assert.AreEqual(p.PrettyName, cp.Name);
            Assert.AreEqual(p.Age, cp.Age);
            // id 1 for the dictionary
            Assert.AreEqual(d.PrettyName, cp.Desires[0].Name);
        }
    }
}
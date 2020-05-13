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
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using NUnit.Framework;

namespace Database.Tests.Tables.ModularHouseholds {
    [TestFixture]
    public class ModularHouseholdTraitTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void LoadFromDatabaseTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var persons = db.LoadPersons();

                db.ClearTable(HouseholdTrait.TableName);
                var affordances = db.LoadAffordances(out var timeBasedProfiles, out _,
                    out var deviceCategories, out var realDevices, out var desires, out var loadTypes, out var timeLimits, out ObservableCollection<DeviceAction> deviceActions,
                    out ObservableCollection<DeviceActionGroup> deviceActionGroups, out var locations, out var variables, out _);
                var tags = db.LoadTraitTags();
                var householdTraits = db.LoadHouseholdTraits(locations, affordances,
                    realDevices, deviceCategories, timeBasedProfiles, loadTypes, timeLimits, desires, deviceActions,
                    deviceActionGroups, tags, variables);
                var hht = new HouseholdTrait("bla", null, "blub", db.ConnectionString, "none", 1, 100, 10, 1, 1,
                    TimeType.Day, 1, 1, TimeType.Day, 1, 0, EstimateType.Theoretical, "", Guid.NewGuid().ToStrGuid());
                hht.SaveToDB();
                householdTraits.Add(hht);
                var chht = new ModularHouseholdTrait(null, null, "hallo", db.ConnectionString,
                    householdTraits[0], null, ModularHouseholdTrait.ModularHouseholdTraitAssignType.Age, Guid.NewGuid().ToStrGuid());
                chht.DeleteFromDB();
                chht.SaveToDB();
                var chht1 = new ModularHouseholdTrait(null, null, "hallo2", db.ConnectionString,
                    householdTraits[0], persons[0], ModularHouseholdTrait.ModularHouseholdTraitAssignType.Name, Guid.NewGuid().ToStrGuid());
                chht1.SaveToDB();
                chht1.DeleteFromDB();
                db.Cleanup();
            }
        }
    }
}
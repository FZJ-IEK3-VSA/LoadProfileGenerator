﻿//-----------------------------------------------------------------------

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

#region

using System;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


#endregion

namespace Database.Tests.Tables {

    public class SettlementHHTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void LoadFromDatabaseTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                ObservableCollection<TraitTag> traitTags = db.LoadTraitTags();
                var affordances = db.LoadAffordances(out var timeprofiles, out _, out _, out var realDevices, out _, out var loadtypes, out _, out _, out _, out var locations, out _, out _);
                var affordanceTaggingSets = db.LoadAffordanceTaggingSets(affordances, loadtypes);
                db.LoadTransportation(locations, out var transportationDeviceSets,
                    out var travelRouteSets, out var _,
                    out var _, loadtypes,
                    out var chargingStationSets, affordanceTaggingSets);
                db.LoadHouseholdsAndHouses(out var modularHouseholds,
                    out var houses, out _, traitTags, chargingStationSets, travelRouteSets, transportationDeviceSets);
                var settlementHhs = new ObservableCollection<SettlementHH>();
                SettlementHH.LoadFromDatabase(settlementHhs, db.ConnectionString, modularHouseholds, houses,
                    false);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SaveToDatabaseTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                ObservableCollection<TraitTag> traitTags = db.LoadTraitTags();
                var affordances = db.LoadAffordances(out var timeprofiles, out _, out var deviceCategories,
                    out var devices, out _, out var loadtypes, out var timeLimits, out var deviceActions,
                    out var deviceActionGroups, out var locations, out var variables, out var dateBasedProfiles);
                var affordanceTaggingSets = db.LoadAffordanceTaggingSets(affordances, loadtypes);
                db.LoadTransportation(locations, out var transportationDeviceSets,
                    out var travelRouteSets, out var _,
                    out var _, loadtypes,
                    out var chargingStationSets, affordanceTaggingSets);
                db.LoadHouseholdsAndHouses(out var modularHouseholds,
                    out var houses, out _, traitTags, chargingStationSets,
                    travelRouteSets, transportationDeviceSets);
                var settlementHhs = new ObservableCollection<SettlementHH>();
                SettlementHH.LoadFromDatabase(settlementHhs, db.ConnectionString, modularHouseholds, houses,
                    false);
                db.ClearTable(SettlementHH.TableName);
                settlementHhs.Clear();
                // new one
                var hh = new SettlementHH(null, modularHouseholds[0], 1, 1, db.ConnectionString, "hhname",
                    Guid.NewGuid().ToStrGuid());
                hh.SaveToDB();
                // test if it's there
                SettlementHH.LoadFromDatabase(settlementHhs, db.ConnectionString, modularHouseholds, houses,
                    false);
                (settlementHhs.Count).Should().Be(1);
                db.Cleanup();
            }
        }

        public SettlementHHTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
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

using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Database.Tests.Tables {

    public class AffordanceTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void LoadFromDatabaseTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                var profiles = db.LoadTimeBasedProfiles();
                var realDevices = db.LoadRealDevices(out var deviceCategories, out var loadTypes,
                    profiles);
                var timeBasedProfiles = db.LoadTimeBasedProfiles();
                var desires = db.LoadDesires();

                var affordances = new ObservableCollection<Affordance>();
                var subaffordances = new ObservableCollection<SubAffordance>();
                var dateBasedProfiles = db.LoadDateBasedProfiles();

                var timeLimits = db.LoadTimeLimits(dateBasedProfiles);
                var deviceActionGroups = db.LoadDeviceActionGroups();

                var deviceActions = db.LoadDeviceActions(timeBasedProfiles, realDevices,
                    loadTypes, deviceActionGroups);
                var locations = db.LoadLocations(realDevices, deviceCategories, loadTypes);
                var variables = db.LoadVariables();
                SubAffordance.LoadFromDatabase(subaffordances, db.ConnectionString, desires, false, locations, variables);
                Affordance.LoadFromDatabase(affordances, db.ConnectionString, timeBasedProfiles, deviceCategories,
                    realDevices, desires, subaffordances, loadTypes, timeLimits, deviceActions, deviceActionGroups,
                    locations, false, variables);

                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalcAverageEnergyTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                foreach (var aff in sim.Affordances.Items)
                {
                    aff.CalculateAverageEnergyUse(sim.DeviceActions.Items);
                }

                db.Cleanup();
            }
        }

        public AffordanceTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
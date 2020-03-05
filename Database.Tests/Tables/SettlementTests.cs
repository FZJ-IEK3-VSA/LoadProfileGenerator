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

#region

using System;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Enums;
using Common.Tests;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using NUnit.Framework;

#endregion

namespace Database.Tests.Tables {
    [TestFixture]
    public class SettlementTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void LoadFromDatabaseTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            ObservableCollection<TraitTag> traitTags = db.LoadTraitTags();
            var timeprofiles = db.LoadTimeBasedProfiles();
            var realDevices = db.LoadRealDevices(out var deviceCategories, out var loadtypes, timeprofiles);
            var locations = db.LoadLocations(realDevices, deviceCategories, loadtypes);
            db.LoadTransportation(locations, out var transportationDeviceSets,
                out var travelRouteSets, out var _,
                out var _, loadtypes,
                out var chargingStationSets);
            db.LoadHouseholdsAndHouses(out var modularHouseholds,
                out var houses, out var timeLimits,traitTags,chargingStationSets,
                travelRouteSets,transportationDeviceSets);
            var settlements = new ObservableCollection<Settlement>();
            var geoloc = db.LoadGeographicLocations(out _, timeLimits);
            var temperaturProfiles = db.LoadTemperatureProfiles();
            Settlement.LoadFromDatabase(settlements, db.ConnectionString, temperaturProfiles, geoloc,
                modularHouseholds, houses, false);
            db.Cleanup();
        }

        [Test]
        [Category("BasicTest")]
        public void JsonCalcSpecTest()
        {
            SkipEndCleaning = true;
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Simulator sim = new Simulator(db.ConnectionString);
            Settlement sett = sim.Settlements[1];

            sett.WriteJsonCalculationSpecs(wd.WorkingDirectory, @"V:\Dropbox\LPGReleases\releases8.6.0\simulationengine.exe");
            db.Cleanup();
        }


        [Test]
        [Category("BasicTest")]
        public void SaveToDatabaseTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            ObservableCollection<TraitTag> traitTags = db.LoadTraitTags();
            var timeprofiles = db.LoadTimeBasedProfiles();
            var realDevices = db.LoadRealDevices(out var deviceCategories, out var loadtypes, timeprofiles);
            var locations = db.LoadLocations(realDevices, deviceCategories, loadtypes);
            db.LoadTransportation(locations, out var transportationDeviceSets,
                out var travelRouteSets, out var _,
                out var _, loadtypes,
                out var chargingStationSets);
            db.LoadHouseholdsAndHouses(out var modularHouseholds,
                out var houses, out var timeLimits,traitTags,
                chargingStationSets,travelRouteSets,transportationDeviceSets);
            var settlements = new ObservableCollection<Settlement>();
            var geoloc = db.LoadGeographicLocations(out _, timeLimits);
            var temperaturProfiles = db.LoadTemperatureProfiles();
            Settlement.LoadFromDatabase(settlements, db.ConnectionString, temperaturProfiles, geoloc,
                modularHouseholds, houses, false);
            settlements.Clear();
            db.ClearTable(Settlement.TableName);
            db.ClearTable(SettlementHH.TableName);
            JsonCalcSpecification jcs = JsonCalcSpecification.MakeDefaultsForTesting();
            jcs.EnergyIntensityType = EnergyIntensityType.EnergySaving;
            var se = new Settlement("blub", null,  "blub",
                 "fdasdf", "blub", "blub", "asdf", db.ConnectionString, geoloc[0], temperaturProfiles[0],  "Testing",
                CreationType.ManuallyCreated, Guid.NewGuid().ToString(),jcs);
            se.SaveToDB();

            se.AddHousehold(modularHouseholds[0], 10);
            Settlement.LoadFromDatabase(settlements, db.ConnectionString, temperaturProfiles, geoloc,
                modularHouseholds, houses, false);
            Assert.AreEqual(1, settlements.Count);
            Assert.AreEqual(1, settlements[0].Households.Count);
            Assert.AreEqual(settlements[0].GeographicLocation, geoloc[0]);
            Assert.AreEqual(settlements[0].TemperatureProfile, temperaturProfiles[0]);
            Assert.AreEqual(settlements[0].EnergyIntensityType, EnergyIntensityType.EnergySaving);
            db.Cleanup();
        }
    }
}
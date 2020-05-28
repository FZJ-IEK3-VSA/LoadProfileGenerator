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
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


#endregion

namespace Database.Tests.Tables {

    public class HouseTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest1)]
        public void HouseLoadCreationAndSave2Test()
        {
            Config.ShowDeleteMessages = false;
            Logger.Threshold = Severity.Error;
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(House.TableName);
                db.ClearTable(HouseTypeDevice.TableName);
                db.ClearTable(HouseHousehold.TableName);
                var houses = new CategoryDBBase<House>("blub");
                houses.CreateNewItem(db.ConnectionString);
                var houses1 = new ObservableCollection<House>();
                var devices = new ObservableCollection<RealDevice>();
                var deviceCategories = new ObservableCollection<DeviceCategory>();
                var timeBasedProfiles = new ObservableCollection<TimeBasedProfile>();
                var timeLimits = new ObservableCollection<TimeLimit>();
                var persons = new ObservableCollection<Person>();
                var temperaturProfiles = new ObservableCollection<TemperatureProfile>();
                var loadTypes = db.LoadLoadTypes();
                var variables = db.LoadVariables();
                var geoLocs = db.LoadGeographicLocations(out _, timeLimits);
                var energyStorages = db.LoadEnergyStorages(loadTypes, variables);
                var trafoDevices = db.LoadTransformationDevices(loadTypes,
                    variables);

                var dateprofiles = db.LoadDateBasedProfiles();
                var generators = db.LoadGenerators(loadTypes, dateprofiles);
                var locations = db.LoadLocations(devices, deviceCategories, loadTypes);
                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions = db.LoadDeviceActions(timeBasedProfiles, devices,
                    loadTypes, deviceActionGroups);
                var houseTypes = db.LoadHouseTypes(devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, locations, deviceActions,
                    deviceActionGroups, variables);
                var householdTraits = new ObservableCollection<HouseholdTrait>();
                var deviceSelections = new ObservableCollection<DeviceSelection>();
                var vacations = db.LoadVacations();
                var householdTags = db.LoadHouseholdTags();
                var traitTags = db.LoadTraitTags();
                var modularHouseholds = db.LoadModularHouseholds(householdTraits,
                    deviceSelections, persons, vacations, householdTags, traitTags);
                db.LoadTransportation(locations, out var transportationDeviceSets, out var travelRouteSets,
                    out var _,
                    out var _, loadTypes, out var chargingStationSets);
                House.LoadFromDatabase(houses1, db.ConnectionString,
                    temperaturProfiles, geoLocs, houseTypes,
                    modularHouseholds, chargingStationSets, transportationDeviceSets, travelRouteSets, false);
                (houses1.Count).Should().Be(1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest1)]
        public void HouseLoadCreationAndSaveTest()
        {
            Config.ShowDeleteMessages = false;
            Logger.Threshold = Severity.Error;
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(House.TableName);
                db.ClearTable(HouseTypeDevice.TableName);
                db.ClearTable(HouseHousehold.TableName);
                var loadTypes = db.LoadLoadTypes();
                var houses = new ObservableCollection<House>();
                var devices = new ObservableCollection<RealDevice>();
                var deviceCategories = new ObservableCollection<DeviceCategory>();
                var timeBasedProfiles = new ObservableCollection<TimeBasedProfile>();
                var timeLimits = new ObservableCollection<TimeLimit>();
                var persons = new ObservableCollection<Person>();
                var temperaturProfiles = new ObservableCollection<TemperatureProfile>();
                var geoLocs = db.LoadGeographicLocations(out _, timeLimits);
                var variables = db.LoadVariables();
                var energyStorages = db.LoadEnergyStorages(loadTypes, variables);
                var trafoDevices = db.LoadTransformationDevices(loadTypes,
                    variables);

                var dateprofiles = db.LoadDateBasedProfiles();
                var locations = db.LoadLocations(devices, deviceCategories, loadTypes);
                var generators = db.LoadGenerators(loadTypes, dateprofiles);
                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions = db.LoadDeviceActions(timeBasedProfiles, devices,
                    loadTypes, deviceActionGroups);
                var houseTypes = db.LoadHouseTypes(devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, locations, deviceActions,
                    deviceActionGroups, variables);
                var householdTraits = new ObservableCollection<HouseholdTrait>();
                var deviceSelections = new ObservableCollection<DeviceSelection>();
                var vacations = db.LoadVacations();
                var householdTags = db.LoadHouseholdTags();
                var traitTags = db.LoadTraitTags();
                var modularHouseholds = db.LoadModularHouseholds(householdTraits,
                    deviceSelections, persons, vacations, householdTags, traitTags);
                db.LoadTransportation(locations, out var transportationDeviceSets, out var travelRouteSets,
                    out var _,
                    out var _, loadTypes, out var chargingStationSets);
                House.LoadFromDatabase(houses, db.ConnectionString, temperaturProfiles, geoLocs, houseTypes,
                    modularHouseholds, chargingStationSets, transportationDeviceSets, travelRouteSets, false);
                (houses.Count).Should().Be(0);
                var house = new House("haus1", "blub", null, null, null, db.ConnectionString, EnergyIntensityType.Random,
                    "Testing", CreationType.ManuallyCreated, Guid.NewGuid().ToStrGuid());
                house.SaveToDB();
                House.LoadFromDatabase(houses, db.ConnectionString, temperaturProfiles, geoLocs, houseTypes,
                    modularHouseholds, chargingStationSets, transportationDeviceSets, travelRouteSets, false);
                (houses.Count).Should().Be(1);
                // ModularHousehold hh = new ModularHousehold("bla", null, "blub", db.ConnectionString, null, "test",null,null, EnergyIntensityType.Random,CreationType.TemplateCreated);
                //h/h.SaveToDB();
                //households.Add(hh);
                var house2 = houses[0];
                (house2.Households.Count).Should().Be(0);

                house.AddHousehold(modularHouseholds[0],  null, null, null);
                house.AddHousehold(modularHouseholds[0],  null, null, null);
                house.SaveToDB();
                houses.Clear();
                House.LoadFromDatabase(houses, db.ConnectionString, temperaturProfiles, geoLocs, houseTypes,
                    modularHouseholds, chargingStationSets, transportationDeviceSets, travelRouteSets, false);
                (houses.Count).Should().Be(1);
                var house3 = houses[0];
                (house3.Households.Count).Should().Be(2);
                house3.DeleteHouseholdFromDB(house3.Households[0]);
                houses.Clear();

                House.LoadFromDatabase(houses, db.ConnectionString, temperaturProfiles, geoLocs, houseTypes,
                    modularHouseholds, chargingStationSets, transportationDeviceSets, travelRouteSets, false);
                (houses.Count).Should().Be(1);
                var house4 = houses[0];
                (house4.Households.Count).Should().Be(1);
                db.Cleanup();
            }
        }

        public HouseTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
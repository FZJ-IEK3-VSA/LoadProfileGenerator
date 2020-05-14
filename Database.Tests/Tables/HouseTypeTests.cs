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
using Common.Enums;
using Common.Tests;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Houses;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Database.Tests.Tables {
    [TestFixture]
    public class HouseTypeTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseDeviceLoadCreationAndSaveTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(HouseType.TableName);
                db.ClearTable(HouseTypeDevice.TableName);
                db.ClearTable(HouseTypeEnergyStorage.TableName);
                db.ClearTable(HouseTypeGenerator.TableName);
                var loadTypes = db.LoadLoadTypes();
                var houseTypes = new ObservableCollection<HouseType>();
                var devices = new ObservableCollection<RealDevice>();
                var deviceCategories = new ObservableCollection<DeviceCategory>();
                var timeBasedProfiles = new ObservableCollection<TimeBasedProfile>();
                var timeLimits = new ObservableCollection<TimeLimit>();
                var variables = db.LoadVariables();
                var energyStorages = db.LoadEnergyStorages(loadTypes, variables);
                var trafoDevices = db.LoadTransformationDevices(loadTypes,
                    variables);

                var dateprofiles = db.LoadDateBasedProfiles();
                var generators = db.LoadGenerators(loadTypes, dateprofiles);
                var locations = db.LoadLocations(devices, deviceCategories, loadTypes);
                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions = db.LoadDeviceActions(timeBasedProfiles,
                    devices, loadTypes, deviceActionGroups);
                Assert.AreEqual(0, houseTypes.Count);
                var housetype = new HouseType("haus1", "blub", 1000, 5, 10, loadTypes[0], db.ConnectionString, 1, 1,
                    loadTypes[1], false, 0, false, 0, 1, 100, Guid.NewGuid().ToStrGuid());
                housetype.SaveToDB();
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations,
                    deviceActions, deviceActionGroups, variables);
                Assert.AreEqual(1, houseTypes.Count);
                var houseType2 = houseTypes[0];
                Assert.AreEqual(0, houseType2.HouseDevices.Count);
                var rd = new RealDevice("test", 1, "bla", null, "name", true, false, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                rd.SaveToDB();
                devices.Add(rd);
                var rd2 = new RealDevice("test2", 1, "bla", null, "name", true, false, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                rd2.SaveToDB();
                devices.Add(rd2);

                var dt = new TimeLimit("blub", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                timeLimits.Add(dt);
                dt.SaveToDB();
                var tp = new TimeBasedProfile("blub", null, db.ConnectionString, TimeProfileType.Relative,
                    "fake", Guid.NewGuid().ToStrGuid());
                timeBasedProfiles.Add(tp);
                tp.SaveToDB();

                houseType2.AddHouseTypeDevice(rd, dt, tp, 1, loadTypes[0], locations[0], 0, VariableCondition.Equal,
                    variables[0]);
                houseType2.AddHouseTypeDevice(rd, dt, tp, 2, loadTypes[0], locations[0], 0, VariableCondition.Equal,
                    variables[0]);
                houseType2.AddHouseTypeDevice(rd2, dt, tp, 2, loadTypes[0], locations[0], 0, VariableCondition.Equal,
                    variables[0]);
                houseTypes.Clear();
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(1, houseTypes.Count);
                var houseType3 = houseTypes[0];
                Assert.AreEqual(3, houseType3.HouseDevices.Count);
                houseType3.DeleteDeviceFromDB(rd);
                houseTypes.Clear();
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(1, houseTypes.Count);
                var houseType4 = houseTypes[0];
                Assert.AreEqual(1, houseType4.HouseDevices.Count);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseDeviceOrphanCreatingTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(HouseType.TableName);
                db.ClearTable(HouseTypeDevice.TableName);
                var loadTypes = db.LoadLoadTypes();
                var devices = new ObservableCollection<RealDevice>();
                var deviceCategories = new ObservableCollection<DeviceCategory>();
                var timeBasedProfiles = new ObservableCollection<TimeBasedProfile>();
                var timeLimits = new ObservableCollection<TimeLimit>();
                var variables = db.LoadVariables();
                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions = db.LoadDeviceActions(timeBasedProfiles,
                    devices, loadTypes, deviceActionGroups);
                var energyStorages = db.LoadEnergyStorages(loadTypes, variables);
                var trafoDevices = db.LoadTransformationDevices(loadTypes,
                    variables);

                var dateprofiles = db.LoadDateBasedProfiles();
                var generators = db.LoadGenerators(loadTypes, dateprofiles);
                var houseTypes = new ObservableCollection<HouseType>();
                var locations = db.LoadLocations(devices, deviceCategories, loadTypes);
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(0, houseTypes.Count);
                var housetype = new HouseType("haus1", "blub", 1000, 5, 10, loadTypes[0], db.ConnectionString, 1, 1,
                    loadTypes[1], false, 0, false, 0, 1, 100, Guid.NewGuid().ToStrGuid());
                housetype.SaveToDB();
                var rd = new RealDevice("test", 1, "bla", null, "name", true, false, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                rd.SaveToDB();
                devices.Add(rd);

                var dt = new TimeLimit("blub", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                timeLimits.Add(dt);
                dt.SaveToDB();
                var tp = new TimeBasedProfile("blub", null, db.ConnectionString, TimeProfileType.Relative,
                    "fake", Guid.NewGuid().ToStrGuid());
                timeBasedProfiles.Add(tp);
                tp.SaveToDB();
                housetype.AddHouseTypeDevice(rd, dt, tp, 1, loadTypes[0], locations[0], 0, VariableCondition.Equal,
                    variables[0]);
                houseTypes.Clear();
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(1, houseTypes.Count);
                var house3 = houseTypes[0];
                Assert.AreEqual(1, house3.HouseDevices.Count);
                db.ClearTable(HouseType.TableName);
                houseTypes.Clear();
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(0, houseTypes.Count);

                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseEnergyStorageLoadCreationAndSaveTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(HouseType.TableName);
                db.ClearTable(HouseTypeDevice.TableName);
                db.ClearTable(HouseTypeTransformationDevice.TableName);
                var loadTypes = db.LoadLoadTypes();
                var devices = new ObservableCollection<RealDevice>();
                var deviceCategories = new ObservableCollection<DeviceCategory>();
                var timeBasedProfiles = new ObservableCollection<TimeBasedProfile>();
                var timeLimits = new ObservableCollection<TimeLimit>();
                var variables = db.LoadVariables();
                var energyStorages = db.LoadEnergyStorages(loadTypes, variables);
                var trafoDevices = db.LoadTransformationDevices(loadTypes,
                    variables);
                var dateprofiles = db.LoadDateBasedProfiles();
                var generators = db.LoadGenerators(loadTypes, dateprofiles);
                var houseTypes = new ObservableCollection<HouseType>();
                var locations = db.LoadLocations(devices, deviceCategories, loadTypes);

                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions = db.LoadDeviceActions(timeBasedProfiles,
                    devices, loadTypes, deviceActionGroups);
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(0, houseTypes.Count);
                var housetype = new HouseType("haus1", "blub", 1000, 5, 10, loadTypes[0], db.ConnectionString, 1, 1,
                    loadTypes[1], false, 0, false, 0, 1, 100, Guid.NewGuid().ToStrGuid());
                housetype.SaveToDB();

                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups,
                    variables);
                Assert.AreEqual(1, houseTypes.Count);
                var house2 = houseTypes[0];
                Assert.AreEqual(0, house2.HouseTransformationDevices.Count);
                var es = energyStorages[0];
                house2.AddEnergyStorage(es);
                houseTypes.Clear();
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(1, houseTypes.Count);
                var house3 = houseTypes[0];
                Assert.AreEqual(1, house3.HouseEnergyStorages.Count);
                house3.DeleteHouseEnergyStorage(house3.HouseEnergyStorages[0]);
                houseTypes.Clear();
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(1, houseTypes.Count);
                var house4 = houseTypes[0];
                Assert.AreEqual(0, house4.HouseEnergyStorages.Count);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseLoadCreationAndSaveTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Logger.Threshold = Severity.Warning;
                db.ClearTable(HouseType.TableName);
                db.ClearTable(HouseTypeDevice.TableName);
                var devices = new ObservableCollection<RealDevice>();
                var deviceCategories = new ObservableCollection<DeviceCategory>();
                var timeBasedProfiles = new ObservableCollection<TimeBasedProfile>();
                var timeLimits = new ObservableCollection<TimeLimit>();
                var variables = db.LoadVariables();
                var loadTypes = db.LoadLoadTypes();
                var energyStorages = db.LoadEnergyStorages(loadTypes, variables);
                var trafoDevices = db.LoadTransformationDevices(loadTypes,
                    variables);
                var dateprofiles = db.LoadDateBasedProfiles();
                var generators = db.LoadGenerators(loadTypes, dateprofiles);

                var locations = db.LoadLocations(devices, deviceCategories, loadTypes);
                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions = db.LoadDeviceActions(timeBasedProfiles,
                    devices, loadTypes, deviceActionGroups);
                var dt = new TimeLimit("blub", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                dt.SaveToDB();
                timeLimits.Add(dt);
                var rd = new RealDevice("blub", 1, string.Empty, null, string.Empty, true, false, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                rd.SaveToDB();
                devices.Add(rd);
                var tbp = new TimeBasedProfile("blub", null, db.ConnectionString, TimeProfileType.Relative,
                    "fake", Guid.NewGuid().ToStrGuid());
                tbp.SaveToDB();
                timeBasedProfiles.Add(tbp);
                var tempP = new TemperatureProfile("blub", null, string.Empty, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                tempP.SaveToDB();
                var houseTypes = new ObservableCollection<HouseType>();
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(0, houseTypes.Count);
                var housetype = new HouseType("haus1", "blub", 1000, 5, 10, loadTypes[0], db.ConnectionString, 1, 1,
                    loadTypes[1], false, 0, false, 0, 1, 100, Guid.NewGuid().ToStrGuid());
                housetype.SaveToDB();
                housetype.AddHouseTypeDevice(rd, dt, tbp, 1, loadTypes[0], locations[0], 0, VariableCondition.Equal,
                    variables[0]);
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(1, houseTypes.Count);
                Assert.AreEqual(1, houseTypes[0].HouseDevices.Count);
                var house2 = houseTypes[0];
                Assert.AreEqual("haus1", house2.Name);
                Assert.AreEqual("blub", house2.Description);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseTransformationDeviceLoadCreationAndSaveTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(HouseType.TableName);
                db.ClearTable(HouseTypeDevice.TableName);
                db.ClearTable(HouseTypeTransformationDevice.TableName);
                var loadTypes = db.LoadLoadTypes();
                var devices = new ObservableCollection<RealDevice>();
                var deviceCategories = new ObservableCollection<DeviceCategory>();
                var timeBasedProfiles = new ObservableCollection<TimeBasedProfile>();
                var timeLimits = new ObservableCollection<TimeLimit>();
                var variables = db.LoadVariables();
                var energyStorages = db.LoadEnergyStorages(loadTypes, variables);
                var trafoDevices = db.LoadTransformationDevices(loadTypes,
                    variables);

                var dateprofiles = db.LoadDateBasedProfiles();
                var generators = db.LoadGenerators(loadTypes, dateprofiles);
                var houseTypes = new ObservableCollection<HouseType>();
                var locations = db.LoadLocations(devices, deviceCategories, loadTypes);
                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions = db.LoadDeviceActions(timeBasedProfiles,
                    devices, loadTypes, deviceActionGroups);
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(0, houseTypes.Count);
                var housetype = new HouseType("haus1", "blub", 1000, 5, 10, loadTypes[0], db.ConnectionString, 1, 1,
                    loadTypes[1], false, 0, false, 0, 1, 100, Guid.NewGuid().ToStrGuid());
                housetype.SaveToDB();
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(1, houseTypes.Count);
                var house2 = houseTypes[0];
                Assert.AreEqual(0, house2.HouseTransformationDevices.Count);
                var td = trafoDevices[0];
                house2.AddTransformationDevice(td);
                houseTypes.Clear();
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(1, houseTypes.Count);
                var house3 = houseTypes[0];
                Assert.AreEqual(1, house3.HouseTransformationDevices.Count);
                house3.DeleteHouseTransformationDeviceFromDB(house3.HouseTransformationDevices[0]);
                houseTypes.Clear();
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.AreEqual(1, houseTypes.Count);
                var house4 = houseTypes[0];
                Assert.AreEqual(0, house4.HouseTransformationDevices.Count);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void HouseTypeLoadCreationAndSave2Test()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(House.TableName);
                db.ClearTable(HouseTypeDevice.TableName);
                db.ClearTable(HouseHousehold.TableName);
                var houses = new CategoryDBBase<House>("blub");
                houses.CreateNewItem(db.ConnectionString);
                var devices = new ObservableCollection<RealDevice>();
                var deviceCategories = new ObservableCollection<DeviceCategory>();
                var timeBasedProfiles = new ObservableCollection<TimeBasedProfile>();
                var timeLimits = new ObservableCollection<TimeLimit>();
                var loadTypes = db.LoadLoadTypes();
                var variables = db.LoadVariables();
                var energyStorages = db.LoadEnergyStorages(loadTypes, variables);
                var trafoDevices = db.LoadTransformationDevices(loadTypes,
                    variables);

                var dateprofiles = db.LoadDateBasedProfiles();
                var generators = db.LoadGenerators(loadTypes, dateprofiles);
                var houseTypes = new ObservableCollection<HouseType>();
                var locations = db.LoadLocations(devices, deviceCategories, loadTypes);
                var deviceActionGroups = db.LoadDeviceActionGroups();
                var deviceActions = db.LoadDeviceActions(timeBasedProfiles,
                    devices, loadTypes, deviceActionGroups);
                HouseType.LoadFromDatabase(houseTypes, db.ConnectionString, devices, deviceCategories, timeBasedProfiles,
                    timeLimits, loadTypes, trafoDevices, energyStorages, generators, false, locations, deviceActions,
                    deviceActionGroups, variables);
                Assert.GreaterOrEqual(houseTypes.Count, 1);
                db.Cleanup();
            }
        }

        public HouseTypeTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
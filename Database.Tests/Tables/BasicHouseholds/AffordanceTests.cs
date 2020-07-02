using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Common.Tests;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.BasicHouseholds {

    public class AffordanceTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void AffordanceRequirementVariableTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Config.IsInUnitTesting = true;
                var col = new ColorRGB(255, 0, 0);

                var aff = new Affordance("bla", null, null, true, PermittedGender.Female, 0.1m, col,
                    "affordanceCategory", null, "desc", db.ConnectionString, true, true, 0, 99, false,
                    ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
                aff.SaveToDB();
                aff.ExecutedVariables.Count.Should().Be(0);
                var va = new Variable("var1", "desc", "unit", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                va.SaveToDB();
                aff.AddVariableRequirement(1, VariableLocationMode.CurrentLocation, null, VariableCondition.Equal, va,
                    string.Empty);
                aff.RequiredVariables.Count.Should().Be(1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void AffordanceStandbyTests1()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Config.IsInUnitTesting = true;
                var col = new ColorRGB(255, 0, 0);
                var aff = new Affordance("bla", null, null, true, PermittedGender.Female, 0.1m, col,
                    "AffordanceCategory", null, "desc", db.ConnectionString, true, true, 0, 99, false,
                    ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
                aff.SaveToDB();
                aff.AffordanceStandbys.Count.Should().Be(0);
                var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                rd2.SaveToDB();
                aff.AddStandby(rd2);
                aff.AffordanceStandbys.Count.Should().Be(1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void AffordanceStandbyTests2()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(Affordance.TableName);
                db.ClearTable(AffordanceStandby.TableName);
                db.ClearTable(AffordanceDevice.TableName);
                db.ClearTable(AffordanceDesire.TableName);
                db.ClearTable(AffordanceSubAffordance.TableName);
                db.ClearTable(AffordanceTaggingEntry.TableName);
                db.ClearTable(HHTAffordance.TableName);
                db.ClearTable(AffVariableRequirement.TableName);

                Config.IsInUnitTesting = true;
                var col = new ColorRGB(255, 0, 0);
                var aff = new Affordance("bla", null, null, true, PermittedGender.Female, 0.1m, col,
                    "AffordanceCategory", null, "desc", db.ConnectionString, true, true, 0, 99, false,
                    ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
                aff.SaveToDB();
                aff.AffordanceStandbys.Count.Should().Be(0);
                var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                rd2.SaveToDB();
                aff.AddStandby(rd2);
                aff.SaveToDB();
                aff.AffordanceStandbys.Count.Should().Be(1);
                var sim2 = new Simulator(db.ConnectionString);
                var loadedAffordance = sim2.Affordances.Items[0];
                loadedAffordance.AffordanceStandbys.Count.Should().Be(1);
                loadedAffordance.AffordanceStandbys[0].Device?.Name.Should().Be("rd2");
                loadedAffordance.DeleteStandby(loadedAffordance.AffordanceStandbys[0]);
                loadedAffordance.AffordanceStandbys.Count.Should().Be(0);
                var sim3 = new Simulator(db.ConnectionString);
                var affordanceLoaded2 = sim3.Affordances.Items[0];
                affordanceLoaded2.AffordanceStandbys.Count.Should().Be(0);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void AffordanceVariableTests1()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Config.IsInUnitTesting = true;
                var col = new ColorRGB(255, 0, 0);
                var aff = new Affordance("bla", null, null, true, PermittedGender.Female, 0.1m, col,
                    "AffordanceCategory", null, "desc", db.ConnectionString, true, true, 0, 99, false,
                    ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
                var v = new Variable("var1", "desc", "1", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                v.SaveToDB();
                aff.SaveToDB();
                aff.ExecutedVariables.Count.Should().Be(0);

                aff.AddVariableOperation(1, VariableLocationMode.CurrentLocation, null, VariableAction.SetTo, v,
                    string.Empty,
                    VariableExecutionTime.Beginning);
                aff.ExecutedVariables.Count.Should().Be(1);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void AffordanceVariableTests2()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(Affordance.TableName);
                db.ClearTable(AffordanceStandby.TableName);
                db.ClearTable(AffordanceDevice.TableName);
                db.ClearTable(AffordanceDesire.TableName);
                db.ClearTable(AffordanceSubAffordance.TableName);
                db.ClearTable(AffordanceTaggingEntry.TableName);
                db.ClearTable(HHTAffordance.TableName);
                db.ClearTable(AffVariableOperation.TableName);
                var v = new Variable("var1", "desc", "unit", db.ConnectionString, Guid.NewGuid().ToStrGuid());
                v.SaveToDB();
                Config.IsInUnitTesting = true;
                var col = new ColorRGB(255, 0, 0);
                var aff = new Affordance("bla", null, null, true, PermittedGender.Female, 0.1m, col,
                    "affordanceCategory", null, "desc", db.ConnectionString, true, true, 0, 99, false,
                    ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
                aff.SaveToDB();
                aff.ExecutedVariables.Count.Should().Be(0);
                aff.AddVariableOperation(0, VariableLocationMode.CurrentLocation, null, VariableAction.SetTo, v,
                    string.Empty,
                    VariableExecutionTime.Beginning);
                aff.SaveToDB();
                aff.ExecutedVariables.Count.Should().Be(1);
                var sim2 = new Simulator(db.ConnectionString);
                var affordanceLoaded = sim2.Affordances.Items[0];
                affordanceLoaded.ExecutedVariables.Count.Should().Be(1);
                affordanceLoaded.ExecutedVariables[0].Name.Should().Be("var1");
                affordanceLoaded.DeleteVariableOperation(affordanceLoaded.ExecutedVariables[0]);
                affordanceLoaded.ExecutedVariables.Count.Should().Be(0);
                var sim3 = new Simulator(db.ConnectionString);
                var affordanceLoaded2 = sim3.Affordances.Items[0];
                affordanceLoaded2.ExecutedVariables.Count.Should().Be(0);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalculateAverageEnergyUseTestDeviceAction()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Config.IsInUnitTesting = true;
                var col = new ColorRGB(255, 0, 0);
                var devices = new ObservableCollection<RealDevice>();
                var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                    db.ConnectionString,
                    LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
                lt.SaveToDB();
                rd2.SaveToDB();
                rd2.AddLoad(lt, 666, 0, 0);

                devices.Add(rd2);

                var deviceCategories = new ObservableCollection<DeviceCategory>();
                var deviceActions = new ObservableCollection<DeviceAction>();
                var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                    string.Empty,
                    db.ConnectionString, true, true, 0,
                    100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(),
                    BodilyActivityLevel.Low);
                aff.SaveToDB();

                var tp = new TimeBasedProfile("tp", null, db.ConnectionString, TimeProfileType.Relative, "fake", Guid.NewGuid().ToStrGuid());
                tp.SaveToDB();
                tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 100, false);
                tp.AddNewTimepoint(new TimeSpan(0, 2, 0), 0, false);
                var dag = new DeviceActionGroup("dag", db.ConnectionString, string.Empty, Guid.NewGuid().ToStrGuid());
                dag.SaveToDB();
                var da = new DeviceAction("da", null, string.Empty, db.ConnectionString, dag, rd2, Guid.NewGuid().ToStrGuid());
                da.SaveToDB();
                da.AddDeviceProfile(tp, 0, lt, 1);
                deviceActions.Add(da);
                var tbp = new TimeBasedProfile("name", 1, db.ConnectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToStrGuid());
                aff.AddDeviceProfile(dag, tbp, 0, devices, deviceCategories, lt, 1);
                var res = aff.CalculateAverageEnergyUse(deviceActions);
                foreach (var keyValuePair in res)
                {
                    Logger.Info(keyValuePair.Key + ": " + keyValuePair.Value);
                }
                res.Count.Should().Be(1);
                var first = res.First();
                first.Value.Should().Be(666 * 2);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalculateAverageEnergyUseTestRealDevice()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Config.IsInUnitTesting = true;
                var col = new ColorRGB(255, 0, 0);
                var devices = new ObservableCollection<RealDevice>();
                var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                    db.ConnectionString,
                    LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
                lt.SaveToDB();
                rd2.SaveToDB();
                rd2.AddLoad(lt, 666, 0, 0);

                devices.Add(rd2);

                var deviceCategories = new ObservableCollection<DeviceCategory>();
                var deviceActions = new ObservableCollection<DeviceAction>();
                var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                    string.Empty,
                    db.ConnectionString, true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(),
                    BodilyActivityLevel.Low);
                aff.SaveToDB();

                var tp = new TimeBasedProfile("tp", null, db.ConnectionString, TimeProfileType.Relative, "fake", Guid.NewGuid().ToStrGuid());
                tp.SaveToDB();
                tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 100, false);
                tp.AddNewTimepoint(new TimeSpan(0, 2, 0), 0, false);
                aff.AddDeviceProfile(rd2, tp, 0, devices, deviceCategories, lt, 1);
                var res = aff.CalculateAverageEnergyUse(deviceActions);
                foreach (var keyValuePair in res)
                {
                    Logger.Info(keyValuePair.Key + ": " + keyValuePair.Value);
                }

                (res.Count).Should().Be(1);
                var first = res.First();
                (first.Value).Should().Be(666 * 2);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalculateMaximumInternalTimeResolutionTestForDeviceAction()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Config.IsInUnitTesting = true;
                var col = new ColorRGB(255, 0, 0);
                var devices = new ObservableCollection<RealDevice>();
                var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                rd2.SaveToDB();
                devices.Add(rd2);

                var deviceCategories = new ObservableCollection<DeviceCategory>();
                var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                    string.Empty,
                    db.ConnectionString, true, true, 0, 100,
                    false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
                aff.SaveToDB();

                var tp = new TimeBasedProfile("tp", null, db.ConnectionString, TimeProfileType.Relative, "fake", Guid.NewGuid().ToStrGuid());
                tp.SaveToDB();
                tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 1, false);
                tp.AddNewTimepoint(new TimeSpan(0, 1, 0), 1, false);
                tp.AddNewTimepoint(new TimeSpan(0, 10, 0), 1, false);
                var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                    db.ConnectionString,
                    LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
                lt.SaveToDB();
                var dag = new DeviceActionGroup("dag", db.ConnectionString, string.Empty, Guid.NewGuid().ToStrGuid());
                dag.SaveToDB();
                var da = new DeviceAction("da", null, string.Empty, db.ConnectionString, dag, rd2, Guid.NewGuid().ToStrGuid());
                da.SaveToDB();
                da.AddDeviceProfile(tp, 0, lt, 1);
                new ObservableCollection<DeviceAction>().Add(da);
                var tbp = new TimeBasedProfile("name", 1, db.ConnectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToStrGuid());
                aff.AddDeviceProfile(da, tbp, 0, devices, deviceCategories, lt, 1);

                var ts = aff.CalculateMaximumInternalTimeResolution();
                (ts.TotalSeconds).Should().Be(60);
                Logger.Info(ts.ToString());
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalculateMaximumInternalTimeResolutionTestForDeviceActionGroup()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Config.IsInUnitTesting = true;
                var col = new ColorRGB(255, 0, 0);
                var devices = new ObservableCollection<RealDevice>();
                var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                rd2.SaveToDB();
                devices.Add(rd2);

                var deviceCategories = new ObservableCollection<DeviceCategory>();
                var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                    string.Empty,
                    db.ConnectionString, true, true, 0, 100, false,
                    ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
                aff.SaveToDB();

                var tp = new TimeBasedProfile("tp", null, db.ConnectionString, TimeProfileType.Relative, "fake", Guid.NewGuid().ToStrGuid());
                tp.SaveToDB();
                tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 1, false);
                tp.AddNewTimepoint(new TimeSpan(0, 1, 0), 1, false);
                tp.AddNewTimepoint(new TimeSpan(0, 10, 0), 1, false);
                var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                    db.ConnectionString,
                    LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
                lt.SaveToDB();
                var dag = new DeviceActionGroup("dag", db.ConnectionString, string.Empty, Guid.NewGuid().ToStrGuid());
                dag.SaveToDB();
                var da = new DeviceAction("da", null, string.Empty, db.ConnectionString, dag, rd2, Guid.NewGuid().ToStrGuid());
                da.SaveToDB();
                da.AddDeviceProfile(tp, 0, lt, 1);
                new ObservableCollection<DeviceAction>().Add(da);
                aff.AddDeviceProfile(dag, null, 0, devices, deviceCategories, lt, 1);

                var ts = aff.CalculateMaximumInternalTimeResolution();
                (ts.TotalSeconds).Should().Be(60);
                Logger.Info(ts.ToString());
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalculateMaximumInternalTimeResolutionTestForRealDevice()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Config.IsInUnitTesting = true;
                var col = new ColorRGB(255, 0, 0);
                var devices = new ObservableCollection<RealDevice>();
                var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                rd2.SaveToDB();
                devices.Add(rd2);

                var deviceCategories = new ObservableCollection<DeviceCategory>();
                //var deviceActions = new ObservableCollection<DeviceAction>();
                var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                    string.Empty,
                    db.ConnectionString, true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(),
                    BodilyActivityLevel.Low);
                aff.SaveToDB();

                var tp = new TimeBasedProfile("tp", null, db.ConnectionString, TimeProfileType.Relative, "fake", Guid.NewGuid().ToStrGuid());
                tp.SaveToDB();
                tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 1, false);
                tp.AddNewTimepoint(new TimeSpan(0, 1, 0), 1, false);
                tp.AddNewTimepoint(new TimeSpan(0, 10, 0), 1, false);

                var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                    db.ConnectionString,
                    LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
                lt.SaveToDB();
                aff.AddDeviceProfile(rd2, tp, 0, devices, deviceCategories, lt, 1);

                var ts = aff.CalculateMaximumInternalTimeResolution();
                (ts.TotalSeconds).Should().Be(60);
                Logger.Info(ts.ToString());
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void IsAffordanceAvailableTestAffordanceHasNoDevice() {
            // Location: realdevice
            // affordance: nothing
            Config.IsInUnitTesting = true;
            var col = new ColorRGB(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(),
                BodilyActivityLevel.Low);
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid());
            // check if the device is not there
            var allDevices2 = new List<IAssignableDevice>
            {
                rd2
            };
            var result = aff.IsAffordanceAvailable(allDevices2, deviceActions);
            (result).Should().BeFalse();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void IsAffordanceAvailableTestCheckDeviceActionGroupInDeviceAction() {
            // Location: device action
            // affordance: device Action Group
            Config.IsInUnitTesting = true;
            var col =  new ColorRGB(255, 0, 0);
            var connectionString = string.Empty;
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid());

            var dg = new DeviceActionGroup("group", string.Empty, string.Empty, Guid.NewGuid().ToStrGuid());
            var da = new DeviceAction("device action 1", null, "blub", string.Empty, dg, rd1, Guid.NewGuid().ToStrGuid());
            deviceActions.Add(da);
            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices,  Guid.NewGuid().ToStrGuid(),null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            (dc1.SubDevices.Count).Should().Be(1);
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToStrGuid());
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
            aff.AffordanceDevices.Add(new AffordanceDevice(dg, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(), new ObservableCollection<DeviceCategory>(),
                "name", lt, string.Empty, 1, Guid.NewGuid().ToStrGuid()));
            var allDevices3 = new List<IAssignableDevice>
            {
                da
            };
            if (da.DeviceActionGroup == null)
            {
                throw new LPGException("Device action group was null");
            }
            var relevantDeviceActionGroup = da.DeviceActionGroup.GetDeviceActions(deviceActions);
            (relevantDeviceActionGroup.Count).Should().Be(1);
            (aff.IsAffordanceAvailable(allDevices3, deviceActions)).Should().BeTrue();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void IsAffordanceAvailableTestCheckDeviceActionGroupInDeviceActionGroup() {
            // Location: device action group
            // affordance: device Action Group
            Config.IsInUnitTesting = true;
            var col = new ColorRGB(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid());

            var dg = new DeviceActionGroup("group", string.Empty, string.Empty, Guid.NewGuid().ToStrGuid());
            var da = new DeviceAction("device action 1", null, "blub", string.Empty, dg, rd1, Guid.NewGuid().ToStrGuid());
            deviceActions.Add(da);
            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices, Guid.NewGuid().ToStrGuid(), null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            (dc1.SubDevices.Count).Should().Be(1);
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToStrGuid());
            aff.AffordanceDevices.Add(new AffordanceDevice(dg, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(),
                new ObservableCollection<DeviceCategory>(), "name", lt, string.Empty, 1, Guid.NewGuid().ToStrGuid()));
            var allDevices3 = new List<IAssignableDevice>
            {
                dg
            };
            if (da.DeviceActionGroup== null)
            {
                throw new LPGException("device action group was null");
            }
            var relevantDeviceActionGroup = da.DeviceActionGroup.GetDeviceActions(deviceActions);
            (relevantDeviceActionGroup.Count).Should().Be(1);
            (aff.IsAffordanceAvailable(allDevices3, deviceActions)).Should().BeTrue();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void IsAffordanceAvailableTestCheckDeviceActionInDeviceAction() {
            // Location: device action
            // affordance: device Action
            Config.IsInUnitTesting = true;
            var col = new ColorRGB(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid());
            var dg = new DeviceActionGroup("group", string.Empty, string.Empty, Guid.NewGuid().ToStrGuid());
            var da = new DeviceAction(string.Empty, null, "blub", string.Empty, dg, rd1, Guid.NewGuid().ToStrGuid());
            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices, Guid.NewGuid().ToStrGuid(), null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            (dc1.SubDevices.Count).Should().Be(1);
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToStrGuid());
            aff.AffordanceDevices.Add(new AffordanceDevice(da, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(), new ObservableCollection<DeviceCategory>(), "name",
                lt, string.Empty, 1, Guid.NewGuid().ToStrGuid()));
            var allDevices3 = new List<IAssignableDevice>
            {
                da
            };
            (aff.IsAffordanceAvailable(allDevices3, deviceActions)).Should().BeTrue();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void IsAffordanceAvailableTestCheckDeviceActionInDeviceDeviceAction() {
            // Location: realDevice
            // affordance: device Action
            Config.IsInUnitTesting = true;
            var col =  new ColorRGB(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid());
            var dg = new DeviceActionGroup("group", string.Empty, string.Empty, Guid.NewGuid().ToStrGuid());
            var da = new DeviceAction(string.Empty, null, "blub", string.Empty, dg, rd1, Guid.NewGuid().ToStrGuid());
            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices, Guid.NewGuid().ToStrGuid(), null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            (dc1.SubDevices.Count).Should().Be(1);
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToStrGuid());
            aff.AffordanceDevices.Add(new AffordanceDevice(da, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(), new ObservableCollection<DeviceCategory>(), "name", lt,
                string.Empty, 1, Guid.NewGuid().ToStrGuid()));
            var allDevices3 = new List<IAssignableDevice>
            {
                rd1
            };
            (aff.IsAffordanceAvailable(allDevices3, deviceActions)).Should().BeTrue();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void IsAffordanceAvailableTestCheckDeviceActionInDeviceDeviceAction2() {
            // Location: other realDevice
            // affordance: device Action
            Config.IsInUnitTesting = true;
            var col = new ColorRGB(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid());
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid());
            var dg = new DeviceActionGroup("group", string.Empty, string.Empty, Guid.NewGuid().ToStrGuid());
            var da = new DeviceAction(string.Empty, null, "blub", string.Empty, dg, rd1, Guid.NewGuid().ToStrGuid());
            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices, Guid.NewGuid().ToStrGuid(), null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            (dc1.SubDevices.Count).Should().Be(1);
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToStrGuid());
            aff.AffordanceDevices.Add(new AffordanceDevice(da, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(), new ObservableCollection<DeviceCategory>(),
                "name", lt, string.Empty, 1, Guid.NewGuid().ToStrGuid()));
            var allDevices3 = new List<IAssignableDevice>
            {
                rd2
            };
            (aff.IsAffordanceAvailable(allDevices3, deviceActions)).Should().BeFalse();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void IsAffordanceAvailableTestCheckDeviceCategoryInDeviceCategory() {
            // Location: device category
            // affordance: device category
            Config.IsInUnitTesting = true;
            var col = new ColorRGB(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid());

            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices, Guid.NewGuid().ToStrGuid(), null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            (dc1.SubDevices.Count).Should().Be(1);
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToStrGuid());
            aff.AffordanceDevices.Add(new AffordanceDevice(dc1, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(),
                new ObservableCollection<DeviceCategory>(), "name", lt, string.Empty, 1, Guid.NewGuid().ToStrGuid()));
            var allDevices3 = new List<IAssignableDevice>
            {
                dc1
            };
            (aff.IsAffordanceAvailable(allDevices3, deviceActions)).Should().BeTrue();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void IsAffordanceAvailableTestCheckRealDeviceInDeviceCategory() {
            // Location: realdevice
            // affordance: device category
            Config.IsInUnitTesting = true;
            var col = new ColorRGB(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid());

            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices, Guid.NewGuid().ToStrGuid(), null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            (dc1.SubDevices.Count).Should().Be(1);
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToStrGuid());

            aff.AffordanceDevices.Add(new AffordanceDevice(dc1, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(), new ObservableCollection<DeviceCategory>(),
                "name", lt, string.Empty, 1, Guid.NewGuid().ToStrGuid()));
            var allDevices3 = new List<IAssignableDevice>
            {
                rd1
            };
            (aff.IsAffordanceAvailable(allDevices3, deviceActions)).Should().BeTrue();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void IsAffordanceAvailableTestRealDeviceIsNotThere() {
            // Location: realdevice
            // affordance: other realdevice
            Config.IsInUnitTesting = true;
            var col = new ColorRGB(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid());
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid());
            // check if the device is not there
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToStrGuid());
            aff.AffordanceDevices.Add(new AffordanceDevice(rd1, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(),
                new ObservableCollection<DeviceCategory>(), "name", lt, string.Empty, 1, Guid.NewGuid().ToStrGuid()));
            var allDevices2 = new List<IAssignableDevice>
            {
                rd2
            };
            var result = aff.IsAffordanceAvailable(allDevices2, deviceActions);
            (result).Should().BeFalse();
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void IsAffordanceAvailableTestRealDeviceIsThere() {
            // Location: realdevice
            // affordance: same realdevice
            Config.IsInUnitTesting = true;
            var col = new ColorRGB(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToStrGuid(), BodilyActivityLevel.Low);
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToStrGuid());
            var allDevices1 = new List<IAssignableDevice>
            {
                // check if only the device is there
                rd1
            };
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToStrGuid());

            aff.AffordanceDevices.Add(new AffordanceDevice(rd1, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(),
                new ObservableCollection<DeviceCategory>(), "name", lt, string.Empty, 1, Guid.NewGuid().ToStrGuid()));
            (aff.IsAffordanceAvailable(allDevices1, deviceActions)).Should().BeTrue();
        }

        public AffordanceTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Automation;
using Automation.ResultFiles;
using Common;
using Common.Enums;
using Common.Tests;
using Database.Helpers;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using NUnit.Framework;

namespace Database.Tests.Tables.BasicHouseholds {
    [TestFixture]
    public class AffordanceTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void AffordanceRequirementVariableTest() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);

            var aff = new Affordance("bla", null, null, true, PermittedGender.Female, 0.1m, col,
                "affordanceCategory", null, "desc", db.ConnectionString, true, true, 0, 99, false,
                ActionAfterInterruption.GoBackToOld,false, Guid.NewGuid().ToString());
            aff.SaveToDB();
            Assert.AreEqual(0, aff.ExecutedVariables.Count);
            var va = new Variable("var1", "desc", "unit", db.ConnectionString, Guid.NewGuid().ToString());
            va.SaveToDB();
            aff.AddVariableRequirement(1, VariableLocationMode.CurrentLocation, null, VariableCondition.Equal, va,
                string.Empty);
            Assert.AreEqual(1, aff.RequiredVariables.Count);
            db.Cleanup();
        }

        [Test]
        [Category("BasicTest")]
        public void AffordanceStandbyTests1() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var aff = new Affordance("bla", null, null, true, PermittedGender.Female, 0.1m, col,
                "AffordanceCategory", null, "desc", db.ConnectionString, true, true, 0, 99, false,
                ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            aff.SaveToDB();
            Assert.AreEqual(0, aff.AffordanceStandbys.Count);
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToString());
            rd2.SaveToDB();
            aff.AddStandby(rd2);
            Assert.AreEqual(1, aff.AffordanceStandbys.Count);
            db.Cleanup();
        }

        [Test]
        [Category("BasicTest")]
        public void AffordanceStandbyTests2() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            db.ClearTable(Affordance.TableName);
            db.ClearTable(AffordanceStandby.TableName);
            db.ClearTable(AffordanceDevice.TableName);
            db.ClearTable(AffordanceDesire.TableName);
            db.ClearTable(AffordanceSubAffordance.TableName);
            db.ClearTable(AffordanceTaggingEntry.TableName);
            db.ClearTable(HHTAffordance.TableName);
            db.ClearTable(AffVariableRequirement.TableName);

            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var aff = new Affordance("bla", null, null, true, PermittedGender.Female, 0.1m, col,
                "AffordanceCategory", null, "desc", db.ConnectionString, true, true, 0, 99, false,
                ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            aff.SaveToDB();
            Assert.AreEqual(0, aff.AffordanceStandbys.Count);
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToString());
            rd2.SaveToDB();
            aff.AddStandby(rd2);
            aff.SaveToDB();
            Assert.AreEqual(1, aff.AffordanceStandbys.Count);
            var sim2 = new Simulator(db.ConnectionString);
            var loadedAffordance = sim2.Affordances.It[0];
            Assert.AreEqual(1, loadedAffordance.AffordanceStandbys.Count);
            Assert.AreEqual("rd2", loadedAffordance.AffordanceStandbys[0].Device?.Name);
            loadedAffordance.DeleteStandby(loadedAffordance.AffordanceStandbys[0]);
            Assert.AreEqual(0, loadedAffordance.AffordanceStandbys.Count);
            var sim3 = new Simulator(db.ConnectionString);
            var affordanceLoaded2 = sim3.Affordances.It[0];
            Assert.AreEqual(0, affordanceLoaded2.AffordanceStandbys.Count);
            db.Cleanup();
        }

        [Test]
        [Category("BasicTest")]
        public void AffordanceVariableTests1() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var aff = new Affordance("bla", null, null, true, PermittedGender.Female, 0.1m, col,
                "AffordanceCategory", null, "desc", db.ConnectionString, true, true, 0, 99, false,
                ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            var v = new Variable("var1", "desc", "1", db.ConnectionString, Guid.NewGuid().ToString());
            v.SaveToDB();
            aff.SaveToDB();
            Assert.AreEqual(0, aff.ExecutedVariables.Count);

            aff.AddVariableOperation(1, VariableLocationMode.CurrentLocation, null, VariableAction.SetTo, v,
                string.Empty,
                VariableExecutionTime.Beginning);
            Assert.AreEqual(1, aff.ExecutedVariables.Count);
            db.Cleanup();
        }

        [Test]
        [Category("BasicTest")]
        public void AffordanceVariableTests2() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            db.ClearTable(Affordance.TableName);
            db.ClearTable(AffordanceStandby.TableName);
            db.ClearTable(AffordanceDevice.TableName);
            db.ClearTable(AffordanceDesire.TableName);
            db.ClearTable(AffordanceSubAffordance.TableName);
            db.ClearTable(AffordanceTaggingEntry.TableName);
            db.ClearTable(HHTAffordance.TableName);
            db.ClearTable(AffVariableOperation.TableName);
            var v = new Variable("var1", "desc", "unit", db.ConnectionString, Guid.NewGuid().ToString());
            v.SaveToDB();
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var aff = new Affordance("bla", null, null, true, PermittedGender.Female, 0.1m, col,
                "affordanceCategory", null, "desc", db.ConnectionString, true, true, 0, 99, false,
                ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            aff.SaveToDB();
            Assert.AreEqual(0, aff.ExecutedVariables.Count);
            aff.AddVariableOperation(0, VariableLocationMode.CurrentLocation, null, VariableAction.SetTo, v,
                string.Empty,
                VariableExecutionTime.Beginning);
            aff.SaveToDB();
            Assert.AreEqual(1, aff.ExecutedVariables.Count);
            var sim2 = new Simulator(db.ConnectionString);
            var affordanceLoaded = sim2.Affordances.It[0];
            Assert.AreEqual(1, affordanceLoaded.ExecutedVariables.Count);
            Assert.AreEqual("var1", affordanceLoaded.ExecutedVariables[0].Name);
            affordanceLoaded.DeleteVariableOperation(affordanceLoaded.ExecutedVariables[0]);
            Assert.AreEqual(0, affordanceLoaded.ExecutedVariables.Count);
            var sim3 = new Simulator(db.ConnectionString);
            var affordanceLoaded2 = sim3.Affordances.It[0];
            Assert.AreEqual(0, affordanceLoaded2.ExecutedVariables.Count);
            db.Cleanup();
        }

        [Test]
        [Category("BasicTest")]
        public void CalculateAverageEnergyUseTestDeviceAction() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var devices = new ObservableCollection<RealDevice>();
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToString());
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                db.ConnectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            lt.SaveToDB();
            rd2.SaveToDB();
            rd2.AddLoad(lt, 666, 0, 0);

            devices.Add(rd2);

            var deviceCategories = new ObservableCollection<DeviceCategory>();
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty,
                db.ConnectionString, true, true, 0,
                100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            aff.SaveToDB();

            var tp = new TimeBasedProfile("tp", null, db.ConnectionString, TimeProfileType.Relative, "fake", Guid.NewGuid().ToString());
            tp.SaveToDB();
            tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 100, false);
            tp.AddNewTimepoint(new TimeSpan(0, 2, 0), 0, false);
            var dag = new DeviceActionGroup("dag", db.ConnectionString, string.Empty,Guid.NewGuid().ToString());
            dag.SaveToDB();
            var da = new DeviceAction("da", null, string.Empty, db.ConnectionString, dag, rd2, Guid.NewGuid().ToString());
            da.SaveToDB();
            da.AddDeviceProfile(tp, 0, lt, 1);
            deviceActions.Add(da);
            var tbp = new TimeBasedProfile("name", 1, db.ConnectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToString());
            aff.AddDeviceProfile(dag, tbp, 0, devices, deviceCategories, lt, 1);
            var res = aff.CalculateAverageEnergyUse(deviceActions);
            foreach (var keyValuePair in res) {
                Logger.Info(keyValuePair.Key + ": " + keyValuePair.Value);
            }
            Assert.AreEqual(1, res.Count);
            var first = res.First();
            Assert.AreEqual(666 * 2, first.Value);
            db.Cleanup();
        }

        [Test]
        [Category("BasicTest")]
        public void CalculateAverageEnergyUseTestRealDevice() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var devices = new ObservableCollection<RealDevice>();
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToString());
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                db.ConnectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            lt.SaveToDB();
            rd2.SaveToDB();
            rd2.AddLoad(lt, 666, 0, 0);

            devices.Add(rd2);

            var deviceCategories = new ObservableCollection<DeviceCategory>();
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty,
                db.ConnectionString, true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            aff.SaveToDB();

            var tp = new TimeBasedProfile("tp", null, db.ConnectionString, TimeProfileType.Relative, "fake", Guid.NewGuid().ToString());
            tp.SaveToDB();
            tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 100, false);
            tp.AddNewTimepoint(new TimeSpan(0, 2, 0), 0, false);
            aff.AddDeviceProfile(rd2, tp, 0, devices, deviceCategories, lt, 1);
            var res = aff.CalculateAverageEnergyUse(deviceActions);
            foreach (var keyValuePair in res) {
                Logger.Info(keyValuePair.Key + ": " + keyValuePair.Value);
            }
            Assert.AreEqual(1, res.Count);
            var first = res.First();
            Assert.AreEqual(666 * 2, first.Value);
            db.Cleanup();
        }

        [Test]
        [Category("BasicTest")]
        public void CalculateMaximumInternalTimeResolutionTestForDeviceAction() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var devices = new ObservableCollection<RealDevice>();
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToString());
            rd2.SaveToDB();
            devices.Add(rd2);

            var deviceCategories = new ObservableCollection<DeviceCategory>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty,
                db.ConnectionString, true, true, 0, 100,
                false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            aff.SaveToDB();

            var tp = new TimeBasedProfile("tp", null, db.ConnectionString, TimeProfileType.Relative, "fake", Guid.NewGuid().ToString());
            tp.SaveToDB();
            tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 1, false);
            tp.AddNewTimepoint(new TimeSpan(0, 1, 0), 1, false);
            tp.AddNewTimepoint(new TimeSpan(0, 10, 0), 1, false);
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                db.ConnectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            lt.SaveToDB();
            var dag = new DeviceActionGroup("dag", db.ConnectionString, string.Empty, Guid.NewGuid().ToString());
            dag.SaveToDB();
            var da = new DeviceAction("da", null, string.Empty, db.ConnectionString, dag, rd2, Guid.NewGuid().ToString());
            da.SaveToDB();
            da.AddDeviceProfile(tp, 0, lt, 1);
            new ObservableCollection<DeviceAction>().Add(da);
            var tbp = new TimeBasedProfile("name", 1, db.ConnectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToString());
            aff.AddDeviceProfile(da, tbp, 0, devices, deviceCategories, lt, 1);

            var ts = aff.CalculateMaximumInternalTimeResolution();
            Assert.AreEqual(60, ts.TotalSeconds);
            Console.WriteLine(ts);
            db.Cleanup();
        }

        [Test]
        [Category("BasicTest")]
        public void CalculateMaximumInternalTimeResolutionTestForDeviceActionGroup() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var devices = new ObservableCollection<RealDevice>();
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToString());
            rd2.SaveToDB();
            devices.Add(rd2);

            var deviceCategories = new ObservableCollection<DeviceCategory>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty,
                db.ConnectionString, true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            aff.SaveToDB();

            var tp = new TimeBasedProfile("tp", null, db.ConnectionString, TimeProfileType.Relative, "fake", Guid.NewGuid().ToString());
            tp.SaveToDB();
            tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 1, false);
            tp.AddNewTimepoint(new TimeSpan(0, 1, 0), 1, false);
            tp.AddNewTimepoint(new TimeSpan(0, 10, 0), 1, false);
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                db.ConnectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            lt.SaveToDB();
            var dag = new DeviceActionGroup("dag", db.ConnectionString, string.Empty, Guid.NewGuid().ToString());
            dag.SaveToDB();
            var da = new DeviceAction("da", null, string.Empty, db.ConnectionString, dag, rd2, Guid.NewGuid().ToString());
            da.SaveToDB();
            da.AddDeviceProfile(tp, 0, lt, 1);
            new ObservableCollection<DeviceAction>().Add(da);
            aff.AddDeviceProfile(dag, null, 0, devices, deviceCategories, lt, 1);

            var ts = aff.CalculateMaximumInternalTimeResolution();
            Assert.AreEqual(60, ts.TotalSeconds);
            Console.WriteLine(ts);
            db.Cleanup();
        }

        [Test]
        [Category("BasicTest")]
        public void CalculateMaximumInternalTimeResolutionTestForRealDevice() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var devices = new ObservableCollection<RealDevice>();
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, db.ConnectionString, Guid.NewGuid().ToString());
            rd2.SaveToDB();
            devices.Add(rd2);

            var deviceCategories = new ObservableCollection<DeviceCategory>();
            //var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty,
                db.ConnectionString, true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            aff.SaveToDB();

            var tp = new TimeBasedProfile("tp", null, db.ConnectionString, TimeProfileType.Relative, "fake", Guid.NewGuid().ToString());
            tp.SaveToDB();
            tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 1, false);
            tp.AddNewTimepoint(new TimeSpan(0, 1, 0), 1, false);
            tp.AddNewTimepoint(new TimeSpan(0, 10, 0), 1, false);

            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                db.ConnectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            lt.SaveToDB();
            aff.AddDeviceProfile(rd2, tp, 0, devices, deviceCategories, lt, 1);

            var ts = aff.CalculateMaximumInternalTimeResolution();
            Assert.AreEqual(60, ts.TotalSeconds);
            Console.WriteLine(ts);
            db.Cleanup();
        }

        [Test]
        [Category("BasicTest")]
        public void IsAffordanceAvailableTestAffordanceHasNoDevice() {
            // Location: realdevice
            // affordance: nothing
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToString());
            // check if the device is not there
            var allDevices2 = new List<IAssignableDevice>
            {
                rd2
            };
            var result = aff.IsAffordanceAvailable(allDevices2, deviceActions);
            Assert.IsFalse(result);
        }

        [Test]
        [Category("BasicTest")]
        public void IsAffordanceAvailableTestCheckDeviceActionGroupInDeviceAction() {
            // Location: device action
            // affordance: device Action Group
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var connectionString = string.Empty;
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToString());

            var dg = new DeviceActionGroup("group", string.Empty, string.Empty, Guid.NewGuid().ToString());
            var da = new DeviceAction("device action 1", null, "blub", string.Empty, dg, rd1, Guid.NewGuid().ToString());
            deviceActions.Add(da);
            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices,  Guid.NewGuid().ToString(),null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            Assert.AreEqual(1, dc1.SubDevices.Count);
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToString());
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            aff.AffordanceDevices.Add(new AffordanceDevice(dg, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(), new ObservableCollection<DeviceCategory>(),
                "name", lt, string.Empty, 1, Guid.NewGuid().ToString()));
            var allDevices3 = new List<IAssignableDevice>
            {
                da
            };
            if (da.DeviceActionGroup == null)
            {
                throw new LPGException("Device action group was null");
            }
            var relevantDeviceActionGroup = da.DeviceActionGroup.GetDeviceActions(deviceActions);
            Assert.AreEqual(1, relevantDeviceActionGroup.Count);
            Assert.IsTrue(aff.IsAffordanceAvailable(allDevices3, deviceActions));
        }

        [Test]
        [Category("BasicTest")]
        public void IsAffordanceAvailableTestCheckDeviceActionGroupInDeviceActionGroup() {
            // Location: device action group
            // affordance: device Action Group
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToString());

            var dg = new DeviceActionGroup("group", string.Empty, string.Empty, Guid.NewGuid().ToString());
            var da = new DeviceAction("device action 1", null, "blub", string.Empty, dg, rd1, Guid.NewGuid().ToString());
            deviceActions.Add(da);
            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices, Guid.NewGuid().ToString(), null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            Assert.AreEqual(1, dc1.SubDevices.Count);
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToString());
            aff.AffordanceDevices.Add(new AffordanceDevice(dg, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(),
                new ObservableCollection<DeviceCategory>(), "name", lt, string.Empty, 1, Guid.NewGuid().ToString()));
            var allDevices3 = new List<IAssignableDevice>
            {
                dg
            };
            if (da.DeviceActionGroup== null)
            {
                throw new LPGException("device action group was null");
            }
            var relevantDeviceActionGroup = da.DeviceActionGroup.GetDeviceActions(deviceActions);
            Assert.AreEqual(1, relevantDeviceActionGroup.Count);
            Assert.IsTrue(aff.IsAffordanceAvailable(allDevices3, deviceActions));
        }

        [Test]
        [Category("BasicTest")]
        public void IsAffordanceAvailableTestCheckDeviceActionInDeviceAction() {
            // Location: device action
            // affordance: device Action
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToString());
            var dg = new DeviceActionGroup("group", string.Empty, string.Empty, Guid.NewGuid().ToString());
            var da = new DeviceAction(string.Empty, null, "blub", string.Empty, dg, rd1, Guid.NewGuid().ToString());
            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices, Guid.NewGuid().ToString(), null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            Assert.AreEqual(1, dc1.SubDevices.Count);
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToString());
            aff.AffordanceDevices.Add(new AffordanceDevice(da, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(), new ObservableCollection<DeviceCategory>(), "name",
                lt, string.Empty, 1, Guid.NewGuid().ToString()));
            var allDevices3 = new List<IAssignableDevice>
            {
                da
            };
            Assert.IsTrue(aff.IsAffordanceAvailable(allDevices3, deviceActions));
        }

        [Test]
        [Category("BasicTest")]
        public void IsAffordanceAvailableTestCheckDeviceActionInDeviceDeviceAction() {
            // Location: realDevice
            // affordance: device Action
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToString());
            var dg = new DeviceActionGroup("group", string.Empty, string.Empty, Guid.NewGuid().ToString());
            var da = new DeviceAction(string.Empty, null, "blub", string.Empty, dg, rd1, Guid.NewGuid().ToString());
            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices, Guid.NewGuid().ToString(), null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            Assert.AreEqual(1, dc1.SubDevices.Count);
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToString());
            aff.AffordanceDevices.Add(new AffordanceDevice(da, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(), new ObservableCollection<DeviceCategory>(), "name", lt,
                string.Empty, 1, Guid.NewGuid().ToString()));
            var allDevices3 = new List<IAssignableDevice>
            {
                rd1
            };
            Assert.IsTrue(aff.IsAffordanceAvailable(allDevices3, deviceActions));
        }

        [Test]
        [Category("BasicTest")]
        public void IsAffordanceAvailableTestCheckDeviceActionInDeviceDeviceAction2() {
            // Location: other realDevice
            // affordance: device Action
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToString());
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToString());
            var dg = new DeviceActionGroup("group", string.Empty, string.Empty, Guid.NewGuid().ToString());
            var da = new DeviceAction(string.Empty, null, "blub", string.Empty, dg, rd1, Guid.NewGuid().ToString());
            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices, Guid.NewGuid().ToString(), null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            Assert.AreEqual(1, dc1.SubDevices.Count);
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToString());
            aff.AffordanceDevices.Add(new AffordanceDevice(da, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(), new ObservableCollection<DeviceCategory>(),
                "name", lt, string.Empty, 1, Guid.NewGuid().ToString()));
            var allDevices3 = new List<IAssignableDevice>
            {
                rd2
            };
            Assert.IsFalse(aff.IsAffordanceAvailable(allDevices3, deviceActions));
        }

        [Test]
        [Category("BasicTest")]
        public void IsAffordanceAvailableTestCheckDeviceCategoryInDeviceCategory() {
            // Location: device category
            // affordance: device category
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToString());

            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices, Guid.NewGuid().ToString(), null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            Assert.AreEqual(1, dc1.SubDevices.Count);
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToString());
            aff.AffordanceDevices.Add(new AffordanceDevice(dc1, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(),
                new ObservableCollection<DeviceCategory>(), "name", lt, string.Empty, 1, Guid.NewGuid().ToString()));
            var allDevices3 = new List<IAssignableDevice>
            {
                dc1
            };
            Assert.IsTrue(aff.IsAffordanceAvailable(allDevices3, deviceActions));
        }

        [Test]
        [Category("BasicTest")]
        public void IsAffordanceAvailableTestCheckRealDeviceInDeviceCategory() {
            // Location: realdevice
            // affordance: device category
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToString());

            var devices = new ObservableCollection<RealDevice>
            {
                rd1
            };
            // check if it works with a device category that has the device
            var dc1 = new DeviceCategory("dc1", 0, string.Empty, false, devices, Guid.NewGuid().ToString(), null, true);
            rd1.DeviceCategory = dc1;
            dc1.RefreshSubDevices();
            Assert.AreEqual(1, dc1.SubDevices.Count);
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToString());

            aff.AffordanceDevices.Add(new AffordanceDevice(dc1, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(), new ObservableCollection<DeviceCategory>(),
                "name", lt, string.Empty, 1, Guid.NewGuid().ToString()));
            var allDevices3 = new List<IAssignableDevice>
            {
                rd1
            };
            Assert.IsTrue(aff.IsAffordanceAvailable(allDevices3, deviceActions));
        }

        [Test]
        [Category("BasicTest")]
        public void IsAffordanceAvailableTestRealDeviceIsNotThere() {
            // Location: realdevice
            // affordance: other realdevice
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToString());
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToString());
            // check if the device is not there
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToString());
            aff.AffordanceDevices.Add(new AffordanceDevice(rd1, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(),
                new ObservableCollection<DeviceCategory>(), "name", lt, string.Empty, 1, Guid.NewGuid().ToString()));
            var allDevices2 = new List<IAssignableDevice>
            {
                rd2
            };
            var result = aff.IsAffordanceAvailable(allDevices2, deviceActions);
            Assert.IsFalse(result);
        }

        [Test]
        [Category("BasicTest")]
        public void IsAffordanceAvailableTestRealDeviceIsThere() {
            // Location: realdevice
            // affordance: same realdevice
            Config.IsInUnitTesting = true;
            var col = Color.FromRgb(255, 0, 0);
            var deviceActions = new ObservableCollection<DeviceAction>();
            var aff = new Affordance("bla", null, null, false, PermittedGender.All, 1, col, string.Empty, null,
                string.Empty, string.Empty,
                true, true, 0, 100, false, ActionAfterInterruption.GoBackToOld, false, Guid.NewGuid().ToString());
            var rd1 = new RealDevice("rd1", 1, string.Empty, null, string.Empty, false, false, string.Empty, Guid.NewGuid().ToString());
            var allDevices1 = new List<IAssignableDevice>
            {
                // check if only the device is there
                rd1
            };
            var connectionString = string.Empty;
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1, connectionString,
                LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            var tbp = new TimeBasedProfile("name", 1, connectionString, TimeProfileType.Absolute, "data source", Guid.NewGuid().ToString());

            aff.AffordanceDevices.Add(new AffordanceDevice(rd1, tbp, null, 0, null,
                new ObservableCollection<RealDevice>(),
                new ObservableCollection<DeviceCategory>(), "name", lt, string.Empty, 1, Guid.NewGuid().ToString()));
            Assert.IsTrue(aff.IsAffordanceAvailable(allDevices1, deviceActions));
        }
    }
}
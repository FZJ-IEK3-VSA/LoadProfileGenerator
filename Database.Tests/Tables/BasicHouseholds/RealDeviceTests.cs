using System;
using System.Collections.ObjectModel;
using System.Linq;
using Automation;
using Common;
using Common.Tests;
using Database;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tests;
using NUnit.Framework;

namespace DatabaseIO.Tables.BasicHouseholds.Tests
{
    [TestFixture()]
    public class RealDeviceTests : UnitTestBaseClass
    {
        [Test()]
        [Category(UnitTestCategories.BasicTest)]
        public void ImportFromOtherDeviceTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            Simulator sim = new Simulator(db.ConnectionString);
            var newDevice = sim.RealDevices.CreateNewItem(sim.ConnectionString);
            newDevice.ImportFromOtherDevice(sim.RealDevices.It[0]);
            // ReSharper disable once AssignNullToNotNullAttribute
            newDevice.ImportFromOtherDevice(null);
            newDevice.SaveToDB();
            db.Cleanup();
        }
    }
}

namespace DatabaseIO.Tests.Tables.BasicHouseholds {
    [TestFixture]
    public class RealDeviceTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CalculateAverageEnergyUseTestAbsoluteProfile() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            Config.IsInUnitTesting = true;
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false,
                db.ConnectionString, Guid.NewGuid().ToString());
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                db.ConnectionString, LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            lt.SaveToDB();
            rd2.SaveToDB();
            rd2.AddLoad(lt, 666, 0, 0);
            var tp = new TimeBasedProfile("tp", null, db.ConnectionString,
                TimeProfileType.Absolute, "fake", Guid.NewGuid().ToString());
            tp.SaveToDB();
            tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 100, false);
            tp.AddNewTimepoint(new TimeSpan(0, 2, 0), 0, false);

            var allActions = new ObservableCollection<DeviceAction>();

            var res = rd2.CalculateAverageEnergyUse(lt, allActions, tp, 1, 1);
            foreach (var keyValuePair in res) {
                Logger.Info(keyValuePair.Item1 + ": " + keyValuePair.Item2);
            }
            Assert.AreEqual(1, res.Count);
            var first = res.First();
            Assert.AreEqual(200, first.Item2);
            db.Cleanup();
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CalculateAverageEnergyUseTestRelativeProfile() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);

            Config.IsInUnitTesting = true;
            var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false,
                db.ConnectionString, Guid.NewGuid().ToString());
            var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                db.ConnectionString, LoadTypePriority.Mandatory, true, Guid.NewGuid().ToString());
            lt.SaveToDB();
            rd2.SaveToDB();
            rd2.AddLoad(lt, 666, 0, 0);
            var tp = new TimeBasedProfile("tp", null, db.ConnectionString,
                TimeProfileType.Relative, "fake", Guid.NewGuid().ToString());
            tp.SaveToDB();
            tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 100, false);
            tp.AddNewTimepoint(new TimeSpan(0, 2, 0), 0, false);

            var allActions = new ObservableCollection<DeviceAction>();

            var res = rd2.CalculateAverageEnergyUse(lt, allActions, tp, 1, 1);
            foreach (var keyValuePair in res) {
                Logger.Info(keyValuePair.Item1 + ": " + keyValuePair.Item2);
            }
            Assert.AreEqual(1, res.Count);
            var first = res.First();
            Assert.AreEqual(666 * 2, first.Item2);
            // multiplier test
            var res2 = rd2.CalculateAverageEnergyUse(lt, allActions, tp, 0.5, 1);
            foreach (var keyValuePair in res2) {
                Logger.Info(keyValuePair.Item1 + ": " + keyValuePair.Item2);
            }
            Assert.AreEqual(1, res2.Count);
            var first2 = res2.First();
            Assert.AreEqual(666 * 2 * 0.5, first2.Item2);
            // probabilityTest
            var res3 = rd2.CalculateAverageEnergyUse(lt, allActions, tp, 1, 0.5);
            foreach (var keyValuePair in res3) {
                Logger.Info(keyValuePair.Item1 + ": " + keyValuePair.Item2);
            }
            Assert.AreEqual(1, res3.Count);
            var first3 = res3.First();
            Assert.AreEqual(666 * 2 * 0.5, first3.Item2);
            db.Cleanup();
        }
    }
}
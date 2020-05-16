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
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace DatabaseIO.Tables.BasicHouseholds.Tests
{

    public class RealDeviceTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void ImportFromOtherDeviceTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                var newDevice = sim.RealDevices.CreateNewItem(sim.ConnectionString);
                newDevice.ImportFromOtherDevice(sim.RealDevices.It[0]);
                // ReSharper disable once AssignNullToNotNullAttribute
                newDevice.ImportFromOtherDevice(null);
                newDevice.SaveToDB();
                db.Cleanup();
            }
        }

        public RealDeviceTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}

namespace DatabaseIO.Tests.Tables.BasicHouseholds {

    public class RealDeviceTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalculateAverageEnergyUseTestAbsoluteProfile()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Config.IsInUnitTesting = true;
                var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false,
                    db.ConnectionString, Guid.NewGuid().ToStrGuid());
                var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                    db.ConnectionString, LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
                lt.SaveToDB();
                rd2.SaveToDB();
                rd2.AddLoad(lt, 666, 0, 0);
                var tp = new TimeBasedProfile("tp", null, db.ConnectionString,
                    TimeProfileType.Absolute, "fake", Guid.NewGuid().ToStrGuid());
                tp.SaveToDB();
                tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 100, false);
                tp.AddNewTimepoint(new TimeSpan(0, 2, 0), 0, false);

                var allActions = new ObservableCollection<DeviceAction>();

                var res = rd2.CalculateAverageEnergyUse(lt, allActions, tp, 1, 1);
                foreach (var keyValuePair in res)
                {
                    Logger.Info(keyValuePair.Item1 + ": " + keyValuePair.Item2);
                }
                (res.Count).Should().Be(1);
                var first = res.First();
                (first.Item2).Should().Be(200);
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CalculateAverageEnergyUseTestRelativeProfile()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Config.IsInUnitTesting = true;
                var rd2 = new RealDevice("rd2", 1, string.Empty, null, string.Empty, false, false,
                    db.ConnectionString, Guid.NewGuid().ToStrGuid());
                var lt = new VLoadType("lt", string.Empty, "bla", "blub", 1, 1, new TimeSpan(0, 1, 0), 1,
                    db.ConnectionString, LoadTypePriority.Mandatory, true, Guid.NewGuid().ToStrGuid());
                lt.SaveToDB();
                rd2.SaveToDB();
                rd2.AddLoad(lt, 666, 0, 0);
                var tp = new TimeBasedProfile("tp", null, db.ConnectionString,
                    TimeProfileType.Relative, "fake", Guid.NewGuid().ToStrGuid());
                tp.SaveToDB();
                tp.AddNewTimepoint(new TimeSpan(0, 0, 0), 100, false);
                tp.AddNewTimepoint(new TimeSpan(0, 2, 0), 0, false);

                var allActions = new ObservableCollection<DeviceAction>();

                var res = rd2.CalculateAverageEnergyUse(lt, allActions, tp, 1, 1);
                foreach (var keyValuePair in res)
                {
                    Logger.Info(keyValuePair.Item1 + ": " + keyValuePair.Item2);
                }
                (res.Count).Should().Be(1);
                var first = res.First();
                (first.Item2).Should().Be(666 * 2);
                // multiplier test
                var res2 = rd2.CalculateAverageEnergyUse(lt, allActions, tp, 0.5, 1);
                foreach (var keyValuePair in res2)
                {
                    Logger.Info(keyValuePair.Item1 + ": " + keyValuePair.Item2);
                }
                (res2.Count).Should().Be(1);
                var first2 = res2.First();
                (first2.Item2).Should().Be(666 * 2 * 0.5);
                // probabilityTest
                var res3 = rd2.CalculateAverageEnergyUse(lt, allActions, tp, 1, 0.5);
                foreach (var keyValuePair in res3)
                {
                    Logger.Info(keyValuePair.Item1 + ": " + keyValuePair.Item2);
                }
                (res3.Count).Should().Be(1);
                var first3 = res3.First();
                (first3.Item2).Should().Be(666 * 2 * 0.5);
                db.Cleanup();
            }
        }

        public RealDeviceTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
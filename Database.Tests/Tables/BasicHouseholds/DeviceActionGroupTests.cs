using System;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Database.Tests.Tables.BasicHouseholds
{
    [TestFixture]
    public class DeviceActionGroupTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void DeviceActionGroupTestDeleteFromSim()
        {
            Config.IsInUnitTesting = true;
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Logger.Threshold = Severity.Error;
                Simulator sim = new Simulator(db.ConnectionString);
                sim.DeviceActionGroups.It[0].DeleteFromDB();
                db.Cleanup();
            }
        }

        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void DeviceActionGroupTestsAll()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                ObservableCollection<DeviceActionGroup> dags = new ObservableCollection<DeviceActionGroup>();

                DeviceActionGroup.LoadFromDatabase(dags, db.ConnectionString, false);
                db.ClearTable(DeviceActionGroup.TableName);
                DeviceActionGroup dag = new DeviceActionGroup("bla", db.ConnectionString, "desc", Guid.NewGuid().ToStrGuid());
                dag.SaveToDB();
                dags.Clear();
                DeviceActionGroup.LoadFromDatabase(dags, db.ConnectionString, false);
                Assert.AreEqual(1, dags.Count);
                dags[0].DeleteFromDB();
                dags.Clear();
                DeviceActionGroup.LoadFromDatabase(dags, db.ConnectionString, false);
                Assert.AreEqual(0, dags.Count);
                db.Cleanup();
            }
        }

        public DeviceActionGroupTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
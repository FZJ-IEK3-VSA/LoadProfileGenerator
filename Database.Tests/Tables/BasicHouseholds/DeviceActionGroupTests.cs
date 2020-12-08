using System;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.BasicHouseholds
{

    public class DeviceActionGroupTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void DeviceActionGroupTestDeleteFromSim()
        {
            Config.IsInUnitTesting = true;
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Logger.Threshold = Severity.Error;
                Simulator sim = new Simulator(db.ConnectionString);
                sim.DeviceActionGroups.Items[0].DeleteFromDB();
                db.Cleanup();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
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
                (dags.Count).Should().Be(1);
                dags[0].DeleteFromDB();
                dags.Clear();
                DeviceActionGroup.LoadFromDatabase(dags, db.ConnectionString, false);
                (dags.Count).Should().Be(0);
                db.Cleanup();
            }
        }

        public DeviceActionGroupTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
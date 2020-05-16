using System;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.BasicHouseholds
{

    public class DeviceActionTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void DeviceActionTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                ObservableCollection<TimeBasedProfile> profiles = db.LoadTimeBasedProfiles();
                ObservableCollection<RealDevice> devices = db.LoadRealDevices(out _, out var loadTypes, profiles);
                ObservableCollection<DeviceAction> das = new ObservableCollection<DeviceAction>();
                DeviceActionGroup dag = new DeviceActionGroup("blub", db.ConnectionString, "desc", Guid.NewGuid().ToStrGuid());
                dag.SaveToDB();
                ObservableCollection<DeviceActionGroup> dags = db.LoadDeviceActionGroups();
                // try loading
                DeviceAction.LoadFromDatabase(das, db.ConnectionString, profiles, devices, loadTypes, dags, false);
                // clear db
                db.ClearTable(DeviceActionGroup.TableName);
                db.ClearTable(DeviceAction.TableName);
                db.ClearTable(DeviceActionProfile.TableName);
                // create new one, save, load
                DeviceAction da = new DeviceAction("bla", null, "desc", db.ConnectionString, dag, devices[0], Guid.NewGuid().ToStrGuid());
                da.SaveToDB();
                da.AddDeviceProfile(profiles[0], 1, loadTypes[0], 1);
                das.Clear();
                DeviceAction.LoadFromDatabase(das, db.ConnectionString, profiles, devices, loadTypes, dags, false);
                (das.Count).Should().Be(1);
                (das[0].Profiles.Count).Should().Be(1);
                das[0].DeleteProfileFromDB(das[0].Profiles[0]);
                (das[0].Profiles.Count).Should().Be(0);
                das[0].DeleteFromDB();
                das.Clear();
                DeviceAction.LoadFromDatabase(das, db.ConnectionString, profiles, devices, loadTypes, dags, false);
                (das.Count).Should().Be(0);
                db.Cleanup();
            }
        }

        public DeviceActionTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
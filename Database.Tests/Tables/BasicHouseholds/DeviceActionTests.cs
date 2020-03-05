using System;
using System.Collections.ObjectModel;
using Common;
using Common.Tests;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using NUnit.Framework;

namespace Database.Tests.Tables.BasicHouseholds
{
    [TestFixture]
    public class DeviceActionTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void DeviceActionTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            ObservableCollection<TimeBasedProfile> profiles = db.LoadTimeBasedProfiles();
            ObservableCollection<RealDevice> devices = db.LoadRealDevices(out _, out var loadTypes, profiles);
            ObservableCollection<DeviceAction> das = new ObservableCollection<DeviceAction>();
            DeviceActionGroup dag = new DeviceActionGroup("blub", db.ConnectionString, "desc", Guid.NewGuid().ToString());
            dag.SaveToDB();
            ObservableCollection<DeviceActionGroup> dags = db.LoadDeviceActionGroups();
            // try loading
            DeviceAction.LoadFromDatabase(das, db.ConnectionString, profiles, devices, loadTypes, dags, false);
            // clear db
            db.ClearTable(DeviceActionGroup.TableName);
            db.ClearTable(DeviceAction.TableName);
            db.ClearTable(DeviceActionProfile.TableName);
            // create new one, save, load
            DeviceAction da = new DeviceAction("bla", null, "desc", db.ConnectionString, dag, devices[0], Guid.NewGuid().ToString());
            da.SaveToDB();
            da.AddDeviceProfile(profiles[0], 1, loadTypes[0], 1);
            das.Clear();
            DeviceAction.LoadFromDatabase(das, db.ConnectionString, profiles, devices, loadTypes, dags, false);
            Assert.AreEqual(1, das.Count);
            Assert.AreEqual(1, das[0].Profiles.Count);
            das[0].DeleteProfileFromDB(das[0].Profiles[0]);
            Assert.AreEqual(0, das[0].Profiles.Count);
            das[0].DeleteFromDB();
            das.Clear();
            DeviceAction.LoadFromDatabase(das, db.ConnectionString, profiles, devices, loadTypes, dags, false);
            Assert.AreEqual(0, das.Count);
            db.Cleanup();
        }
    }
}
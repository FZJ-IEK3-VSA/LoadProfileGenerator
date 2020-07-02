using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Automation;
using CalculationController.Helpers;
using Common;
using Common.Tests;
using Database;
using Database.Helpers;
using Database.Tables.BasicHouseholds;
using Database.Tables.ModularHouseholds;
using Database.Tests;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace CalculationController.Tests.Helpers
{
    public class DeviceCategoryPickerTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void DeviceCategoryPickerTestDeviceCategories()
        {
            Random r = new Random(5);
            DeviceSelection ds = new DeviceSelection("ds", 1, string.Empty, string.Empty, Guid.NewGuid().ToStrGuid());
            DeviceCategoryPicker dcp = new DeviceCategoryPicker(r, ds);
            ObservableCollection<RealDevice> allDevices = new ObservableCollection<RealDevice>();
            DeviceCategory dc = new DeviceCategory("dc", -1, "bla", false, allDevices, Guid.NewGuid().ToStrGuid());
            RealDevice rd = new RealDevice("bla", 0, string.Empty, dc, "desc", false, true, string.Empty,
                Guid.NewGuid().ToStrGuid(),-1);
            allDevices.Add(rd);
            Location loc = new Location("bla", -1, string.Empty, Guid.NewGuid().ToStrGuid());
            List<IAssignableDevice> devices = new List<IAssignableDevice>
            {
                rd
            };
            ObservableCollection<DeviceAction> deviceActions = new ObservableCollection<DeviceAction>();

            // put in an dc with one rd, get the rd 1
            Logger.Info("put in a rd at the Location, get back null since it already exists");
            devices.Clear();
            devices.Add(rd);
            RealDevice result3 = dcp.GetOrPickDevice(dc, loc, EnergyIntensityType.EnergyIntensive, devices,
                deviceActions);
            result3.Should().BeNull();

            Logger.Info("put in a rd, get back rd");
            devices.Clear();
            DeviceCategoryPicker dcp2 = new DeviceCategoryPicker(r, ds);

            RealDevice result4 = dcp2.GetOrPickDevice(dc, loc, EnergyIntensityType.EnergyIntensive, devices,
                deviceActions);
            rd.Should().Be(result4);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void DeviceCategoryPickerTestRealDevices()
        {
            Random r = new Random(5);
            DeviceSelection ds = new DeviceSelection("ds", 1, string.Empty, string.Empty, Guid.NewGuid().ToStrGuid());
            DeviceCategoryPicker dcp = new DeviceCategoryPicker(r, ds);
            ObservableCollection<RealDevice> allDevices = new ObservableCollection<RealDevice>();
            DeviceCategory dc = new DeviceCategory("dc", -1, "bla", false, allDevices, Guid.NewGuid().ToStrGuid());
            RealDevice rd = new RealDevice("bla", 0, string.Empty, dc, "desc", false, true, string.Empty,Guid.NewGuid().ToStrGuid(), -1);
            allDevices.Add(rd);
            Location loc = new Location("bla", -1, string.Empty, Guid.NewGuid().ToStrGuid());
            List<IAssignableDevice> devices = new List<IAssignableDevice>
            {
                rd
            };
            ObservableCollection<DeviceAction> deviceActions = new ObservableCollection<DeviceAction>();
            Logger.Info("put in a rd, get back the same rd");
            RealDevice result = dcp.GetOrPickDevice(rd, loc, EnergyIntensityType.EnergyIntensive, devices, deviceActions);
            rd.Should().Be( result);
            // put in an dc with one rd, get the rd 1
            Logger.Info("put in a rd, get back null, since the rd is already there as hhdevloc");
            RealDevice result2 = dcp.GetOrPickDevice(dc, loc, EnergyIntensityType.EnergyIntensive, devices,
                deviceActions);
            result2.Should().BeNull();
        }


        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TestMultiplePickingShouldAlwaysGiveSameResult()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Simulator sim = new Simulator(db.ConnectionString);
                for (int i = 0; i < 100; i++)
                {
                    Random r = new Random(5);
                    DeviceCategoryPicker dcp = new DeviceCategoryPicker(r, null);
                    Location loc = new Location("bla", -1, string.Empty, Guid.NewGuid().ToStrGuid());
                    List<IAssignableDevice> devices = new List<IAssignableDevice>();
                    devices.Add(sim.RealDevices[0]);
                    dcp.GetOrPickDevice(sim.DeviceActionGroups[1], loc, EnergyIntensityType.Random, devices, sim.DeviceActions.Items);
                }
            }
            //ObservableCollection<RealDevice> allDevices = new ObservableCollection<RealDevice>();
            //DeviceCategory dc = new DeviceCategory("dc", -1, "bla", false, allDevices, Guid.NewGuid().ToStrGuid());
            //RealDevice rd = new RealDevice("bla", 0, string.Empty, dc, "desc", false, true, string.Empty, Guid.NewGuid().ToStrGuid(), -1);
            //allDevices.Add(rd);

            //List<IAssignableDevice> devices = new List<IAssignableDevice>
            //{
            //    rd
            //};
            //ObservableCollection<DeviceAction> deviceActions = new ObservableCollection<DeviceAction>();
            //Logger.Info("put in a rd, get back the same rd");
            //RealDevice result = dcp.GetOrPickDevice(rd, loc, EnergyIntensityType.EnergyIntensive, devices, deviceActions);
            //(result).Should().Be(rd);
            //// put in an dc with one rd, get the rd 1
            //Logger.Info("put in a rd, get back null, since the rd is already there as hhdevloc");
            //RealDevice result2 = dcp.GetOrPickDevice(dc, loc, EnergyIntensityType.EnergyIntensive, devices,
            //    deviceActions);
            //(result2).Should().Be(null);
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void DeviceCategoryPickerDeviceActionGroupAutoDev()
        {
            Random r = new Random(2);
            DeviceSelection ds = new DeviceSelection("ds", 1, string.Empty, string.Empty, Guid.NewGuid().ToStrGuid());
            DeviceCategoryPicker picker = new DeviceCategoryPicker(r, ds);
            // device stuff
            ObservableCollection<RealDevice> allDevices = new ObservableCollection<RealDevice>();
            DeviceCategory dc = new DeviceCategory("dc", -1, "bla", false, allDevices, Guid.NewGuid().ToStrGuid());
            RealDevice rd1 = new RealDevice("device1", 0, string.Empty, dc, "desc", false, true, string.Empty, Guid.NewGuid().ToStrGuid(), - 1);
            RealDevice rd2 = new RealDevice("device2", 0, string.Empty, dc, "desc", false, true, string.Empty, Guid.NewGuid().ToStrGuid(), -1);
            DeviceActionGroup dag = new DeviceActionGroup("Dag1", string.Empty, "blub",Guid.NewGuid().ToStrGuid(), -1);
            DeviceAction da1 = new DeviceAction("da1", -1, "blub", string.Empty, dag, rd1, Guid.NewGuid().ToStrGuid());
            DeviceAction da2 = new DeviceAction("da2", -1, "blub", string.Empty, dag, rd2, Guid.NewGuid().ToStrGuid());
            ObservableCollection<DeviceAction> deviceActions = new ObservableCollection<DeviceAction>
            {
                da1,
                da2
            };
            allDevices.Add(rd1);
            List<IAssignableDevice> otherDevicesAtLocation = new List<IAssignableDevice>
            {
                dag
            };

            DeviceAction pickedDeviceAction = picker.GetAutoDeviceActionFromGroup(dag, otherDevicesAtLocation,
                EnergyIntensityType.Random, deviceActions, 5);
            Logger.Info("Device Action 1 " + pickedDeviceAction);
            for (int i = 0; i < 50; i++)
            {
                DeviceAction deviceAction2 = picker.GetAutoDeviceActionFromGroup(dag, otherDevicesAtLocation,
                    EnergyIntensityType.Random, deviceActions, 5);
                Logger.Info("Device Action  " + i + " " + deviceAction2);
                pickedDeviceAction.Should().Be(deviceAction2);
            }
        }

        public DeviceCategoryPickerTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
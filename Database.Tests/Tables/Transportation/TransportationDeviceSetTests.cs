﻿using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using Database.Tables.Transportation;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.Transportation
{

    public class TransportationDeviceSetTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TransportationDeviceSetTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(TransportationDeviceSet.TableName);
                db.ClearTable(TransportationDevice.TableName);
                db.ClearTable(TransportationDeviceCategory.TableName);
                TransportationDeviceSet tds =
                    new TransportationDeviceSet("transportationdeviceset", null, db.ConnectionString, "desc",
                        System.Guid.NewGuid().ToStrGuid());
                tds.SaveToDB();

                TransportationDeviceCategory tdc = new TransportationDeviceCategory("transportationdevicecategory",
                    null, db.ConnectionString, "desc", true, System.Guid.NewGuid().ToStrGuid());
                tdc.SaveToDB();
                VLoadType vlt = (VLoadType)VLoadType.CreateNewItem(null, db.ConnectionString);
                vlt.SaveToDB();
                TransportationDevice tdev = new TransportationDevice("mydevice", null, db.ConnectionString, "", 1, SpeedUnit.Kmh,
                    tdc, 1000, 10, 100, 100, vlt,
                    System.Guid.NewGuid().ToStrGuid());
                tdev.SaveToDB();
                ObservableCollection<TransportationDevice> transportationDevices = new ObservableCollection<TransportationDevice>
            {
                tdev
            };
                tds.AddDevice(tdev);
                /*ObservableCollection<TransportationDeviceCategory> categories = new ObservableCollection<TransportationDeviceCategory>
                {
                    tdc
                };*/
                ObservableCollection<TransportationDeviceSet> result = new ObservableCollection<TransportationDeviceSet>();
                TransportationDeviceSet.LoadFromDatabase(result, db.ConnectionString, false, transportationDevices);
                db.Cleanup();
                (result.Count).Should().Be(1);
                (result[0].TransportationDeviceSetEntries.Count).Should().Be(1);
            }
        }
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void TransportationDeviceImportTest()
        {
            using (var db1 = new DatabaseSetup(Utili.GetCurrentMethodAndClass() + "1"))
            {
                using (var db2 = new DatabaseSetup(Utili.GetCurrentMethodAndClass() + "2"))
                {
                    db1.ClearTable(TransportationDeviceSet.TableName);
                    var srcSim = new Simulator(db2.ConnectionString);
                    var dstSim = new Simulator(db1.ConnectionString);
                    foreach (var device in srcSim.TransportationDeviceSets.Items)
                    {
                        TransportationDeviceSet.ImportFromItem(device, dstSim);
                    }
                }
            }
        }

        public TransportationDeviceSetTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
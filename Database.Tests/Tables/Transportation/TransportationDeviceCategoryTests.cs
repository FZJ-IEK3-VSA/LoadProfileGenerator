using System.Collections.ObjectModel;
using Common;
using Common.Tests;
using Database.Tables.Transportation;
using NUnit.Framework;

namespace Database.Tests.Tables.Transportation
{
    [TestFixture]
    public class TransportationDeviceCategoryTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void TransportationDeviceCategoryTest()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.DatabaseIo);
            db.ClearTable(TransportationDeviceCategory.TableName);
            TransportationDeviceCategory sl = new TransportationDeviceCategory("name", null, db.ConnectionString, "desc",true, System.Guid.NewGuid().ToString());
            ObservableCollection<TransportationDeviceCategory> slocs = new ObservableCollection<TransportationDeviceCategory>();

            sl.SaveToDB();

            TransportationDeviceCategory.LoadFromDatabase(slocs, db.ConnectionString, false);
            db.Cleanup();
            Assert.AreEqual(1, slocs.Count);
        }
    }
}
using System.Collections.ObjectModel;
using Automation;
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
        [Category(UnitTestCategories.BasicTest)]
        public void TransportationDeviceCategoryTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(TransportationDeviceCategory.TableName);
                TransportationDeviceCategory sl = new TransportationDeviceCategory("name", null, db.ConnectionString, "desc", true, System.Guid.NewGuid().ToStrGuid());
                ObservableCollection<TransportationDeviceCategory> slocs = new ObservableCollection<TransportationDeviceCategory>();

                sl.SaveToDB();

                TransportationDeviceCategory.LoadFromDatabase(slocs, db.ConnectionString, false);
                db.Cleanup();
                Assert.AreEqual(1, slocs.Count);
            }
        }
    }
}
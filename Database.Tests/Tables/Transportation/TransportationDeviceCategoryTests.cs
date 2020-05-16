using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.Transportation;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.Transportation
{

    public class TransportationDeviceCategoryTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
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
                (slocs.Count).Should().Be(1);
            }
        }

        public TransportationDeviceCategoryTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
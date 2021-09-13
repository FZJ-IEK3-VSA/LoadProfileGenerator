using Automation;
using CalculationController.DtoFactories;
using CalculationEngine.HouseholdElements;
using CalculationEngine.Transportation;
using Common.JSON;
using Common.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Calculation.Tests.Transportation
{
    public class DeviceOwnershipMappingTest : UnitTestBaseClass
    {
        public DeviceOwnershipMappingTest([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void RunDoubleActivationTest()
        {
            var dom = new DeviceOwnershipMapping<string, object>();
            var owner1 = "owner1";
            var owner2 = "owner2";
            var device1 = new List<double>(); // arbitrary objects to own
            var device2 = new List<string>();

            Assert.Null(dom.GetDevice(owner1));
            Assert.Null(dom.GetOwner(device1));

            Assert.True(dom.CanUse(owner1, device1));
            Assert.True(dom.TrySetOwnership(owner1, device1));
            Assert.Equal(device1, dom.GetDevice(owner1));
            Assert.Equal(owner1, dom.GetOwner(device1));
            Assert.Null(dom.GetDevice(owner2));
            Assert.Null(dom.GetOwner(device2));

            Assert.True(dom.CanUse(owner1, device1));
            Assert.False(dom.CanUse(owner1, device2)); // cannot own another device simultaneously
            Assert.False(dom.CanUse(owner2, device1)); // device1 is already owned
            Assert.True(dom.CanUse(owner2, device2));

            Assert.True(dom.TrySetOwnership(owner1, device1));
            Assert.False(dom.TrySetOwnership(owner1, device2)); // cannot own another device simultaneously
            Assert.False(dom.TrySetOwnership(owner2, device1)); // device1 is already owned
            Assert.True(dom.TrySetOwnership(owner2, device2));
            Assert.True(device2 == dom.GetDevice(owner2));
            Assert.True(owner2 == dom.GetOwner(device2));
            Assert.True(dom.CanUse(owner2, device2));

            dom.RemoveOwnership(owner1);
            Assert.Null(dom.GetDevice(owner1));
            Assert.Null(dom.GetOwner(device1));
            Assert.True(dom.CanUse(owner1, device1));
            Assert.True(device2 == dom.GetDevice(owner2));
            Assert.True(owner2 == dom.GetOwner(device2));

            dom.RemoveOwnership(owner1); // redundant calls, should do nothing
            dom.RemoveOwnership(device1);

            dom.RemoveOwnership(device2);
            Assert.Null(dom.GetDevice(owner2));
            Assert.Null(dom.GetOwner(device2));
            Assert.True(dom.CanUse(owner1, device1));
            Assert.True(dom.CanUse(owner1, device2));
            Assert.True(dom.CanUse(owner2, device1));
            Assert.True(dom.CanUse(owner2, device2));
        }
    }
}

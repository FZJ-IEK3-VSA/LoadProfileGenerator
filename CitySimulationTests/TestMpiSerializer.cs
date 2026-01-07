using Automation;
using CalculationEngine.CitySimulation;
using CitySimulation;
using CitySimulation.Scenarios;
using Common.JSON;
using Common.Tests;
using FluentAssertions;
using Xunit.Abstractions;

namespace CitySimulationTests
{
    /// <summary>
    /// Tests whether the MpiSerializer used in the City Simulation can serialize and
    /// deserialize relevant objects without changing them.
    /// </summary>
    /// <param name="testOutputHelper"></param>
    public sealed class TestMpiSerializer(ITestOutputHelper testOutputHelper) : UnitTestBaseClass(testOutputHelper)
    {
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CitySimulationTest)]
        public void TestCalcParameters()
        {
            var serializer = new MPIJsonSerializer();

            // use non-default values for all properties in order to notice incorrect property values
            CalcParameters cp = new([CalcOption.MakePDF], new(2020, 1, 1), new(2020, 12, 31),
                new(0, 1, 0), "_", new(0, 10, 0), true, true, 10, 10, ["Electricity"], LoadTypePriority.Mandatory,
                DeviceProfileHeaderMode.OnlyDeviceCategories, true, true, true, "-", true, true);

            CheckSerialization(serializer, cp);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CitySimulationTest)]
        public void TestScenarioPart()
        {
            var serializer = new MPIJsonSerializer();

            // use non-default values for all properties in order to notice incorrect property values
            ScenarioPart p = new([new("myid", "filepath", 99)], [new(new("poi-id"), new("json ref"))], "dbpath", new(), CalcParameters.CreateDefaultParamsForTesting(), new([]), new(new()));

            CheckSerialization(serializer, p);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CitySimulationTest)]
        public void TestMessageContainer()
        {
            var serializer = new MPIJsonSerializer();
            var m = new MessageContainer();
            CheckSerialization(serializer, m);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CitySimulationTest)]
        public void TestRemoteActivityStart()
        {
            var serializer = new MPIJsonSerializer();
            PersonIdentifier person = new("name", new("hhkey"), "house-id", 4);
            var x = new RemoteActivityStart(person, true, "aff", new("poi1"), new("poi2"), 25);
            CheckSerialization(serializer, x);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CitySimulationTest)]
        public void TestRemoteActivityFinished()
        {
            var serializer = new MPIJsonSerializer();
            PersonIdentifier person = new("name", new("hhkey"), "house-id", 4);
            var x = new RemoteActivityFinished(person, new PointOfInterestId("poi-id"), true);
            CheckSerialization(serializer, x);
        }

        /// <summary>
        /// Serializes and deserializes the passed object with the specified serializer,
        /// and checks whether the result is equal to the original object
        /// </summary>
        /// <typeparam name="T">type of the object to serialize</typeparam>
        /// <param name="serializer">the serializer to test</param>
        /// <param name="value">the object to serialize</param>
        private void CheckSerialization<T>(MPI.ISerializer serializer, T value)
        {
            using var ms = new MemoryStream();
            serializer.Serialize(ms, value);
            ms.Position = 0;
            var newValue = serializer.Deserialize<T>(ms);
            newValue.Should().BeEquivalentTo(value);
        }
    }
}

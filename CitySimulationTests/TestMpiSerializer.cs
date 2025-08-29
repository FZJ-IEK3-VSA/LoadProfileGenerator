using Automation;
using Common.Tests;
using Xunit;
using Xunit.Abstractions;
using CitySimulation;
using Common.JSON;
using FluentAssertions;

namespace CitySimulationTests
{
    /// <summary>
    /// Tests whether the MpiSerializer used in the City Simulation can transfer relevant
    /// objects without changing them.
    /// </summary>
    /// <param name="testOutputHelper"></param>
    public sealed class TestMpiSerializer(ITestOutputHelper testOutputHelper) : UnitTestBaseClass(testOutputHelper)
    {
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CitySimulationTest)]
        public void TestCalcParameters()
        {
            var serializer = new MPIJsonSerializer();

            var cp = CalcParameters.CreateDefaultParamsForTesting();
            cp.Options.Add(CalcOption.MakePDF);

            CheckSerialization(serializer, cp);
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

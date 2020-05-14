using System.Collections.Generic;
using Automation;
using NUnit.Framework;
using SimulationEngineLib;
using Xunit;

namespace SimulationEngine.Tests
{
    [TestFixture]
    public class MakeResults1PvTests
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            using SimulationEngineTestPreparer se = new SimulationEngineTestPreparer("MakeResults1PV");
            List<string> arguments = new List<string>
            {
                "--MakeResults1PV",
                @"X:\R1_PV_2kw",
                @"F:\DissCalcsResults\PVResults_2kw"
            };
            MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
            se.Clean();
        }
    }
}
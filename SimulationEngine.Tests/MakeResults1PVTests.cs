using System.Collections.Generic;
using Automation;
using NUnit.Framework;

namespace SimulationEngine.Tests
{
    [TestFixture]
    public class MakeResults1PvTests
    {
        [Test]
        [Category(UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            SimulationEngineTestPreparer se = new SimulationEngineTestPreparer("MakeResults1PV");
            List<string> arguments = new List<string>
            {
                "--MakeResults1PV",
                @"X:\R1_PV_2kw",
                @"F:\DissCalcsResults\PVResults_2kw"
            };
            Program.Main(arguments.ToArray());
            se.Clean();
        }
    }
}
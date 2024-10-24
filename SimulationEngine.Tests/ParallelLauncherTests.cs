﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Automation;
using Common;
using Common.Tests;
using SimulationEngineLib;
using SimulationEngineLib.SettlementCalculation;
using Xunit;
using Xunit.Abstractions;

namespace SimulationEngine.Tests
{
    public class ParallelLauncherTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void FindNumberOfCoresTest() => Logger.Info(ParallelLauncher.FindNumberOfCores().ToString(CultureInfo.InvariantCulture));

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunTest()
        {
            const string path = @"G:\2016.2.1..20.6";
            Directory.SetCurrentDirectory(path);
            List<string> arguments = new List<string>
            {
                "--LaunchParallel",
                "--NumberCores",
                "3",
                "--Batchfile",
                "Start-DissR2NoBridge.cmd",
                "--Archive",
                @"x:\R2_BridgeDaysWithout"
            };
            MainSimEngine.Run(arguments.ToArray(), "simulationengine.exe");
        }

        public ParallelLauncherTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
using System;
using System.IO;
using Automation;
using CalculationController.Integrity;
using Common;
using Common.Enums;
using Common.Tests;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;

namespace Database.Tests {

    public class ModularHouseholdSerializerTests : UnitTestBaseClass
    {
        /// <summary>
        /// exportiert alle haushalte und reimportiert sie einmal
        /// </summary>
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.LongTest2)]
        public void Run()
        {
            Logger.Info("Starting test");
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                using (var wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
                {
                    Directory.SetCurrentDirectory(wd.WorkingDirectory);
                    //wd.SkipCleaning = true;
                    var sim = new Simulator(db.ConnectionString) { MyGeneralConfig = { PerformCleanUpChecks = "true" } };
                    Logger.Info("First hh");
                    for (var i = 0; i < sim.ModularHouseholds.Items.Count && i < 5; i++)
                    {
                        var mhh = sim.ModularHouseholds[i];
                        Logger.Info("exporting and importing " + mhh.Name);
                        var start = DateTime.Now;
                        if (mhh.CreationType != CreationType.ManuallyCreated)
                        {
                            continue;
                        }
                        var filename = Path.Combine(wd.WorkingDirectory, "testexport." + i + ".csv");
                        ModularHouseholdSerializer.ExportAsCSV(mhh, sim, filename);
                        ModularHouseholdSerializer.ImportFromCSV(filename, sim);
                        var import = DateTime.Now;
                        SimIntegrityChecker.Run(sim);
                        var durationTotal = DateTime.Now - start;
                        var durationIntegrityCheck = DateTime.Now - import;
                        Logger.Info("Duration: total " + durationTotal.TotalSeconds + " seconds, integrity check: " +
                                    durationIntegrityCheck.TotalSeconds);
                    }
                    Logger.Info("finished");
                    Directory.SetCurrentDirectory(wd.PreviousCurrentDir);
                    db.Cleanup();
// wd.CleanUp(1);
                }
            }
        }
        /*
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunJuelich() {
            var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(),DatabaseSetup.TestPackage.DatabaseIo);
            var wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            Directory.SetCurrentDirectory(wd.WorkingDirectory);

            var sim = new Simulator(db.ConnectionString);
            sim.MyGeneralConfig.PerformCleanUpChecks = "true";
            const string filename = "e:\\CHR05a_Sc2v1.csv";
            ModularHouseholdSerializer.ImportFromCSV(filename, sim);
            SimIntegrityChecker.Run(sim);

            db.Cleanup();
        }*/
        public ModularHouseholdSerializerTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
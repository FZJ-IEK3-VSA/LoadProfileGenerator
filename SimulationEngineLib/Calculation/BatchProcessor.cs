using Automation;
using JetBrains.Annotations;
using PowerArgs;
using SimulationEngineLib.SettlementCalculation;

namespace SimulationEngineLib.Calculation {
    //[UsedImplicitly]
    //public class TestBatchOptions {
    //    [ArgDescription("Makes a batchfile to calculate all houses")]
    //    [ArgShortcut(null)]
    //    [UsedImplicitly]
    //    public bool Houses { get; set; }

    //    [ArgDescription("Makes a batchfile to calculate all modular households")]
    //    [ArgShortcut(null)]
    //    [UsedImplicitly]
    //    public bool ModularHouseholds { get; set; }

    //    [ArgDescription("Makes a batchfile to calculate all settlements")]
    //    [ArgShortcut(null)]
    //    [UsedImplicitly]
    //    public bool Settlements { get; set; }
    //}

    public class BatchOptions {
        [ArgDescription(
            "Sets the device selection. Useful for example if you want to use a specific kind of light bulbs in all houses.")]
        [ArgShortcut(null)]
        [CanBeNull]
        [UsedImplicitly]
        public string DeviceSelectionName { get; set; }

        [CanBeNull]
        [ArgDescription(
            "Sets the end date for the calculations. If nothing is set the settings from the settlement are used.")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public string EndDate { get; set; }

        [ArgDescription("Sets the Energy Intensity.")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        [ArgDefaultValue(EnergyIntensityType.Random)]
        public EnergyIntensityType EnergyIntensity { get; set; }

        [ArgDescription("Sets the external time resolution.")]
        [ArgShortcut(null)]
        [CanBeNull]
        [UsedImplicitly]
        public string ExternalTimeResolution { get; set; }

        [ArgDescription("Sets the number of parallel processes to start for parallel execution.")]
        [ArgShortcut(null)]
        [CanBeNull]
        [UsedImplicitly]
        public string ParallelCores { get; set; }

        [ArgDescription("Sets the geographic location to be used.")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        [CanBeNull]
        public int? GeographicLocationIndex { get; set; }

        [ArgDescription("Sets the output mode to a certain level, which determines which files will be generated.")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        [ArgDefaultValue(OutputFileDefault.ForSettlementCalculations)]
        public OutputFileDefault OutputFileDefault { get; set; } = OutputFileDefault.ForSettlementCalculations;

        [ArgDescription(
            "Makes a batchfile to calculate all the houses in the settlement individually (this is very useful combined with the parallel launching function)")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        [CanBeNull]
        public int? SettlementIndex { get; set; }

        [ArgDescription(
            "Makes a batchfile to calculate all the houses in the settlement individually (this is very useful combined with the parallel launching function)")]
        [ArgShortcut(null)]
        [CanBeNull]
        [UsedImplicitly]
        public string SettlementName { get; set; }

        [CanBeNull]
        [ArgDescription(
            "Sets the start date for the calculations. If nothing is set the settings from the settlement are used.")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public string StartDate { get; set; }

        [CanBeNull]
        [ArgDescription("Suffix for the batch file name to identify the batch file later.")]
        [ArgShortcut(null)]
        [UsedImplicitly]
        public string Suffix { get; set; }
    }

    internal static class BatchProcessor {

        //private static void MakeBatchfile([JetBrains.Annotations.NotNull] string connectionString, CalcObjectType type) {
        //    Logger.Info("Loading...");
        //    var sim = new Simulator(connectionString);
        //    Logger.Info("Loading finished.");
        //    List<DBBase> items;
        //    string dirname;
        //    switch (type) {
        //        case CalcObjectType.ModularHousehold:
        //            items = new List<DBBase>(
        //                sim.ModularHouseholds.It.Where(x => !x.Name.StartsWith("x ", StringComparison.Ordinal)));
        //            dirname = "CHH";
        //            break;
        //        case CalcObjectType.House:
        //            items = new List<DBBase>(sim.Houses.It);
        //            dirname = "House";
        //            break;
        //        case CalcObjectType.Settlement:
        //            items = new List<DBBase>(sim.Settlements.It);
        //            dirname = "Settlement";
        //            break;
        //        default: throw new LPGException("Unknown type");
        //    }
        //    var batchfilename = "Start-" + type + ".cmd";
        //    using (var sw = new StreamWriter(batchfilename)) {
        //        for (var index = 0; index < items.Count; index++) {
        //            var cmd = "SimulationEngine.exe Calculate -CalcObjectType " + type + " -CalcObjectNumber " +
        //                      index +
        //                      " -OutputFileDefault Reasonable -LoadtypePriority RecommendedForHouses  -SkipExisting " +
        //                      " -OutputDirectory " + dirname + "_PDF_" + index + string.Empty;
        //            sw.WriteLine(cmd);
        //        }
        //    }
        //    Logger.Info("Finished writing to " + batchfilename);
        //}

        public static void RunProcessing([JetBrains.Annotations.NotNull] BatchOptions bo, [JetBrains.Annotations.NotNull] string connectionString) {
            if (bo.SettlementIndex != null) {
                BatchfileFromSettlement.MakeBatchfileFromSettlement(connectionString, bo);
            }
        }

        //public static void RunTestBatching([JetBrains.Annotations.NotNull] TestBatchOptions bo, [JetBrains.Annotations.NotNull] string connectionString) {
        //    if (bo.ModularHouseholds) {
        //        MakeBatchfile(connectionString, CalcObjectType.ModularHousehold);
        //        return;
        //    }

        //    if (bo.Houses) {
        //        MakeBatchfile(connectionString, CalcObjectType.House);
        //        return;
        //    }

        //    if (bo.Settlements) {
        //        MakeBatchfile(connectionString, CalcObjectType.Settlement);
        //    }
        //}
    }
}
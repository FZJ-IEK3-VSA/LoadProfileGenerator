using JetBrains.Annotations;
using PowerArgs;

namespace SimulationEngine {
    [UsedImplicitly]
    public class HouseJobProcessingOptions
    {
        [ArgDescription("Path to the house job json file")]
        [ArgShortcut("J")]
        [UsedImplicitly]
        [CanBeNull]
        public string JsonPath { get; set; }

    }
    [UsedImplicitly]
    public class DistrictDefinitionProcessingOptions
    {
        [ArgDescription("Path to the directory with all the json files")]
        [ArgShortcut("J")]
        [UsedImplicitly]
        [CanBeNull]
        public string JsonPath { get; set; }

        [ArgDescription("Path to the calculation specification that configures which files will be generated, " +
                        "the start and end date and more. You can generate an example using the option CreateDistrictDefinition.")]
        [ArgShortcut("cd")]
        [UsedImplicitly]
        [CanBeNull]
        public string CalculationDefinition { get; set; }


        [ArgDescription("Path to the results directory where all the newly created databases will be stored.")]
        [ArgShortcut("D")]
        [UsedImplicitly]
        [CanBeNull]
        public string DstPath { get; set; }

        [ArgDescription("Do not delete existing files.")]
        [ArgShortcut("Skip")]
        [UsedImplicitly]
        public bool SkipExisiting { get; set; }

        [ArgDescription("Limit the processing to the first x files (mostly for testing, since generating thousands of houses can take an hour or more)." +
                        " If this is not set, all files in the directory will be processed.")]
        [ArgShortcut("L")]
        [UsedImplicitly]
        public int Limit { get; set; }

    }
}
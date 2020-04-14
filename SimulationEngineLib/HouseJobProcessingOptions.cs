using JetBrains.Annotations;
using PowerArgs;

namespace SimulationEngineLib {
    [UsedImplicitly]
    public class HouseJobProcessingOptions
    {
        [ArgDescription("Path to the house job json file")]
        [ArgShortcut("J")]
        [UsedImplicitly]
        [CanBeNull]
        public string JsonPath { get; set; }

    }
}
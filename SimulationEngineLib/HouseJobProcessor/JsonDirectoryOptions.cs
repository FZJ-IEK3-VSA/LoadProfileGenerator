using JetBrains.Annotations;
using PowerArgs;

namespace SimulationEngineLib.HouseJobProcessor {
    public class JsonDirectoryOptions
    {
        public JsonDirectoryOptions()
        {
        }

        public JsonDirectoryOptions([CanBeNull] string input) => Input = input;

        [UsedImplicitly]
        [ArgDescription("Sets the filename of the json file to use. Required.")]
        [ArgShortcut("i")]
        [CanBeNull]
        [ArgRequired]
        public string Input { get; set; }
    }
}